using Bam.Data;

namespace Bam.Tests
{
    /// <summary>
    /// A Dao whose derived index identifier exceeds Oracle's thirty-character limit, used to
    /// exercise identifier truncation.
    /// </summary>
    [Table("ALongTableNameForIdentifierLimits", "IndexTestOracle")]
    public class LongNameIndexTestTableDao : Dao
    {
        public LongNameIndexTestTableDao() : base()
        {
            this.SetKeyColumnName();
        }

        [KeyColumn(Name = "Id", DbDataType = "BigInt", MaxLength = "19")]
        public ulong? Id
        {
            get => GetULongValue("Id");
            set => SetValue("Id", value!);
        }

        [Column(Name = "DescriptionText", DbDataType = "VarChar", MaxLength = "128", AllowNull = true)]
        [Index]
        public string? DescriptionText
        {
            get => GetStringValue("DescriptionText");
            set => SetValue("DescriptionText", value!);
        }

        public override IQueryFilter GetUniqueFilter()
        {
            QueryFilter filter = new QueryFilter("Id") == Id;
            return filter;
        }
    }

    /// <summary>
    /// A Dao declaring one uniformly descending index, used to exercise Firebird's
    /// index-level direction syntax.
    /// </summary>
    [Table("DescendingIndexTestTable", "IndexTestFirebird")]
    public class DescendingIndexTestTableDao : Dao
    {
        public DescendingIndexTestTableDao() : base()
        {
            this.SetKeyColumnName();
        }

        [KeyColumn(Name = "Id", DbDataType = "BigInt", MaxLength = "19")]
        public ulong? Id
        {
            get => GetULongValue("Id");
            set => SetValue("Id", value!);
        }

        [Column(Name = "CreatedAt", DbDataType = "DateTime", AllowNull = true)]
        [Index(Order = SortOrder.Descending)]
        public DateTime CreatedAt
        {
            get => GetDateTimeValue("CreatedAt");
            set => SetValue("CreatedAt", value!);
        }

        public override IQueryFilter GetUniqueFilter()
        {
            QueryFilter filter = new QueryFilter("Id") == Id;
            return filter;
        }
    }

    /// <summary>
    /// An invalid Dao declaring an index on a property with no column attribute, used to
    /// exercise declaration validation.
    /// </summary>
    [Table("NoColumnIndexTestTable", "IndexTestInvalidProperty")]
    public class NoColumnIndexTestTableDao : Dao
    {
        public NoColumnIndexTestTableDao() : base()
        {
            this.SetKeyColumnName();
        }

        [KeyColumn(Name = "Id", DbDataType = "BigInt", MaxLength = "19")]
        public ulong? Id
        {
            get => GetULongValue("Id");
            set => SetValue("Id", value!);
        }

        [Index]
        public string? NotAColumn { get; set; }

        public override IQueryFilter GetUniqueFilter()
        {
            QueryFilter filter = new QueryFilter("Id") == Id;
            return filter;
        }
    }

    /// <summary>
    /// An invalid Dao whose class-level index names a column no property declares, used to
    /// exercise declaration validation.
    /// </summary>
    [Table("UnknownColumnIndexTestTable", "IndexTestInvalidClass")]
    [Index("Nope")]
    public class UnknownColumnIndexTestTableDao : Dao
    {
        public UnknownColumnIndexTestTableDao() : base()
        {
            this.SetKeyColumnName();
        }

        [KeyColumn(Name = "Id", DbDataType = "BigInt", MaxLength = "19")]
        public ulong? Id
        {
            get => GetULongValue("Id");
            set => SetValue("Id", value!);
        }

        public override IQueryFilter GetUniqueFilter()
        {
            QueryFilter filter = new QueryFilter("Id") == Id;
            return filter;
        }
    }
}
