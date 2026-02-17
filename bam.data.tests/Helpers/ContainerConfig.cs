namespace Bam.Container
{
    public class ContainerConfig
    {
        public string Name { get; set; } = null!;
        public string Image { get; set; } = null!;
        public string PortMapping { get; set; } = null!;
        public string[] EnvironmentVariables { get; set; } = Array.Empty<string>();
    }
}
