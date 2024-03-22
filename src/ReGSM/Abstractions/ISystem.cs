namespace ReGSM.Abstractions;

public interface ISystem
{
    bool Init();
    void Shutdown();
}