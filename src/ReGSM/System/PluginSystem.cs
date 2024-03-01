using ReGSM.Fundamental;

namespace ReGSM.System;

internal interface IPluginSystem
{
    
}

internal interface IPluginSystemInternal : IPluginSystem, ISystem;

internal class PluginSystem : IPluginSystemInternal
{

    public PluginSystem(IShareSystemInternal shareSystem)
    {

    }

    private readonly List<PluginInstance> _instances = [];

    public bool Init()
    {

        return true;
    }

    public void Shutdown()
    {
    }

    private void LoadPlugins()
    {

    }
}