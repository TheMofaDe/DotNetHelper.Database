

Task("Publish-AzurePipeline")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsRunningOnWindows, "Publish-AzurePipeline works only on Windows agents.")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsRunningOnAzurePipeline,   "Publish-AzurePipeline only works on devops.")
    .WithCriteria<BuildParameters>((context, parameters) => !parameters.IsPullRequest,     "Publish-AzurePipeline works only for non-PR commits.")
    .IsDependentOnWhen("UnitTest", isSingleStageRun)
    .IsDependentOnWhen("Pack", isSingleStageRun)
    .Does<BuildParameters>((parameters) =>
{
    foreach(var artifact in parameters.Paths.Directories.CoverageResults)
    {
        if (FileExists(artifact.FullPath)) { AzurePipelines.Commands.UploadArtifact("", artifact.FullPath, "coverage-results"); }
    }
    foreach(var results in parameters.Paths.Directories.TestResults)
    {
        if (FileExists(results.FullPath)) { AzurePipelines.Commands.UploadArtifact("", results.FullPath, "test-results"); }
    }
    foreach(var package in parameters.Paths.Directories.NugetPackages)
    {
        if (FileExists(package.FullPath)) { AppVeyor.UploadArtifact(package.FullPath); }
    }
})
.OnError(exception =>
{
    Information("Publish-AzurePipeline Task failed, but continuing with next Task...");
    Error(exception.Dump());
    publishingError = true;
});
