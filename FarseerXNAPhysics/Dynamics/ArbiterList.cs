using System;
using System.Collections.Generic;
using System.Text;

using FarseerGames.FarseerXNAPhysics.Dynamics;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    internal class ArbiterList : List<Arbiter> {

        internal static bool ContactCountEqualsZero(Arbiter a) {
            return a.ContactCount == 0;
        }

        internal static bool ContainsDisposedBody(Arbiter a) {
            return a.ContainsDisposedRigidBody();
        }
    }
}
