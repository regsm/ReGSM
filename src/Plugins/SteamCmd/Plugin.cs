using ReGSM.Fundamental;
using ReGSM.System;

namespace SteamCmd;

public interface ISteamCmd : IShareable
{
    void InstallGame(int appId);
}

[Plugin(Name = "SteamCmd", Author = "ReGSM", Version = "1.0.0", Description = "SteamCmd插件")]
// ReSharper disable once UnusedMember.Global
public class Plugin : ReGsmPlugin, ISteamCmd
{
    private readonly IShareSystem _shareSystem;
    public Plugin(IShareSystem shareSystem, string? dllPath, string? rootPath)
    {
        ArgumentNullException.ThrowIfNull(dllPath);
        ArgumentNullException.ThrowIfNull(rootPath);

        _shareSystem = shareSystem;
    }

    public override bool OnLoad()
    {
        Console.WriteLine($"OnLoad: Name = {Name}, Author = {Author}, Version = {Version}, Description = {Description}, Url = {Url}");
        _shareSystem.AddInterface(this, this);
        return true;
    }

    public override void OnAllLoaded()
    {
        var export = _shareSystem.GetRequiredInterface<ISteamCmd>(1);
        export.InstallGame(730);
    }

    public override void OnUnload()
    {
    }

    public override bool QueryRunning()
    {
        return true;
    }

    public override void NotifyInterfaceDrop(IShareable @interface)
    {
    }

    public void InstallGame(int appId)
    {
        Console.WriteLine($"AppId: {appId}");
    }

    public string InterfaceName => "ISteamCmd";
    public uint InterfaceVersion => 1;
}