using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using Xenial.Delicious.Commanders;
using Xenial.Delicious.Reporters;

namespace Xenial
{
    public static class Commander
    {
        public static IAsyncEnumerable<TestCaseResult> TasteProcess(
            Uri connectionString,
            string command,
            string arguments = "",
            string? workingDirectory = null,
            string? windowsName = null,
            string? windowsArgs = null,
            Action<IDictionary<string, string>>? configureEnvironment = null,
            Action<TastyProcessCommander>? configureCommander = null)
        {
            using var commander = new TastyProcessCommander(
                connectionString,
                new Func<ProcessStartInfo>(() =>
                    ProcessStartInfoHelper.Create(
                        command,
                        arguments,
                        workingDirectory,
                        windowsName: windowsName,
                        windowsArgs: windowsArgs,
                        configureEnvironment: configureEnvironment
                )));

            configureCommander?.Invoke(commander);

            return commander.Run();
        }

        //public static IAsyncEnumerable<TestCaseResult> TasteProcess(string scheme, string command, string arguments)
        //{
        //    using var commander = new TastyProcessCommander(connectionString, new Func<ProcessStartInfo>(() => ProcessStartInfoHelper.Create(command, arguments));
        //    return commander.Run();
        //}

        //public static IAsyncEnumerable<TestCaseResult> TasteProcess(string command, string arguments)
        //{
        //    using var commander = new TastyProcessCommander(connectionString, new Func<ProcessStartInfo>(() => ProcessStartInfoHelper.Create(command, arguments));
        //    return commander.Run();
        //}
    }
}
