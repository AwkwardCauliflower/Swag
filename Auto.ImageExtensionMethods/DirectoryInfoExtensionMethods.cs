using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Auto
{
    /// <summary>
    /// Extension methods for DirectoryInfo.
    /// </summary>
	public static class DirectoryInfoExtensionMethods
    {
		public static IEnumerable<FileInfo> EnumerateFilesByExtensions( this DirectoryInfo dir, IEnumerable<string> extensions )
		{
			if ( extensions == null )
			{
				throw new ArgumentNullException( "extensions" );
			}

			IEnumerable<FileInfo> files = dir.EnumerateFiles();
			return files.Where( f => extensions.Select( e => e.ToLower() ).Contains( f.Extension.ToLower() ) );
		}
	}
}
