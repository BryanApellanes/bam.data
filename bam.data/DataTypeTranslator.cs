namespace Bam.Data
{
    /// <summary>
    /// A class for converting between different type representations.
    /// </summary>
    public class DataTypeTranslator : IDataTypeTranslator
    {
        private static readonly object _dataTypeTranslatorLock = new object();
        private static IDataTypeTranslator _dafault = null!;
        /// <summary>
        /// Gets or sets the default IDataTypeTranslator instance.
        /// </summary>
        public static IDataTypeTranslator Default
        {
            get
            {
                return _dataTypeTranslatorLock.DoubleCheckLock(ref _dafault, () => new DataTypeTranslator());
            }
            set => _dafault = value;
        }
        
        /// <summary>
        /// Converts a CLR Type to its corresponding DataTypes enum value.
        /// </summary>
        /// <param name="type">The CLR type to convert.</param>
        /// <returns>The corresponding DataTypes enum value.</returns>
        public virtual DataTypes EnumFromType(Type type)
        {
            // Unwrap Nullable<T> so nullable value types (e.g. DateTime?, int?) map to the same
            // DataTypes as their underlying type. Without this they fall through to
            // DataTypes.Default and are silently dropped from object-data persistence — the cause
            // of RepoData.Created never round-tripping (BryanApellanes/bam.data#8).
            if (type != null)
            {
                Type? underlyingType = Nullable.GetUnderlyingType(type);
                if (underlyingType != null)
                {
                    type = underlyingType;
                }
            }

            if (type == typeof(object) || type == null)
            {
                return DataTypes.Default;
            }
            
            if (type == typeof(bool))
            {
                return DataTypes.Boolean;
            }

            if (type == typeof(int))
            {
                return DataTypes.Int;
            }

            if (type == typeof(uint))
            {
                return DataTypes.UInt;
            }

            if (type == typeof(ulong))
            {
                return DataTypes.ULong;
            }

            if (type == typeof(long))
            {
                return DataTypes.Long;
            }

            if (type == typeof(decimal))
            {
                return DataTypes.Decimal;
            }

            if (type == typeof(string))
            {
                return DataTypes.String;
            }

            if (type == typeof(byte[]))
            {
                return DataTypes.ByteArray;
            }

            if (type == typeof(DateTime))
            {
                return DataTypes.DateTime;
            }

            if (type == typeof(Vector))
            {
                return DataTypes.Vector;
            }

            if (type == typeof(Json))
            {
                return DataTypes.Json;
            }

            if (type == typeof(Guid[]))
            {
                return DataTypes.UuidArray;
            }

            return DataTypes.Default;
        }
        
        /// <summary>
        /// Converts a database data type string to its corresponding CLR Type.
        /// </summary>
        /// <param name="dbDataType">The database data type string.</param>
        /// <returns>The corresponding CLR Type.</returns>
        public virtual Type TypeFromDbDataType(string dbDataType)
        {
            return TypeFromDataType(TranslateDataType(dbDataType));
        }

        /// <summary>
        /// Converts a DataTypes enum value to its corresponding CLR Type.
        /// </summary>
        /// <param name="dataType">The DataTypes enum value.</param>
        /// <returns>The corresponding CLR Type.</returns>
        public virtual Type TypeFromDataType(DataTypes dataType)
        {
            switch (dataType)
            {
                case DataTypes.Default:
                    return typeof(object);
                case DataTypes.Boolean:
                    return typeof(bool);
                case DataTypes.Int:
                    return typeof(int);
                case DataTypes.UInt:
                    return typeof(uint);
                case DataTypes.Long:
                    return typeof(long);
                case DataTypes.ULong:
                    return typeof(ulong);
                case DataTypes.Decimal:
                    return typeof(decimal);
                case DataTypes.String:
                    return typeof(string);
                case DataTypes.ByteArray:
                    return typeof(byte[]);
                case DataTypes.DateTime:
                    return typeof(DateTime);
                case DataTypes.Vector:
                    return typeof(Vector);
                case DataTypes.Json:
                    return typeof(Json);
                case DataTypes.UuidArray:
                    return typeof(Guid[]);
                default:
                    return typeof(object);
            }
        }

        /// <summary>
        /// Translates a database data type string to its corresponding DataTypes enum value.
        /// </summary>
        /// <param name="dbDataType">The database data type string (e.g., "varchar", "int").</param>
        /// <returns>The corresponding DataTypes enum value.</returns>
        public virtual DataTypes TranslateDataType(string dbDataType)
        {
            string dataType = dbDataType.Trim().ToLowerInvariant();
            switch (dataType)
            {
                case "json":
                case "jsonb":
                    return DataTypes.Json;
                case "uuid[]":
                    return DataTypes.UuidArray;
                case "bigint":
                    return DataTypes.ULong;
                case "binary":
                    return DataTypes.ByteArray;
                case "bit":
                    return DataTypes.Boolean;
                case "blob":
                    return DataTypes.ByteArray;
                case "char":
                    return DataTypes.String;
                case "date":
                case "datetime":
                    return DataTypes.DateTime;
                case "decimal":
                    return DataTypes.Decimal;
                case "double":
                    return DataTypes.Decimal;
                case "enum":
                    return DataTypes.String;
                case "float":
                    return DataTypes.Decimal;
                case "int":
                    return DataTypes.Int;
                case "smallint":
                    return DataTypes.Int;
                case "text":
                    return DataTypes.String;
                case "time":
                    return DataTypes.DateTime;
                case "timestamp":
                    return DataTypes.String;
                case "tinyblob":
                    return DataTypes.ByteArray;
                case "tinyint":
                    return DataTypes.Int;
                case "tinytext":
                    return DataTypes.String;
                case "varbinary":
                    return DataTypes.ByteArray;
                case "varchar":
                    return DataTypes.String;
                case "vector":
                    return DataTypes.Vector;
                case "year":
                    return DataTypes.String;
                default:
                    return DataTypes.String;
            }
        }
    }
}
