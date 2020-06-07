using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static System.Console;

namespace Tasty.Build.Helpers
{
    public class XenialVersionInfo
    {
        public static string VersionSuffix => GitVersion.Current.PreReleaseNumber.HasValue
            ? $"-z{GitVersion.Current.PreReleaseTagWithDash}"
            : string.Empty;

        public void PrintVersion()
        {
            WriteLine();
            WriteLine();
            WriteLine($"BranchName: '{GitVersion.Current.BranchName}'");
            WriteLine($"Sha: '{GitVersion.Current.Sha}'");
            WriteLine($"VersionSuffix: '{VersionSuffix}'");
            WriteLine($"PreReleaseTag: '{GitVersion.Current.PreReleaseTag}'");
            WriteLine($"PreReleaseTagWithDash: '{GitVersion.Current.PreReleaseTagWithDash}'");
            WriteLine();
            WriteLine($"Version: '{Version}'");
            WriteLine($"VersionSuffix: '{VersionSuffix}'");
            WriteLine($"AssemblyVersion: '{AssemblyVersion}'");
            WriteLine($"InformationalVersion: '{InformationalVersion}'");
            WriteLine();
            WriteLine($"##vso[build.updatebuildnumber]{GitVersion.Current.NuGetVersionV2}");
            WriteLine();
            WriteLine();
        }

        public string Version =>
            $"{GitVersion.Current.MajorMinorPatch}{VersionSuffix}";

        public string InformationalVersion =>
            $"{Version} {DateTime.Now} Branch.{GitVersion.Current.BranchName}.Sha.{GitVersion.Current.Sha}";

        public string AssemblyVersion =>
            $"{GitVersion.Current.MajorMinorPatch}.{GitVersion.Current.CommitsSinceVersionSource}";

        public string Publisher =>
            "CN=Xenial, O=Xenial, L=Graz, S=Styria, C=Austria";
    }
}
