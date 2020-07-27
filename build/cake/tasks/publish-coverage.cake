Task("Publish-Coverage")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsRunningOnWindows, "Publish-Coverage works only on Windows agents.")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsRunningOnMainBuildSystem,     "Publish-Coverage works on the main BuildSystem")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsStableRelease() || parameters.IsPreRelease(), "Publish-Coverage works only for releases.")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsChangeLogUpToDate,  "Don't be lazy keep your change log up to date before publishing")
    .IsDependentOn("UnitTest")
    .Does<BuildParameters>((context,parameters) =>
{
 
    var token = parameters.Credentials.CodeCov.Token;
    if(string.IsNullOrEmpty(token)) {
        throw new InvalidOperationException("Could not resolve CodeCov token.");
    }

     var tool = parameters.IsRunningOnUnix ? "dotnet" : "dotnet.exe";
    foreach (var coverageFile in parameters.Paths.Directories.CoverageResults) {
        
        var formatResult = context.ExecuteCommand(tool,$"codecov -f {coverageFile.ToString()} -t {token}");
        // Codecov(new CodecovSettings {
        //     Files = new [] { coverageFile.ToString() },
        //     Token = token
        // });
    }
});