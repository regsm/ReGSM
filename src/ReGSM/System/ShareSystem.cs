using Microsoft.Extensions.DependencyInjection;
using ReGSM.Fundamental;

namespace ReGSM.System;



public interface IShareSystem
{
    void AddInterface(IShareable @interface, IPlugin plugin);

    T GetRequiredInterface<T>(uint version) where T : class, IShareable;

    T? GetInterface<T>(uint version) where T : class, IShareable;

    IReGsm ReGsm { get; }
}

internal interface IShareSystemInternal : IShareSystem
{
    IEnumerable<IShareable> GetPluginInterfaces(IPlugin plugin);
}

internal abstract class ShareSystemInternal : IShareSystemInternal
{
    public virtual void AddInterface(IShareable @interface, IPlugin plugin)
    {
        throw new NotImplementedException();
    }

    public virtual T GetRequiredInterface<T>(uint version) where T : class, IShareable
    {
        throw new NotImplementedException();
    }

    public virtual T? GetInterface<T>(uint version) where T : class, IShareable
    {
        throw new NotImplementedException();
    }

    public virtual IReGsm ReGsm => throw new NotImplementedException();

    public virtual IEnumerable<IShareable> GetPluginInterfaces(IPlugin plugin)
    {
        throw new NotImplementedException();
    }
}

internal class ShareSystem(IServiceProvider provider) : ShareSystemInternal
{
    public override IReGsm ReGsm { get; } = provider.GetRequiredService<IReGsm>();

    public override IEnumerable<IShareable> GetPluginInterfaces(IPlugin plugin)
        => _interfaces
            .Where(x => x.Plugin == plugin)
            .Select(x => x.Instance);

    private record ShareableInfo(IShareable Instance, IPlugin Plugin);

    /// <summary>
    /// 有些插件可以有多个接口, 没必要搞得这么复杂, 一个List完事.
    /// </summary>
    private readonly List<ShareableInfo> _interfaces = [];


    public override void AddInterface(IShareable @interface, IPlugin plugin)
    {
        var name = @interface.InterfaceName;
        var type = @interface.GetType();
        
        Console.WriteLine($"AddInterface::Type: {typeof(IShareable).IsAssignableFrom(type)}");
        if (_interfaces.Any(x => x.Instance.InterfaceName == name))
        {
            throw new InvalidOperationException($"Interface with name {name} already exists.");
        }

        _interfaces.Add(new ShareableInfo(@interface, plugin));
    }

    public override T GetRequiredInterface<T>(uint version)
    {
        Console.WriteLine("GetRequiredInterface");
        var @interface =
            _interfaces.SingleOrDefault(x =>
                x.Instance.GetType().GetInterfaces().Any(t => t == typeof(T))
            ) ??
            throw new NotImplementedException($"Interface <{nameof(T)}> not found.");

        if (@interface.Instance.InterfaceVersion < version)
        {
            throw new NotImplementedException($"Interface <{nameof(T)}> version is lower.");
        }

        return (T)@interface.Instance;
    }

    public override T? GetInterface<T>(uint version) where T: class
        => (T?) _interfaces
            .SingleOrDefault(x =>
                x.GetType() == typeof(T) &&
                x.Instance.InterfaceVersion >= version
            )?
            .Instance;
}