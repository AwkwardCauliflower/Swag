using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto.ImageTree
{
    /// <summary>
    /// An <see cref="ImageItem"/> plus a relative depth number.
    /// </summary>
	public class DescendantImageItem
	{
		public ImageItem Item { get; set; }
		public int Depth { get; set; }
	}
}
