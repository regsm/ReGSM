using System.Reflection;

namespace ReGSM.Fundamental.Extension;


public static class ReflectionExtension
{
    public static void SetPublicReadOnlyField<TInstance, TValue>(this Type type, string name, TInstance instance, TValue value)
    {
        var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public)
                    ?? throw new MissingFieldException(type.FullName, name);
        field.SetValue(instance, value);
    }

    public static void SetReadOnlyField<TInstance, TValue>(this Type type, string name, TInstance instance, TValue value)
    {
        var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new MissingFieldException(type.FullName, name);
        field.SetValue(instance, value);
    }

    public static void SetReadonlyProperty<TInstance, TValue>(this Type type, string name, TInstance instance, TValue value)
    {
        var fName = $"<{name}>k__BackingField";
        var field = type.GetField(fName, BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new MissingFieldException(type.FullName, fName);

        field.SetValue(instance, value);
    }

    public static IEnumerable<T> GetAllServices<T>(this IServiceProvider provider)
    {
        var site = typeof(ServiceProvider)
            .GetProperty("CallSiteFactory", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(provider)!;
        var desc = site
            .GetType()
            .GetField("_descriptors", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(site) as ServiceDescriptor[];
        return desc!.Select(s => provider.GetRequiredService(s.ServiceType)).OfType<T>();
    }

    public static void CheckReturnAndParameters(this MethodInfo method, Type returnType, Type[] @paramsType)
    {
        if (method.ReturnParameter.ParameterType != typeof(void))
        {
            throw new BadImageFormatException("Bad return value: " + returnType.Name);
        }

        var @params = method.GetParameters();
        if (@params.Length != @paramsType.Length)
        {
            throw new BadImageFormatException("Parameters count mismatch");
        }

        for (var i = 0; i < @paramsType.Length; i++)
        {
            var type = @params[i].ParameterType;
            if (type != @paramsType[i])
            {
                throw new BadImageFormatException("Bad parameter type: " + type.Name);
            }
        }
    }
}