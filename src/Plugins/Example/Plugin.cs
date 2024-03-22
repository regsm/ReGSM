using ReGSM.Abstractions;
using ReGSM.Attributes;

namespace Example;

[Plugin(Name = "Example", Author = "...", Description = "Description", Url = "None", Version = "1.0.0")]
public class Plugin : ReGsmPlugin
{
    public override bool OnLoad()
    {
        return base.OnLoad();
    }

    public override void OnAllLoaded()
    {
        base.OnAllLoaded();
    }

    public override void OnUnload()
    {
        base.OnUnload();
    }
}