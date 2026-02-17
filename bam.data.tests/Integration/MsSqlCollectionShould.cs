using Microsoft.Data.SqlClient;
using Bam.Data.MsSql;
using Bam.Data.Tests.Dao;
using Bam.Data.Tests.Helpers;
using Bam.Test;
using Bam.Test.Integration;

namespace Bam.Data.Tests.Integration;

[IntegrationTestMenu("MsSql Collection should", "msc")]
public class MsSqlCollectionShould : IntegrationTestMenuContainer
{
    private const string ContainerName = "bam-data-test-mssql";
    private const string SaPassword = "BamTest1!";
    private const string MasterConnectionString =
        "Data Source=tcp:127.0.0.1,1433;Initial Catalog=master;User ID=sa;Password=" + SaPassword + ";TrustServerCertificate=true;";
    private const string TestDbConnectionString =
        "Data Source=tcp:127.0.0.1,1433;Initial Catalog=BamDataTest;User ID=sa;Password=" + SaPassword + ";TrustServerCertificate=true;";

    private static MsSqlDatabase SetupDb()
    {
        SqlConnection.ClearAllPools();

        PodmanContainerHelper.WaitForReady(ContainerName, 60, () =>
        {
            using var conn = new SqlConnection(MasterConnectionString);
            conn.Open();
            return true;
        });

        using (var conn = new SqlConnection(MasterConnectionString))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BamDataTest') CREATE DATABASE BamDataTest";
            cmd.ExecuteNonQuery();
        }

        MsSqlDatabase db = new MsSqlDatabase(TestDbConnectionString, "BamDataTest");
        db.TryEnsureSchema<TestOrder>();
        db.ExecuteSql("DELETE FROM TestOrderTag");
        db.ExecuteSql("DELETE FROM TestOrderLine");
        db.ExecuteSql("DELETE FROM TestTag");
        db.ExecuteSql("DELETE FROM TestOrder");
        return db;
    }

    [IntegrationTest]
    public void DaoCollectionSaveAndReload()
    {
        MsSqlDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("saves order with 2 lines and reloads", (database) =>
        {
            TestOrder order = new TestOrder(database)
            {
                CustomerName = "MsSql-Alice",
                OrderDate = DateTime.Now
            };
            order.Save(database);

            new TestOrderLine(database) { TestOrderId = order.Id, ProductName = "Widget", Quantity = 2, UnitPrice = 9.99m }.Save(database);
            new TestOrderLine(database) { TestOrderId = order.Id, ProductName = "Gadget", Quantity = 1, UnitPrice = 19.99m }.Save(database);

            return TestOrder.Where(c => c.CustomerName == "MsSql-Alice", database);
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestOrderCollection>("has 2 order lines", col => col?[0]?.TestOrderLinesByTestOrderId.Count == 2);
        })
        .SoBeHappy(_ => { })
        .UnlessItFailed();
    }

    [IntegrationTest]
    public void XrefCollectionSaveAndReload()
    {
        MsSqlDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("saves order with 2 tags via xref and reloads", (database) =>
        {
            TestOrder order = new TestOrder(database) { CustomerName = "MsSql-Bob" };
            order.Save(database);

            TestTag tag1 = new TestTag(database) { TagName = "Urgent" };
            tag1.Save(database);
            TestTag tag2 = new TestTag(database) { TagName = "VIP" };
            tag2.Save(database);

            var xref = order.TestTags;
            xref.Add(tag1);
            xref.Add(tag2);
            xref.Commit(database);

            return TestOrder.Where(c => c.CustomerName == "MsSql-Bob", database);
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestOrderCollection>("has 2 tags", col => col?[0]?.TestTags.Count == 2)
                .As<TestOrderCollection>("contains Urgent", col => col?[0]?.TestTags.Any(t => t.TagName == "Urgent") == true)
                .As<TestOrderCollection>("contains VIP", col => col?[0]?.TestTags.Any(t => t.TagName == "VIP") == true);
        })
        .SoBeHappy(_ => { })
        .UnlessItFailed();
    }

    [IntegrationTest]
    public void XrefRemoveAndClear()
    {
        MsSqlDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("adds 3 tags, removes 1, clears rest", (database) =>
        {
            TestOrder order = new TestOrder(database) { CustomerName = "MsSql-Charlie" };
            order.Save(database);

            TestTag tag1 = new TestTag(database) { TagName = "Red" };
            tag1.Save(database);
            TestTag tag2 = new TestTag(database) { TagName = "Green" };
            tag2.Save(database);
            TestTag tag3 = new TestTag(database) { TagName = "Blue" };
            tag3.Save(database);

            var xref = order.TestTags;
            xref.Add(tag1);
            xref.Add(tag2);
            xref.Add(tag3);
            xref.Commit(database);

            // Remove one
            xref.Remove(tag1);
            var afterRemoveCol = TestOrder.Where(c => c.CustomerName == "MsSql-Charlie", database);
            int countAfterRemove = afterRemoveCol[0].TestTags.Count;

            // Clear remaining
            afterRemoveCol[0].TestTags.Clear(database);
            var afterClearCol = TestOrder.Where(c => c.CustomerName == "MsSql-Charlie", database);
            int countAfterClear = afterClearCol[0].TestTags.Count;

            long tagCount = TestTag.Count(database);

            return new object[] { countAfterRemove, countAfterClear, tagCount };
        })
        .TheTest
        .ShouldPass(because =>
        {
            var results = (object[])because.Result!;
            because.ItsTrue("2 tags after remove", (int)results[0] == 2);
            because.ItsTrue("0 tags after clear", (int)results[1] == 0);
            because.ItsTrue("all 3 tags still in DB", (long)results[2] == 3);
        })
        .SoBeHappy(_ => { })
        .UnlessItFailed();
    }
}
