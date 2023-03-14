/*
	Copyright © Bryan Apellanes 2015  
*/
using System;

namespace Bam.Net.Data.Schema
{
    public interface ISchemaDefinition
    {
        string DbType { get; set; }
        string File { get; set; }
        IForeignKeyColumn[] ForeignKeys { get; set; }
        Exception LastException { get; }
        string Name { get; set; }
        ITable[] Tables { get; set; }
        IXrefTable[] Xrefs { get; set; }

        ISchemaManagerResult AddForeignKey(IForeignKeyColumn fk);
        ISchemaManagerResult AddTable(ITable table);
        ISchemaManagerResult AddXref(IXrefTable xref);
        ISchemaDefinition CombineWith(ISchemaDefinition schemaDefinition);
        IXrefInfo[] LeftXrefsFor(string tableName);
        void RemoveTable(string tableName);
        void RemoveTable(ITable table);
        void RemoveXref(string name);
        void RemoveXref(IXrefTable xrefTable);
        IXrefInfo[] RightXrefsFor(string tableName);
        void Save();
        void Save(string filePath);
    }
}