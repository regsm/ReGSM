using ReGSM.System;

namespace ReGSM.Abstractions;

public interface IPlugin
{
    bool OnLoad();

    void OnAllLoaded();

    void OnUnload();

    bool QueryRunning();

    void NotifyInterfaceDrop(ISharable @interface);
}

public abstract class ReGsmPlugin : IPlugin
{
    public record PluginInfo(string Name, string Author, string Version, string Url, string Description);

    public readonly PluginInfo MyInfo = null!;

    public record InterfaceBridge(
        string EntryPath,
        string PluginPath,
        IShareSystem ShareSystem,
        IPluginSystem PluginSystem
    );

    private readonly InterfaceBridge _bridge = null!;

    protected string InstancePath => _bridge.EntryPath;
    protected string MyPath => _bridge.PluginPath;
    protected IShareSystem ShareSystem => _bridge.ShareSystem;
    protected IPluginSystem PluginSystem => _bridge.PluginSystem;

    public virtual bool OnLoad() => true;

    public virtual void OnAllLoaded()
    {

    }

    public virtual void OnUnload()
    {

    }

    public virtual bool QueryRunning() => true;

    public virtual void NotifyInterfaceDrop(ISharable @interface)
    {
        
    }
}