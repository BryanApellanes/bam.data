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
	[Bam.Data.Table("TestTag", "BamDataTest")]
	public partial class TestTag : Bam.Data.Dao
	{
		public TestTag() : base()
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestTag(DataRow data)
			: base(data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestTag(IDatabase db)
			: base(db)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestTag(IDatabase db, DataRow data)
			: base(db, data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		[Bam.Exclude]
		public static implicit operator TestTag(DataRow data)
		{
			return new TestTag(data);
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

		[Bam.Data.Column(Name = "TagName", DbDataType = "VarChar", MaxLength = "255", AllowNull = true)]
		public string? TagName
		{
			get => GetStringValue("TagName");
			set => SetValue("TagName", value!);
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
				var colFilter = new TestTagColumns();
				return (colFilter.KeyColumn == GetDbId());
			}
		}

		public static TestTagCollection LoadAll(IDatabase? database = null)
		{
			IDatabase db = database ?? Db.For<TestTag>();
			ISqlStringBuilder sql = db.GetSqlStringBuilder();
			sql.Select<TestTag>();
			var results = new TestTagCollection(db, sql.ExecuteGetDataTable(db))
			{
				Database = db
			};
			return results;
		}

		[Bam.Exclude]
		public static TestTagCollection Where(Func<TestTagColumns, QueryFilter<TestTagColumns>> where, OrderBy<TestTagColumns>? orderBy = null, IDatabase? database = null)
		{
			database = database ?? Db.For<TestTag>();
			return new TestTagCollection(database.GetQuery<TestTagColumns, TestTag>(where, orderBy!), true);
		}

		[Bam.Exclude]
		public static TestTagCollection Where(WhereDelegate<TestTagColumns> where, IDatabase? database = null)
		{
			database = database ?? Db.For<TestTag>();
			var results = new TestTagCollection(database, database.GetQuery<TestTagColumns, TestTag>(where), true);
			return results;
		}

		[Bam.Exclude]
		public static TestTag? OneWhere(WhereDelegate<TestTagColumns> where, IDatabase? database = null)
		{
			var result = Top(1, where, database);
			return result.Count == 1 ? result[0] : null;
		}

		[Bam.Exclude]
		public static TestTag? GetById(ulong id, IDatabase? database = null)
		{
			return OneWhere(c => c.KeyColumn == id, database);
		}

		[Bam.Exclude]
		public static TestTagCollection Top(int count, WhereDelegate<TestTagColumns> where, IDatabase? database = null)
		{
			return Top(count, where, null, database);
		}

		[Bam.Exclude]
		public static TestTagCollection Top(int count, WhereDelegate<TestTagColumns> where, OrderBy<TestTagColumns>? orderBy, IDatabase? database = null)
		{
			TestTagColumns c = new TestTagColumns();
			IQueryFilter filter = where(c);

			IDatabase db = database ?? Db.For<TestTag>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestTag>(count);
			query.Where(filter);

			if (orderBy != null)
			{
				query.OrderBy<TestTagColumns>(orderBy);
			}

			query.Execute(db);
			var results = query.Results.As<TestTagCollection>(0);
			results.Database = db;
			return results;
		}

		public static long Count(IDatabase? database = null)
		{
			IDatabase db = database ?? Db.For<TestTag>();
			IQuerySet query = GetQuerySet(db);
			query.Count<TestTag>();
			query.Execute(db);
			return (long)query.Results[0].DataRow[0];
		}
	}
}
