using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto.ImageTree
{
	public class Constants
	{
        /// <summary>
        /// A list of image file extensions to include during processing.
        /// </summary>
		public static readonly string[] IMAGE_FILE_EXTENSIONS =
		{
			".jpg",
			".jpeg",
			".jpe",
			".png",
			".gif",
			".bmp"
		};

        /// <summary>
        /// A list of image file extensions to include during processing plus the .lnk extension.
        /// </summary>
        public static readonly string[] IMAGE_FILE_EXTENSIONS_PLUS_LINK =
		{
			".jpg",
			".jpeg",
			".jpe",
			".png",
			".gif",
			".bmp",
			".lnk"
		};

        /// <summary>
        /// The .lnk extension.
        /// </summary>
		public const string LINK_FILE_EXTENSION = ".lnk";
	}
}
