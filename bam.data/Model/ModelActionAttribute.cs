/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data.Model
{
    public class ModelActionAttribute : Attribute
    {
        public ModelActionAttribute()
        {
        }

        public ModelActionAttribute(string description)
        {
            this.Description = description;
        }

        public string Description { get; set; }
    }
}
