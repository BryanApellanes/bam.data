/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Represents a closing parenthesis ")" token in a SQL filter expression.
    /// </summary>
    public class CloseParen: FilterToken
    {
        public CloseParen()
        { }

        public override string ToString()
        {
            return ")";
        }
    }
}
