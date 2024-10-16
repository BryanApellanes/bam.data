/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bam.Data;

namespace Bam.Data
{
    public static class Select<C> where C : IQueryFilter, IFilterToken, new()
    {
        public static Query<C, T> From<T>() where T: IDao, new()
        {
            return new Query<C, T>();
        }

        public static Query<C, T> From<T>(WhereDelegate<C> where) where T : IDao, new()
        {
            return new Query<C, T>(where);
        }
        
        public static Query<C, T> From<T>(Func<C, QueryFilter<C>> where) where T : IDao, new()
        {
            return new Query<C, T>(where);
        }
    }
}
