using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Bam;
using Bam.Data;

namespace Bam.Data.Tests.Dao
{
    public class TestItemColumns : QueryFilter<TestItemColumns>, IFilterToken
    {
        public TestItemColumns() { }
        public TestItemColumns(string columnName, bool isForeignKey = false)
            : base(columnName)
        {
            _isForeignKey = isForeignKey;
        }

        public bool IsKey()
        {
            return (bool)ColumnName?.Equals(KeyColumn.ColumnName);
        }

        private bool? _isForeignKey;
        public bool IsForeignKey
        {
            get
            {
                if (_isForeignKey == null)
                {
                    PropertyInfo prop = DaoType
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

        public TestItemColumns KeyColumn => new TestItemColumns("Id");

        public TestItemColumns Id => new TestItemColumns("Id");
        public TestItemColumns Name => new TestItemColumns("Name");
        public TestItemColumns Description => new TestItemColumns("Description");
        public TestItemColumns Quantity => new TestItemColumns("Quantity");
        public TestItemColumns Price => new TestItemColumns("Price");
        public TestItemColumns IsActive => new TestItemColumns("IsActive");
        public TestItemColumns Created => new TestItemColumns("Created");

        public Type DaoType => typeof(TestItem);

        public string Operator { get; set; }

        public override string ToString()
        {
            return base.ColumnName;
        }
    }
}
