using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Data;

namespace Bam.Data.Tests.Dao
{
    public class TestOrderTagCollection : DaoCollection<TestOrderTagColumns, TestOrderTag>
    {
        public TestOrderTagCollection() { }
        public TestOrderTagCollection(IDatabase db, DataTable table, IDao? dao = null, string? rc = null) : base(db, table, dao!, rc!) { }
        public TestOrderTagCollection(DataTable table, IDao? dao = null, string? rc = null) : base(table, dao!, rc!) { }
        public TestOrderTagCollection(IQuery<TestOrderTagColumns, TestOrderTag> q, Bam.Data.Dao? dao = null, string? rc = null) : base(q, dao!, rc!) { }
        public TestOrderTagCollection(IDatabase db, IQuery<TestOrderTagColumns, TestOrderTag> q, bool load) : base(db, q, load) { }
        public TestOrderTagCollection(IQuery<TestOrderTagColumns, TestOrderTag> q, bool load) : base(q, load) { }
    }
}
