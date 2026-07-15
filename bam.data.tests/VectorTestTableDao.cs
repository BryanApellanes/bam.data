using Bam.Data;

namespace Bam.Tests
{
    /// <summary>
    /// A hand-written Dao mirroring the shape codegen produces for a vector column, used to
    /// exercise vector DDL and hydration without a database.
    /// </summary>
    [Table("VectorTestTable", "VectorTest")]
    public class VectorTestTableDao : Dao
    {
        public VectorTestTableDao() : base()
        {
            this.SetKeyColumnName();
        }

        public VectorTestTableDao(IDatabase db) : base(db)
        {
            this.SetKeyColumnName();
        }

        [KeyColumn(Name = "Id", DbDataType = "BigInt", MaxLength = "19")]
        public ulong? Id
        {
            get => GetULongValue("Id");
            set => SetValue("Id", value!);
        }

        [Column(Name = "Label", DbDataType = "VarChar", MaxLength = "64", AllowNull = true)]
        public string? Label
        {
            get => GetStringValue("Label");
            set => SetValue("Label", value!);
        }

        [VectorColumn(3, Name = "Embedding")]
        [VectorIndex(Lists = 50)]
        public Vector? Embedding
        {
            get => GetVectorValue("Embedding");
            set => SetValue("Embedding", value!);
        }

        public override IQueryFilter GetUniqueFilter()
        {
            QueryFilter filter = new QueryFilter("Embedding") == (Embedding?.ToString() ?? string.Empty);
            return filter;
        }
    }
}
