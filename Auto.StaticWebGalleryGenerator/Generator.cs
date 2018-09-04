using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auto.ImageTree;
using System.IO;
using System.Threading;
using System.Reflection;

namespace Auto.StaticWebGalleryGenerator
{
    /// <summary>
    /// Traverses a directory and all subdirectories, finds all images, then generates a static website for viewing those images. 
    /// </summary>
    public class Generator
    {
        const string INDEX_BUTTON_DISABLED_CLASS = "indexButtonDisabled";

        /// <summary>
        /// Initializes and instance of this class.
        /// </summary>
        /// <param name="path">The root of the website.</param>
        /// <param name="webFolderName">The name of the web folder to create to hold all generated files.</param>
        /// <param name="blacklist">Any path containing any of these strings will be ignored.</param>
        /// <param name="maxDeepImages">The maximum number of images to include in a recursoive slideshow of any given directory. Limits the size of the resultant JSON files.</param>
        /// <param name="deleteWebFolder">If true, an existing web folder of the same name will be deleted before generating the files.</param>
        public Generator( string path, string webFolderName, List<string> blacklist, int maxDeepImages = 10000, bool deleteWebFolder = false )
        {
            if ( string.IsNullOrWhiteSpace( path ) )
            {
                throw new Exception( "Path must be specified." );
            }

            if ( string.IsNullOrWhiteSpace( webFolderName ) )
            {
                throw new Exception( "WebFolderName must be specified." );
            }

            RootDirectory = new DirectoryInfo( path );

            if ( !RootDirectory.Exists )
            {
                throw new Exception( "Path doesn't exist" );
            }

            WebFolderName = webFolderName;
            Blacklist = blacklist;
            MaxDeepImages = maxDeepImages;
            DeleteWebFolder = deleteWebFolder;
        }

        /// <summary>
        /// The root of the website.
        /// </summary>
        public DirectoryInfo RootDirectory { get; private set; }

        /// <summary>
        /// The root <see crefImageTreeDirectory"/> created during processing. ALso contains all descendant ImageTreeDirectories.
        /// </summary>
        public ImageTreeDirectory RootImageTree { get; private set; }

        /// <summary>
        /// The name of the web folder to create to hold all generated files. 
        /// </summary>
        public string WebFolderName { get; private set; }

        /// <summary>
        /// Any path containing any of these strings will be ignored.
        /// </summary>
        public List<string> Blacklist { get; private set; }

        /// <summary>
        /// Set to true when <see cref="PopulateImageTree"/> has completed processing.
        /// </summary>
        public bool IsImageTreePopulated { get; private set; }

        /// <summary>
        /// The maximum number of images to include in a recursoive slideshow of any given directory. Limits the size of the resultant JSON files.
        /// </summary>
        public int MaxDeepImages { get; private set; }

        /// <summary>
        /// If true, an existing web folder of the same name will be deleted before generating the files.
        /// </summary>
        public bool DeleteWebFolder { get; private set; }

        /// <summary>
        /// Populates the image tree and sets <see cref="RootImageTree"/>. Must be called before calling <see cref="GenerateWebsite"/>.
        /// </summary>
        /// <param name="token"></param>
        public void PopulateImageTree( CancellationToken token )
        {
            if ( !IsImageTreePopulated )
            {
                EnsureWebFolderIsBlacklisted();

                RootImageTree = new ImageTreeDirectory( RootDirectory )
                {
                    Blacklist = Blacklist
                };

                //TODO: handle and echo an event each time a new directory is being parsed, to provide some textual feedback.

                RootImageTree.Populate( token );

                //If cancel was requested, don't let IsImageTreePopulated be set to true.
                if ( token.IsCancellationRequested ) return;

                IsImageTreePopulated = true;
            }
        }

        private void EnsureWebFolderIsBlacklisted()
        {
            if ( !Blacklist.Any( p => p.ToLower().Trim() == WebFolderName.ToLower().Trim() ) )
            {
                Blacklist.Add( WebFolderName );
            }
        }

