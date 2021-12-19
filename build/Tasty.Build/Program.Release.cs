using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static SimpleExec.Command;

namespace Tasty.Build
{
    static partial class Program
    {
        enum VersionIncrement
        {
            Patch,
            Minor,
            Major
        }

        public static async Task Release()
        {
            if (!await ConfirmBranch()) return;

            await PullChanges();
            await FetchTags();

            var tags = await ListTags();
            var versions = await ParseTags(tags);
            var maxVersion = await MaxVersion(versions);
            var versionIncrement = await AskVersion(maxVersion);
            if (versionIncrement.HasValue)
            {
                var nextVersion = NextVersion(maxVersion, versionIncrement.Value);
                var increment = ConfirmVersion(nextVersion);

                if (increment)
                {
                    await TagVersion(nextVersion);
                    await PushTags();
                }
            }
            else
            {
                Console.WriteLine("\tExiting...");
            }
        }

        private static bool ConfirmVersion(Version nextVersion)
        {
            Header("Confirm version");

            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.Write($"\tThe next version will be ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ToSemVer(nextVersion));

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\tDo you want to continue? (y/N)");

            if (Console.ReadKey().Key != ConsoleKey.Y)
            {
                Console.WriteLine("\tNo. Exiting...");
                return false;
            }

            Console.WriteLine("\tYes. Continuing...");
            Console.WriteLine();
            return true;
        }

        static async Task<bool> ConfirmBranch()
        {
            var currentBranch = (await ReadAsync("git", "branch --show-current")).Trim();
            if (!currentBranch.Equals("main", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.Write($"\tThe current branch you are working on is not ");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("main");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($" actually it is ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(currentBranch);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\tDo you want to continue? (y/N)");

                if (Console.ReadKey().Key != ConsoleKey.Y)
                {
                    Console.WriteLine("\tNo. Exiting...");
                    return false;
                }

                Console.WriteLine("\tYes. Continuing...");
                Console.WriteLine();
            }
            return true;
        }

        private static async Task PullChanges()
        {
            Header("Fetching latest changes");
            await RunAsync("git", "pull");
        }

        private static async Task FetchTags()
        {
            Header("Fetching latest tags");
            await RunAsync("git", "pull --tags");
        }

        private static async Task<IEnumerable<string>> ListTags()
        {
            Header("List tags");
            var tags = await ReadAsync("git", "tag");
            LogVerbose(tags);
            return tags.Split("\n");
        }

        static Task<IEnumerable<Version>> ParseTags(IEnumerable<string> tags)
        {
            Header("Parse versions");
            IEnumerable<string> CollectVersion(char versionSelector)
                => tags.Where(v => v.StartsWith(versionSelector)).Select(v => v.TrimStart(versionSelector));

            var versionsWithV = CollectVersion('v').Concat(CollectVersion('V'));

            var versions = versionsWithV
                .Select(v =>
                {
                    if (Version.TryParse(v, out var version))
                    {
                        return version;
                    }
                    return null;
                })
                .Where(r => r != null)
                .OrderBy(r => r)
                .ToList();

            LogVerbose(string.Join("\n", versions));

            return Task.FromResult(versions.AsEnumerable());
        }
        static Task<Version> MaxVersion(IEnumerable<Version> versions)
        {
            Header("Maximum version");
            var version = versions.Max();
            LogVerbose(version?.ToString());
            return Task.FromResult(version);
        }

        static Task<VersionIncrement?> AskVersion(Version maxVersion)
        {
            Header($"Current version is {maxVersion}");

            Tabify("Which increment would you like to make?");

            void WriteInfo(string shortCut, VersionIncrement increment)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"\t({shortCut}) ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($"{increment} - {ToSemVer(NextVersion(maxVersion, increment))}");
                Console.WriteLine();
            }

            WriteInfo("m", VersionIncrement.Major);
            WriteInfo("i", VersionIncrement.Minor);
            WriteInfo("p", VersionIncrement.Patch);

            var key = Console.ReadKey().Key;

            VersionIncrement? result = key switch
            {
                ConsoleKey.M => VersionIncrement.Major,
                ConsoleKey.I => VersionIncrement.Minor,
                ConsoleKey.P => VersionIncrement.Patch,
                _ => null
            };

            return Task.FromResult(result);
        }

        static async Task TagVersion(Version nextVersion)
        {
            var tag = $"v{ToSemVer(nextVersion)}";
            Header($"Tagging {tag}");

            await RunAsync("git", $"tag {tag}");
        }

        static async Task PushTags()
        {
            Header($"Pushing tags");

            await RunAsync("git", $"push --tags");
        }

        private static Version NextVersion(Version version, VersionIncrement increment)
            => increment switch
            {
                VersionIncrement.Major => new Version(version.Major + 1, 0, 0, 0),
                VersionIncrement.Minor => new Version(version.Major, version.Minor + 1, 0, 0),
                VersionIncrement.Patch => new Version(version.Major, version.Minor, version.Build + 1, 0),
                _ => version
            };

        private static string ToSemVer(Version version)
            => $"{version.Major}.{version.Minor}.{version.Build}";

        private static void Header(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            var msg = $"\t{message}...";
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\t{new string('-', msg.Length)}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }

        private static void LogVerbose(string tags)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(Tabify(tags));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }

    }
}
