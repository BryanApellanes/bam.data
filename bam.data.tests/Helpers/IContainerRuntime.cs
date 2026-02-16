namespace Bam.Container
{
    public interface IContainerRuntime
    {
        string Run(ContainerConfig config);
        void Stop(string name, int timeoutSeconds = 5);
        void Remove(string name, bool force = true);
        bool IsRunning(string name);
        string Inspect(string name, string format);
    }
}
