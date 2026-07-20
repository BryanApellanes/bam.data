/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Reflection;
using Bam.Data.Npgsql;
using Bam.DependencyInjection;
using Bam.Services;

namespace Bam.Data
{
    public class NpgsqlSqlStringBuilder : SchemaWriter
    {
        private bool _vectorExtensionWritten;

        public NpgsqlSqlStringBuilder()
            : base()
        {
            GoText = ";\r\n";
            CreateTableFormat = "CREATE TABLE {0} ({1})";
            AddForeignKeyColumnFormat = "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY (\"{2}\") REFERENCES {3} (\"{4}\")";
            TableNameFormatter = (s) => "{0}".Format(s);
            ColumnNameFormatter = NpgsqlFormatProvider.ColumnNameFormatter;
        }

        protected override RowCapSyntax RowCap => RowCapSyntax.Limit;

        public override SqlStringBuilder Id(string idAs)
        {
            Builder.AppendFormat(" RETURNING {0} AS {1}{2}", ColumnNameFormatter("Id"), idAs, this.GoText);
            return this;
        }

        public override void Reset()
        {
            base.Reset();
            this.GoText = ";\r\n";
            this._vectorExtensionWritten = false;
        }

        /// <summary>
        /// Gets the pgvector distance operator for the specified distance semantics.
        /// </summary>
        /// <param name="distance">The distance semantics.</param>
        public static string GetVectorDistanceOperator(VectorDistance distance)
        {
            switch (distance)
            {
                case VectorDistance.Euclidean:
                    return "<->";
                case VectorDistance.InnerProduct:
                    return "<#>";
                case VectorDistance.Cosine:
                default:
                    return "<=>";
            }
        }

        /// <summary>
        /// Orders results by pgvector distance from the specified value, nearest first, binding the
        /// query vector as a parameter cast server-side via <c>::vector</c>, e.g.
        /// <c>ORDER BY "Embedding" &lt;=&gt; :Embedding1::vector</c>. Pair with a not-null filter on the
        /// column: approximate vector indexes skip null vectors.
        /// </summary>
        /// <param name="columnName">The vector column to measure distance against.</param>
        /// <param name="value">The query vector.</param>
        /// <param name="distance">The distance semantics to order by.</param>
        public override ISqlStringBuilder OrderByNearest(string columnName, Vector value, VectorDistance distance)
        {
            VectorDistanceOrdering ordering = new VectorDistanceOrdering(columnName, GetVectorDistanceOperator(distance), value, distance)
            {
                ColumnNameFormatter = this.ColumnNameFormatter,
                ParameterPrefix = ":"
            };
            NextNumber = ordering.SetNumber(NextNumber);
            this.parameters.Add(ordering);
            Builder.AppendFormat("ORDER BY {0}", ordering.ToString());
            return this;
        }

        /// <summary>
        /// Writes one CREATE INDEX statement with PostgreSQL syntax — <c>IF NOT EXISTS</c>, an
        /// optional <c>USING</c> access method, per-column operator class, and an optional
        /// <c>WITH (...)</c> storage-parameter clause. A <see cref="VectorIndexAttribute"/>
        /// declaration renders e.g.
        /// <c>CREATE INDEX IF NOT EXISTS ix_Table_Column ON Table USING ivfflat ("Column" vector_cosine_ops) WITH (lists = 100)</c>.
        /// </summary>
        /// <param name="index">The index to write.</param>
        protected override void WriteCreateIndex(IndexDefinition index)
        {
            string usingClause = string.IsNullOrEmpty(index.AccessMethod) ? string.Empty : $" USING {index.AccessMethod}";
            string operatorClassSuffix = string.IsNullOrEmpty(index.OperatorClass) ? string.Empty : $" {index.OperatorClass}";
            string columnList = string.Join(", ", index.Columns.Select(column => $"{ColumnNameFormatter(column.ColumnName)}{operatorClassSuffix}{GetSortOrderSuffix(column.Order)}"));
            string withClause = string.IsNullOrEmpty(index.StorageParameters) ? string.Empty : $" WITH ({index.StorageParameters})";
            Builder.AppendFormat("CREATE {0}INDEX IF NOT EXISTS {1} ON {2}{3} ({4}){5}",
                index.Unique ? "UNIQUE " : string.Empty,
                index.Name,
                TableNameFormatter(index.TableName),
                usingClause,
                columnList,
                withClause);
        }
        
        public static void Register(DependencyProvider incubator)
        {
            NpgsqlSqlStringBuilder builder = new NpgsqlSqlStringBuilder();
            incubator.Set(typeof(SqlStringBuilder), builder);
            incubator.Set<SqlStringBuilder>(builder);
        }
        
        public override string GetKeyColumnDefinition(KeyColumnAttribute keyColumn)
        {
            KeyColumnAttribute key = keyColumn.CopyAs<KeyColumnAttribute>();
            key.DbDataType = "SERIAL";
            return $"{GetColumnDefinition(key)} PRIMARY KEY ";
        }

