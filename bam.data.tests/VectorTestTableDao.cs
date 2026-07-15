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
