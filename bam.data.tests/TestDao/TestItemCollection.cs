using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Data;

namespace Bam.Data.Tests.Dao
{
    public class TestItemCollection : DaoCollection<TestItemColumns, TestItem>
    {
        public TestItemCollection() { }
        public TestItemCollection(IDatabase db, DataTable table, IDao dao = null, string rc = null) : base(db, table, dao, rc) { }
        public TestItemCollection(DataTable table, IDao dao = null, string rc = null) : base(table, dao, rc) { }
        public TestItemCollection(IQuery<TestItemColumns, TestItem> q, Bam.Data.Dao dao = null, string rc = null) : base(q, dao, rc) { }
        public TestItemCollection(IDatabase db, IQuery<TestItemColumns, TestItem> q, bool load) : base(db, q, load) { }
        public TestItemCollection(IQuery<TestItemColumns, TestItem> q, bool load) : base(q, load) { }
    }
}
