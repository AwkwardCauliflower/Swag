using System;
using System.IO;

namespace Auto.ImageTree
{
    /// <summary>
    /// A wrapper class for the Shell.Application COM object used for parsing Windows shortcuts.
    /// </summary>
	public class ShortcutUtility
	{
		dynamic shell;

		public ShortcutUtility()
		{
			var shellAppType = Type.GetTypeFromProgID( "Shell.Application" );
			shell = Activator.CreateInstance( shellAppType );
		}

        /// <summary>
        /// Generated a Windows shortcut file.
        /// </summary>
        /// <param name="outputPath">The path of the shortcut file.</param>
        /// <param name="linkFileName">The name of the shortcut file.</param>
        /// <param name="workingDirectory">The working directory to store in the shortcut file.</param>
        /// <param name="targetFilePath">The target path of the shortcut file.</param>
		public void CreateShortcut( string outputPath, string linkFileName, string workingDirectory, string targetFilePath )
		{
			var linkFileNamePlusLnk = linkFileName + ".lnk";
			var shortcutPath = Path.Combine( outputPath, linkFileNamePlusLnk );

			//Create empty file.
			using ( StreamWriter shortcutWriter = new StreamWriter( shortcutPath, false ) )
			{
			}

			dynamic directory = shell.NameSpace( outputPath );
			dynamic item = directory.Items().Item( linkFileNamePlusLnk );
			dynamic link = item.GetLink;

			link.Path = targetFilePath;
			link.WorkingDirectory = workingDirectory;

			link.SetIconLocation( link.Path, 0 );
			link.Save();
		}

        /// <summary>
        /// Resolves a shortcut file into a FileInfo for the target file.
        /// </summary>
        /// <param name="linkFile"></param>
        /// <returns></returns>
		public FileInfo ResolveShortcut( FileInfo linkFile )
		{
			if ( !linkFile.Exists )
			{
				throw new FileNotFoundException();
			}

			dynamic folder = shell.NameSpace( linkFile.DirectoryName );
			dynamic folderItem = folder.ParseName( linkFile.Name );

			if ( !folderItem.IsLink )
			{
				throw new ItemIsNotLinkException();
			}

			dynamic link = folderItem.GetLink;

			return new FileInfo( link.Path );
		}
	}


	[Serializable]
	public class ItemIsNotLinkException : Exception
	{
		public ItemIsNotLinkException() { }
		public ItemIsNotLinkException( string message ) : base( message ) { }
		public ItemIsNotLinkException( string message, Exception inner ) : base( message, inner ) { }
		protected ItemIsNotLinkException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context ) : base( info, context )
		{ }
	}
}