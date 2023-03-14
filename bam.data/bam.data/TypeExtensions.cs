using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net.Data.Repositories
{
    public static class TypeExtensions
    {

        public static bool ExtendsType(this Type type, Type extends)
        {
            if (type == extends)
            {
                return false;
            }

            TypeInheritanceDescriptor descriptor = new TypeInheritanceDescriptor(type);
            return descriptor.Extends(extends);
        }
    }
}
