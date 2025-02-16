/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public class SelectColumn<T>: SqlStringBuilder where T: Dao
    {
        public SelectColumn(string column)
        {
            this.Select(Dao.TableName(typeof(T)), column);            
        }
    }
}
