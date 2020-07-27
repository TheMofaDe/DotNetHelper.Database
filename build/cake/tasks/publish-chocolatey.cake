Task("Publish-Chocolatey-Internal")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.EnabledPublishChocolatey, "Publish-Chocolatey was disabled.")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsRunningOnWindows,       "Publish-Chocolatey works only on Windows agents.")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsReleasingCI,            "Publish-Chocolatey works only on Releasing CI.")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsStableRelease(),        "Publish-Chocolatey works only for releases.")
    .IsDependentOnWhen("Pack-Chocolatey", isSingleStageRun)
    .Does<BuildParameters>((parameters) =>
{
    var apiKey = parameters.Credentials.Chocolatey.ApiKey;
    if(string.IsNullOrEmpty(apiKey)) {
        throw new InvalidOperationException("Could not resolve Chocolatey API key.");
    }

    var apiUrl = parameters.Credentials.Chocolatey.ApiUrl;
    if(string.IsNullOrEmpty(apiUrl)) {
        throw new InvalidOperationException("Could not resolve Chocolatey API url.");
    }

    foreach(var package in parameters.Packages.Chocolatey)
    {
        if (FileExists(package.PackagePath))
        {
            try
            {
                // Push the package.
                ChocolateyPush(package.PackagePath, new ChocolateyPushSettings
                {
                    ApiKey = apiKey,
                    Source = apiUrl,
                    Force = true
                });
            }
            catch (System.Exception)
            {
                // chocolatey sometimes fails with an error, even if the package gets pushed
            }
        }
    }
})
.OnError(exception =>
{
    Information("Publish-Chocolatey Task failed, but continuing with next Task...");
    Error(exception.Dump());
    publishingError = true;
});
