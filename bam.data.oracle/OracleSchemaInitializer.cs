/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public class OracleSchemaInitializer: SchemaInitializer
    {
        public OracleSchemaInitializer() : base() { }

        public OracleSchemaInitializer(string schemaContextAssemblyQaulifiedName)
            : base(schemaContextAssemblyQaulifiedName, typeof(OracleRegistrarCaller).AssemblyQualifiedName!)
        { }

        public OracleSchemaInitializer(Type schemaContextType)
            : base(schemaContextType, typeof(OracleRegistrarCaller))
        { }
    }
}
