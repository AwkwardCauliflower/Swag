using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto.ImageTree
{
    public class DirectoryScanInfo
    {
        public DirectoryInfo Directory { get; internal set; }
        public Exception Exception { get; internal set; }
    }
}
