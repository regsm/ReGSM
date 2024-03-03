using System.Reflection;
using McMaster.NETCore.Plugins;
using ReGSM.System;

namespace ReGSM.Fundamental;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PluginAttribute : Attribute
{
    public required string Name { get; set; }
    public required string Author { get; set; }
    public required string Version { get; set; }
    public string Url { get; set; } = "https://github.com/regsm";
    public string Description { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{GetType().Name}(" +
               $"Name = \"{Name}\", " +
               $"Author = \"{Author}\", " +
               $"Version = \"{Version}\"," +
               $"Url = \"{Url}\"," +
               $"Description = \"{Description}\"" +
               $")";
    }
}

/// <summary>
/// 插件状态
/// </summary>
public enum PluginStatus
{
    None = 0,
    /// <summary>
    /// 已加载
    /// </summary>
    Checked,
    /// <summary>
    /// 运行中
    /// </summary>
    Running,
    /// <summary>
    /// 运行时发生错误
    /// </summary>
    Error,
    /// <summary>
    /// 发生错误无法运行
    /// </summary>
    Failed,
}

public interface IPlugin
{
    bool OnLoad();
    void OnAllLoaded();
    void OnUnload();
    bool QueryRunning();
    void NotifyInterfaceDrop(IShareable @interface);
}

public abstract class ReGsmPlugin : IPlugin
{
    public string Name { get; internal set; } = null!;
    public string Author { get; internal set; } = null!;
    public string Version { get; internal set; } = null!;
    public string Url { get; internal set; } = string.Empty;
    public string Description { get; internal set; } = string.Empty;

    public virtual bool OnLoad() => true;

    public virtual void OnAllLoaded()
    {

    }

    public virtual void OnUnload()
    {

    }

    public virtual bool QueryRunning() => true;

    public virtual void NotifyInterfaceDrop(IShareable @interface)
    {
        
    }
}

internal class PluginInstance
{
    public ReGsmPlugin? Instance { get; private set; }
    private PluginLoader? _loader;
    private readonly IShareSystemInternal _shareSystem;
    private readonly string _dllPath;
    private readonly string _dllFile;
    private readonly string _rootPath;
    private readonly int _threadId;

    public string EntryPath => _dllPath;

    public PluginStatus Status { get; private set; }

    

    public PluginInstance(IShareSystemInternal shareSystem, string rootPath, string dllFile, string dllPath)
    {
        _shareSystem = shareSystem;
        _rootPath = rootPath;
        _dllFile = dllFile;
        _dllPath = dllPath;
        _threadId = Environment.CurrentManagedThreadId;
        Status = PluginStatus.Checked;
    }

    public void Query()
    {
        try
        {

            if (!Instance!.QueryRunning())
            {
                throw new InvalidOperationException();
            }
        }
        catch (Exception e)
        {
            Status = PluginStatus.Failed;
            if (e is not InvalidOperationException)
            {
                Console.WriteLine($"Failed to query plugin \"{Instance!.Name}\": {e.Message}{Environment.NewLine}{e.StackTrace}");
            }
        }
    }

    public bool Init()
    {
        var loader = PluginLoader.CreateFromAssemblyFile(_dllFile, config =>
        {
            config.PreferSharedTypes = true;
            config.IsUnloadable = true;
            config.LoadInMemory = true;
        });

        var asm = loader.LoadDefaultAssembly();
        var plugin = asm.GetTypes().FirstOrDefault(t => typeof(ReGsmPlugin).IsAssignableFrom(t) && !t.IsAbstract) ??
                     throw new BadImageFormatException("This plugin does not derived from ReGsmPlugin!");
        var attr = Attribute.GetCustomAttribute(plugin, typeof(PluginAttribute)) as PluginAttribute ??
                   throw new BadImageFormatException("Plugin metadata not found");
        Console.WriteLine($"{attr}");
        if (Activator.CreateInstance(plugin, _shareSystem, _dllPath, _rootPath) is not ReGsmPlugin instance)
        {
            loader.Dispose();
            return false;
        }
        
        // 防呆
        instance.Name = attr.Name ?? throw new ArgumentException("Cannot retrieve plugin's name!");
        instance.Author = attr.Author ?? throw new ArgumentException("Cannot retrieve plugin's author!");
        instance.Version = attr.Version ?? throw new ArgumentException("Cannot retrieve plugin's version!");
        instance.Url = attr.Url;
        instance.Description = attr.Description;

        Instance = instance;
        _loader = loader;

        return true;
    }

    public bool Load()
    {
        try
        {

            if (!Instance!.OnLoad())
            {
                throw new InvalidOperationException();
            }

            Status = PluginStatus.Running;

            return true;
        }
        catch (Exception e)
        {
            Status = PluginStatus.Failed;
            Console.WriteLine(
                $"Failed to load plugin {Instance!.Name}: {e.Message}{Environment.NewLine}{e.StackTrace}"
            );
            return false;
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