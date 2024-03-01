using System.Reflection;

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

public class ReGsm : IReGsmInternal
{
    public bool Init()
    {

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
}