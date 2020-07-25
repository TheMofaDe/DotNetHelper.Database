Task("Publish-Coverage")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsRunningOnWindows, "Publish-Coverage works only on Windows agents.")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsRunningOnAzurePipeline,     "Publish-Coverage works only on Azure Pipeline.")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsStableRelease() || parameters.IsPreRelease(), "Publish-Coverage works only for releases.")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsChangeLogUpToDate,  "Don't be lazy keep your change log up to date before publishing")
    .IsDependentOn("UnitTest")
    .Does<BuildParameters>((parameters) =>
{
 
    var token = parameters.Credentials.CodeCov.Token;
    if(string.IsNullOrEmpty(token)) {
        throw new InvalidOperationException("Could not resolve CodeCov token.");
    }

    foreach (var coverageFile in parameters.Paths.Directories.CoverageResults) {
        Codecov(new CodecovSettings {
            Files = new [] { coverageFile.ToString() },
            Token = token
        });
    }
});