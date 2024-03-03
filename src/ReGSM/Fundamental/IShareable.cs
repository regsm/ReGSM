namespace ReGSM.Fundamental;


/// <summary>
/// 所有对外暴露的接口全部继承自这个接口.
/// </summary>
public interface IShareable
{
    /// <summary>
    /// 接口名
    /// </summary>
    string InterfaceName { get; }

    /// <summary>
    /// 接口版本
    /// </summary>
    uint InterfaceVersion { get; }
}