        public override string GetColumnDefinition(ColumnAttribute column)
        {
            string max = $"({column.MaxLength})";
            string type = column.DbDataType.ToLowerInvariant();

            if (type.Equals("bigint") ||
                type.Equals("int"))
            {
                type = "INT";
                max = "";
            }
            else if (type.Equals("bit"))
            {
                type = "boolean";
                max = "";
            }
            else if (type.Equals("decimal"))
            {
                max = $"({column.MaxLength}, 2)";
            }
            else if (type.Equals("datetime"))
            {
                type = "timestamp";
                max = "";
            }
            else if (type.Equals("varbinary"))
            {
                type = "bytea";
                max = "";
            }
            else if (type.Equals("serial"))
            {
                max = "";
            }
            else if (type.Equals("jsonb") || type.Equals("uuid[]"))
            {
                // parameterless types: an empty MaxLength would otherwise render "jsonb()"
                max = "";
            }

            return $"{ColumnNameFormatter(column.Name)} {type}{max}{GetColumnDefaultClause(column)}{(column.AllowNull ? "" : " NOT NULL")}";
        }

        public override ISqlStringBuilder Where(string columnName, object value)
        {
            AssignValue assignValue = new AssignValue(columnName, value, ColumnNameFormatter) {ParameterPrefix = ":"};
            return Where(assignValue);
        }
        
        public override SqlStringBuilder Where(IQueryFilter filter)
        {
            WhereFormat where = NpgsqlFormatProvider.GetWhereFormat(filter, StringBuilder, NextNumber);
            NextNumber = where.NextNumber;
            this.parameters.AddRange(where.Parameters);
            return this;
        }
        
        public override SqlStringBuilder And(IQueryFilter filter)
        {
            AndFormat where = NpgsqlFormatProvider.GetAndFormat(filter, StringBuilder, NextNumber);
            NextNumber = where.NextNumber;
            this.parameters.AddRange(where.Parameters);
            return this;
        }

        public override ISqlStringBuilder And(string columnName, object value)
        {
            AssignValue assignValue = new AssignValue(columnName, value, ColumnNameFormatter){ParameterPrefix = ":"};
            return And(assignValue);
        }

        public override SqlStringBuilder Update(string tableName, params AssignValue[] values)
        {
            Builder.AppendFormat("UPDATE {0} ", TableNameFormatter(tableName));
            SetFormat set = NpgsqlFormatProvider.GetSetFormat(tableName, StringBuilder, NextNumber, values);
            NextNumber = set.NextNumber;
            this.parameters.AddRange(set.Parameters);
            return this;
        }

        public override ISqlStringBuilder Select(string tableName, params string[] columnNames)
        {
            return base.Select(tableName, columnNames);
        }

        /// <summary>
        /// Projects vector columns as <c>::text</c> so results are readable without the pgvector
        /// Npgsql plugin; <see cref="Dao"/> hydration parses the literal back into a Vector.
        /// </summary>
        /// <param name="column">The column to project.</param>
        protected override string GetSelectColumnExpression(ColumnAttribute column)
        {
            if ("vector".Equals(column.DbDataType, StringComparison.OrdinalIgnoreCase))
            {
                string formattedName = ColumnNameFormatter(column.Name);
                return $"{formattedName}::text AS {formattedName}";
            }
            return base.GetSelectColumnExpression(column);
        }
        
        protected override void WriteCreateTable(Type daoType)
        {
            ColumnAttribute[] columns = GetColumns(daoType);

            if (!_vectorExtensionWritten && columns.Any(c => "vector".Equals(c.DbDataType, StringComparison.OrdinalIgnoreCase)))
            {
                Builder.Append("CREATE EXTENSION IF NOT EXISTS vector");
                Go();
                _vectorExtensionWritten = true;
            }

            Builder.AppendFormat(CreateTableFormat,
                TableNameFormatter(Dao.TableName(daoType)),
                columns.ToDelimited(c =>
                {
                    if (c is KeyColumnAttribute)
                    {
                        return GetKeyColumnDefinition((KeyColumnAttribute)c);
                    }
                    else
                    {
                        return GetColumnDefinition(c);
                    }
                }));
        }

        protected override void WriteDropForeignKeys(Type daoType)
        {
            TableAttribute? table = null;
            if (daoType.HasCustomAttributeOfType<TableAttribute>(out table))
            {
                PropertyInfo[] properties = daoType.GetProperties();
                foreach (PropertyInfo prop in properties)
                {
                    ForeignKeyAttribute? fk = null;
                    if (prop.HasCustomAttributeOfType<ForeignKeyAttribute>(out fk))
                    {
                        Builder.AppendFormat("ALTER TABLE {0} DROP CONSTRAINT {1}", TableNameFormatter(table.TableName), fk.ForeignKeyName);
                        Go();
                    }
                }
            }
        }

        public override SchemaWriter WriteDropTable(string tableName)
        {
            Builder.AppendFormat("DROP TABLE IF EXISTS {0}", TableNameFormatter(tableName));
            Go();
            return this;
        }
    }
}
