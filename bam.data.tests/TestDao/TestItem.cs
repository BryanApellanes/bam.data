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
	[Bam.Data.Table("TestItem", "BamDataTest")]
	public partial class TestItem : Bam.Data.Dao
	{
		public TestItem() : base()
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestItem(DataRow data)
			: base(data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestItem(IDatabase db)
			: base(db)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestItem(IDatabase db, DataRow data)
			: base(db, data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		[Bam.Exclude]
		public static implicit operator TestItem(DataRow data)
		{
			return new TestItem(data);
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

		[Bam.Data.Column(Name = "Name", DbDataType = "VarChar", MaxLength = "255", AllowNull = true)]
		public string? Name
		{
			get => GetStringValue("Name");
			set => SetValue("Name", value!);
		}

		[Bam.Data.Column(Name = "Description", DbDataType = "VarChar", MaxLength = "4000", AllowNull = true)]
		public string? Description
		{
			get => GetStringValue("Description");
			set => SetValue("Description", value!);
		}

		[Bam.Data.Column(Name = "Quantity", DbDataType = "Int", MaxLength = "10", AllowNull = true)]
		public int? Quantity
		{
			get => GetIntValue("Quantity");
			set => SetValue("Quantity", value!);
		}

		[Bam.Data.Column(Name = "Price", DbDataType = "Decimal", MaxLength = "18", AllowNull = true)]
		public decimal? Price
		{
			get => GetDecimalValue("Price");
			set => SetValue("Price", value!);
		}

		[Bam.Data.Column(Name = "IsActive", DbDataType = "Bit", MaxLength = "1", AllowNull = true)]
		public bool? IsActive
		{
			get => GetBooleanValue("IsActive");
			set => SetValue("IsActive", value!);
		}

		[Bam.Data.Column(Name = "Created", DbDataType = "DateTime", MaxLength = "8", AllowNull = true)]
		public DateTime? Created
		{
			get => GetDateTimeValue("Created");
			set => SetValue("Created", value!);
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
				var colFilter = new TestItemColumns();
				return (colFilter.KeyColumn == GetDbId());
			}
		}

		public static TestItemCollection LoadAll(IDatabase? database = null)
		{
			IDatabase db = database ?? Db.For<TestItem>();
			ISqlStringBuilder sql = db.GetSqlStringBuilder();
			sql.Select<TestItem>();
			var results = new TestItemCollection(db, sql.ExecuteGetDataTable(db))
			{
				Database = db
			};
			return results;
		}

		[Bam.Exclude]
		public static TestItemCollection Where(Func<TestItemColumns, QueryFilter<TestItemColumns>> where, OrderBy<TestItemColumns>? orderBy = null, IDatabase? database = null)
		{
			database = database ?? Db.For<TestItem>();
			return new TestItemCollection(database.GetQuery<TestItemColumns, TestItem>(where, orderBy!), true);
		}

		[Bam.Exclude]
		public static TestItemCollection Where(WhereDelegate<TestItemColumns> where, IDatabase? database = null)
		{
			database = database ?? Db.For<TestItem>();
			var results = new TestItemCollection(database, database.GetQuery<TestItemColumns, TestItem>(where), true);
			return results;
		}

		[Bam.Exclude]
		public static TestItem? OneWhere(WhereDelegate<TestItemColumns> where, IDatabase? database = null)
		{
			var result = Top(1, where, database);
			return result.Count > 0 ? result[0] : null;
		}

		[Bam.Exclude]
		public static TestItem? GetById(ulong id, IDatabase? database = null)
		{
			return OneWhere(c => c.KeyColumn == id, database);
		}

		[Bam.Exclude]
		public static TestItemCollection Top(int count, WhereDelegate<TestItemColumns> where, IDatabase? database = null)
		{
			return Top(count, where, null, database);
		}

		[Bam.Exclude]
		public static TestItemCollection Top(int count, WhereDelegate<TestItemColumns> where, OrderBy<TestItemColumns>? orderBy, IDatabase? database = null)
		{
			TestItemColumns c = new TestItemColumns();
			IQueryFilter filter = where(c);

			IDatabase db = database ?? Db.For<TestItem>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestItem>(count);
			query.Where(filter);

			if (orderBy != null)
			{
				query.OrderBy<TestItemColumns>(orderBy);
			}

			query.Execute(db);
			var results = query.Results.As<TestItemCollection>(0);
			results.Database = db;
			return results;
		}

		public static long Count(IDatabase? database = null)
		{
			IDatabase db = database ?? Db.For<TestItem>();
			IQuerySet query = GetQuerySet(db);
			query.Count<TestItem>();
			query.Execute(db);
			return (long)query.Results[0].DataRow[0];
		}
	}
}
