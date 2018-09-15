using Auto.ImageTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Auto.StaticWebGalleryGenerator;
using System.Text.RegularExpressions;

namespace Swag
{
    class Program
    {
        static void Main( string[] args )
        {
            try
            {
                Args parsedArgs = ValidateAndParseArgs( args );

                if ( parsedArgs != null )
                {
                    GenerateGallery( parsedArgs.Path, parsedArgs.WebFolderName, parsedArgs.Blacklist );
                }
            }
            catch ( CommandLineArgumentException e )
            {
                WriteUsage( "ERROR: " + e.Message );
            }

            Console.WriteLine( "Press any key..." );
            Console.ReadKey();
        }

        private static Args ValidateAndParseArgs( string[] args )
        {
            if ( args.Length < 2 )
            {
                throw new CommandLineArgumentException( "Not enough args." );
            }

            DirectoryInfo dir = new DirectoryInfo( args[ 0 ] );

            if ( !dir.Exists )
            {
                throw new CommandLineArgumentException( "First arg must be a directory which exists." );
            }

            if ( !IsValidFilename( args[ 1 ].Trim() ) )
            {
                throw new CommandLineArgumentException( "Second arg must be a valid Windows folder name." );
            }

            Args parsedArgs = new Args()
            {
                Path = dir.FullName,
                WebFolderName = args[ 1 ].Trim(),
                Blacklist = args.Skip( 2 ).ToList()
            };

            return parsedArgs;
        }

        private static bool IsValidFilename( string testName )
        {
            Regex containsABadCharacter = new Regex( "[" + Regex.Escape( new string( Path.GetInvalidPathChars() ) ) + "]" );

            return !containsABadCharacter.IsMatch( testName ) && !string.IsNullOrWhiteSpace( testName );
        }

        private static void WriteUsage( string message = null )
        {
            const string usage =
               @"Swag is a Static Web Album Generator. It generates a recursive image-viewing web site for an entire tree of images using just HTML, JavaScript, and JSON. The site files will be generated in a child directory under the root of your web site.

Swag root webFolder [blacklistedName]*

  root             The root of the website. All images must be cointained
                   within.
                   
  webFolder        A name of a folder into which the site files will be
                   generated. E.g., www.
                   
  blacklistedName  Any number of strings separated by spaces. Paths
                   containing any of the strings will be ignored. The 
                   webFolder name is automatically blacklisted, as are all
                   system folders.
                   
Example usage:

If you have a website running at the root of your X: drive with two folders named 'GIFs' and 'Thumbnails' which you don't want included, and you want Swag's folder to be named 'www', you would use the following command:
    
    Swag X:\ www GIFs Thumbnails

Note: this will exclude all directories and files that contain the words 'GIFs' or 'Thumbnails'. If you'd prefer, you can specify a full path to a directory to exclude. Use double quotes if the path contains spaces.";

            if ( message != null )
            {
                Console.WriteLine( message );
                Console.WriteLine();
            }

            Console.WriteLine( usage );
        }

        private static void GenerateGallery( string path, string webFolderName, List<string> blacklist = null )
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Generator gen = new Generator( path, webFolderName, blacklist, deleteWebFolder: true );
            gen.ScanStarted += Gen_ScanStarted;
            gen.ScanFailed += Gen_ScanFailed;
            gen.GenerationStarted += Gen_GenerationStarted;
            gen.GenerationFailed += Gen_GenerationFailed;

            gen.PopulateImageTree( cancellationTokenSource.Token );
            gen.GenerateWebsite( cancellationTokenSource.Token );

            sw.Stop();

            var seconds = sw.Elapsed.TotalSeconds;

            Console.WriteLine( "Elapsed seconds: " + Math.Round( seconds, 1 ) );
        }

        private static void Gen_ScanStarted( DirectoryScanInfo info )
        {
            WriteOperationStart( "Scan", info.Directory );
        }

        private static void Gen_ScanFailed( DirectoryScanInfo info )
        {
            WriteOperationException( "Scan", info.Directory, info.Exception );
        }

        private static void Gen_GenerationStarted( DirectoryGenerateInfo info )
        {
            WriteOperationStart( "Generate", info.Directory );
        }

        private static void Gen_GenerationFailed( DirectoryGenerateInfo info )
        {
            WriteOperationException( "Generate", info.Directory, info.Exception );
        }

        private static void WriteOperationStart( string operationType, DirectoryInfo directory )
        {
            Console.WriteLine( "{0} {1}", operationType, directory.FullName );
        }

        private static void WriteOperationException( string operationType, DirectoryInfo directory, Exception ex )
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine( "{0} {1}", operationType, directory.FullName );
            Console.WriteLine( ex.ToString() );
            Console.ResetColor();
        }
    }

    [Serializable]
    public class CommandLineArgumentException : Exception
    {
        public CommandLineArgumentException() { }
        public CommandLineArgumentException( string message ) : base( message ) { }
        public CommandLineArgumentException( string message, Exception inner ) : base( message, inner ) { }
        protected CommandLineArgumentException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base( info, context )
        { }
    }
}
