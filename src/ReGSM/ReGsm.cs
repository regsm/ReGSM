using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ReGSM.System;

namespace ReGSM;

public interface IReGsm
{
    string ReGsmPath { get; }
}

internal interface IReGsmInternal : IReGsm
{
    bool Init();
    void Shutdown();
}

internal class ReGsm : IReGsmInternal
{
    private readonly IServiceProvider _provider;
    public ReGsm()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _provider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IReGsm, ReGsm>();

        services.AddSingleton<IShareSystemInternal, ShareSystem>();
        services.AddSingleton<IPluginSystemInternal, PluginSystem>();
    }

    private void StartupServices()
    {
        _provider.GetRequiredService<IShareSystemInternal>();
        _provider.GetRequiredService<IPluginSystemInternal>();
    }


    public bool Init()
    {
        StartupServices();
        if (!GetPluginSystem().Init())
        {
            return false;
        }
        return true;
    }

    public void Shutdown()
    {
    }

    public string ReGsmPath
    {
        get
        {
            // ${ReGSMDir}/bin/regsm.exe
            var cwd = Assembly.GetExecutingAssembly().Location;
            var root = Directory.GetParent(cwd);
            // ${ReGSMDir}
            return root!.FullName;
        }
    }

    private IPluginSystemInternal GetPluginSystem() => _provider.GetRequiredService<IPluginSystemInternal>();
}