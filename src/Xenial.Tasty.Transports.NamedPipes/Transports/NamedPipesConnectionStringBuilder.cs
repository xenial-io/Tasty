using System;

namespace Xenial.Delicious.Transports
{
    public static class NamedPipesConnectionStringBuilder
    {
        public static string Scheme => Uri.UriSchemeNetPipe;
        public static Uri CreateNewConnection()
            => new Uri($"{Scheme}://localhost/{Guid.NewGuid()}");
    }
}
