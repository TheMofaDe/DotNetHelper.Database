    public class BuildVersion
    {
        public string GitBaseVersion { get; set; }
        public string GitSemVersion { get; set; }
        public string GitCommits { get; set; }
        public string GitBaseVersionMajor { get; set; }= "0";
        public string GitBaseVersionMinor { get; set; } = "0";
        public string GitBaseVersionPatch { get; set; }= "0";
        public string GitSemVerMajor { get; set; } = "0";
        public string GitSemVerMinor { get; set; } = "0";
        public string GitSemVerPatch { get; set; } = "0";
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
        public string InformationalVersion { get; set; }
        public string PackageVersion { get; set; }
        public string Version { get; set; }
        public string VersionPrefix { get; set; }
        public string VersionSuffix { get; set; }
        public string SemVerDashLabel  { get; set; }
        public string GitTag  { get; set; }
        public string GitBaseTag  { get; set; }
        public string GitIsDirty  { get; set; }

        public string SemVersion => $"{GitSemVerMajor}.{GitSemVerMinor}.{GitSemVerPatch}";
        public string BaseVesion => $"{GitBaseVersionMajor}.{GitBaseVersionMinor}.{GitBaseVersionPatch}";
    }