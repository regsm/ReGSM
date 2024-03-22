using McMaster.NETCore.Plugins;
using ReGSM.Abstractions;
using ReGSM.Attributes;
using ReGSM.Enums;
using ReGSM.Fundamental.Extension;
using ReGSM.System;

namespace ReGSM.Fundamental;

internal class PluginInstance(
    IShareSystemInternal shareSystem,
    IPluginSystemInternal pluginSystem,
    string instanceFile,
    string pluginPath)
{
    public ReGsmPlugin? Instance { get; private set; }
    private PluginLoader? _loader;
    private readonly int _threadId = Environment.CurrentManagedThreadId;

    public string EntryPath => pluginPath;

    public PluginStatus Status { get; set; } = PluginStatus.Checked;
    public Exception? Error { get; set; }

    public bool Init()
    {
        var loader = PluginLoader.CreateFromAssemblyFile(instanceFile, config =>
        {
            config.PreferSharedTypes = true;
            config.IsUnloadable = true;
            config.LoadInMemory = true;
        });

        var asm = loader.LoadDefaultAssembly();
        var plugin = asm.GetTypes().FirstOrDefault(t => typeof(ReGsmPlugin).IsAssignableFrom(t) && !t.IsAbstract) ??
                     throw new BadImageFormatException("This plugin does not derived from PylonPlugin!");
        var attr = Attribute.GetCustomAttribute(plugin, typeof(PluginAttribute)) as PluginAttribute ??
                   throw new BadImageFormatException("Plugin metadata not found");
        Console.WriteLine($"{attr}");
        if (Activator.CreateInstance(plugin) is not ReGsmPlugin instance)
        {
            loader.Dispose();
            Status = PluginStatus.Failed;
            return false;
        }

        plugin.SetPublicReadOnlyField("MyInfo", instance,
            new ReGsmPlugin.PluginInfo(
                attr.Name,
                attr.Author,
                attr.Version,
                attr.Url,
                attr.Description
            )
        );
        plugin.BaseType!.SetReadOnlyField("_bridge", instance,
            new ReGsmPlugin.InterfaceBridge(
                instanceFile,
                pluginPath,
                shareSystem,
                pluginSystem
            )
        );


        Instance = instance;
        _loader = loader;
        
        return true;
    }

    public void Load()
    {
        try
        {

            if (!Instance!.OnLoad())
            {
                throw new InvalidOperationException();
            }

            Status = PluginStatus.Running;

        }
        catch (Exception e)
        {
            Status = PluginStatus.Failed;
            Error = e;
            Console.WriteLine(
                $"Failed to load plugin {Instance!.MyInfo.Name}: {e.Message}{Environment.NewLine}{e.StackTrace}"
            );
        }
    }

    public void Unload()
    {
        if (_loader == null || Instance == null)
        {
            return;
        }

        Instance.OnUnload();
        Instance = null;

        _loader.Dispose();
        _loader = null;

        Status = PluginStatus.None;

    }
}