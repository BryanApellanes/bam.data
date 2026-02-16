namespace Bam.Container
{
    public class ContainerLifecycleManager
    {
        public ContainerLifecycleManager(IContainerRuntime runtime)
        {
            Runtime = runtime;
        }

        public IContainerRuntime Runtime { get; }

        public string StartFresh(ContainerConfig config)
        {
            StopAndRemove(config.Name);
            return Runtime.Run(config);
        }

        public void WaitForReady(string name, int maxWaitSeconds, Func<bool> isReady)
        {
            System.Console.WriteLine($"Waiting up to {maxWaitSeconds}s for {name} to be ready...");
            DateTime deadline = DateTime.UtcNow.AddSeconds(maxWaitSeconds);
            Exception? lastException = null;
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    if (isReady())
                    {
                        System.Console.WriteLine($"{name} is ready.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                Thread.Sleep(2000);
            }

            throw new TimeoutException(
                $"Container {name} was not ready after {maxWaitSeconds}s. Last error: {lastException?.Message}",
                lastException);
        }

        public void StopAndRemove(string name)
        {
            try
            {
                Runtime.Stop(name);
            }
            catch
            {
                // container may not be running
            }

            try
            {
                Runtime.Remove(name, force: true);
            }
            catch
            {
                // container may not exist
            }
        }
    }
}
