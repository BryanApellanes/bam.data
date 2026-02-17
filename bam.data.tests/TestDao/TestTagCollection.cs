using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Data;

namespace Bam.Data.Tests.Dao
{
    public class TestTagCollection : DaoCollection<TestTagColumns, TestTag>
    {
        public TestTagCollection() { }
        public TestTagCollection(IDatabase db, DataTable table, IDao? dao = null, string? rc = null) : base(db, table, dao!, rc!) { }
        public TestTagCollection(DataTable table, IDao? dao = null, string? rc = null) : base(table, dao!, rc!) { }
        public TestTagCollection(IQuery<TestTagColumns, TestTag> q, Bam.Data.Dao? dao = null, string? rc = null) : base(q, dao!, rc!) { }
        public TestTagCollection(IDatabase db, IQuery<TestTagColumns, TestTag> q, bool load) : base(db, q, load) { }
        public TestTagCollection(IQuery<TestTagColumns, TestTag> q, bool load) : base(q, load) { }
    }
}
