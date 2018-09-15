using Auto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Auto.ImageTree
{
    /// <summary>
    /// Delegate type for directory scanning events.
    /// </summary>
    public delegate void DirectoryScanStart( DirectoryScanInfo info );

    /// <summary>
    /// Holds a tree of images as well as a flattened list of descendant images at every level. Each time an 
    /// image is found, all ancestor nodes are informed of this file and it's relative depth. This saves needing to
    /// traverse the tree to find the same descendants over and over again when using the data.
    /// </summary>
	public class ImageTreeDirectory
	{
        /// <summary>
        /// The parent ImageTreeDirectory. Null if this is the root.
        /// </summary>
		public ImageTreeDirectory Parent { get; set; }

        /// <summary>
        /// The directory info of this directory.
        /// </summary>
		public DirectoryInfo DirectoryInfo { get; private set; }

        /// <summary>
        /// A list of the direct descendant directories.
        /// </summary>
		public List<ImageTreeDirectory> ChildDirectories { get; } = new List<ImageTreeDirectory>();

        /// <summary>
        /// A flat list of all descendant images.
        /// </summary>
		public List<DescendantImageItem> DescendantImages { get; } = new List<DescendantImageItem>();

        /// <summary>
        /// Any path containing any of these strings will be ignored.
        /// </summary>
		public List<string> Blacklist { get; set; }

        /// <summary>
        /// An instance of the ShortcutUtility class to use when resolving shortcuts. If null, a new instance will be created.
        /// </summary>
        /// <remarks>A prime example of premature optimization. I wasn't sure if creating potentially thousands of 
        /// COM objects would slow down the program, so I decided to pass the same one around. Turns out it doesn't. </remarks>
		public ShortcutUtility ShortcutUtility { get; set; }

        /// <summary>
        /// Set to true after the Populate method completes.
        /// </summary>
		public bool IsPopulated { get; private set; }        

        /// <summary>
        /// Event called when scanning a directory for images starts.
        /// </summary>
        public event DirectoryScanStart ScanStarted;

        /// <summary>
        /// Event called when scanning a directory causes an exception.
        /// </summary>
        public event DirectoryScanStart ScanFailed;

        public ImageTreeDirectory( DirectoryInfo directory, ImageTreeDirectory parent = null )
		{
			if ( !directory.Exists )
			{
				throw new DirectoryNotFoundException( "The specified directory does not exist" );
			}

			DirectoryInfo = directory;
			Parent = parent;
		}

        /// <summary>
        /// Scans the tree to find all images, and all shortcuts to images.
        /// </summary>
        /// <param name="token"></param>
		public void Populate( CancellationToken token )
		{
            ScanStarted?.Invoke( new DirectoryScanInfo { Directory = DirectoryInfo } );

			if ( token.IsCancellationRequested ) return;

            try
            {
                EnsureShortcutUtility();

                List<ImageItem> children = new List<ImageItem>();
                ImageItem child;

                foreach ( var file in DirectoryInfo.EnumerateFilesByExtensions( Constants.IMAGE_FILE_EXTENSIONS_PLUS_LINK ) )
                {
                    if ( token.IsCancellationRequested ) return;

                    child = new ImageItem( file, ShortcutUtility );

                    if ( child.IsImage )
                    {
                        children.Add( child );
                    }
                }

                if ( token.IsCancellationRequested ) return;

                AddDescendants( children, 0 );

                EnsureBlacklist();

                foreach ( var subDir in DirectoryInfo.EnumerateDirectories() )
                {
                    if ( token.IsCancellationRequested ) return;

                    if ( !BlacklistExcludes( subDir.FullName ) && !subDir.Attributes.HasFlag( FileAttributes.System ) )
                    {
                        var subTreeDir = new ImageTreeDirectory( subDir, this )
                        {
                            Blacklist = Blacklist,
                            ShortcutUtility = ShortcutUtility
                        };

                        subTreeDir.ScanStarted += ScanStarted;
                        subTreeDir.ScanFailed += ScanFailed;

                        subTreeDir.Populate( token );

                        if ( token.IsCancellationRequested ) return;

                        ChildDirectories.Add( subTreeDir );
                    }
                }

                IsPopulated = true;
            }
            catch ( Exception ex )
            {
                ScanFailed?.Invoke( new DirectoryScanInfo { Directory = DirectoryInfo, Exception = ex } );
            }
		}

		private void EnsureShortcutUtility()
		{
			if ( ShortcutUtility == null )
			{
				ShortcutUtility = new ShortcutUtility();
			}
		}

		private void EnsureBlacklist()
		{
			if ( Blacklist == null )
			{
				Blacklist = new List<string>();
			}
		}

		private bool BlacklistExcludes( string fullName )
		{
			return Blacklist.Any( p => fullName.ToLower().Contains( p.ToLower() ) );
		}

		/// <summary>
		/// Add descendents to this instance and all ancestor instances.
		/// </summary>
		/// <param name="items"></param>
		/// <param name="depth"></param>
		private void AddDescendants( IEnumerable<ImageItem> items, int depth )
		{
			DescendantImages.AddRange( items.Select( p => new DescendantImageItem() { Item = p, Depth = depth } ) );

			if ( Parent != null )
			{
				Parent.AddDescendants( items, depth + 1 );
			}
		}
	}
}
