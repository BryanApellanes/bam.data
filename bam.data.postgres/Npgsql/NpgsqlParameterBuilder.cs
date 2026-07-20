/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data.Common;
using Npgsql;
using NpgsqlTypes;

namespace Bam.Data
{
    public class NpgsqlParameterBuilder: ParameterBuilder
    {
        public override DbParameter BuildParameter(string name, object value)
        {
            if (value is Vector vector)
            {
                return BuildVectorParameter(EnsurePrefix(name, ":"), vector);
            }
            if (value is Json json)
            {
                return BuildJsonParameter(EnsurePrefix(name, ":"), json);
            }
            return new NpgsqlParameter(EnsurePrefix(name, ":"), value);
        }

        public override DbParameter BuildParameter(IParameterInfo c)
        {
            string parameterName = string.Format(":{0}{1}", c.ColumnName, c.Number);
            object? value = c.Value;
            if (value is DateTime || value is DateTime?)
            {
                value = new Instant((DateTime)value).ToDateTime();
            }
            else if (value is ulong || value is uint)
            {
                value = Convert.ToDecimal(value);
            }
            else if (value is Vector vector)
            {
                return BuildVectorParameter(parameterName, vector);
            }
            else if (value is Json json)
            {
                return BuildJsonParameter(parameterName, json);
            }

            return new NpgsqlParameter(parameterName, value);
        }

        /// <summary>
        /// Binds a vector as its pgvector text literal with an unknown parameter type so the
        /// server infers vector via the type's input function. A text-typed parameter would
        /// require an explicit cast at every use site; unknown-typed literals convert on both
        /// INSERT assignment and operator expressions.
        /// </summary>
        /// <param name="parameterName">The prefixed parameter name.</param>
        /// <param name="vector">The vector value to bind.</param>
        private static NpgsqlParameter BuildVectorParameter(string parameterName, Vector vector)
        {
            return new NpgsqlParameter(parameterName, NpgsqlDbType.Unknown)
            {
                Value = vector.ToString()
            };
        }

        /// <summary>
        /// Binds a JSON value as a jsonb-typed parameter carrying the raw JSON text. Typing the
        /// parameter in the driver removes the need for explicit ::jsonb casts in SQL text on
        /// both INSERT assignment and expression use.
        /// </summary>
        /// <param name="parameterName">The prefixed parameter name.</param>
        /// <param name="json">The JSON value to bind.</param>
        private static NpgsqlParameter BuildJsonParameter(string parameterName, Json json)
        {
            return new NpgsqlParameter(parameterName, NpgsqlDbType.Jsonb)
            {
                Value = json.ToString()
            };
        }
    }
}
