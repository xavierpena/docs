# Getting started with .NET Core for Windows Developers


## Installation

https://www.microsoft.com/net

* Select ".NET Core 1.0"
* Select "Windows"
* Visual Studio 2015 must be installed
* Download and install ".NET Core 1.0.1 VS 2015 Tooling Preview 2"


## Console apps

### Some commands

To see directory tree in windows cmd:

	tree

To also see the files:

	tree /F


### Create the project

With Visual Studio:

New Project => .NET Core => Class Library / Console Application / ASP.NET Core Web Application

With the Command Line:

	dotnet new -t Lib

(in this case, of "type Library")


### Nuget

A .Net Core project core's references work only through Nuget packages.


#### Reverse package search

Search for a type/member and it will show packages containing a match:

https://packagesearch.azurewebsites.net


#### How to include a new Nuget package

Besides the old ways to do that, now you can go to: 

`project.json` => "dependencies" => add "nuget-package-name":"version-number".

When `project.json` is saved, VisualStudio automatically includes the new package in the project. (= "dotnet restore" in the command line)


### Deployment

`.xproj` is used by VisualStudio to keep track of the project (it's going away in a future version of .Net Core).

cmd => "dotnet run" => looks for a `project.json` file in the folder and it compiles+executes the application from the command line.

"dotnet publish" => it compiles, and publishes the result (for ".NETCoreApp,Version=v1.0" by default) and puts it into \Debug\netcoreapp1.0\publish

In order to run the published code (in any platform) => "dotnet MyProjectName.dll"


#### Framework dependend deployment (FDD)

(this is the one that was performed above)

It assumes that the target machine will have the shared runtime on it.

Like in the past (with the .net framework) it assumed that the .net runtime had already been installed on the target machine.


#### Self-contained deployment (SCD)

The target machine does not need to have the runtime on it (no dependencies outside this folder).


#### Multi-platform deployment

Source:

http://www.hanselman.com/blog/SelfcontainedNETCoreApplications.aspx

After running "dotnet restore" you'll want to build for each of these like this:

	dotnet build -r win10-x64
	dotnet build -r osx.10.10-x64
	dotnet build -r ubuntu.14.04-x64

And then publish release versions after you've tested, etc.

	dotnet publish -c release -r win10-x64
	dotnet publish -c release -r osx.10.10-x64
	dotnet publish -c release -r ubuntu.14.04-x64

### Throug the command line

* Create source: "dotnet new"
* Import class libraries and dependencies: "dotnet restore"
* Compile: "dotnet build" => creates dll
* Execute: "dotnet run", or "dotnet MyCompiledDllName.dll"


## Building a UWP

It looks a lot like a WPF app (in the standard .Net framework).

UWP is cross-device in the Windows space.

(skip?)


## Targeting the .NET standard library

First create a lib project with the Command Line:

	dotnet new -t Lib

...or with Visual Studio:

New Project => .NET Core => Class Library / Console Application / ASP.NET Core Web Application

The class library, instead of .NET Core app, targets "netstandard1.6" (as can be seen in the `project.json`). "netstandard" is understood by .NET Core and other flavours of .NET.

One problem (by now): `.csproj` cannot reference a `.xproj` (this will probably go away, since `.xproj` is going away). 

So for now we have to use a different approach:

New Project => Windows => Class Library (Portable)

Target: "Windows Universal 10.0" ## "ASP.NET Core 1.0" (although it won't matter because we are going to re-target it later at the dotnet standard)

Once it's created:

Project => Properties => Library => Targets => "Target .NET PlatformStandard"

We can also remove the `Microsoft.NETCore.Portable.Compatibility` from the `project.json` "dependencies".

Now we have a `.csproj`, and we'll be able to reference it on other projects.

We can add Nuget package dependencies to this project.

Once we add this shared library to another project, it's added in the `project.json` under "frameworks" (instead of "dependencies", although we could move it back to "dependencies"), and it has `{ "target" : "project"}` instead of "(version number)".

If this shared library is to be used in a classic .NET (let's say v4.51), then we need to modify the shared library's `project.json`, and bump down "frameworks" => "netstandard1.3" to "netstandard1.1".

### Cross-compiling a Console App for Multiple Frameworks in a Single Project

Go to `project.json` => frameworks => change:

	"frameworks" : {
		"netcoreapp1.0" : {
			"imports" : "dnxcore50",
			
			// the following must be cut from "dependencies" and pasted here:
			"Microsoft.NETCore.App": {
				"type": "platform",
				"version": "1.0.1"
			}		
		},
		"net451" : {
			// empty
		}
	}

If there is a dependency that targets both "dotnet framwork" ## "net451", it needs to be under "dependencies" (global) instead of "frameworks" => under a specific framework.

Under \Debug, you'll see:
* \net451
* \netcoreapp1.0


## Sharing .NET Core Libraries as NuGet Packages

We'll do that through the command line:

* Go to project code folder
* Run "dotnet build" (it's using `project.json`)
* It generates the folder \netstandard1.1 in \Debug and \obj
* This \netstandard1.1 contains the dll
* To create the NuGet package: "dotnet pack" (still in the root project cod folder)
* This NuGet packages are produced under \Debug (.nupkg, .symbols.nupkg)
* Those files can be inspected through the "NuGet Package Explorer", or renaming them as .zip and opening them.
* If you want to modify the info (author, version, description...) => `project.json` (for now! it will be moving to the .csproj)

	"authors": ["Author Name"],
	"description": "Whatever you want",
	"version": "1.0.1-*"

(`-*` to be able to inject a new number inside the version string)

An option that is helpful when you are producing builds in a CI environment (to indicate that it is a pre-release):
	dotnet pack --version-suffix ci12

### How to share those NuGet packages

[ (An article from Scott Hanselman about it)](http://www.hanselman.com/blog/HowToHostYourOwnNuGetServerAndPackageFeed.aspx)

Check if the command "nuget" is recongized. If not, open an admin cmd, and write:

	choco install nuget.commandline -y

To create a local private package feed, write this command line (where "-Source" is where the private nuget packages will be stored):

	nuget add MyProjName.nupkg -Source C:\privatenugets

To point to our custom package feed from our solution, first create a `NuGet.Config` (with the following code) and then copy it next to the .sln:

	<?xml version="1.0" encoding="utf-8"?>
	<configuration>
		<packageSources>
			<add key="Local feed" value="C:\privatenugets" />	
		</packageSources>
	</configuration>

Then, in "Manage NuGet Packages":
* "Package source:" to "All"
* check "Include prerelease"
(sometimes we need to close the solution and re-open it for it to work)


## Testing in .NET Core

You can't create a `test project` with Visual Studio (yet).

You have to go to the command line and do:

# Create a new folder (where your test project will be placed): \test\<ProjectName>Tests
# `dotnet new -t xunittest` (or also `nunittest`, no `mstest` yet though)
# `dotnet restore`
# `dotnet test` (SUMMARY: Total: 1 targets, Passed: 1, Failed:0)

To open it in VisualStudio: go to `add project` and select the `project.json` of the test project.

Remember that you can organize you projects in virtual folders in your Visual Studio solution.

### global.json

Is the solution configuration.

`"projects": ["src", "test"]` => default folders where projects are placed.

If your projects are not in those default folders, the cross references won't work.

If you change those folders afterwards, you'll need to remove those references from `project.json` and then add them again.

### xproj vs csproj

If your test project is xproj and you are testing a referenced csproj, those don't play well together and Intellisense will not make sense of it.

But the tests will work anyway. It's just that it will be difficult to work with the code.

### Tests targeting different frameworks

Under the `project.json` of your test project, you can add multiple of them:

	"frameworks": {
		"net40": {},
		"netcoreapp1.0" : {
			"dependencies": {
				"Microsoft.NETCore.App": {
					"type": "platform",
					"version": "1.0.1"
				}
			}
		}
	}

In that case, remember to remove your project dependency outside of "frameworks" and under "dependencies", as we explained before (when targeting multiple frameworks at the same time).