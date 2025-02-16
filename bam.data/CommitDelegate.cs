/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public delegate SqlStringBuilder CommitDelegate(string tableName, params AssignValue[] values);
}
