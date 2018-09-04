using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto.ImageTree
{
    /// <summary>
    /// Contains the details of an image file or a shortcut to an iamage file.
    /// </summary>
	public class ImageItem
	{
		/// <summary>
		/// The image or link file.
		/// </summary>
		public FileInfo FileInfo { get; private set; }

		/// <summary>
		/// If the item exists and is a link, this is the target, else it's null.
		/// </summary>
		public FileInfo TargetFileInfo { get; private set; }

		/// <summary>
		/// The file itself or the link target if it exists and is an image.
		/// </summary>
		public FileInfo ImageFileInfo { get; set; }

		/// <summary>
		/// True if the specified file exists.
		/// </summary>
		public bool FileExists { get; set; }

		/// <summary>
		/// True if the file exists and is a link.
		/// </summary>
		public bool IsLink { get; private set; }

		/// <summary>
		/// True if the file exists, is a link, and the target file exists.
		/// </summary>
		public bool TargetExists { get; private set; }

		/// <summary>
		/// True if the file and/or link target exists and is an image.
		/// </summary>
		public bool IsImage { get; set; }

		///// <summary>
		///// The height of the image in piexels.
		///// </summary>
		//public int Height { get; private set; }

		///// <summary>
		///// The width of the image in pixels.
		///// </summary>
		//public int Width { get; private set; }

		///// <summary>
		///// The image aspect ratio: width / height.
		///// </summary>
		//public double AspectRatio { get; private set; }

		///// <summary>
		///// True if the image's height is greater than or equal to the width.
		///// </summary>
		//public bool IsTall { get; private set; }

		///// <summary>
		///// True if the image's width is greater than or equal to the height.
		///// </summary>
		//public bool IsWide { get; private set; }

		/// <summary>
		/// Constructs an instance of the class. Can be used with image or .lnk files.
		/// </summary>
		/// <param name="file">The image file or .lnk file.</param>
		/// <param name="shortcutUtility">An instance of ShortcutUtility for resolving shortcuts.</param>
		public ImageItem( FileInfo file, ShortcutUtility shortcutUtility = null )
		{
			FileInfo = file;
			FileExists = file.Exists;

			if ( FileExists )
			{
				IsLink = file.Extension.ToLower() == ".lnk";

				if ( IsLink )
				{
					if ( shortcutUtility == null )
					{
						throw new Exception( "You must specify a ShortcutUtility when passing in a link." );
					}

					TargetFileInfo = shortcutUtility.ResolveShortcut( file );
					TargetExists = TargetFileInfo.Exists;

					if ( TargetExists )
					{
						SetImageProperties( TargetFileInfo );
					}
				}
				else
				{
					SetImageProperties( FileInfo );
				}				
			}
		}

		private void SetImageProperties( FileInfo fileInfo )
		{
			IsImage = fileInfo.Exists && Constants.IMAGE_FILE_EXTENSIONS.Any( p => p == fileInfo.Extension.ToLower() );

			if ( IsImage )
			{
				ImageFileInfo = fileInfo;

				//using ( Image image = Image.FromFile( ImageFileInfo.FullName ) )
				//{
				//	Width = image.Width;
				//	Height = image.Height;
				//	AspectRatio = (double) Width / Height;
				//	IsWide = AspectRatio >= 1;
				//	IsTall = AspectRatio <= 1;
				//}
            }
		}
	}

	[Serializable]
	public class TargetNotFoundException : Exception
	{
		public TargetNotFoundException() { }
		public TargetNotFoundException( string message ) : base( message ) { }
		public TargetNotFoundException( string message, Exception inner ) : base( message, inner ) { }
		protected TargetNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context ) : base( info, context )
		{ }
	}
}
