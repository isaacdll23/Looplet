using System.Collections.Concurrent;
using System.Reflection;
using Looplet.Abstractions.Interfaces;
using Serilog;

namespace Looplet.Worker.Infrastructure.Features.Plugins;

public static class PluginLoader
{
    private static readonly ConcurrentDictionary<string, PluginLoadContext> _loadContexts = new();
    private static readonly string _pluginsDirectory = Path.Combine(AppContext.BaseDirectory, "Plugins");
    private static void LoadPlugin(string pluginPath, IServiceCollection? services)
    {
        var pluginName = Path.GetFileNameWithoutExtension(pluginPath);
        if (_loadContexts.ContainsKey(pluginName))
        {
            Log.Logger.Information("Plugin {Plugin} already loaded", pluginName);
            return;
        }

        var loadContext = new PluginLoadContext(pluginPath);
        try
        {
            // Load the plugin’s main assembly
            var name = new AssemblyName(pluginName);
            Assembly asm = loadContext.LoadFromAssemblyName(name);
            Log.Logger.Information("Loaded plugin {Name}", asm.FullName);

            // register modules
            IEnumerable<Type> mods = asm.GetTypes()
                .Where(t => typeof(IPlugin).IsAssignableFrom(t)
                         && !t.IsAbstract && !t.IsInterface);

            foreach (Type m in mods)
            {
                var module = (IPlugin) Activator.CreateInstance(m)!;
                if (services == null)
                {
                    Log.Logger.Information("No services provided for plugin {Type}", m.FullName);
                    continue;
                }
                module.ConfigureServices(services);
                Log.Logger.Information("Configured module {Type}", m.FullName);
            }

            _loadContexts.TryAdd(pluginName, loadContext);
            Log.Logger.Information("Loaded plugin {Plugin}", pluginName);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Failed loading plugin {File}", pluginName);
            loadContext.Unload();
        }
    }

    public static bool UnloadPlugin(string pluginPath)
    {
        var pluginName = Path.GetFileNameWithoutExtension(pluginPath);
        if (!_loadContexts.TryRemove(pluginName, out var loadContext))
        {
            Log.Logger.Information("Plugin {Plugin} not loaded", pluginName);
            return false;
        }

        try
        {
            loadContext.Unload();
            Log.Logger.Information("Unloaded plugin {Plugin}", pluginName);
            return true;
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Failed unloading plugin {Plugin}", pluginName);
            return false;
        }
    }

    public static List<string> GetLoadedPlugins()
    {
        return [.. _loadContexts.Keys];
    }

    public static List<string> GetAvailablePlugins()
    {
        var pluginsPath = Path.Combine(AppContext.BaseDirectory, "Plugins");

        if (!Directory.Exists(pluginsPath))
        {
            Log.Logger.Information("Plugins directory {PluginsPath} does not exist", pluginsPath);
            return [];
        }

        // Get all DLL files in the plugins directory and its subdirectories
        var pluginFiles = Directory.GetFiles(pluginsPath, "*.dll", SearchOption.AllDirectories);

        var available = new List<string>();

        foreach (var pluginFile in pluginFiles)
        {
            var pluginName = Path.GetFileNameWithoutExtension(pluginFile);
            available.Add(pluginName);
        }

        return available;
    }

    public static List<string> GetAvailableJobs(string pluginName)
    {
        if (!_loadContexts.TryGetValue(pluginName, out var loadContext))
        {
            Log.Logger.Information("Plugin {Plugin} not loaded. Loading to list jobs.", pluginName);

            var pluginPath = Path.Combine(_pluginsDirectory, pluginName, $"{pluginName}.dll");
            LoadPlugin(pluginPath, new ServiceCollection());
            if (!_loadContexts.TryGetValue(pluginName, out loadContext))
            {
                Log.Logger.Error("Failed to load plugin {Plugin} to list jobs.", pluginName);
                return [];
            }
        }

        Assembly asm = loadContext.LoadFromAssemblyName(new AssemblyName(pluginName));
        var jobs = asm.GetTypes()
            .Where(t => typeof(IJob).IsAssignableFrom(t)
                     && !t.IsAbstract && !t.IsInterface)
            .Select(t => t.Name)
            .ToList();

        return jobs;
    }
}
