# Swag

Static Web Album Generator

Swag generates a recursive image-viewing web site for an entire directory tree of images using just HTML, JavaScript, and JSON. The site files will be generated in a child directory under the root of your web site. It was designed to be VR-friendly for VR games that feature a web browser, and run locally in a web server on your PC.

#Features

For any directory in your website, you can view a slideshow of all images in that folder, or of all images in all descendant folders, too, in one large slideshow. The generated website has a directory navigator, and within each directory you can start one of the types of slideshows. Once you've begun a slideshow, you can pause and resume it, manually navigate forward or backward, view the path of the current image, or go back up to the directory page. Each of these features are hidden until you hover over the appropriate region of the image.

Swag will find all of your images and write their paths into a number of JSON files. It does not find the images as you browse. This is because it's designed to support showing random images from among tens or even hundreds of thousands of images in large directory trees. Reading the files each time would be too slow. The downside is: if you add, remove, or move your images later, you must re-run Swag for it to update the JSON files.

Swag will only store up to 10,000 images in any one JSON file. If you have more than that in a directory and its subdirectories, it will select 10,000 random ones from among them. When displaying this random slideshow, those (up to) 10,000 images will be shown in a random order. This makes the random slideshow different every time, but you will only ever see those same 10,000 images. You need to run the tool again to get a different subset of images.

Swag does not copy any of your images to its web folder or create any thumbnails. It's designed to simply let you browse your existing images in place. This requires that all of your images be contained in the same website as the files Swag generates. The broadest usage is to create a website at the root of your hard drive, and run Swag pointed at that drive. This will let you use Swag to browse every directory and image on the entire drive.

Swag will not display a directory in its directory browser unless it or some of its descendant directories contain images.

Swag will read and use Windows shortcut files as long as they point to images within the website. This lets you create mixes of images from folders that aren't related to eachother into one slideshow. Simple "Copy" and "Paste Shortcut" multiple images into one folder, and Swag will be able to read them and create a slideshow.

#Usage

First, you must setup your website. This can be as simple as creating a new website in IIS (Internet Information Services, built into Windows) that points to the root folder of images, and then running the tool pointing at that same path. There are many simple web server programs you can install as well. Any vanilla HTTP server will do. There is no back-end code in Swag. It simply generates static HTML, CSS, JavaScript, and JSON files.

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
                       
    Example usage:
    
If you have a website running at the root of your X: drive with two folders named 'Y' and 'Z' which you don't want included, and you want Swag's folder to be named 'www', you would use the following command:
    
    Swag X:\ www Y Z
      
#Building Swag Yourself

Swag requires .Net 4.5. It has no external dependencies, but it uses [ILMerge](https://www.microsoft.com/en-us/download/details.aspx?id=17630) in a post-build event to create a self-contained EXE. You can remove this post-build event if you don't wish to install or use ILMerge.

#Future Changes

* A GUI would be nice. There's already code to handle a Cancel button during generation.
* Event-based progress indicators.
    * Currently contains Console.Write commands right in the libraries. Lazy!
* If remaining a command line tool, a revised argument handling system to allow for named options would be good. 
    * Like an option for the size limit of the JSONs instead of being hard-coded to 10,000 files.