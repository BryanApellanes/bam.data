/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data.Common;

namespace Bam.Data
{
    /// <summary>
    /// Abstract base class for building database parameters from query filter information.
    /// </summary>
    public abstract class ParameterBuilder: IParameterBuilder
    {
        /// <summary>
        /// Ensures the parameter name starts with the specified prefix, adding it only if not already present.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="prefix">The prefix to ensure (e.g., "@" or ":").</param>
        /// <returns>The name with the prefix applied exactly once.</returns>
        protected static string EnsurePrefix(string name, string prefix)
        {
            return name.StartsWith(prefix) ? name : $"{prefix}{name}";
        }

        /// <summary>
        /// Builds a database parameter with the specified name and value.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>A new DbParameter instance.</returns>
        public abstract DbParameter BuildParameter(string name, object value);
        /// <summary>
        /// Builds a database parameter from the specified parameter info.
        /// </summary>
        /// <param name="c">The parameter info.</param>
        /// <returns>A new DbParameter instance.</returns>
        public abstract DbParameter BuildParameter(IParameterInfo c);

        /// <summary>
        /// Builds an array of database parameters from an IN comparison.
        /// </summary>
        /// <param name="c">The IN comparison containing multiple parameter values.</param>
        /// <returns>An array of DbParameter instances.</returns>
        public DbParameter[] BuildParameters(InComparison c)
        {
            DbParameter[] results = new DbParameter[c.Parameters.Length];

            for (int i = 0; i < c.Parameters.Length; i++)
            {
                results[i] = BuildParameter(c.Parameters[i]);
            }

            return results;
        }

        #region IParameterBuilder<T> Members

        /// <summary>
        /// Gets all database parameters from the specified filter's tokens.
        /// </summary>
        /// <param name="filter">The filter containing parameter tokens.</param>
        /// <returns>An array of DbParameter instances.</returns>
        public DbParameter[] GetParameters(IHasFilters filter)
        {
            List<DbParameter> parameters = new List<DbParameter>();
            foreach (IFilterToken token in filter.Filters)
            {
                if (token is IParameterInfo c)
                {
                    if (c is InComparison inC)
                    {
                        parameters.AddRange(BuildParameters(inC));
                    }
                    else
                    {
                        parameters.Add(this.BuildParameter(c));
                    }
                }
            }

            return parameters.ToArray();
        }

        #endregion
    }
}
