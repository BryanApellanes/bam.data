namespace Bam.Data
{
    public interface IDaoRepoGenerationConfig
    {
        bool CheckForIds { get; set; }
        string FromNamespace { get; set; }
        string SchemaName { get; set; }
        string TemplatePath { get; set; }
        string TypeAssembly { get; set; }
        bool UseInheritanceSchema { get; set; }
        string WriteSourceTo { get; set; }
        bool WarningsAsErrors { get; set; }
    }
}