using Microsoft.Data.SqlClient;
using Bam.Console;
using Bam.Test;
using FirebirdSql.Data.FirebirdClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace Bam.Data.Tests.Integration;

public class IntegrationTestCleanup
{
    private const string Password = "BamTest1!";

    private const string PostgresConnectionString =
        "Host=localhost;Database=bamtest;Username=postgres;Password=" + Password;

    private const string MsSqlConnectionString =
        "Data Source=tcp:127.0.0.1,1433;Initial Catalog=BamDataTest;User ID=sa;Password=" + Password + ";TrustServerCertificate=true;";

    private const string MySqlConnectionString =
        "Server=localhost;Database=bamtest;User=root;Password=" + Password;

    private const string OracleConnectionString =
        "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XE)));User Id=system;Password=" + Password + ";";

    private const string FirebirdConnectionString =
        "DataSource=localhost;Database=/firebird/data/bamtest.fdb;User=SYSDBA;Password=" + Password + ";Port=3050";

    [AfterIntegrationTests]
    public static void DropTestTables()
    {
        Message.PrintLine("[AfterIntegrationTests] DropTestTables");

        TryDrop("Postgres", () =>
        {
            using var conn = new NpgsqlConnection(PostgresConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DROP TABLE IF EXISTS TestItem";
            cmd.ExecuteNonQuery();
        });

        TryDrop("MSSQL", () =>
        {
            using var conn = new SqlConnection(MsSqlConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "IF OBJECT_ID('dbo.TestItem','U') IS NOT NULL DROP TABLE dbo.TestItem";
            cmd.ExecuteNonQuery();
        });

        TryDrop("MySQL", () =>
        {
            using var conn = new MySqlConnection(MySqlConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DROP TABLE IF EXISTS TestItem";
            cmd.ExecuteNonQuery();
        });

        TryDrop("Oracle", () =>
        {
            using var conn = new OracleConnection(OracleConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "BEGIN EXECUTE IMMEDIATE 'DROP TABLE \"TESTITEM\"'; EXCEPTION WHEN OTHERS THEN NULL; END;";
            cmd.ExecuteNonQuery();
        });

        TryDrop("Firebird", () =>
        {
            FbConnection.ClearAllPools();
            using var conn = new FbConnection(FirebirdConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "EXECUTE BLOCK AS BEGIN IF (EXISTS(SELECT 1 FROM RDB$RELATIONS WHERE RDB$RELATION_NAME = 'TestItem')) THEN EXECUTE STATEMENT 'DROP TABLE \"TestItem\"'; END";
            cmd.ExecuteNonQuery();
        });
    }

    private static void TryDrop(string dbName, Action drop)
    {
        try
        {
            drop();
            Message.PrintLine($"  [{dbName}] TestItem table dropped");
        }
        catch (Exception ex)
        {
            Message.PrintLine($"  [{dbName}] drop failed: {ex.Message}");
        }
    }
}
