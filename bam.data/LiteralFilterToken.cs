/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public class LiteralFilterToken : FilterToken
    {
        public LiteralFilterToken(string value)
        {
            this.Value = value;
        }

        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}
