/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bam.Data;

namespace Bam.Data
{
    public abstract class FilterToken : IFilterToken
    {
        public FilterToken() { }
        public FilterToken(string oper)
        {
            this.Operator = oper;
        }

        public string Operator { get; set; }
        
        public override string ToString()
        {
            return this.Operator;
        }
    }
}
