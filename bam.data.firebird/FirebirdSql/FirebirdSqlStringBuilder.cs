﻿/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Bam.Data.Schema;
using Bam.Incubation;
using Bam.Data;
using Bam.Data.FirebirdSql;

namespace Bam.Data
{
    public class FirebirdSqlSqlStringBuilder : SchemaWriter
    {
        public FirebirdSqlSqlStringBuilder()
            : base()
        {
            GoText = ";\r\n";
            CreateTableFormat = "CREATE TABLE {0} ({1})";
            AddForeignKeyColumnFormat = "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY (\"{2}\") REFERENCES {3} (\"{4}\")";
            TableNameFormatter = (s) => "\"{0}\"".Format(s);
            ColumnNameFormatter = (s) => "\"{0}\"".Format(s);
        }
        public override SqlStringBuilder Id(string idAs)
        {
            Builder.AppendFormat(" RETURNING \"Id\" AS \"{0}\"{1}", idAs, this.GoText);
            return this;
        }
        public override void Reset()
        {
            base.Reset();
            this.GoText = ";\r\n";
        }
        public static void Register(DependencyProvider incubator)
        {
            FirebirdSqlSqlStringBuilder builder = new FirebirdSqlSqlStringBuilder();
            incubator.Set(typeof(SqlStringBuilder), builder);
            incubator.Set<SqlStringBuilder>(builder);
        }
        public override string GetKeyColumnDefinition(KeyColumnAttribute keyColumn)
        {
            KeyColumnAttribute key = keyColumn.CopyAs<KeyColumnAttribute>();
            key.DbDataType = "SERIAL";
            return string.Format("{0} PRIMARY KEY ", GetColumnDefinition(key));
        }
        public override string GetColumnDefinition(ColumnAttribute column)
        {
            string max = string.Format("({0})", column.MaxLength);
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
                max = string.Format("({0}, 2)", column.MaxLength);
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

            return string.Format("{0} {1}{2}{3}", ColumnNameFormatter(column.Name), type, max, column.AllowNull ? "" : " NOT NULL");
        }
        public override SqlStringBuilder Where(IQueryFilter filter)
        {
            WhereFormat where = FirebirdSqlFormatProvider.GetWhereFormat(filter, StringBuilder, NextNumber);
            NextNumber = where.NextNumber;
            this.parameters.AddRange(where.Parameters);
            return this;
        }
        public override SqlStringBuilder Update(string tableName, params AssignValue[] values)
        {
            Builder.AppendFormat("UPDATE {0} ", TableNameFormatter(tableName));
            SetFormat set = FirebirdSqlFormatProvider.GetSetFormat(tableName, StringBuilder, NextNumber, values);
            NextNumber = set.NextNumber;
            this.parameters.AddRange(set.Parameters);
            return this;
        }

        public override ISqlStringBuilder Select(string tableName, params string[] columnNames)
        {
            return base.Select(tableName, columnNames);
        }

        protected override void WriteCreateTable(Type daoType)
        {
            ColumnAttribute[] columns = GetColumns(daoType);

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
            TableAttribute table = null;
            if (daoType.HasCustomAttributeOfType<TableAttribute>(out table))
            {
                PropertyInfo[] properties = daoType.GetProperties();
                foreach (PropertyInfo prop in properties)
                {
                    ForeignKeyAttribute fk = null;
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
