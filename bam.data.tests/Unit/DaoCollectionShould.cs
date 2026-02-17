using Bam.Data.SQLite;
using Bam.Data.Tests.Dao;
using Bam.Test;

namespace Bam.Data.Tests.Unit;

[UnitTestMenu("DaoCollection should", "co")]
public class DaoCollectionShould : UnitTestMenuContainer
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
        db.TryEnsureSchema<TestOrder>();
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
    public void SaveParentAndLoadChildren()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("saves order with 2 order lines and reloads", (database) =>
        {
            TestOrder order = new TestOrder(database)
            {
                CustomerName = "Alice",
                OrderDate = DateTime.Now
            };
            order.Save(database);

            new TestOrderLine(database)
            {
                TestOrderId = order.Id,
                ProductName = "Widget",
                Quantity = 2,
                UnitPrice = 9.99m
            }.Save(database);

            new TestOrderLine(database)
            {
                TestOrderId = order.Id,
                ProductName = "Gadget",
                Quantity = 1,
                UnitPrice = 19.99m
            }.Save(database);

            var reloaded = TestOrder.Where(c => c.CustomerName == "Alice", database);
            return reloaded;
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestOrderCollection>("found 1 order", col => col?.Count == 1)
                .As<TestOrderCollection>("has 2 order lines", col =>
                    col?[0]?.TestOrderLinesByTestOrderId.Count == 2)
                .As<TestOrderCollection>("contains Widget line", col =>
                    col?[0]?.TestOrderLinesByTestOrderId.Any(l => l.ProductName == "Widget") == true)
                .As<TestOrderCollection>("contains Gadget line", col =>
                    col?[0]?.TestOrderLinesByTestOrderId.Any(l => l.ProductName == "Gadget") == true);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [UnitTest]
    public void ChildForeignKeySetAutomatically()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("adds child via collection and checks FK", (database) =>
        {
            TestOrder order = new TestOrder(database)
            {
                CustomerName = "Bob"
            };
            order.Save(database);

            var lines = order.TestOrderLinesByTestOrderId;
            TestOrderLine line = new TestOrderLine(database)
            {
                ProductName = "Sprocket",
                Quantity = 5,
                UnitPrice = 3.50m
            };
            lines.Add(line);
            lines.Commit(database);

            return line;
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestOrderLine>("TestOrderId is set", l => l?.TestOrderId != null && l.TestOrderId > 0);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [UnitTest]
    public void AddRangeOfChildren()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("adds 3 children via AddRange and reloads", (database) =>
        {
            TestOrder order = new TestOrder(database)
            {
                CustomerName = "Charlie"
            };
            order.Save(database);

            var lines = order.TestOrderLinesByTestOrderId;
            lines.AddRange(new[]
            {
                new TestOrderLine(database) { ProductName = "A", Quantity = 1, UnitPrice = 1.00m },
                new TestOrderLine(database) { ProductName = "B", Quantity = 2, UnitPrice = 2.00m },
                new TestOrderLine(database) { ProductName = "C", Quantity = 3, UnitPrice = 3.00m }
            });
            lines.Commit(database);

            var reloaded = TestOrder.Where(c => c.CustomerName == "Charlie", database);
            return reloaded;
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestOrderCollection>("has 3 order lines", col =>
                    col?[0]?.TestOrderLinesByTestOrderId.Count == 3);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [UnitTest]
    public void DeleteChildCollection()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("adds children, deletes collection, reloads", (database) =>
        {
            TestOrder order = new TestOrder(database)
            {
                CustomerName = "Diana"
            };
            order.Save(database);

            new TestOrderLine(database) { TestOrderId = order.Id, ProductName = "X", Quantity = 1, UnitPrice = 1.00m }.Save(database);
            new TestOrderLine(database) { TestOrderId = order.Id, ProductName = "Y", Quantity = 2, UnitPrice = 2.00m }.Save(database);

            // Reload to pick up children, then delete them
            var orders = TestOrder.Where(c => c.CustomerName == "Diana", database);
            var reloaded = orders[0];
            var childLines = reloaded.TestOrderLinesByTestOrderId;
            childLines.Delete(database);

            // Reload again to verify deletion
            var afterDelete = TestOrder.Where(c => c.CustomerName == "Diana", database);
            return afterDelete;
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestOrderCollection>("has 0 order lines after delete", col =>
                    col?[0]?.TestOrderLinesByTestOrderId.Count == 0);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }
}
