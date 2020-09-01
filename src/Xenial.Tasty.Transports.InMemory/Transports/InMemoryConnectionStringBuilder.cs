using System;
using System.Collections.Generic;
using System.Text;

using Xenial.Delicious.Plugins;

namespace Xenial.Delicious.Transports
{
    public static class InMemoryConnectionStringBuilder
    {
        public static string Scheme => "inmem";
        public static Uri CreateNewConnection()
            => new Uri($"{Scheme}://localhost/{Guid.NewGuid()}");
    }
}
