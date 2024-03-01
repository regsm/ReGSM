namespace ReGSM;

internal class Program
{
    private static readonly IReGsmInternal Instance = new ReGsm();
    static void Main(string[] args)
    {
        if (!Instance.Init())
        {
            throw new ApplicationException("Startup ReGSM Failed!");
        }
        Instance.Shutdown();
        
    }
}