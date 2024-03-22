namespace ReGSM.Attributes;

/// <summary>
/// 插件元数据
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PluginAttribute : Attribute
{
    /// <summary>
    /// 插件名
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 插件作者
    /// </summary>
    public required string Author { get; set; }
    
    /// <summary>
    /// 插件版本
    /// </summary>
    public required string Version { get; set; }
    
    /// <summary>
    /// 插件地址
    /// </summary>
    public string Url { get; set; } = "https://github.com/regsm";

    /// <summary>
    /// 插件介绍
    /// </summary>
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