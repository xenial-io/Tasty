using System;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;

using static SimpleExec.Command;

namespace Tasty.Build.Helpers
{
    public class GitVersion
    {
        static Lazy<GitVersion> _Current = new Lazy<GitVersion>(() =>
        {
            GitVersion ReadGitVersionFromTool()
            {
                var result = Read("dotnet", "gitversion");
                return JsonConvert.DeserializeObject<GitVersion>(result.Trim());
            }
            GitVersion ReadGitVersion()
            {
                if (Environment.GetEnvironmentVariable("TF_BUILD") != null)
                {
                    var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var cacheFileName = Path.Combine(directory, "gitversion.cache.json");
                    if (File.Exists(cacheFileName))
                    {
                        Console.WriteLine("Reading GitVersion from CacheLocation: '{0}'", cacheFileName);
                        return JsonConvert.DeserializeObject<GitVersion>(File.ReadAllText(cacheFileName));
                    }
                    var resultToCache = ReadGitVersionFromTool();
                    Console.WriteLine("Storing GitVersion to CacheLocation: '{0}'", cacheFileName);
                    File.WriteAllText(cacheFileName, JsonConvert.SerializeObject(resultToCache, Formatting.Indented));
                    return resultToCache;
                }
                return ReadGitVersionFromTool();
            }

            try
            {
                return ReadGitVersion();
            }
            catch (SimpleExec.NonZeroExitCodeException)
            {
                //Can't find dotnet gitversion, assuming tools are not installed
                Run("dotnet", "tool restore");
                return ReadGitVersion();
            }
        });

        public static GitVersion Current => _Current.Value;

        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public string PreReleaseTag { get; set; }
        public string PreReleaseTagWithDash { get; set; }
        public string PreReleaseLabel { get; set; }
        public int? PreReleaseNumber { get; set; }
        public int? WeightedPreReleaseNumber { get; set; }
        public string BuildMetaData { get; set; }
        public string BuildMetaDataPadded { get; set; }
        public string FullBuildMetaData { get; set; }
        public string MajorMinorPatch { get; set; }
        public string SemVer { get; set; }
        public string LegacySemVer { get; set; }
        public string LegacySemVerPadded { get; set; }
        public string AssemblySemVer { get; set; }
        public string AssemblySemFileVer { get; set; }
        public string FullSemVer { get; set; }
        public string InformationalVersion { get; set; }
        public string BranchName { get; set; }
        public string Sha { get; set; }
        public string ShortSha { get; set; }
        public string NuGetVersionV2 { get; set; }
        public string NuGetVersion { get; set; }
        public string NuGetPreReleaseTagV2 { get; set; }
        public string NuGetPreReleaseTag { get; set; }
        public string VersionSourceSha { get; set; }
        public int CommitsSinceVersionSource { get; set; }
        public string CommitsSinceVersionSourcePadded { get; set; }
        public string CommitDate { get; set; }
    }

}