        /// <summary>
        /// Generates all website files.
        /// </summary>
        /// <param name="token"></param>
        public void GenerateWebsite( CancellationToken token )
        {
            if ( !IsImageTreePopulated )
            {
                throw new Exception( "ImageTree is not populated. Call PopulateImageTree() first." );
            }

            DirectoryInfo wwwDirectory;

            if ( DeleteWebFolder )
            {
                wwwDirectory = new DirectoryInfo( Path.Combine( RootDirectory.FullName, WebFolderName ) );

                if ( wwwDirectory.Exists )
                {
                    wwwDirectory.Delete( true );
                }
            }

            if ( token.IsCancellationRequested ) return;

            wwwDirectory = RootDirectory.CreateSubdirectory( WebFolderName );

            WriteStaticResources( wwwDirectory );

            if ( token.IsCancellationRequested ) return;

            //TODO: expose an event each time a new directory is being parsed, to provide some feedback.
            CreateGalleryPage( RootImageTree, wwwDirectory, token );
        }

        private void CreateGalleryPage( ImageTreeDirectory currentImageTree, DirectoryInfo currentSlideDirectory, CancellationToken token )
        {
            Console.WriteLine( "Generate: " + currentSlideDirectory.FullName );

            if ( token.IsCancellationRequested ) return;

            var currentImages = currentImageTree.DescendantImages.Where( p => p.Depth == 0 ).OrderBy( p => p.Item.FileInfo.Name );
            var deepImages = currentImageTree.DescendantImages;

            int countOwnImages = currentImages.Count();
            //Don't count current images as deep images, even though they will be included in the recursive JSON.
            int countDeepImages = deepImages.Count( p => p.Depth > 0 );

            var subTreesWithImages = currentImageTree.ChildDirectories.Where( p => p.DescendantImages.Count > 0 );

            WriteJson( currentSlideDirectory, currentImages, deepImages, countOwnImages, countDeepImages );

            if ( token.IsCancellationRequested ) return;

            WriteIndexPage( currentImageTree, subTreesWithImages, currentSlideDirectory, countOwnImages, countDeepImages );
            WriteSlideshowPage( currentSlideDirectory );

            foreach ( var subImageTree in subTreesWithImages )
            {
                if ( token.IsCancellationRequested ) return;

                var subSlideDirectory = currentSlideDirectory.CreateSubdirectory( subImageTree.DirectoryInfo.Name );

                CreateGalleryPage( subImageTree, subSlideDirectory, token );
            }
        }

        private void WriteJson( DirectoryInfo currentSlideDirectory, IEnumerable<DescendantImageItem> currentImages, IEnumerable<DescendantImageItem> deepImages, int countOwnImages, int countDeepImages )
        {
            if ( countOwnImages > 0 )
            {
                var jsonDir = currentSlideDirectory.CreateSubdirectory( "json" );
                string currentImagesJson = GetJsonImageList( currentImages.Select( p => p.Item.ImageFileInfo ) );

                File.WriteAllText( Path.Combine( jsonDir.FullName, "images.json" ), currentImagesJson );
            }

            if ( countDeepImages > 0 )
            {
                var deepImagesRandom = RandomizeImages( deepImages );
                IEnumerable<DescendantImageItem> deepImagesToUse;
                deepImagesToUse = deepImagesRandom.Take( MaxDeepImages );

                var jsonDir = currentSlideDirectory.CreateSubdirectory( "json" );
                string deepImagesJson = GetJsonImageList( deepImagesToUse.Select( p => p.Item.ImageFileInfo ) );
                File.WriteAllText( Path.Combine( jsonDir.FullName, "recursive.json" ), deepImagesJson );
            }
        }

