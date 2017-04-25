# Custom NuGet Packages

## Source

This video: https://www.youtube.com/watch?v=BsTeUs0Y5TM

The official NuGet CLI Reference: https://docs.microsoft.com/en-us/nuget/tools/nuget-exe-cli-reference

Official documentation: ["Package creation workflow"](https://docs.microsoft.com/en-us/nuget/create-packages/overview-and-workflow)


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
	
If dependencies must be added, and according to [this stackoverflow answer](http://stackoverflow.com/a/16310138/831138), the script would be:

	nuget pack "../src/ProjectDirectory/MyNuspecFile.nuspec" -OutputDirectory "\path\to\your\packages\directory" -IncludeReferencedProjects
	PAUSE

This would make unnecessary the `<dependencies>` tag in the .nuspec file.


## Full example

	<?xml version="1.0"?>
	<package >
	  <metadata>
		<id>YourProjectName</id>
		<version>0.0.4</version>
		<title>YourProjectName</title>
		<authors>Xavier Peña</authors>
		<owners>Xavier Peña</owners>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<description>Some words to describe your project</description>
		<releaseNotes>What has changed in this release</releaseNotes>
		<copyright>Copyright 2017</copyright>
		<tags>Licensing</tags>
		<dependencies>
			<dependency id="SameAsInPackages" version="1.1.0" />
		</dependencies>  	
	  </metadata>
	  <files>
		<file src="..\src\YourProjName\bin\Debug\YourProjName.dll" target="lib"></file>
	  </files>
	</package>


## Symbol packages

Supplying symbols for your library that allow consumers to step into your code while debugging (see [official documentation](https://docs.microsoft.com/en-us/nuget/create-packages/symbol-packages)).

You can create both packages with the -Symbols option, either from a .nuspec file or a project file:

    nuget pack MyPackage.nuspec -Symbols

    nuget pack MyProject.csproj -Symbols


## Script

It needs those other files in the same folder:

* `publish.bat`, as shown below
* `config.nuspec`, as shown above
* `version.txt`, which contains an integer of the last package verion that you previously published

The script will:

* Rebuild the solution
* Read the current version
* Add one to this version
* Publish the nuget package
* Save the publication event in a log file

`publish.bat` file:

	:: Rebuild solution ::
	set pathMSBuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin"
	@echo off
	cls
	call %pathMSBuild%\msbuild.exe "..\..\src\MySolutionFile.sln" /p:configuration=debug
	if %errorlevel% neq 0 (
		echo "An error occurred. Aborted."
		pause
		exit /b %errorlevel%
	)

	:: Update the package version ::
	set /p currentVersion=<version.txt
	set /a nextVersion=%currentVersion%+1
	echo Publishing next version: %nextVersion%

	:: Publish nuget package ::
	set fullVersion="0.0.%nextVersion%"
	..\nuget.exe pack config.nuspec -Version %fullVersion% -OutputDirectory "\\my\output\directory" -IncludeReferencedProjects -Symbols
	if %errorlevel% neq 0 (
		echo "An error occurred. Aborted."
		pause
		exit /b %errorlevel%
	)

	:: Save new version ::
	>version.txt echo %nextVersion%

	:: Update log ::
	echo %time%: %fullVersion% >> log.txt 

	echo Publication sucessful.

	pause


## Other (automatized) solutions

### NuGetizer3000

Apparently [NuGetizer3000](https://github.com/NuGet/NuGet.Build.Packaging) is the only one that works right now in VS2017.

1. Install [the VS2017 extension](http://bit.ly/nugetizer-2017)
2. Right-click on the project (in VS2017)
3. "Create NuGet Package"
4. Done: `Created package at \src\MyProjName\bin\Debug\MyProjName.0.0.1.nupkg.`

