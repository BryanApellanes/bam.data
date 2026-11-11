using Bam.Data;
using Bam.Data.FirebirdSql;
using Bam.Data.MsSql;
using Bam.Data.MySql;
using Bam.Data.Npgsql;
using Bam.Data.Oracle;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

/// <summary>
/// Covers a database that has a connection string resolver but no <see cref="Database.ConnectionName"/>
/// (bam.data#37). The getters pass an empty name to the resolver in place of null; every provider resolver
/// must still build its connection string, and <see cref="NullConnectionStringResolver"/> must still throw.
/// </summary>
[UnitTestMenu("A database without a connection name should")]
public class NamelessConnectionShould : UnitTestMenuContainer
{
    public NamelessConnectionShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void ResolveItsConnectionStringOnPostgres()
    {
        When.A<NpgsqlDatabase>("resolves a connection string with no connection name",
            () => new NpgsqlDatabase
            {
                ConnectionStringResolver = new NpgsqlConnectionStringResolver("pghost", "pgdb"),
                ConnectionName = null
            },
            database => database.ConnectionString ?? string.Empty)
        .TheTest
        .ShouldPass<string>((because, _, connectionString) =>
        {
            because.ItsTrue("the provider resolver built the connection string", connectionString.Contains("pghost"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ResolveItsConnectionStringOnMsSql()
    {
        When.A<MsSqlDatabase>("resolves a connection string with no connection name",
            () => new MsSqlDatabase
            {
                ConnectionStringResolver = new MsSqlConnectionStringResolver("mssqlhost", "mssqldb"),
                ConnectionName = null
            },
            database => database.ConnectionString ?? string.Empty)
        .TheTest
        .ShouldPass<string>((because, _, connectionString) =>
        {
            because.ItsTrue("the provider resolver built the connection string", connectionString.Contains("mssqlhost"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ResolveItsConnectionStringOnMySql()
    {
        When.A<MySqlDatabase>("resolves a connection string with no connection name",
            () => new MySqlDatabase
            {
                ConnectionStringResolver = new MySqlConnectionStringResolver("mysqlhost", "mysqldb", new MySqlCredentials { UserId = "user", Password = "placeholder" }),
                ConnectionName = null!
            },
            database => database.ConnectionString ?? string.Empty)
        .TheTest
        .ShouldPass<string>((because, _, connectionString) =>
        {
            because.ItsTrue("the provider resolver built the connection string", connectionString.Contains("mysqlhost"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ResolveItsConnectionStringOnFirebird()
    {
        When.A<FirebirdSqlDatabase>("resolves a connection string with no connection name",
            () => new FirebirdSqlDatabase
            {
                ConnectionStringResolver = new FirebirdSqlConnectionStringResolver("fbhost", "fbdb", new FirebirdSqlCredentials { UserId = "user", Password = "placeholder" }),
                ConnectionName = null!
            },
            database => database.ConnectionString ?? string.Empty)
        .TheTest
        .ShouldPass<string>((because, _, connectionString) =>
        {
            because.ItsTrue("the provider resolver built the connection string", connectionString.Contains("fbhost"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ResolveItsConnectionStringOnOracle()
    {
        When.A<OracleDatabase>("resolves a connection string with no connection name",
            () => new OracleDatabase
            {
                ConnectionStringResolver = new OracleConnectionStringResolver("orahost", new OracleCredentials { UserId = "user", Password = "placeholder" }),
                ConnectionName = null
            },
            database => database.ConnectionString ?? string.Empty)
        .TheTest
        .ShouldPass<string>((because, _, connectionString) =>
        {
            because.ItsTrue("the provider resolver built the connection string", connectionString.Contains("orahost"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void StillThrowTheNoResolverDiagnostic()
    {
        When.A<NpgsqlDatabase>("asks NullConnectionStringResolver for a connection string with no connection name",
            () => new NpgsqlDatabase
            {
                ConnectionStringResolver = new NullConnectionStringResolver(),
                ConnectionName = null
            },
            database =>
            {
                try
                {
                    string? ignored = database.ConnectionString;
                    return string.Empty;
                }
                catch (InvalidOperationException ex)
                {
                    return ex.Message;
                }
            })
        .TheTest
        .ShouldPass<string>((because, _, message) =>
        {
            because.ItsTrue("the diagnostic is thrown", message.Contains("No ConnectionStringResolver was specified"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }
}
