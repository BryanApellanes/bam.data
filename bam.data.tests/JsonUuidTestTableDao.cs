using Bam.Data;

namespace Bam.Tests
{
    /// <summary>
    /// A hand-written Dao mirroring the shape codegen produces for jsonb and uuid[] columns, used
    /// to exercise JSON/uuid-array DDL and hydration without a database.
    /// </summary>
    [Table("JsonUuidTestTable", "JsonUuidTest")]
    public class JsonUuidTestTableDao : Dao
    {
        public JsonUuidTestTableDao() : base()
        {
            this.SetKeyColumnName();
        }

        public JsonUuidTestTableDao(IDatabase db) : base(db)
        {
            this.SetKeyColumnName();
        }

        [KeyColumn(Name = "Id", DbDataType = "BigInt", MaxLength = "19")]
        public ulong? Id
        {
            get => GetULongValue("Id");
            set => SetValue("Id", value!);
        }

        [JsonColumn("'{}'", Name = "Metadata")]
        public Json? Metadata
        {
            get => GetJsonValue("Metadata");
            set => SetValue("Metadata", value!);
        }

        [UuidArrayColumn(Name = "RelatedEpisodeIds")]
        public Guid[]? RelatedEpisodeIds
        {
            get => GetUuidArrayValue("RelatedEpisodeIds");
            set => SetValue("RelatedEpisodeIds", value!);
        }

        public override IQueryFilter GetUniqueFilter()
        {
            QueryFilter filter = new QueryFilter("Metadata") == (Metadata?.ToString() ?? string.Empty);
            return filter;
        }
    }
}
