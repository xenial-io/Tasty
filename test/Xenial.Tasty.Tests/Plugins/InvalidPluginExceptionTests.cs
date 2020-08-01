using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Shouldly;
using Xenial.Delicious.Plugins;
using static Xenial.Tasty;

namespace Xenial.Delicious.Tests.Plugins
{
    public static class InvalidPluginExceptionTests
    {
        public static void InvalidPluginException()
            => Describe(nameof(InvalidPluginException), () =>
            {
                const string message = "Invalid Load";
                var attribute = new TastyPluginAttribute(typeof(InvalidPluginExceptionTests), nameof(InvalidPluginException));
                var exception = new InvalidPluginException(message, attribute, new Exception());

                It($"should provide {nameof(exception.TastyPluginType)}",
                    () => exception.TastyPluginType == typeof(InvalidPluginExceptionTests).FullName
                );

                It($"should provide {nameof(exception.TastyPluginEntryPoint)}",
                    () => exception.TastyPluginEntryPoint == nameof(InvalidPluginException)
                );

                Describe("should serialize and keep context", () =>
                {
                    var serializedException = DeserializeFromBytes(SerializeToBytes(exception));

                    It($"should provide {nameof(exception.TastyPluginType)}",
                        () => serializedException.TastyPluginType == typeof(InvalidPluginExceptionTests).FullName
                    );

                    It($"should provide {nameof(exception.TastyPluginEntryPoint)}",
                        () => serializedException.TastyPluginEntryPoint == nameof(InvalidPluginException)
                    );
                });

                Describe($"throws {nameof(ArgumentNullException)}", () =>
                {
                    It($"for {nameof(TastyPluginAttribute)}",
                        () => Should.Throw<ArgumentNullException>(
                            () => new InvalidPluginException(string.Empty, null!, null!)
                        )
                    );
                });
            });

        static byte[] SerializeToBytes(InvalidPluginException e)
        {
            using var stream = new MemoryStream();
            new BinaryFormatter().Serialize(stream, e);
            return stream.GetBuffer();
        }

        static InvalidPluginException DeserializeFromBytes(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            return (InvalidPluginException)new BinaryFormatter().Deserialize(stream);
        }
    }
}
