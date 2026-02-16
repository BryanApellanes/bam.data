using System.Diagnostics;
using Bam.CommandLine;

namespace Bam.Container.Podman
{
    public class PodmanRuntime : IContainerRuntime
    {
        public string Run(ContainerConfig config)
        {
            string envArgs = string.Join(" ", config.EnvironmentVariables.Select(e => $"-e {e}"));
            string args = $"run -d --name {config.Name} -p {config.PortMapping} {envArgs} {config.Image}";
            return Execute(args).Trim();
        }

        public void Stop(string name, int timeoutSeconds = 5)
        {
            Execute($"stop -t {timeoutSeconds} {name}");
        }

        public void Remove(string name, bool force = true)
        {
            string forceFlag = force ? " -f" : "";
            Execute($"rm{forceFlag} {name}");
        }

        public bool IsRunning(string name)
        {
            try
            {
                string output = Inspect(name, "{{.State.Running}}");
                return output.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public string Inspect(string name, string format)
        {
            return Execute($"inspect --format \"{format}\" {name}").Trim();
        }

        private string Execute(string args)
        {
            System.Console.WriteLine($"[podman] podman {args}");
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

            if (!string.IsNullOrWhiteSpace(result.StandardOutput))
            {
                System.Console.WriteLine($"[podman]   stdout: {result.StandardOutput.Trim()}");
            }

            if (!string.IsNullOrWhiteSpace(result.StandardError))
            {
                System.Console.WriteLine($"[podman]   stderr: {result.StandardError.Trim()}");
            }

            if (result.ExitCode != 0 && !string.IsNullOrWhiteSpace(result.StandardError))
            {
                throw new InvalidOperationException($"podman {args} failed: {result.StandardError}");
            }

            return result.StandardOutput;
        }
    }
}