        private void WriteIndexPage( ImageTreeDirectory currentImageTree, IEnumerable<ImageTreeDirectory> subDirectoriesWithImages, DirectoryInfo currentSlideDirectory, int countOwnImages, int countDeepImages )
        {
            string template = GetEmbeddedResourceString( "Auto.StaticWebGalleryGenerator.StaticResources.Html.index.html" );
            string subDirTableRows = GetSubDirectoryLinks( subDirectoriesWithImages );

            string output =
                template
                .Replace( "`imageFolderPath`", currentImageTree.DirectoryInfo.FullName )
                .Replace( "`webFolderName`", WebFolderName )
                .Replace( "`subDirs`", subDirTableRows )
                .Replace( "`numberOfOwnImages`", countOwnImages.ToString() )
                .Replace( "`numberOfRecursiveImages`", Math.Min( MaxDeepImages, countDeepImages ).ToString() );

            var ownImageButtonsExtraClass = countOwnImages > 0 ? string.Empty : INDEX_BUTTON_DISABLED_CLASS;
            var deepImageButtonsExtraClass = countDeepImages > 0 ? string.Empty : INDEX_BUTTON_DISABLED_CLASS;

            output = output.Replace( @"`SlideshowExtraClass`", ownImageButtonsExtraClass );
            output = output.Replace( @"`SlideshowRndExtraClass`", ownImageButtonsExtraClass );
            output = output.Replace( @"`SlideshowRecursiveRndExtraClass`", deepImageButtonsExtraClass );
            //numberOfOwnImages
            //numberOfRecursiveImages

            File.WriteAllText( Path.Combine( currentSlideDirectory.FullName, "index.html" ), output );
        }

        private string GetSubDirectoryLinks( IEnumerable<ImageTreeDirectory> subDirectoriesWithImages )
        {
            const string template =
@"	<a class='subDirectoryButton' href='{0}'>
		<span class='link'>{1}</span>
	</a>";

            var links =
                string.Join(
                    Environment.NewLine,
                    subDirectoriesWithImages.Select(
                        p => string.Format(
                            template,
                            CleanPathForHref( p.DirectoryInfo.Name ),
                             GetHtmlDescendingSizeWords( p.DirectoryInfo.Name )
                        )
                    )
                );

            return links;
        }

        private string GetHtmlDescendingSizeWords( string name )
        {
            const string template = @"<span style=""font-size: {0}pt"">{1}</span>";

            var words = name.Split( ' ', '_' );

            StringBuilder sb = new StringBuilder();

            int startingPointSize = 12;

            //Starts at startingPointSize and goes down by 1pt every two words.
            for ( int i = 0; i < words.Length; i++ )
            {
                sb.AppendFormat( template, Math.Max( startingPointSize - i / 2, 1 ), words[ i ] );

                if ( i < words.Length - 1 )
                {
                    sb.Append( " " );
                }
            }

            return sb.ToString();
        }

        private object CleanPathForHref( string name )
        {
            //Fenix hates # in hrefs
            return name.Replace( "#", "%23" );
        }

        private void WriteSlideshowPage( DirectoryInfo currentSlideDirectory )
        {
            string template = GetEmbeddedResourceString( "Auto.StaticWebGalleryGenerator.StaticResources.Html.slideshow.html" );

            var imagesJsonPath = GetWebPath( Path.Combine( currentSlideDirectory.FullName, @"json\images.json" ), RootDirectory );
            var recursveJsonPath = GetWebPath( Path.Combine( currentSlideDirectory.FullName, @"json\recursive.json" ), RootDirectory );

            string output =
                template
                .Replace( "`webFolderName`", WebFolderName )
                .Replace( "`imagesJsonPath`", imagesJsonPath )
                .Replace( "`recursveJsonPath`", recursveJsonPath );

            File.WriteAllText( Path.Combine( currentSlideDirectory.FullName, "slideshow.html" ), output );
        }

        private IEnumerable<DescendantImageItem> RandomizeImages( IEnumerable<DescendantImageItem> descendantImages )
        {
            Random r = new Random();

            return descendantImages.Select( p => new Tuple<DescendantImageItem, int>( p, r.Next() ) ).OrderBy( p => p.Item2 ).Select( p => p.Item1 );
        }

        private string GetJsonImageList( IEnumerable<FileInfo> images )
        {
            var webPaths = GetWebPaths( images );

            var json = GetJsonImageString( webPaths );

            return json;
        }

