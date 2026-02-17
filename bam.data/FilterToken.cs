/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Abstract base class for tokens used in SQL filter expressions, such as comparisons and parentheses.
    /// </summary>
    public abstract class FilterToken : IFilterToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterToken"/> class.
        /// </summary>
        public FilterToken() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterToken"/> class with the specified operator.
        /// </summary>
        /// <param name="oper">The SQL operator string.</param>
        public FilterToken(string oper)
        {
            this.Operator = oper;
        }

        /// <summary>
        /// Gets or sets the SQL operator string for this filter token.
        /// </summary>
        public string Operator { get; set; } = null!;
        
        public override string ToString()
        {
            return this.Operator;
        }
    }
}
