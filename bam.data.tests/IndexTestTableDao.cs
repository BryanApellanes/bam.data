using Bam.Data;

namespace Bam.Tests
{
    /// <summary>
    /// A hand-written Dao mirroring the shape codegen produces, declaring a class-level
    /// composite index plus property-level descending and unique indexes, used to exercise
    /// general index DDL without a database.
    /// </summary>
    [Table("IndexTestTable", "IndexTest")]
    [Index("TenantId", "CreatedAt", ColumnOrders = new SortOrder[] { SortOrder.Unspecified, SortOrder.Descending })]
    public class IndexTestTableDao : Dao
    {
        public IndexTestTableDao() : base()
        {
            this.SetKeyColumnName();
        }

        public IndexTestTableDao(IDatabase db) : base(db)
        {
            this.SetKeyColumnName();
        }

        [KeyColumn(Name = "Id", DbDataType = "BigInt", MaxLength = "19")]
        public ulong? Id
        {
            get => GetULongValue("Id");
            set => SetValue("Id", value!);
        }

        [Column(Name = "TenantId", DbDataType = "BigInt", MaxLength = "19", AllowNull = true)]
        public ulong? TenantId
        {
            get => GetULongValue("TenantId");
            set => SetValue("TenantId", value!);
        }

        [Column(Name = "CreatedAt", DbDataType = "DateTime", AllowNull = true)]
        [Index(Order = SortOrder.Descending)]
        public DateTime CreatedAt
        {
            get => GetDateTimeValue("CreatedAt");
            set => SetValue("CreatedAt", value!);
        }

        [Column(Name = "Email", DbDataType = "VarChar", MaxLength = "256", AllowNull = true)]
        [Index(Unique = true)]
        public string? Email
        {
            get => GetStringValue("Email");
            set => SetValue("Email", value!);
        }

        public override IQueryFilter GetUniqueFilter()
        {
            QueryFilter filter = new QueryFilter("Email") == (Email ?? string.Empty);
            return filter;
        }
    }
}
