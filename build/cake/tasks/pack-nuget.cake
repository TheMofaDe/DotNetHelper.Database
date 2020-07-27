
Task("Pack-Nuget")
    .IsDependentOnWhen("Generate-Docs", isSingleStageRun)
    .Does<BuildParameters>((context,parameters) =>  {


    var buildSettings = new DotNetCoreMSBuildSettings (){};
    buildSettings.WithProperty("GeneratePackageOnBuild", "true");
    buildSettings.WithProperty("PackageVersion", parameters.Version.SemVersion);
    buildSettings.WithProperty("Version", parameters.Version.SemVersion);
     foreach(var project in parameters.SolutionProjects)
     {
        DotNetCorePack(project.ProjectFilePath.FullPath, new DotNetCorePackSettings { 
            Configuration = parameters.Configuration,
            OutputDirectory =  parameters.Paths.Directories.NugetRoot,
            NoBuild = true,
            NoRestore = true,
            MSBuildSettings = buildSettings,
        });
      }
       
});

