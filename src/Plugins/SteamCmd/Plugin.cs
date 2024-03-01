using ReGSM.Fundamental;
using ReGSM.System;

namespace SteamCmd;

[Plugin(Name = "SteamCmd", Author = "ReGSM", Version = "1.0.0", Description = "SteamCmd插件")]
// ReSharper disable once UnusedMember.Global
public class Plugin : ReGsmPlugin
{
    public Plugin(IShareSystem shareSystem, string? dllPath, string? rootPath)
    {
        Console.WriteLine("SteamCmd: Construct");
    }

    public override bool OnLoad()
    {
        Console.WriteLine($"OnLoad: Name = {Name}, Author = {Author}, Version = {Version}, Description = {Description}, Url = {Url}");
        return true;
    }

    public override void OnAllLoaded()
    {
    }

    public override void OnUnload()
    {
    }

    public override void QueryRunning()
    {
    }
}