public class BuildParameters
{

    private readonly ICakeContext _context;
    // TODO :: START
    public const string MainRepoOwner  = "TheMoFaDe";
    public const string MainRepoName = "DotNet.Solution.Template";
    public string SolutionFile { get; } = ""; // You can leave empty if only one solution file exist in the root folder
    // TODO :: END
    public SolutionParserResult SolutionParserResult {get;}
    public List<CustomProjectParserResult> SolutionProjects {get;} = new List<CustomProjectParserResult>(){};
    public List<string> TargetFrameworks {get;}
    public string Target { get; private set; }  
    public string Configuration { get; private set; } 
    public bool IsRunningOnAppVeyor { get; private set; }
    public bool IsRunningOnTravis { get; private set; }
    public bool IsRunningOnAzurePipeline { get; private set; }
    public bool IsRunningOnGitHubActions { get; private set; }
    public bool IsRunningOnUnix { get; private set; }
    public bool IsRunningOnWindows { get; private set; }
    public bool IsRunningOnLinux { get; private set; }
    public bool IsRunningOnMacOS { get; private set; }
    public bool IsLocalBuild { get; private set; }
    public bool IsMainRepo { get; private set; }
    public bool IsMainBranch { get; private set; }
    public bool IsTagged { get; private set; }
    public bool IsPullRequest { get; private set; }
    public bool IsChangeLogUpToDate { get; private set;}
    public bool IsStableRelease() => !IsLocalBuild && IsMainRepo && IsMainBranch && !IsPullRequest && IsTagged;
    public bool IsPreRelease()    => !IsLocalBuild && IsMainRepo && IsMainBranch && !IsPullRequest && !IsTagged;
    public Dictionary<PlatformFamily, string[]> NativeRuntimes { get; private set; }
    public BuildCredentials Credentials {get;}
    public BuildVersion Version {get;set;} = new BuildVersion();
    public BuildPaths Paths => BuildPaths.GetPaths(_context,Version.SemVersion);

 

    public BuildParameters(ICakeContext context){

        if (context == null)
        {
            throw new ArgumentNullException("context");
        }
        _context = context;
         Credentials = new BuildCredentials(context); 
         Configuration = context.Argument("configuration", "Release");
         Target =        context.Argument("target", "Default");

         // TODO :: START
         if(string.IsNullOrEmpty(SolutionFile))
         SolutionFile = context.GetFiles("./*.sln").First().ToString();
         // TODO :: END

         SolutionParserResult = context.ParseSolution(SolutionFile);
         foreach(var project in SolutionParserResult.Projects){ 
            var file = (IFile)context.FileSystem.GetFile(project.Path);
            if(System.IO.File.Exists(project.Path.FullPath)){ // handles edge cases like solution items folder being consider projects in the solution file
            var projectParsed = file.ParseProjectFile(Configuration);
            SolutionProjects.Add(projectParsed);
            }
         }
         var buildSystem = context.BuildSystem();
         IsChangeLogUpToDate = context.IsChangeLogUpToDate();
         IsLocalBuild  = buildSystem.IsLocalBuild;
         IsPullRequest = buildSystem.IsPullRequest;
         IsMainRepo    = context.IsOnMainRepo();
         IsMainBranch  = context.IsOnMainBranch();
         IsTagged      = context.IsBuildTagged();
         IsRunningOnUnix    = context.IsRunningOnUnix();
         IsRunningOnWindows = context.IsRunningOnWindows();
         IsRunningOnLinux   = context.Environment.Platform.Family == PlatformFamily.Linux;
         IsRunningOnMacOS   = context.Environment.Platform.Family == PlatformFamily.OSX;
         
         IsRunningOnAppVeyor      = buildSystem.IsRunningOnAppVeyor;
         IsRunningOnTravis        = buildSystem.IsRunningOnTravisCI;
         IsRunningOnAzurePipeline = buildSystem.IsRunningOnAzurePipelines || buildSystem.IsRunningOnAzurePipelinesHosted;
         IsRunningOnGitHubActions = buildSystem.IsRunningOnGitHubActions;


         NativeRuntimes = new Dictionary<PlatformFamily, string[]>
        {
            [PlatformFamily.Windows] = new[] { "win-x64", "win-x86" },
            [PlatformFamily.Linux]   = new[] { "ubuntu.18.04-x64,ubuntu.20.04-x64" },
            [PlatformFamily.OSX]     = new[] { "osx-x64" },
        };

    }
}
