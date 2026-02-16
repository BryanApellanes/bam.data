namespace Bam.Container
{
    public class ContainerConfig
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string PortMapping { get; set; }
        public string[] EnvironmentVariables { get; set; } = Array.Empty<string>();
    }
}
