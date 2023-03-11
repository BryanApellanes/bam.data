/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Net.Logging;
using Bam.Net.Incubation;
using System.Reflection;

namespace Bam.Net.Data.Repositories // shared
{
	public partial class BackedupDatabase: Database
	{
		public IRepository Repository { get; private set; }

		protected Database Database => Backup.DatabaseToBackup;

		public DaoBackup Backup { get; set; }

		#region IDatabase Members

		public new DaoTransaction BeginTransaction()
		{
			return Database.BeginTransaction();
		}

		public new string ConnectionName
		{
			get => Database.ConnectionName;
			set => Database.ConnectionName = value;
		}

		public override string ConnectionString
		{
			get => Database.ConnectionString;
			set => Database.ConnectionString = value;
		}

		public override System.Data.Common.DbCommand CreateCommand()
		{
			return Database.CreateCommand();
		}

		public override System.Data.Common.DbConnectionStringBuilder CreateConnectionStringBuilder()
		{
			return Database.CreateConnectionStringBuilder();
		}

		public override void ExecuteSql(SqlStringBuilder builder)
		{
			Database.ExecuteSql(builder);
		}

		public override void ExecuteSql(SqlStringBuilder builder, IParameterBuilder parameterBuilder)
		{
			Database.ExecuteSql(builder, parameterBuilder);
		}

		public override void ExecuteSql(string sqlStatement, System.Data.CommandType commandType, params System.Data.Common.DbParameter[] dbParameters)
		{
			Database.ExecuteSql(sqlStatement, commandType, dbParameters);
		}

		public new void ExecuteSql<T>(SqlStringBuilder builder) where T : Dao
		{
			Database.ExecuteSql<T>(builder);
		}

		public new Dictionary<EnumType, T> FillEnumDictionary<EnumType, T>(Dictionary<EnumType, T> dictionary, string nameColumn) where T : Dao, new()
		{
			return Database.FillEnumDictionary<EnumType, T>(dictionary, nameColumn);
		}

		public new System.Data.DataSet GetDataSetFromSql(string sqlStatement, System.Data.CommandType commandType, params System.Data.Common.DbParameter[] dbParamaters)
		{
			return Database.GetDataSetFromSql(sqlStatement, commandType, dbParamaters);
		}

		public new System.Data.DataSet GetDataSetFromSql(string sqlStatement, System.Data.CommandType commandType, bool releaseConnection, params System.Data.Common.DbParameter[] dbParamaters)
		{
			return Database.GetDataSetFromSql(sqlStatement, commandType, releaseConnection, dbParamaters);
		}

		public new System.Data.DataSet GetDataSetFromSql(string sqlStatement, System.Data.CommandType commandType, bool releaseConnection, System.Data.Common.DbConnection conn, params System.Data.Common.DbParameter[] dbParamaters)
		{
			return Database.GetDataSetFromSql(sqlStatement, commandType, releaseConnection, dbParamaters);
		}

		public new System.Data.DataSet GetDataSetFromSql(string sqlStatement, System.Data.CommandType commandType, bool releaseConnection, System.Data.Common.DbConnection conn, System.Data.Common.DbTransaction tx, params System.Data.Common.DbParameter[] dbParamaters)
		{
			return Database.GetDataSetFromSql(sqlStatement, commandType, releaseConnection, dbParamaters);
		}

		public System.Data.DataTable GetDataTableFromSql(string sqlStatement, System.Data.CommandType commandType, params System.Data.Common.DbParameter[] dbParamaters)
		{
			return Database.GetDataTable(sqlStatement, commandType, dbParamaters);
		}

		public new System.Data.Common.DbConnection GetDbConnection()
		{
			return Database.GetDbConnection();
		}

		public new void TryEnsureSchema(Type type, ILogger logger = null)
		{
			Database.TryEnsureSchema(type, logger);
		}

		public new int MaxConnections
		{
			get => Database.MaxConnections;
			set => Database.MaxConnections = value;
		}

		public new string Name => Database.Name;

		public new Incubator ServiceProvider
		{
			get => Database.ServiceProvider;
			set => Database.ServiceProvider = value;
		}

		#endregion
	}
}
