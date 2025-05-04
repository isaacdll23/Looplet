using System.Reflection;
using System.Runtime.Loader;

namespace Looplet.Worker.Infrastructure.Features.Plugins;

public class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver? _resolver;
    private readonly string _pluginDirectory;

    public PluginLoadContext(string pluginPath)
        : base(Path.GetFileNameWithoutExtension(pluginPath), isCollectible: true)
    {
        _pluginDirectory = Path.GetDirectoryName(pluginPath)!;
        try
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }
        catch (InvalidOperationException)
        {
            _resolver = null;
        }
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (_resolver != null)
        {
            // try to resolve via the plugin’s .deps.json
            var pluginPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (pluginPath != null)
            {
                return LoadFromAssemblyPath(pluginPath);
            }
        }

        // if that fails, try to resolve via the default load context
        var assemblyPath = Path.Combine(_pluginDirectory, assemblyName.Name + ".dll");
        if (File.Exists(assemblyPath))
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }
}
