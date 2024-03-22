using ReGSM.Abstractions;
using ReGSM.Enums;
using ReGSM.Fundamental;

namespace ReGSM.System;

public interface IPluginSystem
{
    public uint GetPluginCount();
}

internal interface IPluginSystemInternal : IPluginSystem, ISystem
{
    delegate void DelegatePluginUnload(ReGsmPlugin instance);

    event DelegatePluginUnload PluginUnload;
}

internal class PluginSystem(IReGsm reGsm, IShareSystemInternal shareSystem) : IPluginSystemInternal
{
    private readonly List<PluginInstance> _instances = [];

    public bool Init()
    {
        ScanPlugins();
        ActivatePlugins();
        return true;
    }

    public void Shutdown()
    {
        DeactivatePlugins();
    }

    public uint GetPluginCount()
    {
        return 999;
    }

    private void ScanPlugins()
    {
        var pluginsDir = Path.Combine(reGsm.ReGsmPath, "plugins");
        foreach (var pluginDir in Directory.GetDirectories(pluginsDir))
        {
            var entryDll = ProbePluginEntryDll(pluginDir);
            if (string.IsNullOrEmpty(entryDll) || !File.Exists(entryDll))
            {
                continue;
            }

            _instances.Add(new PluginInstance(
                shareSystem, 
                this,
                entryDll,
                pluginDir
                )
            );
        }
    }

    private void ActivatePlugins()
    {
        _instances.ForEach(x =>
        {
            if (!x.Init())
            {
                return;
            }

            x.Load();
        });
        _instances.ForEach(x =>
        {
            x.Instance!.OnAllLoaded();
        });
    }

    private void DeactivatePlugins()
    {
        foreach (var instance in _instances)
        {
            instance.Unload();
        }
    }

    private string ProbePluginEntryDll(string pluginDir)
    {
        // ReSharper disable StringLiteralTypo
        // ReSharper disable CommentTypo
        var runtimeConfigFiles = Directory.GetFiles(pluginDir, "*.deps.json");
        var entryDll = runtimeConfigFiles.Length != 1 // 首先看.deps.json的数量
            ? Path.Combine(pluginDir, $"{pluginDir}.dll") // 如果没有, 看和文件夹同名的.dll
            : runtimeConfigFiles[0]
                .Replace(".deps.json", ".dll"); // 如果超过了1个, 也就是2个或以上, 只看第一个.deps.json及其配套.dll
        // ReSharper restore CommentTypo
        // ReSharper restore StringLiteralTypo
        return entryDll;
    }

    public event IPluginSystemInternal.DelegatePluginUnload? PluginUnload;
}