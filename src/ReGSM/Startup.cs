using ReGSM.Components;
using ReGSM.Fundamental;
using ReGSM.System;

namespace ReGSM;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IReGsmInternal, ReGsm>();
        services.AddSingleton<IPluginSystemInternal, PluginSystem>();
        services.AddSingleton<IShareSystemInternal, ShareSystem>();
        services.AddSingleton<IReGsm>(x => x.GetRequiredService<IReGsmInternal>());

        // Add services to the container.
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        /*    private readonly IServiceProvider _provider;
           public ReGsm()
           {
               var services = new ServiceCollection();
               ConfigureServices(services);
               _provider = services.BuildServiceProvider();
           }

           private void ConfigureServices(IServiceCollection services)
           {
               services.AddSingleton<IReGsm, ReGsm>();
               services.AddSingleton<IPluginSystemInternal, PluginSystem>();
               services.AddSingleton<IShareSystemInternal, ShareSystem>();
           }
*/
    }

    public void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env,
        IHostApplicationLifetime lifetime
        )
    {
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseRouting();

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.UseEndpoints(ep =>
        {
            ep.MapRazorComponents<App>().AddInteractiveServerRenderMode();
        });

        lifetime.ApplicationStarted.Register(() =>
        {
            if (!app.ApplicationServices.GetRequiredService<IShareSystemInternal>().Init())
            {
                throw new ApplicationException($"Failed to initialize ShareSystem.");
            }

            if (!app.ApplicationServices.GetRequiredService<IPluginSystemInternal>().Init())
            {
                throw new ApplicationException($"Failed to initialize PluginSystem.");

            }
        });
        lifetime.ApplicationStopping.Register(() =>
        {
            app.ApplicationServices.GetRequiredService<IShareSystemInternal>().Shutdown();
            app.ApplicationServices.GetRequiredService<IPluginSystemInternal>().Shutdown();
        });
        lifetime.ApplicationStopped.Register(() =>
        {
        });
    }
}