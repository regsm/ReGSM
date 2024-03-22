using ReGSM.Abstractions;
using ReGSM.Fundamental;

namespace ReGSM.System;

public interface IShareSystem
{
    void AddInterface(ISharable @interface, IPlugin plugin);

    T GetRequiredInterface<T>(uint version) where T : class, ISharable;

    T? GetInterface<T>(uint version) where T : class, ISharable;
}

internal interface IShareSystemInternal : IShareSystem, ISystem
{
    IEnumerable<ISharable> GetPluginInterfaces(IPlugin plugin);
}

internal class ShareSystem(IServiceProvider provider) : IShareSystemInternal
{
    public IEnumerable<ISharable> GetPluginInterfaces(IPlugin plugin)
        => _interfaces
            .Where(x => x.Plugin == plugin)
    .Select(x => x.Instance);

    private record ShareableInfo(ISharable Instance, IPlugin Plugin);

    /// <summary>
    /// 有些插件可以有多个接口, 没必要搞得这么复杂, 一个List完事.
    /// </summary>
    private readonly List<ShareableInfo> _interfaces = [];


    public void AddInterface(ISharable @interface, IPlugin plugin)
    {
        var name = @interface.InterfaceName;
        var type = @interface.GetType();
        
        if (_interfaces.Any(x => x.Instance.InterfaceName == name))
        {
            throw new InvalidOperationException($"Interface with name {name} already exists.");
        }

        _interfaces.Add(new ShareableInfo(@interface, plugin));
    }

    public T GetRequiredInterface<T>(uint version) where T : class, ISharable
    {
        var @interface =
            _interfaces.SingleOrDefault(x =>
                x.Instance.GetType().GetInterfaces().Any(t => t == typeof(T))
            ) ??
            throw new NotImplementedException($"Interface <{typeof(T).Name}> not found.");

        if (@interface.Instance.InterfaceVersion < version)
        {
            throw new NotImplementedException($"Interface <{nameof(T)}> version is lower.");
        }

        return (T)@interface.Instance;
    }

    public T? GetInterface<T>(uint version) where T : class, ISharable =>
        (T?) _interfaces
            .SingleOrDefault(x =>
                x.Instance.GetType() == typeof(T) &&
                x.Instance.InterfaceVersion >= version
            )?
            .Instance;

    public bool Init() => true;

    public void Shutdown()
    {
    }
}