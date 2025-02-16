/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public class SQLiteSchemaInitializer: SchemaInitializer
    {
        public SQLiteSchemaInitializer() : base() { }

        public SQLiteSchemaInitializer(string schemaContextAssemblyQaulifiedName)
            : base(schemaContextAssemblyQaulifiedName, typeof(SQLiteRegistrarCaller).AssemblyQualifiedName)
        { }

        public SQLiteSchemaInitializer(Type schemaContextType)
            : base(schemaContextType, typeof(SQLiteRegistrarCaller))
        { }
    }
}
