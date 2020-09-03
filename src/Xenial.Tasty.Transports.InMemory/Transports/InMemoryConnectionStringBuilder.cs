using System;

namespace Xenial.Delicious.Transports
{
    public static class InMemoryConnectionStringBuilder
    {
        public static string Scheme => "inmem";
        public static Uri CreateNewConnection()
            => new Uri($"{Scheme}://localhost/{Guid.NewGuid()}");
    }
}
