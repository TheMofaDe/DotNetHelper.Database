public class BuildPaths
{
    public BuildDirectories Directories { get; private set; }
    //public BuildFiles Files {get; private set;}

    public static BuildPaths GetPaths(
        ICakeContext context,
        string semVersion
        )
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        if (string.IsNullOrEmpty(semVersion))
        {
            throw new ArgumentNullException(nameof(semVersion));
        }
     
        var rootDir                       = (DirectoryPath)(context.Directory("."));
        var sourceDir                     = rootDir.Combine("src");
        var artifactsRootDir              = rootDir.Combine("artifacts");
        var artifactsDir                  = artifactsRootDir.Combine("v" + semVersion);
        var artifactsBinDir               = artifactsDir.Combine("bin");
        var nativeDir                     = artifactsDir.Combine("native");
        var nugetRootDir                  = artifactsDir.Combine("nuget");
        var buildArtifactDir              = artifactsDir.Combine("build-artifact");
        var testResultsOutputDir          = artifactsDir.Combine("test-results");
        var releaseNotesOutputFilePath    = buildArtifactDir.CombineWithFilePath("releasenotes.md");

        // Directories
        var buildDirectories = new BuildDirectories(
            rootDir,
            sourceDir,
            artifactsRootDir,
            artifactsDir,
            buildArtifactDir,
            testResultsOutputDir,
            nugetRootDir,
            artifactsBinDir,
            nativeDir,
            context);

        return new BuildPaths
        {
            Directories = buildDirectories,
         //   Files = new BuildFiles()
        };
    }
}

public class BuildDirectories
{
    private ICakeContext Context {get; set;}
    public DirectoryPath Root { get; private set; }
    public DirectoryPath Source { get; private set; }
    public DirectoryPath ArtifactsRoot { get; private set; }
    public DirectoryPath Artifacts { get; private set; }
    public DirectoryPath NugetRoot { get; private set; }
    public DirectoryPath BuildArtifact { get; private set; }
    public DirectoryPath Native { get; private set; }
    public DirectoryPath TestResultsOutput { get; private set; }
    public DirectoryPath ArtifactsBin { get; private set; }
    public List<DirectoryPath> ToClean { get; private set; }

    public ReadOnlyCollection<FilePath> CoverageResults { get {
        var list = new List<FilePath>(){};
        var files = Context.GetFiles($"{TestResultsOutput.FullPath}/**/*.coverage");
        foreach(var file in files){
            list.Add(file);
        }
        return list.AsReadOnly();
    }}
    public ReadOnlyCollection<FilePath> TestResults { get {
        var list = new List<FilePath>(){};
        var files =  Context.GetFiles($"{TestResultsOutput.FullPath}/**/*.results.xml");
        foreach(var file in files){
            list.Add(file);
        }
        return list.AsReadOnly();
    }}
    public ReadOnlyCollection<FilePath> NugetPackages { get {
        var list = new List<FilePath>(){};
        var files =  Context.GetFiles($"{NugetRoot.FullPath}/**/*.nupkg");
        foreach(var file in files){
            list.Add(file);
        }
        return list.AsReadOnly();
    }}

    public BuildDirectories(
        DirectoryPath rootDir,
        DirectoryPath sourceDir,
        DirectoryPath artifactsRootDir,
        DirectoryPath artifactsDir,
        DirectoryPath buildArtifactDir,
        DirectoryPath testResultsOutputDir,
        DirectoryPath nugetRootDir,
        DirectoryPath artifactsBinDir,
        DirectoryPath nativeDir,
        ICakeContext context
        )
    {
        Context = context;
        Root = rootDir;
        Source = sourceDir;
        ArtifactsRoot = artifactsRootDir;
        Artifacts = artifactsDir;
        BuildArtifact = buildArtifactDir;
        Native = nativeDir;
        TestResultsOutput = testResultsOutputDir;
        NugetRoot = nugetRootDir;
        ArtifactsBin = artifactsBinDir;
        ToClean = new List<DirectoryPath>() {
            ArtifactsRoot,
            BuildArtifact,
            TestResultsOutput,
            NugetRoot,
            ArtifactsBin,
            Native,
        };
    }
}

