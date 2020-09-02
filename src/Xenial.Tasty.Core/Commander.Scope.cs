using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Xenial.Delicious.Reporters;
using Xenial.Delicious.Scopes;

namespace Xenial.Delicious.Commanders
{
    public class TastyScopeCommander : TastyRemoteCommander
    {
        public TastyScopeCommander(Uri connectionString, Func<TastyScope> createScope)
            : base(connectionString, () => () =>
            {
                var scope = createScope();
                return scope.Run();
            })
        { }
    }
}
