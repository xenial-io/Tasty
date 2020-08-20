using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xenial.Delicious.Commands;
using Xenial.Delicious.Execution;
using Xenial.Delicious.Metadata;
using Xenial.Delicious.Plugins;
using Xenial.Delicious.Remote;
using Xenial.Delicious.Reporters;

namespace Xenial.Delicious.Scopes
{
    public class TastyScope : TestGroup
    {
        public bool ClearBeforeRun { get; set; } = true;
        public bool LoadPlugins { get; set; }

        private readonly List<AsyncTestReporter> Reporters = new List<AsyncTestReporter>();
        internal readonly List<AsyncTestSummaryReporter> SummaryReporters = new List<AsyncTestSummaryReporter>();
        internal IsInteractiveRun IsInteractiveRunHook { get; set; } = TastyRemoteDefaults.IsInteractiveRun;
        internal ConnectToRemote ConnectToRemoteRunHook { get; set; } = TastyRemoteDefaults.AttachToStream;
        internal List<TransportStreamFactoryFunctor> TransportStreamFactories { get; } = new List<TransportStreamFactoryFunctor>();
        internal Dictionary<string, TastyCommand> Commands { get; } = new Dictionary<string, TastyCommand>();

        internal TestGroup CurrentGroup { get; set; }

        public TastyScope()
        {
            Executor = () => Task.FromResult(true);
            CurrentGroup = this;
            RegisterCommand(ExecuteTestsCommand.Register);
            RegisterCommand(ExitCommand.Register);
        }

        public TastyScope RegisterCommand(string name, Func<RuntimeContext, Task> command, string? description = null, bool isDefault = false)
        {
            _ = string.IsNullOrEmpty(name) ? throw new ArgumentNullException(nameof(command)) : string.Empty;
            _ = command ?? throw new ArgumentNullException(nameof(command));

            if (isDefault)
            {
                foreach (var cmd in Commands.Values)
                {
                    cmd.IsDefault = false;
                }
            }

            Commands[name] = new TastyCommand(name, command, description, isDefault);
            return this;
        }

        public TastyScope RegisterCommand(Func<(string name, Func<RuntimeContext, Task> command)> commandRegistration)
        {
            _ = commandRegistration ?? throw new ArgumentNullException(nameof(commandRegistration));
            var (name, command) = commandRegistration();
            return RegisterCommand(name, command, string.Empty, false);
        }

        public TastyScope RegisterCommand(Func<(string name, Func<RuntimeContext, Task> command, string? description)> commandRegistration)
        {
            _ = commandRegistration ?? throw new ArgumentNullException(nameof(commandRegistration));
            var (name, command, description) = commandRegistration();
            return RegisterCommand(name, command, description, false);
        }

        public TastyScope RegisterCommand(Func<(string name, Func<RuntimeContext, Task> command, string? description, bool? isDefault)> commandRegistration)
        {
            _ = commandRegistration ?? throw new ArgumentNullException(nameof(commandRegistration));
            var (name, command, description, isDefault) = commandRegistration();
            return RegisterCommand(name, command, description, isDefault ?? false);
        }

        public TastyScope RegisterReporter(AsyncTestReporter reporter)
        {
            _ = reporter ?? throw new ArgumentNullException(nameof(reporter));
            Reporters.Add(reporter);
            return this;
        }

        public TastyScope RegisterReporter(AsyncTestSummaryReporter summaryReporter)
        {
            _ = summaryReporter ?? throw new ArgumentNullException(nameof(summaryReporter));
            SummaryReporters.Add(summaryReporter);
            return this;
        }

        public TastyScope RegisterTransport(TransportStreamFactoryFunctor transportStreamFactory)
        {
            _ = transportStreamFactory ?? throw new ArgumentNullException(nameof(transportStreamFactory));
            TransportStreamFactories.Add(transportStreamFactory);
            return this;
        }

        public Task Report(TestCase test)
            => Task.WhenAll(Reporters.Select(async reporter => await reporter(test).ConfigureAwait(false)).ToArray());

        public TestGroup Describe(string name, Action action)
        {
            var group = new TestGroup
            {
                Name = name,
                Executor = () =>
                {
                    action.Invoke();
                    return Task.FromResult(true);
                },
            };
            AddToGroup(group);
            return group;
        }

        public TestGroup Describe(string name, Func<Task> action)
        {
            var group = new TestGroup
            {
                Name = name,
                Executor = async () =>
                {
                    await action().ConfigureAwait(false);
                    return true;
                },
            };
            AddToGroup(group);
            return group;
        }

