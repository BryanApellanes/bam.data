/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// A filter token that outputs a literal string value directly into a SQL filter expression without parameterization.
    /// </summary>
    public class LiteralFilterToken : FilterToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralFilterToken"/> class with the specified literal value.
        /// </summary>
        /// <param name="value">The literal string to output in the filter.</param>
        public LiteralFilterToken(string value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the literal string value.
        /// </summary>
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}
