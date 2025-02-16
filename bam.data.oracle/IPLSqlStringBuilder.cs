/*
	Copyright © Bryan Apellanes 2015  
*/

using Oracle.ManagedDataAccess.Client;

namespace Bam.Data.Oracle
{
	public interface IPLSqlStringBuilder
	{
		OracleParameter IdParameter { get; set; }
		bool ReturnsId { get; set; }
	}
}
