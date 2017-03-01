# Custom NuGet Packages

## Source

This video: https://www.youtube.com/watch?v=BsTeUs0Y5TM

The official NuGet CLI Reference: https://docs.microsoft.com/en-us/nuget/tools/nuget-exe-cli-reference


## Creation

* Go to: https://www.nuget.org/
* Download "latest nuget.exe" (under "Install NuGet").
* Put the `nuget.exe` in your project folder.
* Open the cmd and go to the project folder.
* Execute `nuget spec`.
* This will generate a .nuspec file template. Fill it properly (see code below).
* Execute `nuget pack YourSpecFileName.nuspec`.
* The .nupkg file will appear in the project folder (not in \bin).

	  <files>
		<file src="bin\Debug\net461\YourDll.dll" target="lib"></file>
	  </files>
    
    
## How to consume it

* Create a folder where you will store the NuGet packages.
* Put your new package there
* Add this folder as a new NuGet repository: Manage nuget packages => options (top-right corner) => + => Select your folder
* Select the new nuget repository
* Select your package


## Script

To avoid putting your `nuget.exe` in every single project, you can keep it under a `\tools` directory and have the following .bat script:

	nuget pack "../src/ProjectDirectory/MyNuspecFile.nuspec" -OutputDirectory "\path\to\your\packages\directory"
	PAUSE
	
