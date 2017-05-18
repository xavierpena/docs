
# Cake - Gary Ewan Park demo

Original video [here](https://vimeo.com/171704581).

Original demo files [here](https://github.com/gep13/CakeDemos).


## Setting up the demo (previous steps not shown in the video)

In Visual Studio Code, we first need to install the Cake extension package:

![image01](http://i.imgur.com/TYAWPS0.png)

Then you need to restart Visual Studio Code.

Press `Ctrl+Shift+P` to show all commands.

Now you can write `Cake` and select `Cake: install a bootstraper` (as shown in the video). Then choose `PowerShell` (if you are working on Windows, like I am).


## Next steps

The boostrap file (`build.ps1`) ensures that all executable files (`Cake.exe`) and depenencies are downloaded before starting.

We are going to add a `build.cake` file. We will populate it with the tasks.

Here is the most basic `build.cake` that you can start with:

    // Accepting arguments from the command line:
    // "Find an argument called 'target' from the command line. If you don't find it, default that to the `Default' target"
    var target = Argument("target", "Default")M

    Task("NuGet-Package-Restore")
        .Does(() =>
        {

        }
        );

    // Definition of the 'Default' task (what should be run when 'Default' is started):
    Task("Default")
        .IsDependentOn("NuGet-Package-Restore");

    RunTarget(target);
