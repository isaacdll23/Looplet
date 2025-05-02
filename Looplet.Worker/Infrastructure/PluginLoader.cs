using System.Reflection;
using Looplet.Abstractions.Interfaces;
using Serilog;

namespace Looplet.Worker.Infrastructure;

public static class PluginLoader
{
    private static readonly List<string> _loadedModules = [];
    private static readonly List<string> _loadedJobs = [];
    public static IReadOnlyList<string> LoadedModules => _loadedModules.AsReadOnly();
    public static IReadOnlyList<string> LoadedJobs => _loadedJobs.AsReadOnly();

    public static void LoadPlugins(IServiceCollection services,
                                   string pluginsPath)
    {
        _loadedModules.Clear();
        var dir = Path.Combine(AppContext.BaseDirectory, pluginsPath);
        if (!Directory.Exists(dir))
        {
            Log.Logger.Warning("Plugins dir {Dir} missing – creating it", dir);
            Directory.CreateDirectory(dir);
        }

        var pluginFiles = Directory.GetFiles(dir, "*.dll");

        if (pluginFiles.Length == 0)
        {
            Log.Logger.Information("No plugins found in {Dir}", dir);
            return;
        }

        Log.Logger.Information("Found {Count} plugin(s) in {Dir}",
                               pluginFiles.Length, dir);

        foreach (var dll in pluginFiles)
        {
            try
            {
                var asm = Assembly.LoadFrom(dll);
                Log.Logger.Information("Loaded plugin assembly {Name}", asm.FullName);

                var modules = asm.GetTypes()
                  .Where(t => typeof(IJobModule).IsAssignableFrom(t)
                           && !t.IsAbstract
                           && !t.IsInterface);

                foreach (var modType in modules)
                {
                    try
                    {
                        var module = (IJobModule) Activator.CreateInstance(modType)!;
                        module.ConfigureServices(services);
                        _loadedModules.Add($"{modType.FullName!}, {asm.GetName().Name}");
                        Log.Logger.Information("Configured jobs from module {Type}",
                                               modType.FullName);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex,
                          "Failed to instantiate or configure module {Type}",
                           modType.FullName);
                    }
                }

                List<Type> jobs = asm.GetTypes().Where(t => typeof(IJob).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).ToList();
                jobs.ForEach(job => _loadedJobs.Add($"{job.FullName!}, {asm.GetName().Name}"));

            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex,
                  "Failed to load plugin assembly from {File}", dll);
            }
        }
    }
}

