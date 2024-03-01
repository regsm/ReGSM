using McMaster.NETCore.Plugins;
using ReGSM.System;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;

namespace ReGSM.Fundamental;

public interface IPlugin
{
    bool OnLoad();
    void OnAllLoaded();
    void OnUnload();
    void QueryRunning();
}

public class PluginInstance
{
    private IPlugin? _instance;
    private PluginLoader? _loader;
    private Mutex? _mutex;
    private readonly IShareSystem _shareSystem;
    private readonly string _dllPath;
    private readonly string _dllFile;
    private readonly string _rootPath;
    private readonly int _threadId;

    public string EntryPath => _dllPath;

    public PluginInstance(IShareSystem shareSystem, string dllPath, string dllFile, string rootPath)
    {
        _shareSystem = shareSystem;
        _dllPath = dllPath;
        _dllFile = dllFile;
        _rootPath = rootPath;
        _threadId = Environment.CurrentManagedThreadId;
        _shareSystem = shareSystem;
    }

    public bool Load()
    {
        var loader = PluginLoader.CreateFromAssemblyFile(_dllFile, config =>
        {
            config.LoadInMemory = true;
            config.IsUnloadable = true;
            config.PreferSharedTypes = true;
        });
        var asm = loader.LoadDefaultAssembly();
        var plugin = asm.GetTypes().FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract) ??
                     throw new BadImageFormatException("IPlugin is not implemented.");
        var version = asm.GetName().Version;

        if (version is null)
        {
            loader.Dispose();

            // TODO: Logging
            //Log.Logger.Information("Could not versioning of {module} !", _dllFile);

            return false;
        }

        var matches = Regex.Matches(_dllFile, "[a-zA-Z0-9]+", RegexOptions.Compiled | RegexOptions.Singleline);
        var builder = new StringBuilder();

        foreach (Match m in matches)
        {
            builder.Append(m.Value);
        }

        var key = builder.ToString();
        _mutex = new Mutex(true, key, out var success);

        if (!success)
        {
            loader.Dispose();
            _mutex.Close();
            _mutex = null;

            //Log.Logger.Information("Could not double load of {module} !", _dllFile);

            return false;
        }

        if (Activator.CreateInstance(plugin, _shareSystem, _dllPath, _rootPath, version)
            is not IPlugin instance)
        {
            loader.Dispose();

            return false;
        }

        _loader   = loader;
        _instance = instance;

        return _instance.OnLoad();
    }

    public void Unload()
    {
        _mutex?.ReleaseMutex();
        _mutex?.Close();
        _mutex = null;

        if (_loader is null || _instance is null)
        {
            return;
        }

        //_modSharp.OnExternModuleUnload();
        _instance.OnUnload();
        _instance = null;

        _loader.Dispose();
        _loader = null;
    }
}