﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Bam.Data
{
    public partial class SqlStringBuilder // core
    {
        public IEnumerable<dynamic> ExecuteDynamicReader(IDatabase db)
        {
            // TODO: Reimplement this using RoslynCompiler and Handlebars templates
            //      see - https://github.com/BryanApellanes/Bam/blob/master/Bam/_fx/_Data/Database.cs#L34
            // and
            //      https://github.com/BryanApellanes/Bam/blob/e6f1132b6eedb4fd1372011ce945fdaf775cf588/Bam/_fx/Extensions.cs#L112

            throw new NotImplementedException($"{nameof(ExecuteDynamicReader)} not implemented on this platform");
        }
    }
}
