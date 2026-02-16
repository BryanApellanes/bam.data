/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Represents an opening parenthesis "(" token in a SQL filter expression.
    /// </summary>
    public class OpenParen: FilterToken
    {
        public OpenParen()
        {
        }

        public override string ToString()
        {
            return "(";
        }
    }
}
