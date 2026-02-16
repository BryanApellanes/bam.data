using Bam.Container;
using Bam.Container.Podman;

namespace Bam.Data.Tests.Helpers
{
    public static class PodmanContainerHelper
    {
        private static readonly ContainerLifecycleManager Manager = new(new PodmanRuntime());

        public static string StartContainer(string name, string image, string portMapping, params string[] envVars)
            => Manager.StartFresh(new ContainerConfig
            {
                Name = name,
                Image = image,
                PortMapping = portMapping,
                EnvironmentVariables = envVars
            });

        public static void WaitForReady(string name, int maxWaitSeconds, Func<bool> isReady)
            => Manager.WaitForReady(name, maxWaitSeconds, isReady);

        public static void StopAndRemoveContainer(string name)
            => Manager.StopAndRemove(name);

        public static bool IsContainerRunning(string name)
            => Manager.Runtime.IsRunning(name);
    }
}
