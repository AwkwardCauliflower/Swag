# Swag

Static Web Album Generator

Swag generates a recursive image-viewing web site for an entire directory tree of images using just HTML, JavaScript, and JSON. The site files will be generated in a child directory under the root of your web site. It was designed to be VR-friendly for VR games that feature a web browser, and run locally in a web server on your PC.

# Features

For any directory in your website, you can view a slideshow of all images in that folder, or of all images in all descendant folders, too, in one large slideshow. The generated website has a directory navigator, and within each directory you can start one of the types of slideshows. Once you've begun a slideshow, you can pause and resume it, manually navigate forward or backward, view the path of the current image, or go back up to the directory page. Each of these features are hidden until you hover over the appropriate region of the image. The slideshow will automatically move to the next image every 10 seconds. This is unfortunately hard-coded for now. If you navigate while the slideshow is running, it will not pause it, but it will reset the 10 second timer.

Swag will find all of your images and write their paths into a number of JSON files. It does not find the images as you browse. This is because it's designed to support showing random images from among tens or even hundreds of thousands of images in large directory trees. Reading the files each time would be too slow. The downside is: if you add, remove, or move your images later, you must re-run Swag for it to update the JSON files.

Swag will only store up to 10,000 images in any one JSON file. If you have more than that in a directory and its subdirectories, it will select 10,000 random ones from among them. When displaying this random slideshow, those (up to) 10,000 images will be shown in a random order. This makes the random slideshow different every time, but you will only ever see those same 10,000 images. You need to run the tool again to get a different subset of images.

Swag does not copy any of your images to its web folder or create any thumbnails. It's designed to simply let you browse your existing images in place. This requires that all of your images be contained in the same website as the files Swag generates. The broadest usage is to create a website at the root of your hard drive, and run Swag pointed at that drive. This will let you use Swag to browse every directory and image on the entire drive.

Swag will not display a directory in its directory browser unless it or some of its descendant directories contain images.

Swag will read and use Windows shortcut files as long as they point to images within the website. This lets you create mixes of images from folders that aren't related to each other into one slideshow. Simple "Copy" and "Paste Shortcut" multiple images into one folder, and Swag will be able to read them and create a slideshow.

# Usage

Swag is a command line tool. It has two required arguments: the path to the root of your website, and the name of a folder to create inside of it to contain all of its files. You can optionally blacklist certain paths from being parsed by Swag. It can be useful to create a batch file with your parameters to make repeat usage of the tool easier.

    Swag root webFolder [blacklistedName]*
    
      root             The root of the website. All images must be cointained
                       within.
                       
      webFolder        A name of a folder into which the site files will be
                       generated. E.g., www.
                       
      blacklistedName  Any number of strings separated by spaces. Paths
                       containing any of the strings will be ignored. The 
                       webFolder name is automatically blacklisted, as are all
                       system folders.

If you have a website running at the root of your X: drive with two folders named 'GIFs' and 'Thumbnails' which you don't want included, and you want Swag's folder to be named 'www', you would use the following command:
    
    Swag X:\ www GIFs Thumbnails
    
Note: this will exclude all directories and files that contain the words 'GIFs' or 'Thumbnails'. If you'd prefer, you can specify a full path to a directory to exclude. Use double quotes if the path contains spaces.

When Swag runs, you will see a series of "Scan: " messages, one for each directory it scans. When scanning is complete, you will see a series of "Generate: " messages. If you press Ctrl-C to abort the program before the generation process begins, no changes will have been made to your files. If you abort the program after generation has begun, it will already have deleted the previous Swag output folder and replaced it with a partial one.

If you want to browse the resultant website after Swag runs, you must setup an HTTP server. This can be as simple as creating a new website in IIS (Internet Information Services, built into Windows) that points to the root folder you pointed Swag at. There are many simple web server programs you can install as well. Any vanilla HTTP server will do. There is no back-end code in Swag. It simply generates static HTML, CSS, JavaScript, and JSON files.
      
# Building Swag Yourself

Swag requires .Net 4.5. It was built using [Microsoft Visual Studio Community 2017](https://visualstudio.microsoft.com/free-developer-offers/).

# Future Changes

* A GUI would be nice. There's already code to handle a Cancel button during generation.
* Event-based progress indicators.
    * Currently contains Console.Write commands right in the libraries. Lazy!
* If remaining a command line tool, a revised argument handling system to allow for named options would be good. 
    * Like an option for the size limit of the JSONs instead of being hard-coded to 10,000 files.