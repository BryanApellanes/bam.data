namespace Bam.Data
{
    public interface IDaoRepoGenerationConfig
    {
        bool CheckForIds { get; set; }
        string FromNameSpace { get; set; }
        string SchemaName { get; set; }
        string TemplatePath { get; set; }
        string ToNameSpace { get; set; }
        string TypeAssembly { get; set; }
        bool UseInheritanceSchema { get; set; }
        string WriteSourceTo { get; set; }
    }
}