        private List<string> GetWebPaths( IEnumerable<FileInfo> images )
        {
            return images.Select( p => GetWebPath( p, RootDirectory ) ).ToList();
        }

        private string GetWebPath( FileInfo file, DirectoryInfo webRoot )
        {
            return GetWebPath( file.FullName, webRoot );
        }

        private string GetWebPath( string path, DirectoryInfo webRoot )
        {
            var webPath = Path.Combine( path.Remove( 0, webRoot.FullName.Length ).Replace( @"\", "/" ) );
            //Hack for web roots that are drive letters, because the trailing slash is required for drive letter, and then mistakenly removed from image paths.
            webPath = webPath.StartsWith( "/" ) ? webPath : "/" + webPath;

            webPath = CleanPathForJson( webPath );
            return webPath;
        }

        private string CleanPathForJson( string webPath )
        {
            //Fenix browser hates # in paths.
            return webPath.Replace( "#", "%23" );
        }

        private string GetJsonImageString( IEnumerable<string> imageWebPaths )
        {
            var template =
@"{{
	""images"" : [ 
		{0} 
	] 
}}";
            const string PREFIX = @"""";
            const string SUFFIX = @"""";
            string separator = @"""," + Environment.NewLine + "\t\t\"";

            if ( imageWebPaths.FirstOrDefault() == null )
            {
                return string.Format( template, string.Empty );
            }
            else
            {
                return string.Format( template, PREFIX + string.Join( separator, imageWebPaths ) + SUFFIX );
            }
        }

        #region WriteStaticResources
        private void WriteStaticResources( DirectoryInfo slideRoot )
        {
            WriteStaticResource( slideRoot, "Css", "slideshow-styles.css" );
            WriteStaticResource( slideRoot, "Css", "index-styles.css" );
            WriteStaticResource( slideRoot, "Scripts", "jquery-3.3.1.js" );

            WriteBinraryStaticResource( slideRoot, "Images", "next.png" );
            WriteBinraryStaticResource( slideRoot, "Images", "pause.png" );
            WriteBinraryStaticResource( slideRoot, "Images", "play.png" );
            WriteBinraryStaticResource( slideRoot, "Images", "previous.png" );
            WriteBinraryStaticResource( slideRoot, "Images", "up.png" );
        }

        private void WriteBinraryStaticResource( DirectoryInfo slideRoot, string folderName, string resourceFileName )
        {
            var outputPath = Path.Combine( slideRoot.FullName, folderName, resourceFileName );

            slideRoot.CreateSubdirectory( folderName );

            const string resourcePrefix = "Auto.StaticWebGalleryGenerator.StaticResources.";

            string resourceName = resourcePrefix + folderName + "." + resourceFileName;

            File.WriteAllBytes( outputPath, GetEmbeddedResourceBinary( resourceName ) );
        }

        private void WriteStaticResource( DirectoryInfo slideRoot, string folderName, string resourceFileName )
        {
            var outputPath = Path.Combine( slideRoot.FullName, folderName, resourceFileName );

            slideRoot.CreateSubdirectory( folderName );

            const string resourcePrefix = "Auto.StaticWebGalleryGenerator.StaticResources.";

            string resourceName = resourcePrefix + folderName + "." + resourceFileName;

            File.WriteAllText( outputPath, GetEmbeddedResourceString( resourceName ) );
        }

        private static string GetEmbeddedResourceString( string resourceName )
        {
            var assembly = Assembly.GetExecutingAssembly();

            using ( Stream stream = assembly.GetManifestResourceStream( resourceName ) )
            using ( StreamReader reader = new StreamReader( stream ) )
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }

        private byte[] GetEmbeddedResourceBinary( string resourceName )
        {
            var assembly = Assembly.GetExecutingAssembly();

            using ( Stream stream = assembly.GetManifestResourceStream( resourceName ) )
            using ( MemoryStream ms = new MemoryStream() )
            {
                stream.CopyTo( ms );
                return ms.ToArray();
            }
        }

        #endregion
    }
}
