// Install modules
#module nuget:?package=Cake.DotNetTool.Module&version=0.4.0

// Install addins.
#addin "nuget:?package=Cake.Json&version=5.2.0"
#addin "nuget:?package=Cake.Incubator&version=5.1.0"
#addin "nuget:?package=Cake.DocFx&version=0.13.1"
#addin "nuget:?package=Newtonsoft.Json&version=12.0.2"
// Install tools.
#tool "nuget:?package=docfx.console&version=2.56.1"
#tool "nuget:?package=nuget.commandline&version=5.6.0"
#tool "nuget:?package=ReportGenerator&version=4.6.1"
//#tool "nuget:?package=NUnit.ConsoleRunner&version=3.11.1"
//#tool "nuget:?package=NunitXml.TestLogger&version=2.1.62"

// Load other scripts.
#load "./build/cake/utils.cake"
#load "./build/cake/credentials.cake"
#load "./build/cake/parameters.cake"
#load "./build/cake/version.cake"
#load "./build/cake/paths.cake"


#load "./build/cake/tasks/clean.cake"
#load "./build/cake/tasks/restore.cake"
#load "./build/cake/tasks/formatcode.cake"
#load "./build/cake/tasks/build.cake"
#load "./build/cake/tasks/unittest.cake"
#load "./build/cake/tasks/generatedocs.cake"
#load "./build/cake/tasks/pack.cake"
#load "./build/cake/tasks/pack-nuget.cake"
#load "./build/cake/tasks/pack-self-contained.cake"
#load "./build/cake/tasks/zip.cake"
#load "./build/cake/tasks/publish-coverage.cake"
#load "./build/cake/tasks/publish-appveyor.cake"
#load "./build/cake/tasks/publish-azuredevops.cake"
#load "./build/cake/tasks/publish-nuget.cake"
#load "./build/cake/tasks/publish.cake"

 

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//////////////////////////////////////////////////////////////////////
// PARAMETERS
//////////////////////////////////////////////////////////////////////
bool publishingError = false;
bool isSingleStageRun = Context.IsEnabled("SINGLE_STAGE_BUILD", true);

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup<BuildParameters>(context =>
{
    var parameters = new BuildParameters(context); 

    if(!isSingleStageRun)
        context.SetVersionFromJsonFile(parameters); 
     try
    {
        // Increase verbosity?
        if (context.IsOnMainBranch() && (context.Log.Verbosity != Verbosity.Diagnostic)) {
            Information("Increasing verbosity to diagnostic.");
            context.Log.Verbosity = Verbosity.Diagnostic;
        }

        if (parameters.IsLocalBuild)             Information("Building locally");
        if (parameters.IsRunningOnAppVeyor)      Information("Building on AppVeyor");
        if (parameters.IsRunningOnTravis)        Information("Building on Travis");
        if (parameters.IsRunningOnAzurePipeline) Information("Building on AzurePipeline");
        if (parameters.IsRunningOnGitHubActions) Information("Building on GitHubActions");

        Information("Repository info : IsMainRepo {0}, IsMainBranch {1}, IsTagged: {2}, IsPullRequest: {3}, Single-Stage-Build: {4}",
            parameters.IsMainRepo,
            parameters.IsMainBranch,
            parameters.IsTagged,
            parameters.IsPullRequest,
            isSingleStageRun);

        return parameters;
    }
    catch (Exception exception)
    {
        Error(exception.Dump());
        return null;
    }
});
Teardown<BuildParameters>((context, parameters) =>
{
    try
    {
        Information("Starting Teardown...");
        if(context.Successful)
        {

        }else{

        }
        Information("Finished running tasks.");
    }
    catch (Exception exception)
    {
        Error(exception.Dump());
    }
});







// BUILD
// FORMAT CODE
// GENERATE DOCS 
// TEST
// PACK
// PUBLISH 

zipFilesTask = zipFilesTask.IsDependentOnWhen("Generate-Docs", isSingleStageRun);
zipFilesTask = zipFilesTask.IsDependentOnWhen("Build", isSingleStageRun);

packTask = packTask.IsDependentOnWhen("Zip-Files", isSingleStageRun);
packTask = packTask.IsDependentOnWhen("Pack-Nuget", isSingleStageRun);



publishTask = publishTask.IsDependentOnWhen("Build", isSingleStageRun);
publishTask = publishTask.IsDependentOnWhen("UnitTest", isSingleStageRun);
publishTask = publishTask.IsDependentOnWhen("Format-Code", isSingleStageRun);
publishTask = publishTask.IsDependentOnWhen("Generate-Docs", isSingleStageRun);
publishTask = publishTask.IsDependentOnWhen("Pack", isSingleStageRun);
publishTask = publishTask.IsDependentOnWhen("Publish-AppVeyor", isSingleStageRun);
publishTask = publishTask.IsDependentOnWhen("Publish-AzurePipeline", isSingleStageRun);
publishTask = publishTask.IsDependentOnWhen("Publish-Coverage", isSingleStageRun);
publishTask = publishTask.IsDependentOnWhen("Publish-NuGet", isSingleStageRun);


//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
Task("Default")
    .IsDependentOn("Publish")
    .Does<BuildParameters>((parameters) => {
    
}).ReportError(exception =>
{
    Error(exception.Dump());
});


var target = Argument("target", "Default");
RunTarget(target);

