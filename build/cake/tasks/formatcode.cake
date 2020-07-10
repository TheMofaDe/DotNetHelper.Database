
var formatCodeTask = Task("Format-Code")
    .WithCriteria<BuildParameters>((context, parameters) => parameters.IsLocalBuild,  "Format-Code will only run during a local build")
    .Does<BuildParameters>((context, parameters) => {

     var tool = parameters.IsRunningOnUnix ? "dotnet" : "dotnet.exe";
     var formatResult = context.ExecuteCommand(tool,$"format {System.IO.Path.GetDirectoryName(parameters.SolutionFile)} --verbosity diagnostic --report {parameters.Paths.Directories.ArtifactsRoot}");
        Information(string.Join(Environment.NewLine,formatResult)); 

})
.ReportError(exception =>
{
    Error(exception.Dump());
})
;





