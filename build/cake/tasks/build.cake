
var buildTask = Task("Build")
    .IsDependentOn("Restore")
  .Does<BuildParameters>((context, parameters) => {
  
     foreach (var project in parameters.SolutionProjects)
    {
         if(project.IsTestProject()) // does not support single target .net 5 yet https://github.com/cake-contrib/Cake.Incubator/blob/3858732caa686c9edf16111459869cde2e2694a6/src/Cake.Incubator/Project/ProjectParserExtensions.cs
         continue;

        var msbuildSettings = new DotNetCoreMSBuildSettings {
                MaxCpuCount = 0,
            };
            msbuildSettings.WithProperty("GeneratePackageOnBuild", "false");
        var settings = new DotNetCoreBuildSettings {
            Configuration = parameters.Configuration,
            MSBuildSettings = msbuildSettings,
            NoRestore = true,
            
        };
        DotNetCoreBuild(project.ProjectFilePath.FullPath, settings);  
        context.SetVersionFromJsonFile(parameters);

        CopyDirectory(Directory(System.IO.Path.GetDirectoryName(project.ProjectFilePath.FullPath) + $"/bin/{parameters.Configuration}"), parameters.Paths.Directories.ArtifactsBin);
    }  
}).ReportError(exception =>
{
    Error(exception.Dump());
});



