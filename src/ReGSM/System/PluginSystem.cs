using ReGSM.Fundamental;

namespace ReGSM.System;

internal interface IPluginSystem
{
    
}

internal interface IPluginSystemInternal : IPluginSystem, ISystem
{
    public void Signal();
}

internal class PluginSystem : IPluginSystemInternal
{
    private readonly IReGsm _reGsm;
    private readonly IShareSystemInternal _shareSystem;
    public PluginSystem(IShareSystemInternal shareSystem)
    {
        _shareSystem = shareSystem;
        _reGsm = _shareSystem.ReGsm;
    }

    private readonly List<PluginInstance> _instances = [];

    public bool Init()
    {
        LoadPlugins();
        return true;
    }

    public void Shutdown()
    {
        UnloadPlugins();
    }

    public void Signal()
    {
        foreach (var plugin in _instances.Where(x => x.Status == PluginStatus.Running))
        {
            plugin.Query();
        }
    }

    private void UnloadPlugins()
    {
        foreach (var instance in _instances)
        {
            var interfaces = _shareSystem.GetPluginInterfaces(instance.Instance!).ToList();

            if (interfaces.Any())
            {
                foreach (var @interface in interfaces)
                {
                    foreach (var x in _instances.Where(x => !x.Equals(instance)))
                    {
                        x.Instance!.NotifyInterfaceDrop(@interface);
                    }
                }
            }

            instance.Unload();
        }
    }

    private void LoadPlugins()
    {
        var pluginPath = Path.Combine(Directory.GetParent(_reGsm.ReGsmPath)!.FullName, "plugins");
        foreach (var directory in Directory.GetDirectories(pluginPath))
        {
            var rtConfig = Directory
                .GetFiles(directory, "*.deps.json")
                .FirstOrDefault();
            var entryDll = rtConfig?.Replace(".deps.json", ".dll");

            if (string.IsNullOrEmpty(entryDll) || !File.Exists(entryDll))
            {
                continue;
            }

            var plugin = new PluginInstance(_shareSystem, directory, entryDll, pluginPath);
            if (!plugin.Init())
            {
                // TODO: LogError
                continue;
            }

            if (!plugin.Load())
            {
                continue;
            }

            _instances.Add(plugin);
        }

        _instances.ForEach(x => x.Instance!.OnAllLoaded());
    }
}