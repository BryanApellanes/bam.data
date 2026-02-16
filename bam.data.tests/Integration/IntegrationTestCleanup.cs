using System.Data.SqlClient;
using Bam.Console;
using Bam.Test;
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

    [AfterIntegrationTests]
    public static void DropTestTables()
    {
        Message.PrintLine("[AfterIntegrationTests] DropTestTables");

        TryDrop("Postgres", () =>
        {
            using var conn = new NpgsqlConnection(PostgresConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DROP TABLE IF EXISTS \"TestItem\"";
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
            cmd.CommandText = "BEGIN EXECUTE IMMEDIATE 'DROP TABLE \"TestItem\"'; EXCEPTION WHEN OTHERS THEN NULL; END;";
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
