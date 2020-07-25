var publishTask = Task("Publish")
.WithCriteria<BuildParameters>((context, parameters) => parameters.IsChangeLogUpToDate,  "Don't be lazy keep your change log up to date before publishing")
.Does<BuildParameters>((context,parameters) =>  {

     
})
.Finally(() =>
{
    if (publishingError)
    {
        throw new Exception("An error occurred during the publishing of GitVersion. All publishing tasks have been attempted.");
    }
});