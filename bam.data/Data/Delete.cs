using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net.Data
{
    public static class Delete
    {
        public static ISqlStringBuilder From<T>(IDatabase db = null) where T : Dao, new()
        {
            return GetSqlStringBuilder<T>(db).Delete(Dao.TableName(typeof(T)));
        }

        public static ISqlStringBuilder From<T>(IQueryFilter filter, IDatabase db = null) where T: Dao, new()
        {
            ISqlStringBuilder sql = GetSqlStringBuilder<T>(db);
            return sql.Delete(Dao.TableName(typeof(T))).Where(filter);
        }

        private static ISqlStringBuilder GetSqlStringBuilder<T>(IDatabase db) where T : Dao, new()
        {
            db = db ?? Db.For<T>();
            SqlStringBuilder sql = db.GetService<SqlStringBuilder>();
            return sql;
        }
    }
}
