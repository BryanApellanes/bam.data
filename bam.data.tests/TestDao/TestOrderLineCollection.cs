using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Data;

namespace Bam.Data.Tests.Dao
{
    public class TestOrderLineCollection : DaoCollection<TestOrderLineColumns, TestOrderLine>
    {
        public TestOrderLineCollection() { }
        public TestOrderLineCollection(IDatabase db, DataTable table, IDao? dao = null, string? rc = null) : base(db, table, dao!, rc!) { }
        public TestOrderLineCollection(DataTable table, IDao? dao = null, string? rc = null) : base(table, dao!, rc!) { }
        public TestOrderLineCollection(IQuery<TestOrderLineColumns, TestOrderLine> q, Bam.Data.Dao? dao = null, string? rc = null) : base(q, dao!, rc!) { }
        public TestOrderLineCollection(IDatabase db, IQuery<TestOrderLineColumns, TestOrderLine> q, bool load) : base(db, q, load) { }
        public TestOrderLineCollection(IQuery<TestOrderLineColumns, TestOrderLine> q, bool load) : base(q, load) { }
    }
}
