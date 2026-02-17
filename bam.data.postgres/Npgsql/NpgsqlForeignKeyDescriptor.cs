namespace Bam.Data.Npqsql
{
    public class NpgsqlForeignKeyDescriptor
    {
        public string TableName { get; set; } = null!;
        public string ColumnName { get; set; } = null!;
        public string ReferencedTable { get; set; } = null!;
        public string ReferencedColumn { get; set; } = null!;
    }
}