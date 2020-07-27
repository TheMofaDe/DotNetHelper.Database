Task("Clean")
.Does<BuildParameters>((context,parameters) => {
 
    CleanDirectory(parameters.Paths.Directories.ArtifactsRoot);
    //context.ForceDeleteDirectory("./artifacts");
    foreach (var project in parameters.SolutionProjects)
    {
        context.ForceDeleteDirectory(System.IO.Path.Combine(project.ProjectFilePath.FullPath,"bin"));
        context.ForceDeleteDirectory(System.IO.Path.Combine(project.ProjectFilePath.FullPath,"obj"));
    }
    context.ForceDeleteDirectory(parameters.Paths.Directories.Artifacts);

}).ReportError(exception =>
{
    Error(exception.Dump());
});
