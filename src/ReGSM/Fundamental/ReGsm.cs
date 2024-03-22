using System.Reflection;

namespace ReGSM.Fundamental;

public interface IReGsm
{
    string ReGsmPath { get; }
}

internal interface IReGsmInternal : IReGsm;

internal class ReGsm : IReGsmInternal
{
    public string ReGsmPath
    {
        get
        {
            // ${ReGSMDir}/bin/regsm.exe
            var cwd = Assembly.GetExecutingAssembly().Location;
            var root = Directory.GetParent(cwd)!.Parent!.FullName;
            // ${ReGSMDir}
            return root;
        }
    }

}