using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swag
{
    /// <summary>
    /// Represents the arguments to this exe.
    /// </summary>
	class Args
	{
        /// <summary>
        /// The root website path. 
        /// </summary>
		public string Path { get; set; }

        /// <summary>
        /// The folder in which to generate the web site files.
        /// </summary>
		public string WebFolderName { get; set; }

        /// <summary>
        /// Any path containing any of these strings will be ignored.
        /// </summary>
		public List<string> Blacklist { get; set; }
	}
}
