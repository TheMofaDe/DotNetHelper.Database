Task("Publish-NuGet")
  //  .WithCriteria<BuildParameters>((context, parameters) => parameters.EnabledPublishNuget, "Publish-NuGet was disabled.")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsRunningOnWindows,  "Publish-NuGet works only on Windows agents.")
  //  .WithCriteria<BuildParameters>((context, parameters) => parameters.IsReleasingCI,       "Publish-NuGet works only on Releasing CI.")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsStableRelease() || parameters.IsPreRelease(), "Publish-NuGet works only for releases.")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsChangeLogUpToDate,  "Don't be lazy keep your change log up to date before publishing")
    .Does<BuildParameters>((parameters) =>
{
    if (parameters.IsStableRelease())
    {
        var apiKey = parameters.Credentials.Nuget.ApiKey;
        if(string.IsNullOrEmpty(apiKey)) {
            throw new InvalidOperationException("Could not resolve NuGet API key.");
        }

        var apiUrl = parameters.Credentials.Nuget.ApiUrl;
        if(string.IsNullOrEmpty(apiUrl)) {
            throw new InvalidOperationException("Could not resolve NuGet API url.");
        }

       
        foreach(var package in parameters.Paths.Directories.NugetPackages)
        {
            if (FileExists(package.FullPath))
            {
                // Push the package to nuget.org
                NuGetPush(package.FullPath, new NuGetPushSettings
                {
                    ApiKey = apiKey,
                    Source = apiUrl
                });
            }
        }
    }

    // Push the package to GitHub Packages
    if (parameters.IsRunningOnGitHubActions && parameters.IsMainRepo && parameters.IsMainBranch)
    {
        Information("Publishing nuget to GitHub Packages");

        var token = parameters.Credentials.GitHub.Token;
        if(string.IsNullOrEmpty(token)) {
            throw new InvalidOperationException("Could not resolve Github token.");
        }
        var userName = parameters.Credentials.GitHub.UserName;
        if(string.IsNullOrEmpty(userName)) {
            throw new InvalidOperationException("Could not resolve Github userName.");
        }

        var source = $"https://nuget.pkg.github.com/{BuildParameters.MainRepoOwner}/index.json";

        var nugetSourceSettings = new NuGetSourcesSettings
        {
            UserName = userName,
            Password = token
        };

        Information("Adding NuGet source with user/pass...");
        NuGetAddSource("GitHub", source, nugetSourceSettings);

        foreach(var package in parameters.Paths.Directories.NugetPackages)
        {
            if (FileExists(package.FullPath))
            {
                NuGetPush(package.FullPath, new NuGetPushSettings
                {
                    Source = source
                });
            }
        }
    }
})
.OnError(exception =>
{
    Information("Publish-NuGet Task failed, but continuing with next Task...");
    Error(exception.Dump());
    publishingError = true;
});


