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

            await FetchTags();

            var tags = await ListTags();
            var versions = await ParseTags(tags);
            var maxVersion = await MaxVersion(versions);
        }

        static async Task<bool> ConfirmBranch()
        {
            var currentBranch = await ReadAsync("git", "branch --show-current");
            if (!currentBranch.Equals("master", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.Write($"\tThe current branch you are working on is not ");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("master");
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

        private static async Task FetchTags()
        {
            Header("Fetching latest tags");
            var fetchResult = await ReadAsync("git", "pull --tags");
            LogVerbose(fetchResult);
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
