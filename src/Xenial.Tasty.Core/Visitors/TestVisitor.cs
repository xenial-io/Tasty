﻿using System;
using System.Collections.Generic;
using System.Linq;
using Xenial.Delicious.Execution;
using Xenial.Delicious.Metadata;

namespace Xenial.Delicious.Visitors
{
    internal static class TestIterator
    {
        internal static IEnumerable<IExecutable> Descendants(this TestGroup root)
        {
            var nodes = new Queue<IExecutable>(new[] { root });
            while (nodes.Any())
            {
                var node = nodes.Dequeue();
                yield return node;
                if (node is TestGroup group)
                {
                    foreach (var n in group.Executors)
                    {
                        nodes.Enqueue(n);
                    }
                }
            }
        }
    }
}
