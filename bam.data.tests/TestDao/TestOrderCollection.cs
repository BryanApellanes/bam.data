using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Data;

namespace Bam.Data.Tests.Dao
{
    public class TestOrderCollection : DaoCollection<TestOrderColumns, TestOrder>
    {
        public TestOrderCollection() { }
        public TestOrderCollection(IDatabase db, DataTable table, IDao? dao = null, string? rc = null) : base(db, table, dao!, rc!) { }
        public TestOrderCollection(DataTable table, IDao? dao = null, string? rc = null) : base(table, dao!, rc!) { }
        public TestOrderCollection(IQuery<TestOrderColumns, TestOrder> q, Bam.Data.Dao? dao = null, string? rc = null) : base(q, dao!, rc!) { }
        public TestOrderCollection(IDatabase db, IQuery<TestOrderColumns, TestOrder> q, bool load) : base(db, q, load) { }
        public TestOrderCollection(IQuery<TestOrderColumns, TestOrder> q, bool load) : base(q, load) { }
    }
}
