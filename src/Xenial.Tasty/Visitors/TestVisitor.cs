using System;
using System.Collections.Generic;
using System.Linq;

using Xenial.Delicious.Scopes;
using Xenial.Delicious.Execution;
using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Visitors
{
    internal static class TestIterator
    {
        static internal IEnumerable<IExecutable> Iterate(this TastyScope scope)
        {
            static IEnumerable<IExecutable> IterateNode(IExecutable executable)
            {
                if (executable is TestGroup testGroup)
                {
                    yield return testGroup;
                    foreach (var childExecutable in testGroup.Executors)
                    {
                        foreach (var childNode in IterateNode(childExecutable))
                        {
                            yield return childNode;
                        }
                    }
                }
                else if (executable is TestCase testCase)
                {
                    yield return testCase;
                }
                else
                {
                    yield return executable;
                }
            }

            foreach (var executable in scope.RootExecutors)
            {
                foreach (var node in IterateNode(executable))
                {
                    yield return node;
                }
            }
        }

        internal static IEnumerable<IExecutable> Children(this IExecutable executable)
        {
            if (executable is TestGroup group)
            {
                foreach (var test in group.Executors)
                {
                    yield return test;
                }
            }
            else
            {
                yield return executable;
            }
        }

        internal static IEnumerable<IExecutable> Parents(this IExecutable executable)
        {
            if (executable is TestCase test)
            {
                if (test.Group == null)
                {
                    yield break;
                }
                yield return test.Group;
                foreach (var group in Parents(test.Group))
                {
                    yield return group;
                }
            }
            if (executable is TestGroup testGroup)
            {
                if (testGroup != null && testGroup.ParentGroup != null)
                {
                    yield return testGroup.ParentGroup;
                }
            }
        }
    }

    internal sealed class ForceTestVisitor
    {
        internal static void MarkTestsAsForced(TastyScope scope)
        {
            var nodes = scope.Iterate().ToList();
            var nodesWithForce = nodes
                .OfType<IForceAble>()
                .Where(node => node.IsForced != null)
                .ToList();

            if (nodesWithForce.Count > 0)
            {

                foreach (var forcedNode in nodesWithForce)
                {
                    if (forcedNode is IExecutable executableForcedNode)
                    {
                        var outcome = forcedNode.IsForced?.Invoke();
                        foreach (var forcedChildNode in executableForcedNode.Children().OfType<IForceAble>())
                        {
                            if (forcedChildNode is IExecutable executable)
                            {
                                foreach (var parent in executable.Parents().OfType<IForceAble>())
                                {
                                    if (parent is TestCase parentTest)
                                    {
                                        if (outcome.HasValue)
                                        {
                                            parentTest.IsForced = () => outcome.Value;
                                        }
                                    }
                                    else if (parent is TestGroup parentGroup)
                                    {
                                        if (outcome.HasValue)
                                        {
                                            parentGroup.IsForced = () => outcome.Value;
                                        }
                                    }
                                }
                            }
                            if (forcedChildNode is TestCase test)
                            {
                                if (outcome.HasValue)
                                {
                                    test.IsForced = () => outcome.Value;
                                }
                            }
                            else if (forcedChildNode is TestGroup group)
                            {
                                if (outcome.HasValue)
                                {
                                    group.IsForced = () => outcome.Value;
                                }
                            }
                        }
                    }
                }
                foreach (var unforcedNodes in scope.Iterate().OfType<IForceAble>().Where(m => m.IsForced == null))
                {
                    if (unforcedNodes is TestCase test)
                    {
                        test.IsForced = () => false;
                    }
                    else if (unforcedNodes is TestGroup group)
                    {
                        group.IsForced = () => false;
                    }
                }
            }
            else
            {
                var nodes2 = scope.Iterate();
                var nodesWithForce2 = nodes2
                    .OfType<IForceAble>()
                    .Where(node => node.IsForced == null)
                    .ToList();
                foreach (var node in nodesWithForce2)
                {
                    if (node is TestCase test)
                    {
                        test.IsForced = () => true;
                    }
                    if (node is TestGroup group)
                    {
                        group.IsForced = () => true;
                    }
                }
            }
        }
    }
}