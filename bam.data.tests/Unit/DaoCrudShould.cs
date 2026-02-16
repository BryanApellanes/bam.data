using Bam.Data.SQLite;
using Bam.Data.Tests.Dao;
using Bam.Test;

namespace Bam.Data.Tests.Unit;

[UnitTestMenu("Dao CRUD should", "dc")]
public class DaoCrudShould : UnitTestMenuContainer
{
    private static string GetTempDbDir()
    {
        string dir = Path.Combine(Path.GetTempPath(), "bam_data_tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static SQLiteDatabase SetupDb()
    {
        string dir = GetTempDbDir();
        SQLiteDatabase db = new SQLiteDatabase(dir, "BamDataTest");
        db.TryEnsureSchema<TestItem>();
        return db;
    }

    private static void CleanupDb(SQLiteDatabase db)
    {
        try
        {
            string dir = Path.GetDirectoryName(db.DatabaseFile?.FullName) ?? string.Empty;
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
        catch
        {
            // best effort cleanup
        }
    }

    [UnitTest]
    public void CreateAndRetrieve()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
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
            because.ItsTrue("Description matches", loaded?.Description == "A test widget");
            because.ItsTrue("Quantity matches", loaded?.Quantity == 10);
            because.ItsTrue("IsActive matches", loaded?.IsActive == true);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [UnitTest]
    public void UpdateExisting()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("updates an existing TestItem", (database) =>
        {
            TestItem item = new TestItem(database) { Name = "Original" };
            item.Save(database);

            item.Name = "Updated";
            item.Save(database);

            TestItem? reloaded = TestItem.GetById(item.Id!.Value, database);
            return reloaded;
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

    [UnitTest]
    public void DeleteExisting()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("deletes an existing TestItem", (database) =>
        {
            TestItem item = new TestItem(database) { Name = "ToDelete" };
            item.Save(database);
            ulong savedId = item.Id!.Value;

            item.Delete(database);

            TestItem? afterDelete = TestItem.GetById(savedId, database);
            return afterDelete;
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.ItsTrue("item is null after delete", because.Result == null);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [UnitTest]
    public void SearchByExactMatch()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("searches by exact Name match", (database) =>
        {
            new TestItem(database) { Name = "Alpha" }.Save(database);
            new TestItem(database) { Name = "Beta" }.Save(database);
            new TestItem(database) { Name = "Alpha" }.Save(database);

            var results = TestItem.Where(c => c.Name == "Alpha", database);
            return results;
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

    [UnitTest]
    public void SearchStartsWith()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("searches by Name StartsWith", (database) =>
        {
            new TestItem(database) { Name = "Widget-A" }.Save(database);
            new TestItem(database) { Name = "Widget-B" }.Save(database);
            new TestItem(database) { Name = "Gadget-C" }.Save(database);

            var results = TestItem.Where(c => c.Name.StartsWith("Wid"), database);
            return results;
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

    [UnitTest]
    public void SearchEndsWith()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("searches by Name EndsWith", (database) =>
        {
            new TestItem(database) { Name = "Widget" }.Save(database);
            new TestItem(database) { Name = "Gadget" }.Save(database);
            new TestItem(database) { Name = "Trinket" }.Save(database);

            var results = TestItem.Where(c => c.Name.EndsWith("get"), database);
            return results;
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestItemCollection>("has 2 matches (Widget, Gadget)", col => col?.Count == 2);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [UnitTest]
    public void SearchContains()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("searches by Description Contains", (database) =>
        {
            new TestItem(database) { Name = "A", Description = "This is a test item" }.Save(database);
            new TestItem(database) { Name = "B", Description = "Another test thing" }.Save(database);
            new TestItem(database) { Name = "C", Description = "Something else" }.Save(database);

            var results = TestItem.Where(c => c.Description.Contains("test"), database);
            return results;
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

    [UnitTest]
    public void SearchDoesntContain()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("searches by Description DoesntContain", (database) =>
        {
            new TestItem(database) { Name = "A", Description = "This is a test item" }.Save(database);
            new TestItem(database) { Name = "B", Description = "Another test thing" }.Save(database);
            new TestItem(database) { Name = "C", Description = "Something else" }.Save(database);

            var results = TestItem.Where(c => c.Description.DoesntContain("test"), database);
            return results;
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestItemCollection>("has 1 match", col => col?.Count == 1);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [UnitTest]
    public void SearchMultiCriteria()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("searches with multiple criteria", (database) =>
        {
            new TestItem(database) { Name = "Widget", IsActive = true, Quantity = 5 }.Save(database);
            new TestItem(database) { Name = "Widget", IsActive = false, Quantity = 3 }.Save(database);
            new TestItem(database) { Name = "Gadget", IsActive = true, Quantity = 7 }.Save(database);

            var results = TestItem.Where(c => c.Name == "Widget" && c.IsActive == true, database);
            return results;
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestItemCollection>("has 1 match (active Widget)", col => col?.Count == 1);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [UnitTest]
    public void SearchNoResults()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("searches for non-existent value", (database) =>
        {
            new TestItem(database) { Name = "Widget" }.Save(database);

            var results = TestItem.Where(c => c.Name == "DoesNotExist", database);
            return results;
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestItemCollection>("has 0 matches", col => col?.Count == 0);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [UnitTest]
    public void BatchCreateAndRetrieveAll()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("batch creates 10 items and retrieves all", (database) =>
        {
            for (int i = 0; i < 10; i++)
            {
                new TestItem(database) { Name = $"Item_{i}" }.Save(database);
            }

            var all = TestItem.LoadAll(database);
            return all;
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

    [UnitTest]
    public void CreateWithAllColumnTypes()
    {
        SQLiteDatabase db = SetupDb();
        DateTime testDate = new DateTime(2025, 6, 15, 10, 30, 0);

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
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
