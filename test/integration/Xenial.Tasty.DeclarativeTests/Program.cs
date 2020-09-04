using System;

using Xenial;
using Xenial.Delicious.Plugins;
using Xenial.Delicious.Scopes;

var scope = new TastyScope()
    .UseConsoleReporter()
    .UseRemoteReporter()
    .UseNamedPipesTransport();

var group = scope.Describe("I'm a group", () => { });

group.It("with an test case", () => true);

return await scope.Run(args);
