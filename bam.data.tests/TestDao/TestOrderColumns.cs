using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Bam;
using Bam.Data;

namespace Bam.Data.Tests.Dao
{
    public class TestOrderColumns : QueryFilter<TestOrderColumns>, IFilterToken
    {
        public TestOrderColumns() { }
        public TestOrderColumns(string columnName, bool isForeignKey = false)
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

        public TestOrderColumns KeyColumn => new TestOrderColumns("Id");

        public TestOrderColumns Id => new TestOrderColumns("Id");
        public TestOrderColumns CustomerName => new TestOrderColumns("CustomerName");
        public TestOrderColumns OrderDate => new TestOrderColumns("OrderDate");

        public Type DaoType => typeof(TestOrder);

        public string Operator { get; set; } = null!;

        public override string ToString()
        {
            return base.ColumnName;
        }
    }
}
