using System.Collections.Concurrent;
using System.Reflection;
using Looplet.Abstractions.Interfaces;
using Looplet.Worker.Infrastructure.Features.Plugins;
using Serilog;

namespace Looplet.Worker.Infrastructure.Features.Plugins;

public static class PluginLoader
{
    private static readonly ConcurrentDictionary<string, PluginLoadContext> _loadContexts = new();
    private static void LoadPlugin(string pluginPath, IServiceCollection services)
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
                .Where(t => typeof(IJobModule).IsAssignableFrom(t)
                         && !t.IsAbstract && !t.IsInterface);

            foreach (Type m in mods)
            {
                var module = (IJobModule) Activator.CreateInstance(m)!;
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

        List<string> available = Directory.Exists(pluginsPath)
            ? [.. Directory.EnumerateFiles(pluginsPath, "*.dll").Select(Path.GetFileNameWithoutExtension)]
            : [];

        return available;
    }
}
