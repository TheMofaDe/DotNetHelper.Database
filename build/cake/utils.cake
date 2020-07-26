public static FilePath FindToolInPath(this ICakeContext context, string tool)
{
    var pathEnv = context.EnvironmentVariable("PATH");
    if (string.IsNullOrEmpty(pathEnv) || string.IsNullOrEmpty(tool)) return tool;

    var paths = pathEnv.Split(new []{ context.IsRunningOnUnix() ? ':' : ';'},  StringSplitOptions.RemoveEmptyEntries);
    return paths.Select(path => new DirectoryPath(path).CombineWithFilePath(tool)).FirstOrDefault(filePath => context.FileExists(filePath.FullPath));
}

public static bool IsOnMainRepo(this ICakeContext context)
{
    var buildSystem = context.BuildSystem();
    string repositoryName = null;
    if (buildSystem.IsRunningOnAppVeyor)
    {
        repositoryName = buildSystem.AppVeyor.Environment.Repository.Name;
    }
    else if (buildSystem.IsRunningOnTravisCI)
    {
        repositoryName = buildSystem.TravisCI.Environment.Repository.Slug;
    }
    else if (buildSystem.IsRunningOnAzurePipelines || buildSystem.IsRunningOnAzurePipelinesHosted)
    {
        repositoryName = buildSystem.AzurePipelines.Environment.Repository.RepoName;
    }
    else if (buildSystem.IsRunningOnGitHubActions)
    {
        repositoryName = buildSystem.GitHubActions.Environment.Workflow.Repository;
    }

    context.Information("Repository Name: {0}" , repositoryName);

    return !string.IsNullOrWhiteSpace(repositoryName) && StringComparer.OrdinalIgnoreCase.Equals($"{BuildParameters.MainRepoOwner}/{BuildParameters.MainRepoName}", repositoryName);
}

public static bool IsOnMainBranch(this ICakeContext context)
{
    var buildSystem = context.BuildSystem();
    string repositoryBranch = ExecGitCmd(context, "rev-parse --abbrev-ref HEAD").Single();
    if (buildSystem.IsRunningOnAppVeyor)
    {
        repositoryBranch = buildSystem.AppVeyor.Environment.Repository.Branch;
    }
    else if (buildSystem.IsRunningOnTravisCI)
    {
        repositoryBranch = buildSystem.TravisCI.Environment.Build.Branch;
    }
    else if (buildSystem.IsRunningOnAzurePipelines || buildSystem.IsRunningOnAzurePipelinesHosted)
    {
        repositoryBranch = buildSystem.AzurePipelines.Environment.Repository.SourceBranchName;
    }
    else if (buildSystem.IsRunningOnGitHubActions)
    {
        repositoryBranch = buildSystem.GitHubActions.Environment.Workflow.Ref.Replace("refs/heads/", "");
    }

    context.Information("Repository Branch: {0}" , repositoryBranch);

    return !string.IsNullOrWhiteSpace(repositoryBranch) && StringComparer.OrdinalIgnoreCase.Equals("master", repositoryBranch);
}


public static bool IsOnMainBuildSystem(this ICakeContext context)
{
    // TODO :: START
    var buildSystem = context.BuildSystem();
    if (buildSystem.IsRunningOnAppVeyor)
    {
        return true;
    }
    else if (buildSystem.IsRunningOnTravisCI)
    {
      return false;
    }
    else if (buildSystem.IsRunningOnAzurePipelines || buildSystem.IsRunningOnAzurePipelinesHosted)
    {
        return false;
    }
    else if (buildSystem.IsRunningOnGitHubActions)
    {
       return false;
    }
    return false;
    // TODO :: END-
}

public static bool IsBuildTagged(this ICakeContext context)
{
    var sha = ExecGitCmd(context, "rev-parse --verify HEAD").Single();
    var isTagged = ExecGitCmd(context, "tag --points-at " + sha).Any();

    return isTagged;
}

