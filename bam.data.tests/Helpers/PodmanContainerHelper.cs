using System.Diagnostics;

namespace Bam.Data.Tests.Helpers
{
    public static class PodmanContainerHelper
    {
        public static string StartContainer(string name, string image, string portMapping, params string[] envVars)
        {
            StopAndRemoveContainer(name);

            string envArgs = string.Join(" ", envVars.Select(e => $"-e {e}"));
            string args = $"run -d --name {name} -p {portMapping} {envArgs} {image}";
            return RunCommand(args).Trim();
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
                RunCommand($"stop {name}");
            }
            catch
            {
                // container may not be running
            }

            try
            {
                RunCommand($"rm -f {name}");
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
                string output = RunCommand($"inspect --format \"{{{{.State.Running}}}}\" {name}").Trim();
                return output.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static string RunCommand(string args)
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

            using Process? process = Process.Start(psi);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start podman process");
            }

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error))
            {
                throw new InvalidOperationException($"podman {args} failed: {error}");
            }

            return output;
        }
    }
}
