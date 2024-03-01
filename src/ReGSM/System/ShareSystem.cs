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

internal interface IShareSystemInternal : IShareSystem;

internal class ShareSystem : IShareSystemInternal
{
    public IReGsm ReGsm { get; }

    public ShareSystem(IServiceProvider provider)
    {
        Console.WriteLine("ReGSM.ShareSystem: Construct");

        ReGsm = provider.GetRequiredService<IReGsm>();
    }

    private record ShareableInfo(IShareable Instance, IPlugin Plugin);

    /// <summary>
    /// 有些插件可以有多个接口, 没必要搞得这么复杂, 一个List完事.
    /// </summary>
    private readonly List<ShareableInfo> _interfaces = [];


    public void AddInterface(IShareable @interface, IPlugin plugin)
    {
        var name = @interface.Name;
        if (_interfaces.Any(x => x.Instance.Name == name))
        {
            throw new InvalidOperationException($"Interface with name {name} already exists.");
        }

        _interfaces.Add(new ShareableInfo(@interface, plugin));
    }

    public T GetRequiredInterface<T>(uint version) where T : class, IShareable
    {
        var @interface = _interfaces.SingleOrDefault(x => x.GetType() == typeof(T)) ??
                         throw new NotImplementedException($"Interface <{nameof(T)}> not found.");

        if (@interface.Instance.Version < version)
        {
            throw new NotImplementedException($"Interface <{nameof(T)}> version is lower.");
        }

        return (T)@interface.Instance;
    }

    public T? GetInterface<T>(uint version) where T : class, IShareable
        => (T?) _interfaces
            .SingleOrDefault(x =>
                x.GetType() == typeof(T) &&
                x.Instance.Version >= version
            )?
            .Instance;
}