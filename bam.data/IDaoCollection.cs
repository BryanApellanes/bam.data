/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Data;

namespace Bam.Data
{
    public interface IDaoCollection<C, T>
        where C : IQueryFilter, IFilterToken, new()
        where T : IDao, new()
    {
        T this[int index] { get; }

        bool AutoHydrateChildrenOnDelete { get; set; }
        int Count { get; }
        IDatabase Database { get; set; }
        DataRow DataRow { get; set; }
        DataTable DataTable { get; set; }
        bool Loaded { get; set; }
        int PageCount { get; }
        int PageSize { get; set; }
        IDao Parent { get; }
        IQuery<C, T> Query { get; set; }

        event ICommittableDelegate AfterCommit;

        void Add(object value);
        void Add(T instance);
        T AddChild();
        void AddRange(IEnumerable<T> values);
        To As<To>() where To : IHasDataTable, new();
        void Clear(IDatabase db = null);
        void Commit();
        void Commit(IDatabase db);
        Co Convert<Co>() where Co : IDaoCollection<C, T>, IHasDataTable, new();
        void Delete(IDatabase db = null);
        IEnumerator<T> GetEnumerator();
        List<T> GetPage(int pageNum);
        T JustOne(bool saveIfNew = false);
        T JustOne(IDatabase db, bool saveIfNew = false);
        void Load();
        void Load(IDatabase db);
        bool MoveNextPage();
        void Reload();
        void Save();
        void Save(IDatabase db);
        void SetDataTable(DataTable table);
        List<T> Sorted(Comparison<T> comparison);
        object[] ToJsonSafe();
        void WriteCommit(ISqlStringBuilder sql, IDatabase db = null);
        void WriteDelete(ISqlStringBuilder sql);
    }
}