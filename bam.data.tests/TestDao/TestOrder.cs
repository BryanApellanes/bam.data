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
	[Bam.Data.Table("TestOrder", "BamDataTest")]
	public partial class TestOrder : Bam.Data.Dao
	{
		public TestOrder() : base()
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestOrder(DataRow data)
			: base(data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestOrder(IDatabase db)
			: base(db)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestOrder(IDatabase db, DataRow data)
			: base(db, data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		[Bam.Exclude]
		public static implicit operator TestOrder(DataRow data)
		{
			return new TestOrder(data);
		}

		private void SetChildren()
		{
			if (_database != null)
			{
				this.ChildCollections.Add("TestOrderLine_TestOrderId",
					new TestOrderLineCollection(
						Database.GetQuery<TestOrderLineColumns, TestOrderLine>(
							(c) => c.TestOrderId == GetULongValue("Id", false)),
						this, "TestOrderId"));
			}
			if (_database != null)
			{
				this.ChildCollections.Add("TestOrderTag_TestOrderId",
					new TestOrderTagCollection(
						Database.GetQuery<TestOrderTagColumns, TestOrderTag>(
							(c) => c.TestOrderId == GetULongValue("Id", false)),
						this, "TestOrderId"));
			}
			this.ChildCollections.Add("TestOrder_TestOrderTag_TestTag",
				new XrefDaoCollection<TestOrderTag, TestTag>(this, false));
		}

		[Bam.Exclude]
		[Bam.Data.KeyColumn(Name = "Id", DbDataType = "BigInt", MaxLength = "19")]
		public ulong? Id
		{
			get => GetULongValue("Id");
			set => SetValue("Id", value!);
		}

		[Bam.Data.Column(Name = "CustomerName", DbDataType = "VarChar", MaxLength = "255", AllowNull = true)]
		public string? CustomerName
		{
			get => GetStringValue("CustomerName");
			set => SetValue("CustomerName", value!);
		}

		[Bam.Data.Column(Name = "OrderDate", DbDataType = "DateTime", MaxLength = "8", AllowNull = true)]
		public DateTime? OrderDate
		{
			get => GetDateTimeValue("OrderDate");
			set => SetValue("OrderDate", value!);
		}

		[Bam.Exclude]
		public TestOrderLineCollection TestOrderLinesByTestOrderId
		{
			get
			{
				if (this.IsNew)
				{
					throw new InvalidOperationException("Cannot load child collection for a new (unsaved) TestOrder.");
				}
				if (!this.ChildCollections.ContainsKey("TestOrderLine_TestOrderId"))
				{
					SetChildren();
				}
				var c = (TestOrderLineCollection)this.ChildCollections["TestOrderLine_TestOrderId"];
				if (!c.Loaded)
				{
					c.Load(Database);
				}
				return c;
			}
		}

		[Bam.Exclude]
		public XrefDaoCollection<TestOrderTag, TestTag> TestTags
		{
			get
			{
				if (this.IsNew)
				{
					throw new InvalidOperationException("Cannot load xref collection for a new (unsaved) TestOrder.");
				}
				if (!this.ChildCollections.ContainsKey("TestOrder_TestOrderTag_TestTag"))
				{
					SetChildren();
				}
				var xref = (XrefDaoCollection<TestOrderTag, TestTag>)
					this.ChildCollections["TestOrder_TestOrderTag_TestTag"];
				if (!xref.Loaded)
				{
					xref.Load(Database);
				}
				return xref;
			}
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
				var colFilter = new TestOrderColumns();
				return (colFilter.KeyColumn == GetDbId());
			}
		}

		public static TestOrderCollection LoadAll(IDatabase? database = null)
		{
			IDatabase db = database ?? Db.For<TestOrder>();
			ISqlStringBuilder sql = db.GetSqlStringBuilder();
			sql.Select<TestOrder>();
			var results = new TestOrderCollection(db, sql.ExecuteGetDataTable(db))
			{
				Database = db
			};
			return results;
		}

		[Bam.Exclude]
		public static TestOrderCollection Where(Func<TestOrderColumns, QueryFilter<TestOrderColumns>> where, OrderBy<TestOrderColumns>? orderBy = null, IDatabase? database = null)
		{
			database = database ?? Db.For<TestOrder>();
			return new TestOrderCollection(database.GetQuery<TestOrderColumns, TestOrder>(where, orderBy!), true);
		}

		[Bam.Exclude]
		public static TestOrderCollection Where(WhereDelegate<TestOrderColumns> where, IDatabase? database = null)
		{
			database = database ?? Db.For<TestOrder>();
			var results = new TestOrderCollection(database, database.GetQuery<TestOrderColumns, TestOrder>(where), true);
			return results;
		}

		[Bam.Exclude]
		public static TestOrder? OneWhere(WhereDelegate<TestOrderColumns> where, IDatabase? database = null)
		{
			var result = Top(1, where, database);
			return result.Count == 1 ? result[0] : null;
		}

		[Bam.Exclude]
		public static TestOrder? GetById(ulong id, IDatabase? database = null)
		{
			return OneWhere(c => c.KeyColumn == id, database);
		}

		[Bam.Exclude]
		public static TestOrderCollection Top(int count, WhereDelegate<TestOrderColumns> where, IDatabase? database = null)
		{
			return Top(count, where, null, database);
		}

		[Bam.Exclude]
		public static TestOrderCollection Top(int count, WhereDelegate<TestOrderColumns> where, OrderBy<TestOrderColumns>? orderBy, IDatabase? database = null)
		{
			TestOrderColumns c = new TestOrderColumns();
			IQueryFilter filter = where(c);

			IDatabase db = database ?? Db.For<TestOrder>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestOrder>(count);
			query.Where(filter);

			if (orderBy != null)
			{
				query.OrderBy<TestOrderColumns>(orderBy);
			}

			query.Execute(db);
			var results = query.Results.As<TestOrderCollection>(0);
			results.Database = db;
			return results;
		}

		public static long Count(IDatabase? database = null)
		{
			IDatabase db = database ?? Db.For<TestOrder>();
			IQuerySet query = GetQuerySet(db);
			query.Count<TestOrder>();
			query.Execute(db);
			return Convert.ToInt64(query.Results[0].DataRow[0]);
		}
	}
}
