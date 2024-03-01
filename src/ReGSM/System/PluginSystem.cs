using ReGSM.Fundamental;

namespace ReGSM.System;

internal interface IPluginSystem
{
    
}

internal interface IPluginSystemInternal : IPluginSystem, ISystem;

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
            if (!plugin.Load())
            {
                // TODO: LogError
                continue;
            }

            _instances.Add(plugin);
        }
        
    }
}