using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Bam;
using Bam.Data;

namespace Bam.Data.Tests.Dao
{
    public class TestOrderLineColumns : QueryFilter<TestOrderLineColumns>, IFilterToken
    {
        public TestOrderLineColumns() { }
        public TestOrderLineColumns(string columnName, bool isForeignKey = false)
            : base(columnName)
        {
            _isForeignKey = isForeignKey;
        }

        public bool IsKey()
        {
            return ColumnName?.Equals(KeyColumn.ColumnName) ?? false;
        }

        private bool? _isForeignKey;
        public bool IsForeignKey
        {
            get
            {
                if (_isForeignKey == null)
                {
                    PropertyInfo? prop = DaoType
                        .GetProperties()
                        .FirstOrDefault(pi => ((MemberInfo)pi)
                            .HasCustomAttributeOfType<ForeignKeyAttribute>(out ForeignKeyAttribute foreignKeyAttribute)
                                && foreignKeyAttribute.Name.Equals(ColumnName));
                    _isForeignKey = prop != null;
                }

                return _isForeignKey.Value;
            }
            set => _isForeignKey = value;
        }

        public TestOrderLineColumns KeyColumn => new TestOrderLineColumns("Id");

        public TestOrderLineColumns Id => new TestOrderLineColumns("Id");
        public TestOrderLineColumns TestOrderId => new TestOrderLineColumns("TestOrderId", isForeignKey: true);
        public TestOrderLineColumns ProductName => new TestOrderLineColumns("ProductName");
        public TestOrderLineColumns Quantity => new TestOrderLineColumns("Quantity");
        public TestOrderLineColumns UnitPrice => new TestOrderLineColumns("UnitPrice");

        public Type DaoType => typeof(TestOrderLine);

        public string Operator { get; set; } = null!;

        public override string ToString()
        {
            return base.ColumnName;
        }
    }
}
