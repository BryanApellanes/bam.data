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
	[Bam.Data.Table("TestOrderTag", "BamDataTest")]
	public partial class TestOrderTag : Bam.Data.Dao
	{
		public TestOrderTag() : base()
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestOrderTag(DataRow data)
			: base(data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestOrderTag(IDatabase db)
			: base(db)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestOrderTag(IDatabase db, DataRow data)
			: base(db, data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		[Bam.Exclude]
		public static implicit operator TestOrderTag(DataRow data)
		{
			return new TestOrderTag(data);
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
			Table = "TestOrderTag", Name = "TestOrderId", DbDataType = "BigInt",
			MaxLength = "19", AllowNull = false, ReferencedKey = "Id",
			ReferencedTable = "TestOrder", Suffix = "1")]
		public ulong? TestOrderId
		{
			get => GetULongValue("TestOrderId", false);
			set => SetValue("TestOrderId", value!, false);
		}

		[Bam.Data.ForeignKey(
			Table = "TestOrderTag", Name = "TestTagId", DbDataType = "BigInt",
			MaxLength = "19", AllowNull = false, ReferencedKey = "Id",
			ReferencedTable = "TestTag", Suffix = "2")]
		public ulong? TestTagId
		{
			get => GetULongValue("TestTagId", false);
			set => SetValue("TestTagId", value!, false);
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
				var colFilter = new TestOrderTagColumns();
				return (colFilter.KeyColumn == GetDbId());
			}
		}

		public static TestOrderTagCollection LoadAll(IDatabase? database = null)
		{
			IDatabase db = database ?? Db.For<TestOrderTag>();
			ISqlStringBuilder sql = db.GetSqlStringBuilder();
			sql.Select<TestOrderTag>();
			var results = new TestOrderTagCollection(db, sql.ExecuteGetDataTable(db))
			{
				Database = db
			};
			return results;
		}

		[Bam.Exclude]
		public static TestOrderTagCollection Where(Func<TestOrderTagColumns, QueryFilter<TestOrderTagColumns>> where, OrderBy<TestOrderTagColumns>? orderBy = null, IDatabase? database = null)
		{
			database = database ?? Db.For<TestOrderTag>();
			return new TestOrderTagCollection(database.GetQuery<TestOrderTagColumns, TestOrderTag>(where, orderBy!), true);
		}

		[Bam.Exclude]
		public static TestOrderTagCollection Where(WhereDelegate<TestOrderTagColumns> where, IDatabase? database = null)
		{
			database = database ?? Db.For<TestOrderTag>();
			var results = new TestOrderTagCollection(database, database.GetQuery<TestOrderTagColumns, TestOrderTag>(where), true);
			return results;
		}

		[Bam.Exclude]
		public static TestOrderTag? OneWhere(WhereDelegate<TestOrderTagColumns> where, IDatabase? database = null)
		{
			var result = Top(1, where, database);
			return result.Count == 1 ? result[0] : null;
		}

		[Bam.Exclude]
		public static TestOrderTag? GetById(ulong id, IDatabase? database = null)
		{
			return OneWhere(c => c.KeyColumn == id, database);
		}

		[Bam.Exclude]
		public static TestOrderTagCollection Top(int count, WhereDelegate<TestOrderTagColumns> where, IDatabase? database = null)
		{
			return Top(count, where, null, database);
		}

		[Bam.Exclude]
		public static TestOrderTagCollection Top(int count, WhereDelegate<TestOrderTagColumns> where, OrderBy<TestOrderTagColumns>? orderBy, IDatabase? database = null)
		{
			TestOrderTagColumns c = new TestOrderTagColumns();
			IQueryFilter filter = where(c);

			IDatabase db = database ?? Db.For<TestOrderTag>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestOrderTag>(count);
			query.Where(filter);

			if (orderBy != null)
			{
				query.OrderBy<TestOrderTagColumns>(orderBy);
			}

			query.Execute(db);
			var results = query.Results.As<TestOrderTagCollection>(0);
			results.Database = db;
			return results;
		}

		public static long Count(IDatabase? database = null)
		{
			IDatabase db = database ?? Db.For<TestOrderTag>();
			IQuerySet query = GetQuerySet(db);
			query.Count<TestOrderTag>();
			query.Execute(db);
			return (long)query.Results[0].DataRow[0];
		}
	}
}
