using Bam.Data.SQLite;
using Bam.Data.Tests.Dao;
using Bam.Test;

namespace Bam.Data.Tests.Unit;

[UnitTestMenu("XrefDaoCollection should", "xr")]
public class XrefDaoCollectionShould : UnitTestMenuContainer
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
    public void AddTargetsAndCommitCreatesXrefEntries()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("adds 2 tags to order via xref and reloads", (database) =>
        {
            TestOrder order = new TestOrder(database)
            {
                CustomerName = "XrefAlice",
                OrderDate = DateTime.Now
            };
            order.Save(database);

            TestTag tag1 = new TestTag(database) { TagName = "Urgent" };
            tag1.Save(database);
            TestTag tag2 = new TestTag(database) { TagName = "VIP" };
            tag2.Save(database);

            var xref = order.TestTags;
            xref.Add(tag1);
            xref.Add(tag2);
            xref.Commit(database);

            var reloaded = TestOrder.Where(c => c.CustomerName == "XrefAlice", database);
            return reloaded;
        })
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull()
                .As<TestOrderCollection>("found 1 order", col => col?.Count == 1)
                .As<TestOrderCollection>("has 2 tags", col => col?[0]?.TestTags.Count == 2)
                .As<TestOrderCollection>("contains Urgent tag", col =>
                    col?[0]?.TestTags.Any(t => t.TagName == "Urgent") == true)
                .As<TestOrderCollection>("contains VIP tag", col =>
                    col?[0]?.TestTags.Any(t => t.TagName == "VIP") == true);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [UnitTest]
    public void RemoveTargetDeletesXrefButNotTarget()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("removes one tag from xref, verifies tag still exists", (database) =>
        {
            TestOrder order = new TestOrder(database)
            {
                CustomerName = "XrefBob"
            };
            order.Save(database);

            TestTag tag1 = new TestTag(database) { TagName = "Priority" };
            tag1.Save(database);
            TestTag tag2 = new TestTag(database) { TagName = "Sale" };
            tag2.Save(database);

            var xref = order.TestTags;
            xref.Add(tag1);
            xref.Add(tag2);
            xref.Commit(database);

            // Remove one tag from xref
            xref.Remove(tag1);

            // Reload order and check the removed tag still exists in DB
            var reloaded = TestOrder.Where(c => c.CustomerName == "XrefBob", database);
            int xrefCount = reloaded[0].TestTags.Count;
            var tagStillExists = TestTag.Where(c => c.TagName == "Priority", database);

            return new object[] { xrefCount, tagStillExists.Count };
        })
        .TheTest
        .ShouldPass(because =>
        {
            var results = (object[])because.Result!;
            because.ItsTrue("xref has 1 tag remaining", (int)results[0] == 1);
            because.ItsTrue("removed tag still exists in DB", (int)results[1] == 1);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [UnitTest]
    public void ClearDeletesAllXrefEntries()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("adds 3 tags, clears xref, verifies tags still exist", (database) =>
        {
            TestOrder order = new TestOrder(database)
            {
                CustomerName = "XrefCharlie"
            };
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

            // Clear all xref entries
            xref.Clear(database);

            var reloaded = TestOrder.Where(c => c.CustomerName == "XrefCharlie", database);
            int xrefCount = reloaded[0].TestTags.Count;
            int tagCount = TestTag.LoadAll(database).Count;

            return new object[] { xrefCount, tagCount };
        })
        .TheTest
        .ShouldPass(because =>
        {
            var results = (object[])because.Result!;
            because.ItsTrue("xref has 0 tags after clear", (int)results[0] == 0);
            because.ItsTrue("all 3 tags still exist in DB", (int)results[1] == 3);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }

    [UnitTest]
    public void AddNewUnsavedTargetAutoSaves()
    {
        SQLiteDatabase db = SetupDb();

        After.Setup(reg =>
        {
            reg.Set<IDatabase>(db);
        })
        .When<IDatabase>("adds unsaved tag to xref and commits", (database) =>
        {
            TestOrder order = new TestOrder(database)
            {
                CustomerName = "XrefDiana"
            };
            order.Save(database);

            TestTag unsavedTag = new TestTag(database) { TagName = "AutoSaved" };
            // Do not save the tag explicitly

            var xref = order.TestTags;
            xref.Add(unsavedTag);
            xref.Commit(database);

            var reloaded = TestOrder.Where(c => c.CustomerName == "XrefDiana", database);
            int xrefCount = reloaded[0].TestTags.Count;
            bool tagHasId = unsavedTag.Id != null && unsavedTag.Id > 0;
            bool tagNameMatches = reloaded[0].TestTags.Any(t => t.TagName == "AutoSaved");

            return new object[] { xrefCount, tagHasId, tagNameMatches };
        })
        .TheTest
        .ShouldPass(because =>
        {
            var results = (object[])because.Result!;
            because.ItsTrue("tag got an Id assigned", (bool)results[1]);
            because.ItsTrue("xref has 1 tag", (int)results[0] == 1);
            because.ItsTrue("tag name matches", (bool)results[2]);
        })
        .SoBeHappy(cleanup => CleanupDb(db))
        .UnlessItFailed();
    }
}
