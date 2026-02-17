using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Bam;
using Bam.Data;
using Bam.Data.Qi;

namespace Bam.Data.Tests.Dao
{
	[Serializable]
	[Bam.Data.Table("TestOrderLine", "BamDataTest")]
	public partial class TestOrderLine : Bam.Data.Dao
	{
		public TestOrderLine() : base()
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestOrderLine(DataRow data)
			: base(data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestOrderLine(IDatabase db)
			: base(db)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestOrderLine(IDatabase db, DataRow data)
			: base(db, data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		[Bam.Exclude]
		public static implicit operator TestOrderLine(DataRow data)
		{
			return new TestOrderLine(data);
		}

		private void SetChildren()
		{
		}

		[Bam.Exclude]
		[Bam.Data.KeyColumn(Name = "Id", DbDataType = "BigInt", MaxLength = "19")]
		public ulong? Id
		{
			get => GetULongValue("Id");
			set => SetValue("Id", value!);
		}

		[Bam.Data.ForeignKey(
			Table = "TestOrderLine", Name = "TestOrderId", DbDataType = "BigInt",
			MaxLength = "19", AllowNull = false, ReferencedKey = "Id",
			ReferencedTable = "TestOrder", Suffix = "1")]
		public ulong? TestOrderId
		{
			get => GetULongValue("TestOrderId", false);
			set => SetValue("TestOrderId", value!, false);
		}

		[Bam.Data.Column(Name = "ProductName", DbDataType = "VarChar", MaxLength = "255", AllowNull = true)]
		public string? ProductName
		{
			get => GetStringValue("ProductName");
			set => SetValue("ProductName", value!);
		}

		[Bam.Data.Column(Name = "Quantity", DbDataType = "Int", MaxLength = "10", AllowNull = true)]
		public int? Quantity
		{
			get => GetIntValue("Quantity");
			set => SetValue("Quantity", value!);
		}

		[Bam.Data.Column(Name = "UnitPrice", DbDataType = "Decimal", MaxLength = "18", AllowNull = true)]
		public decimal? UnitPrice
		{
			get => GetDecimalValue("UnitPrice");
			set => SetValue("UnitPrice", value!);
		}

		[Bam.Exclude]
		public override IQueryFilter GetUniqueFilter()
		{
			if (UniqueFilterProvider != null)
			{
				return UniqueFilterProvider(this);
			}
			else
			{
				var colFilter = new TestOrderLineColumns();
				return (colFilter.KeyColumn == GetDbId());
			}
		}

		public static TestOrderLineCollection LoadAll(IDatabase? database = null)
		{
			IDatabase db = database ?? Db.For<TestOrderLine>();
			ISqlStringBuilder sql = db.GetSqlStringBuilder();
			sql.Select<TestOrderLine>();
			var results = new TestOrderLineCollection(db, sql.ExecuteGetDataTable(db))
			{
				Database = db
			};
			return results;
		}

		[Bam.Exclude]
		public static TestOrderLineCollection Where(Func<TestOrderLineColumns, QueryFilter<TestOrderLineColumns>> where, OrderBy<TestOrderLineColumns>? orderBy = null, IDatabase? database = null)
		{
			database = database ?? Db.For<TestOrderLine>();
			return new TestOrderLineCollection(database.GetQuery<TestOrderLineColumns, TestOrderLine>(where, orderBy!), true);
		}

		[Bam.Exclude]
		public static TestOrderLineCollection Where(WhereDelegate<TestOrderLineColumns> where, IDatabase? database = null)
		{
			database = database ?? Db.For<TestOrderLine>();
			var results = new TestOrderLineCollection(database, database.GetQuery<TestOrderLineColumns, TestOrderLine>(where), true);
			return results;
		}

		[Bam.Exclude]
		public static TestOrderLine? OneWhere(WhereDelegate<TestOrderLineColumns> where, IDatabase? database = null)
		{
			var result = Top(1, where, database);
			return result.Count == 1 ? result[0] : null;
		}

		[Bam.Exclude]
		public static TestOrderLine? GetById(ulong id, IDatabase? database = null)
		{
			return OneWhere(c => c.KeyColumn == id, database);
		}

		[Bam.Exclude]
		public static TestOrderLineCollection Top(int count, WhereDelegate<TestOrderLineColumns> where, IDatabase? database = null)
		{
			return Top(count, where, null, database);
		}

		[Bam.Exclude]
		public static TestOrderLineCollection Top(int count, WhereDelegate<TestOrderLineColumns> where, OrderBy<TestOrderLineColumns>? orderBy, IDatabase? database = null)
		{
			TestOrderLineColumns c = new TestOrderLineColumns();
			IQueryFilter filter = where(c);

			IDatabase db = database ?? Db.For<TestOrderLine>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestOrderLine>(count);
			query.Where(filter);

			if (orderBy != null)
			{
				query.OrderBy<TestOrderLineColumns>(orderBy);
			}

			query.Execute(db);
			var results = query.Results.As<TestOrderLineCollection>(0);
			results.Database = db;
			return results;
		}

		public static long Count(IDatabase? database = null)
		{
			IDatabase db = database ?? Db.For<TestOrderLine>();
			IQuerySet query = GetQuerySet(db);
			query.Count<TestOrderLine>();
			query.Execute(db);
			return Convert.ToInt64(query.Results[0].DataRow[0]);
		}
	}
}
