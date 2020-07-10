
var zipFilesTask = Task("Zip-Files")
    .Does<BuildParameters>((context, parameters) => {

    foreach(var project in parameters.SolutionProjects){   

        if(project.IsTestProject()) // does not support single target .net 5 yet https://github.com/cake-contrib/Cake.Incubator/blob/3858732caa686c9edf16111459869cde2e2694a6/src/Cake.Incubator/Project/ProjectParserExtensions.cs
        continue;

        foreach(var outputPath in project.OutputPaths){
            var targetFramework = System.IO.Path.GetFileName(outputPath.FullPath);
            var zipFileName = parameters.Paths.Directories.Artifacts.CombineWithFilePath($"{project.AssemblyName}-{parameters.Version.SemVersion}-{targetFramework}.zip");
            
            CreateDirectory(parameters.Paths.Directories.Artifacts);
            Zip(outputPath.FullPath,zipFileName);
        }
        // (var targetFramework in project.TargetFrameworkVersions){}
    }
});