using Bam.Data.Npgsql;
using Bam.Data.Postgres;
using Bam.Data.Tests.Dao;
using Bam.Data.Tests.Helpers;
using Bam.Test;
using Bam.Test.Integration;
using Npgsql;

namespace Bam.Data.Tests.Integration;

[IntegrationTestMenu("Postgres CRUD should", "pg")]
public class PostgresCrudShould : IntegrationTestMenuContainer
{
    private const string ContainerName = "bam-data-test-postgres";
    private const string Image = "postgres:16";
    private const string Port = "5432:5432";
    private const string Password = "BamTest1!";

    private static PostgresDatabase SetupDb()
    {
        PodmanContainerHelper.StartContainer(ContainerName, Image, Port,
            $"POSTGRES_PASSWORD={Password}", "POSTGRES_DB=bamtest");

        PostgresDatabase db = new PostgresDatabase("localhost", "bamtest",
            new NpgsqlCredentials { UserId = "postgres", Password = Password });

        PodmanContainerHelper.WaitForReady(ContainerName, 60, () =>
        {
            using var conn = new NpgsqlConnection(db.ConnectionString);
            conn.Open();
            return true;
        });

        db.TryEnsureSchema<TestItem>();
        return db;
    }

    private static void CleanupDb(PostgresDatabase db)
    {
        PodmanContainerHelper.StopAndRemoveContainer(ContainerName);
    }

    [IntegrationTest]
    public void CreateAndRetrieve()
    {
        PostgresDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("creates and retrieves a TestItem", (database, reg) =>
        {
            TestItem item = new TestItem(database)
            {
                Name = "Widget",
                Description = "A test widget",
                Quantity = 10,
                Price = 19.99m,
                IsActive = true,
                Created = DateTime.Now
            };
            item.Save(database);
            return item.Id;
        })
        .TheTest
        .ShouldPass(because =>
        {
            ulong? savedId = (ulong?)because.Result;
            because.ItsTrue("Id was assigned", savedId.HasValue && savedId.Value > 0);

            TestItem? loaded = TestItem.GetById(savedId!.Value, db);
            because.ItsTrue("loaded item is not null", loaded != null);
            because.ItsTrue("Name matches", loaded?.Name == "Widget");
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [IntegrationTest]
    public void UpdateExisting()
    {
        PostgresDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("updates an existing TestItem", (database) =>
        {
            TestItem item = new TestItem(database) { Name = "Original" };
            item.Save(database);
            item.Name = "Updated";
            item.Save(database);
            return TestItem.GetById(item.Id!.Value, database);
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestItem>("has updated Name", ti => ti?.Name == "Updated");
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [IntegrationTest]
    public void DeleteExisting()
    {
        PostgresDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("deletes an existing TestItem", (database) =>
        {
            TestItem item = new TestItem(database) { Name = "ToDelete" };
            item.Save(database);
            ulong savedId = item.Id!.Value;
            item.Delete(database);
            return TestItem.GetById(savedId, database);
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.ItsTrue("item is null after delete", because.Result == null);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [IntegrationTest]
    public void SearchByExactMatch()
    {
        PostgresDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("searches by exact Name match", (database) =>
        {
            new TestItem(database) { Name = "Alpha" }.Save(database);
            new TestItem(database) { Name = "Beta" }.Save(database);
            new TestItem(database) { Name = "Alpha" }.Save(database);
            return TestItem.Where(c => c.Name == "Alpha", database);
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestItemCollection>("has 2 matches", col => col?.Count == 2);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [IntegrationTest]
    public void SearchContains()
    {
        PostgresDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("searches by Description Contains", (database) =>
        {
            new TestItem(database) { Name = "A", Description = "This is a test item" }.Save(database);
            new TestItem(database) { Name = "B", Description = "Another test thing" }.Save(database);
            new TestItem(database) { Name = "C", Description = "Something else" }.Save(database);
            return TestItem.Where(c => c.Description.Contains("test"), database);
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestItemCollection>("has 2 matches", col => col?.Count == 2);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [IntegrationTest]
    public void BatchCreateAndRetrieveAll()
    {
        PostgresDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("batch creates 10 items and retrieves all", (database) =>
        {
            for (int i = 0; i < 10; i++)
            {
                new TestItem(database) { Name = $"Item_{i}" }.Save(database);
            }
            return TestItem.LoadAll(database);
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestItemCollection>("has 10 items", col => col?.Count == 10);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [IntegrationTest]
    public void CreateWithAllColumnTypes()
    {
        PostgresDatabase db = SetupDb();
        DateTime testDate = new DateTime(2025, 6, 15, 10, 30, 0);

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("creates item with all column types populated", (database) =>
        {
            TestItem item = new TestItem(database)
            {
                Name = "AllTypes",
                Description = "Testing all column types",
                Quantity = 42,
                Price = 99.95m,
                IsActive = true,
                Created = testDate
            };
            item.Save(database);
            return TestItem.GetById(item.Id!.Value, database);
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestItem>("Name roundtrips", ti => ti?.Name == "AllTypes")
                .As<TestItem>("Description roundtrips", ti => ti?.Description == "Testing all column types")
                .As<TestItem>("Quantity roundtrips", ti => ti?.Quantity == 42)
                .As<TestItem>("Price roundtrips", ti => ti?.Price == 99.95m)
                .As<TestItem>("IsActive roundtrips", ti => ti?.IsActive == true)
                .As<TestItem>("Created roundtrips", ti => ti?.Created != null && Math.Abs((ti.Created!.Value - testDate).TotalSeconds) < 2);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }
}
