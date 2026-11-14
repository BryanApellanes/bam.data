using System.Reflection;
using Bam.Data;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

/// <summary>
/// Runtime-generated Dao assemblies are loaded from bytes and have no file (bam.data#47).
/// </summary>
[UnitTestMenu("A Dao from a file-less assembly should")]
public class FileLessDaoAssemblyShould : UnitTestMenuContainer
{
    public FileLessDaoAssemblyShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void ReturnNoDbIdForANonNumericKeyInsteadOfThrowing()
    {
        When.A<Dao>("comes from an assembly loaded from bytes and has a non-numeric key",
            () =>
            {
                Assembly bytesLoaded = Assembly.Load(File.ReadAllBytes(typeof(IndexTestTableDao).Assembly.Location));
                Type daoType = bytesLoaded.GetType(typeof(IndexTestTableDao).FullName!, true)!;
                Dao dao = (Dao)Activator.CreateInstance(daoType)!;
                dao.PrimaryKey = "not-a-number";
                return dao;
            },
            (dao) =>
            {
                bool threw = false;
                ulong? dbId = null;
                try
                {
                    dbId = dao.GetDbId();
                }
                catch (Exception)
                {
                    threw = true;
                }
                return new DbIdOutcome(dao.GetType().Assembly.TryGetFileInfo(out FileInfo? _), threw, dbId);
            })
        .TheTest
        .ShouldPass<DbIdOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("the Dao's assembly has no file", !outcome.AssemblyHasFile);
            because.ItsTrue("GetDbId does not throw", !outcome.Threw);
            because.ItsTrue("GetDbId returns null", outcome.DbId == null);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    private sealed record DbIdOutcome(bool AssemblyHasFile, bool Threw, ulong? DbId);
}
