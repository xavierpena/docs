# Merging all files into a single .exe


## The problem

Like [ this StackOverflow user](http://stackoverflow.com/questions/10137937/merge-dll-into-exe), I wanted to distribute my compiled solution with **a single .exe**. Instead of having a bunch of **dll**'s, **pdb**'s, **xml**'s, the clickable .exe is the only thing that matters to the user in the end.

So this is a post about how to combine .NET external dlls to a single executable file.


## The solution 

As stated in the StackOverflow, there is a project called ILMerge that does that. But wait, there is more...

If you add a NuGet package called `MSBuild.ILMerge.Task` in your project, it will automatically merge all files into one single .exe every time you hit "Rebuild". How cool is that.

## How does this solution work

So I was concerned that installing ILMerge [ through the official page](https://www.microsoft.com/en-us/download/details.aspx?id=17630), and being it a .msi, my project would depend on stuff installed on my machine. That's a no-no in Continuous Integration.

I started searching and I found this NuGet package, which is great to make this ILMerge task "machine-independent". So you'll find 2 NuGet packages with a lot of downloads: 
* ilmerge
* MSBuild.ILMerge.Task

It turns out `MSBuild.ILMerge.Task` references `ilmerge`. But the advantage of `MSBuild.ILMerge.Task` is that does it all automatically. Really: once the NuGet package is installed, when you hit "rebuild" it automatically creates the single .exe (letting the .config file outside, of course, plus a .pdb). Instead of the myridad of dll's that you had before.

As the [ author says](https://ilmergemsbuild.codeplex.com/discussions/578156):

> This project was intended to provide "quick & reasonably clean" way of doing 95% of most common IL merges. More than that is, in my opinion, not practical - there are simply too many rare and wondrous ILMerge features to wrap, test and document. Anyway, if you want to combine multiple assemblies into an .EXE executable, Costura.Fody would be a better choice in most cases. Merging assemblies into a .DLL with fine-grained control over the merge details is a pretty niche case these days.


## More about how it works

The NuGet package automatically adds two files to your project:

### ILMerge.props

Theoretically, you could find more info about each option here:

http://research.microsoft.com/en-us/people/mbarnett/ilmerge.aspx 

But the page is not found and I can't seem to find it anywhere else.

### ILMergeOrder.txt

It states:

	# this file contains the partial list of the merged assemblies in the merge order
	# you can fill it from the obj\CONFIG\PROJECT.ilmerge generated on every build
	# and finetune merge order to your satisfaction


## A word of caution

### Edits on debug

It now says "Changes are not allowed if the assembly has not been loaded".

#### Quick and dirty solution

If you want to temporarily go back to the "unpacked" build, you can edit your .proj and remove those lines:

	<Target Name="EnsureNuGetPackageBuildImports" BeforeTargets="PrepareForBuild">
	<PropertyGroup>
	  <ErrorText>This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is {0}.</ErrorText>
	</PropertyGroup>
	<Error Condition="!Exists('..\packages\MSBuild.ILMerge.Task.1.0.5\build\MSBuild.ILMerge.Task.props')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\MSBuild.ILMerge.Task.1.0.5\build\MSBuild.ILMerge.Task.props'))" />
	<Error Condition="!Exists('..\packages\MSBuild.ILMerge.Task.1.0.5\build\MSBuild.ILMerge.Task.targets')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\MSBuild.ILMerge.Task.1.0.5\build\MSBuild.ILMerge.Task.targets'))" />
	</Target>
	<Import Project="..\packages\MSBuild.ILMerge.Task.1.0.5\build\MSBuild.ILMerge.Task.targets" Condition="Exists('..\packages\MSBuild.ILMerge.Task.1.0.5\build\MSBuild.ILMerge.Task.targets')" />  


#### A cleaner solution

BUT there is a cleaner solution I came up with:
* You want to build a single exe binary for project A.
* Create another project (project B) in the same solution. I've called it `A.SingleExe`.
* Reference project A in project B
* Install the ILMerge NuGet package on project B
* Your program B will only contain a call to `A.Program.Main(args);`. That's it.
* Now you can debug on project A, and use project B as the builder of the "single exe" binary.

(of course all these steps would be unnecessary if we did the builds through a separate build process, but here I am talking about a simple solution using NuGet)


### Resources set as "content"

Let's say that at runtime you reference a .txt file that you set as "content" and "copy always". Now it won't work, since it is embedded into the .exe.

It is better to use "Embedded resources" as explained [ in this post](http://stackoverflow.com/questions/18108725/reading-an-embedded-text-file).

### Log4net not working

(but it won't stop the execution either)

The console displays this kind of message:

	log4net:ERROR Failed to parse config file. Is the <configSections> specified as: <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler,MyProjectName, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" />
	System.Configuration.ConfigurationErrorsException: An error occurred creating the configuration section handler for log4net: Could not load file or assembly 'log4net' or one of its dependencies. The system cannot find the file specified. (C:\...\MyProjectName.vshost.exe.Config line 4) ---> System.IO.FileNotFoundException: Could not load file or assembly 'log4net' or one of its dependencies. The system cannot find the file specified.

But if instead of the app.config (xml) configuration you use "fluent configuration" for log4net, it will solve the problem:

	
	// Use the following `ConfigureLog4Net()` instead of the old `log4net.Config.XmlConfigurator.Configure()`
	            
	
	private static void ConfigureLog4Net()
	{
	
		/// CONTENT IN THE APP.CONFIG THAT WILL BE TRANSFORMED TO FLUENT CONFIGURATION:
		///
		///    <appender name="LogFileAppender" type="log4net.Appender.RollingFileAppender">
		///      <file value="logfile.txt" />
		///      <appendToFile value="true" />
		///      <rollingStyle value="Size" />
		///      <maxSizeRollBackups value="5" />
		///      <maximumFileSize value="1024KB" />
		///      <staticLogFileName value="true" />
		///      <layout type="log4net.Layout.PatternLayout">
		///        <conversionPattern value="%date %level %logger - %message %exception%newline" />
		///      </layout>
		///    </appender>
		///    <root>
		///      <level value="ALL" />
		///      <appender-ref ref="LogFileAppender" />
		///    </root>
	
		var fileappender = new log4net.Appender.RollingFileAppender();
		fileappender.File = "logfile.txt";
		fileappender.AppendToFile = true;
		fileappender.RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Size;
		fileappender.MaxSizeRollBackups = 5;
		fileappender.MaximumFileSize = "1024KB";
		fileappender.StaticLogFileName = true;
		fileappender.Threshold = log4net.Core.Level.Debug;
		fileappender.Layout = new log4net.Layout.PatternLayout("%date %level %logger - %message %exception%newline");
		fileappender.ActivateOptions();
		((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository()).Root.AddAppender(fileappender);
		log4net.Config.BasicConfigurator.Configure(fileappender);
	}
	


## Alternatives to ILMerge

A nice list of possible alternatives can be found in [ this post](http://manuelmeyer.net/2016/01/net-power-tip-10-merging-assemblies/).

But I wanted to add another alternative. Maybe in your compilation destination you just want:
* \bin folder with a .exe at the root
* All the dll's under \bin\lib.

In that case, what you can use [ the solutions in this post](http://stackoverflow.com/questions/2445556/c-sharp-putting-the-required-dlls-somewhere-other-than-the-root-of-the-output):

1. Copy all dll's under \bin\lib

> You can copy them there manually, use a pre- or post-build event or something completely different.

2. Add this to your app.config:

	  <runtime>
	    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
	      <probing privatePath="lib" />
	    </assemblyBinding>
	  </runtime>

