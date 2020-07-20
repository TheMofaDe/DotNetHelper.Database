

#region Tests

var unitTestTask = Task("UnitTest")
   
    .Does<BuildParameters>((context,parameters) =>
{
        
    var frameworks = new List<string>();
     var actions = new List<Action>(){};
    foreach(var project in parameters.SolutionProjects){  
        
        if(!project.IsTestProject()) // does not support single target .net 5 yet https://github.com/cake-contrib/Cake.Incubator/blob/3858732caa686c9edf16111459869cde2e2694a6/src/Cake.Incubator/Project/ProjectParserExtensions.cs
        continue;

        foreach(var framework in project.TargetFrameworkVersions){

            frameworks.Add(framework);
            
            actions.Add(() =>
            {
                var projectName = $"{project.AssemblyName}.{framework}";
                
          var coverageFile = $"{projectName}.coverage";
          var testResultFile = $"{projectName}.results.xml";
          var resultsPath = MakeAbsolute(parameters.Paths.Directories.TestResultsOutput.CombineWithFilePath(testResultFile));
          var excludeAssembly = project.ProjectFilePath.GetFilenameWithoutExtension();
        
                var settings = new DotNetCoreTestSettings {
                    Framework = framework,
                    NoBuild = false,
                    NoRestore = false,
                    Configuration = parameters.Configuration,
                    ArgumentCustomization = args => args
                     .Append("/p:CollectCoverage=true")
                     .Append("/p:CoverletOutputFormat=cobertura")
                     .Append("/p:Threshold=1")
                     //.Append("/p:Exclude=\"[" + excludeAssembly + "*]*\"")
                     .Append($"/p:CoverletOutput={MakeAbsolute(parameters.Paths.Directories.TestResultsOutput)}/{coverageFile}")
                     .Append("--logger").Append("trx")
                     .Append("--results-directory").Append(parameters.Paths.Directories.TestResultsOutput.ToString())
                };

            
                if (!parameters.IsRunningOnMacOS) {
                    //settings.TestAdapterPath = new DirectoryPath(".");
                    //settings.Logger = $"nunit;LogFilePath={resultsPath}"; // TODO :: enable if your using NUnit as your testing framework
                }

                if (IsRunningOnUnix() && string.Equals(framework, project.IsNetFramework))
                {
                    settings.Filter = "TestCategory!=NoMono";
                }
                DotNetCoreTest(project.ProjectFilePath.FullPath, settings);
        
            });

        }
   } 
    foreach(var framework in frameworks.Distinct())
    {
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = -1,
            CancellationToken = default
        };
        Parallel.Invoke(options, actions.ToArray());
    }

     foreach (var coverageFile in parameters.Paths.Directories.CoverageResults) {
        ReportGenerator(coverageFile,parameters.Paths.Directories.TestResultsOutput + "/" + "htmlreports");
     }
})
.ReportError(exception =>
{
    var error = (exception as AggregateException).InnerExceptions[0];
    Error(error.Dump());
})
.Finally(() =>
{

    var parameters = Context.Data.Get<BuildParameters>();
 
        if(parameters.Paths.Directories.TestResults.Count() > 0){
            var data = new AzurePipelinesPublishTestResultsData {
                TestResultsFiles = parameters.Paths.Directories.TestResults,
                Platform = Context.Environment.Platform.Family.ToString(),
                TestRunner = AzurePipelinesTestRunnerType.NUnit,
                Configuration = parameters.Configuration,
            };   
            if (parameters.IsRunningOnAzurePipeline)
            AzurePipelines.Commands.PublishTestResults(data);
        }

 
        if(parameters.Paths.Directories.CoverageResults.Count() > 0){

            var data = new AzurePipelinesPublishCodeCoverageData {
                AdditionalCodeCoverageFiles = parameters.Paths.Directories.CoverageResults.ToArray(),
                CodeCoverageTool = AzurePipelinesCodeCoverageToolType.Cobertura,
                ReportDirectory = parameters.Paths.Directories.TestResultsOutput + "/" + "htmlreports",
                SummaryFileLocation = parameters.Paths.Directories.CoverageResults[0]
            };   
            if (parameters.IsRunningOnAzurePipeline)
            AzurePipelines.Commands.PublishCodeCoverage(data);
        }
    
    
});




#endregion