public static bool IsChangeLogUpToDate(this ICakeContext context){

    var rootDir = (DirectoryPath)(context.Directory("."));
    var fileInfo = new FileInfo(rootDir.CombineWithFilePath("CHANGELOG.md").ToString());
    if(fileInfo.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-1)){
        return false;
    }
    return true;
}

public static bool IsEnabled(this ICakeContext context, string envVar, bool nullOrEmptyAsEnabled = true)
{
    var value = context.EnvironmentVariable(envVar);
        context.Information($"{envVar}={value}");
    return string.IsNullOrWhiteSpace(value) ? nullOrEmptyAsEnabled : bool.Parse(value);
}

public static List<string> ExecuteCommand(this ICakeContext context, FilePath exe, string args)
{
    context.StartProcess(exe, new ProcessSettings { Arguments = args, RedirectStandardOutput = true }, out var redirectedOutput);

    return redirectedOutput.ToList();
}

public static List<string> ExecGitCmd(this ICakeContext context, string cmd)
{
    var gitExe = context.Tools.Resolve(context.IsRunningOnWindows() ? "git.exe" : "git");
    return context.ExecuteCommand(gitExe, cmd);
}

public static CakeTaskBuilder IsDependentOnWhen(this CakeTaskBuilder builder, string name, bool condition)
{
    if (builder == null)
    {
        throw new ArgumentNullException(nameof(builder));
    }
    // Information($"{name}={condition}");
    if (condition)
    {
       builder =  builder.IsDependentOn(name);
    }
    return builder;
}

public static CakeTaskBuilder IsDependentOnWhen(this CakeTaskBuilder builder, CakeTaskBuilder task, bool condition)
{
    if (builder == null || task == null)
    {
        throw new ArgumentNullException(nameof(builder));
    }
      //Context.Information(condition);
    if (condition)
    {
      
       builder =  builder.IsDependentOn(task);
    }
    return builder;
}

public static void ForceDeleteDirectory(this ICakeContext context, string fileOrDirectory){ 
    
    var settings = new DeleteDirectorySettings (){
         Recursive = true,
         Force = true
    };
    var dir = System.IO.Path.GetDirectoryName(fileOrDirectory);
    if(context.DirectoryExists(dir))
        context.DeleteDirectory(dir,settings);
}

public static void ForceDeleteDirectory(this ICakeContext context, Cake.Core.IO.Path path){ 
    
     context.ForceDeleteDirectory(path.FullPath);
}


public static void SetVersionFromJsonFile(this ICakeContext context,  BuildParameters parameters){

         var jsonFile = parameters.Paths.Directories.ArtifactsRoot.GetFilePath("build.version.json");
         if(context.FileExists(jsonFile))
         parameters.Version = context.DeserializeJsonFromFile<BuildVersion>(jsonFile); 

}


void PackPrepareNative(ICakeContext context, BuildParameters parameters)
{
    // publish single file for all native runtimes (self contained)
    var platform = Context.Environment.Platform.Family;
    var runtimes = parameters.NativeRuntimes[platform];
    foreach (var runtime in runtimes)
    {
        var outputPath = PackPrepareNative(context, parameters, runtime);
    }
}


DirectoryPath PackPrepareNative(ICakeContext context, BuildParameters parameters, string runtime)
{
    var platform = Context.Environment.Platform.Family;
    var outputPath = parameters.Paths.Directories.Native.Combine(platform.ToString().ToLower()).Combine(runtime);

    foreach(var project in parameters.SolutionProjects)
    {
        if(!project.IsNetCore || project.OutputType == "Library") continue;
         var settings = new DotNetCorePublishSettings
         {
            Framework = "netcoreapp5.0",
            Runtime = runtime,
            NoRestore = false,
            Configuration = parameters.Configuration,
            OutputDirectory = outputPath,
        // MSBuildSettings = parameters.MSBuildSettings,
        };

      settings.ArgumentCustomization =
        arg => arg
        .Append("/p:PublishSingleFile=true")
        .Append("/p:PublishTrimmed=true")
        .Append("/p:IncludeSymbolsInSingleFile=true");

         context.DotNetCorePublish(project.ProjectFilePath.FullPath, settings);
    }
    return outputPath;
}