        public TestGroup FDescribe(string name, Action action)
            => Describe(name, action)
                .Forced(() => true);

        public TestGroup FDescribe(string name, Func<Task> action)
           => Describe(name, action)
               .Forced(() => true);

        void AddToGroup(TestGroup group)
        {
            group.ParentGroup = CurrentGroup;
            CurrentGroup.Executors.Add(group);
        }

        public TestCase It(string name, Action action)
        {
            var test = new TestCase
            {
                Name = name,
                Executor = () =>
                {
                    action.Invoke();
                    return Task.FromResult(true);
                },
            };
            AddToGroup(test);
            return test;
        }

        public TestCase It(string name, Func<(bool success, string message)> action)
        {
            var test = new TestCase
            {
                Name = name,
            };
            test.Executor = () =>
            {
                var (success, message) = action.Invoke();
                test.AdditionalMessage = message;
                return Task.FromResult(success);
            };
            AddToGroup(test);
            return test;
        }

        public TestCase It(string name, Func<bool> action)
        {
            var test = new TestCase
            {
                Name = name,
                Executor = () =>
                {
                    var result = action.Invoke();
                    return Task.FromResult(result);
                },
            };
            AddToGroup(test);
            return test;
        }

        public TestCase It(string name, Func<Task> action)
        {
            var test = new TestCase
            {
                Name = name,
                Executor = async () =>
                {
                    await action().ConfigureAwait(false);
                    return true;
                },
            };
            AddToGroup(test);
            return test;
        }

        public TestCase It(string name, Func<Task<(bool success, string message)>> action)
        {
            var test = new TestCase
            {
                Name = name,
            };
            test.Executor = async () =>
            {
                var (success, message) = await action().ConfigureAwait(false);
                test.AdditionalMessage = message;
                return success;
            };
            AddToGroup(test);
            return test;
        }

        public TestCase It(string name, Executable action)
        {
            var test = new TestCase
            {
                Name = name,
                Executor = action,
            };
            AddToGroup(test);
            return test;
        }

        public TestCase FIt(string name, Action action)
            => It(name, action)
                .Forced(() => true);

        public TestCase FIt(string name, Func<Task> action)
            => It(name, action)
                .Forced(() => true);

        public TestCase FIt(string name, Func<bool> action)
           => It(name, action)
               .Forced(() => true);

        public TestCase FIt(string name, Func<Task<bool>> action)
            => It(name, action)
                .Forced(() => true);

        public TestCase FIt(string name, Func<Task<(bool result, string message)>> action)
            => It(name, action)
                .Forced(() => true);

        public TestCase FIt(string name, Func<(bool result, string message)> action)
           => It(name, action)
               .Forced(() => true);

        void AddToGroup(TestCase test)
        {
            test.Group = CurrentGroup;
            CurrentGroup.Executors.Add(test);
        }

        public void BeforeEach(Func<Task> action)
        {
            var hook = new TestBeforeEachHook(async () =>
            {
                await action().ConfigureAwait(false);
                return true;
            }, null);

            AddToGroup(hook);
        }

        public void BeforeEach(Action action)
        {
            var hook = new TestBeforeEachHook(() =>
            {
                action();
                return Task.FromResult(true);
            }, null);

            AddToGroup(hook);
        }

        public void AfterEach(Func<Task> action)
        {
            var hook = new TestAfterEachHook(async () =>
            {
                await action().ConfigureAwait(false);
                return true;
            }, null);

            AddToGroup(hook);
        }

        public void AfterEach(Action action)
        {
            var hook = new TestAfterEachHook(() =>
            {
                action();
                return Task.FromResult(true);
            }, null);

            AddToGroup(hook);
        }

        void AddToGroup(TestBeforeEachHook hook)
        {
            hook.Group = CurrentGroup;
            CurrentGroup.BeforeEachHooks.Add(hook);
        }

        void AddToGroup(TestAfterEachHook hook)
        {
            hook.Group = CurrentGroup;
            CurrentGroup.AfterEachHooks.Add(hook);
        }

        public async Task<int> Run(string[] args)
        {
            _ = args ?? throw new ArgumentNullException(nameof(args));
            if (LoadPlugins)
            {
                await PluginLoader.LoadPlugins(this).ConfigureAwait(false);
            }
            var executor = new TestExecutor(this);
            return await executor.Execute().ConfigureAwait(false);
        }

        public Task<int> Run() => Run(Array.Empty<string>());
    }
}
