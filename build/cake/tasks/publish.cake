var publishTask = Task("Publish")
.Finally(() =>
{
    if (publishingError)
    {
        throw new Exception("An error occurred during the publishing of GitVersion. All publishing tasks have been attempted.");
    }
});