using System.Diagnostics;
using Bam.CommandLine;

namespace Bam.Data.Tests.Helpers
{
    public static class PodmanContainerHelper
    {
        public static string StartContainer(string name, string image, string portMapping, params string[] envVars)
        {
            StopAndRemoveContainer(name);

            string envArgs = string.Join(" ", envVars.Select(e => $"-e {e}"));
            string args = $"run -d --name {name} -p {portMapping} {envArgs} {image}";
            return RunPodman(args).Trim();
        }

        public static bool WaitForReady(string name, int maxWaitSeconds, Func<bool> isReady)
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(maxWaitSeconds);
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    if (isReady())
                    {
                        return true;
                    }
                }
                catch
                {
                    // not ready yet
                }

                Thread.Sleep(2000);
            }

            return false;
        }

        public static void StopAndRemoveContainer(string name)
        {
            try
            {
                RunPodman($"stop {name}");
            }
            catch
            {
                // container may not be running
            }

            try
            {
                RunPodman($"rm -f {name}");
            }
            catch
            {
                // container may not exist
            }
        }

        public static bool IsContainerRunning(string name)
        {
            try
            {
                string output = RunPodman($"inspect --format \"{{{{.State.Running}}}}\" {name}").Trim();
                return output.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static string RunPodman(string args)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "podman",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            ProcessOutput result = psi.Run();

            if (result.ExitCode != 0 && !string.IsNullOrWhiteSpace(result.StandardError))
            {
                throw new InvalidOperationException($"podman {args} failed: {result.StandardError}");
            }

            return result.StandardOutput;
        }
    }
}
