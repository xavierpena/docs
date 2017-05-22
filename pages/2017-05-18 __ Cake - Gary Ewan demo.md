
# A Piece of Cake - C# powered cross platform build automation - Gary Ewan Park

Original video [here](https://vimeo.com/171704581).

Original demo files [here](https://github.com/gep13/CakeDemos).

Since a one hour talk in a conference is not a "how to" video, as I tried to replicate the demo in my computer I came accross some small issues.

This post provides some help about how to solve those issues, along with some "how to" notes taken directly from the video.


## Setting up the demo (previous steps not shown in the video)

In Visual Studio Code, we first need to install the Cake extension package:

![image01](http://i.imgur.com/TYAWPS0.png)

Then you need to restart Visual Studio Code.

Press `Ctrl+Shift+P` to show all commands.

Now you can write `Cake` and select `Cake: install a bootstraper` (as shown in the video). Then choose `PowerShell` (if you are working on Windows, like I am).


## Basic cake example

The boostrap file (`build.ps1`) ensures that all executable files (`Cake.exe`) and depenencies are downloaded before starting.

We are going to add a `build.cake` file. We will populate it with the tasks.

Here is the most basic `build.cake` that you can start with:

    // Accepting arguments from the command line:
    // "Find an argument called 'target' from the command line. If you don't find it, default that to the `Default' target"
    var target = Argument("target", "Default");

    Task("NuGet-Package-Restore")
        .Does(() =>
        {

        });

    // Definition of the 'Default' task (what should be run when 'Default' is started):
    Task("Default")
        .IsDependentOn("NuGet-Package-Restore");

    RunTarget(target);


## Running this basic example

In Visual Sudio Code, select `View` -> `Integrated Terminal`.

Type `.\build.ps1` in that terminal and press enter to execute.

**WARNING 1:** when doing that, I found that my Notepad.exe was automatically launched (instead of executing the .ps1 file). What I had to do to solve that is tell Windows to open .ps1 files with `powershell.exe`(which in my case was placed under `%SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe`). So: right click on the .ps1 file -> open with -> select your powershell.exe location.

**WARNING 2:** then I had problems asking Windows to "always use this program". I found the answer [in this stackoverflow post](https://superuser.com/a/835527). Basically what I did was removing the .ps1 altogether and repeat the process from WARNING 1.

**WARNING 3:** then I tried to run it in VSCode and I got the following error -> `File build.ps1 cannot be loaded because running scripts is disabled on this system. / + CategoryInfo: SecurityError: (:) [], PSSecurityExce /  + FullyQualifiedErrorId : UnauthorizedAccess`. To solve that, I used [this stackoverflow solution](http://stackoverflow.com/a/4038991/831138): I opened PowerShell(x86) as admin (right click -> *run as administrator*), and I typed `Set-ExecutionPolicy RemoteSigned`.

I was finally able to execute my first Cake script. It downloaded all the tools under a newly created .\tools folder, and it completed the script successfully.


## Adding features to the script

    var target = Argument("target", "Default");

    Task("NuGet-Package-Restore")
        .Does(() =>
        {
            // Restores all packages for the selected solution file (.sln):        
            NuGetRestore("./Source/Gep13.Cake.Sample.WebApplication.sln");
        });           

    Task("Default")
        .IsDependentOn("NuGet-Package-Restore");

    RunTarget(target);
    
        
## Building the solution

    var target = Argument("target", "Default");
    // Input argument to specify if we are using a Debug build or a Release build (in this case: default='Release')
    var configuration = Argument("configuration", "Release");

    Task("NuGet-Package-Restore")
        .Does(() =>
        {
            NuGetRestore("./Source/Gep13.Cake.Sample.WebApplication.sln");
        });
        
    Task("Build")
        .IsDependentOn("NuGet-Package-Restore")
        .Does(() =>
        {
            // Build the solution, with multiple properties set as extension methods
            // Documentation here: http://cakebuild.net/api/Cake.Common.Tools.MSBuild/MSBuildAliases/F36093FE
            // MSBuildSettings documentation: http://cakebuild.net/api/Cake.Common.Tools.MSBuild/MSBuildSettings/
            MSBuild("./Source/Gep13.Cake.Sample.WebApplication.sln"), new MSBuildSettings()
                .SetConfiguration(configuration)
                .WithProperty("Windows", "True")
                .WithProperty("TreatWarningsAserrors", "True")
                .UserToolVersion(MSBUildToolVersion.VS2015)
                .SetVerbosity(Verbosity.Minimal)
                .SetNodeReuse(false));
        });             

    Task("Default")
        .IsDependentOn("NuGet-Package-Restore");

    RunTarget(target);


## Tool resolution

To make a tool available for the script, we put that kind of syntax at the top of the script:

    #tool nuget:?package=xunit.runner.console
    
If we want to specify a speciffic nuget repository, we'll put it before the `?` (the other script defaults to `nuget.org`):

    #tool nuget:http://localhost:8081/repository/cake/?package=xunit.runner.console
    
Those will be downloaded into the \tools folder.


## CleanDirectories

This function makes sure that 1) the folder exists, and 2) the folder is empty.

	Task("Clean")
		.Does(() => 
		{
			CleanDirectories(new[] { "./.build/TestResults" });
		});
        

## Creating a NuGet package

Instead of having a .nuspec file (which we could), we'll be creating it on the fly:

	Task("Package")
		.IsDependentOn("SomeOtherPreviousTask")
		.Does(() =>
	{
		var nuGetPackSettings   = new NuGetPackSettings {
			Id                      = "Gep13.Cake.Sample.Common",
			Version                 = "0.1.0",
			Title                   = "Common Library used by Gep13.Cake.Sample Web Application",
			Authors                 = new[] {"gep13"},
			Owners                  = new[] {"gep13"},
			Description             = "The best Common Library you have ever seen",
			ProjectUrl              = new Uri("https://github.com/gep13/CakeDemos"),
			IconUrl                 = new Uri("https://raw.githubusercontent.com/cake-build/graphics/master/png/cake-small.png"),
			LicenseUrl              = new Uri("https://github.com/gep13/CakeDemos/blob/master/LICENSE"),
			Copyright               = "gep13 2016",
			ReleaseNotes            = new [] {"Bug fixes", "Issue fixes", "Typos"},
			Tags                    = new [] {"Gep13", "Cake", "Sample"},
			RequireLicenseAcceptance= false,
			Symbols                 = false,
			NoPackageAnalysis       = true,
			Files                   = new [] {
				new NuSpecContent {Source = "Gep13.Cake.Sample.Common.dll", Target = "bin"},
			},
			BasePath                = "./Source/Gep13.Cake.Sample.Common/bin/" + configuration,
			OutputDirectory         = "./BuildArtifacts/nuget"
		};

		NuGetPack(nuGetPackSettings);
	});


## Debugging the script

We can add a `#break` directive at any line in the script. If there is a debugger attached, it will stop at that point in the cake build.

Instead of using the bootstrapper, use the command line to use cake directly. This attaches a debugger to the process:

    cake .\build.cake --debug
    
This gives you a process id. When you have that id, you go to `VisualStudio -> Debug -> Attach process -> select the cake.exe pid -> attach`. This will directly step into the `#break` in your script.
