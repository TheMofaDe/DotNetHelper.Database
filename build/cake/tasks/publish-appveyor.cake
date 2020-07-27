
Task("Publish-AppVeyor")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsRunningOnWindows,  "Publish-AppVeyor works only on Windows agents.")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsRunningOnAppVeyor, "Publish-AppVeyor works only on AppVeyor.")
    .IsDependentOnWhen("UnitTest", isSingleStageRun)
    .IsDependentOnWhen("Pack", isSingleStageRun)
    .Does<BuildParameters>((parameters) =>
{

    foreach(var artifact in parameters.Paths.Directories.CoverageResults)
    {
        if (FileExists(artifact.FullPath)) { AppVeyor.UploadArtifact( artifact.FullPath); }
    }
    foreach(var package in parameters.Paths.Directories.TestResults)
    {
        if (FileExists(package.FullPath)) { AppVeyor.UploadArtifact(package.FullPath); }
    }
    foreach(var package in parameters.Paths.Directories.NugetPackages)
    {
        if (FileExists(package.FullPath)) { AppVeyor.UploadArtifact(package.FullPath); }
    }
})
.OnError(exception =>
{
    Information("Publish-AppVeyor Task failed, but continuing with next Task...");
    Error(exception.Dump());
    publishingError = true;
});
