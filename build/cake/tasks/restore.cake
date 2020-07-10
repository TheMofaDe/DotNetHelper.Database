Task("Restore")
    .IsDependentOn("Clean")
.Does<BuildParameters>((context,parameters) => {
    
    foreach (var project in parameters.SolutionProjects)
    {
        DotNetCoreRestore(project.ProjectFilePath.FullPath);
    }
}).ReportError(exception =>
{
    Error(exception.Dump());
});