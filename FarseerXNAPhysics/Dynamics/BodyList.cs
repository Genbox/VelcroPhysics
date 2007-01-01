using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    internal class BodyList : List<Body> {
        internal static bool IsDisposed(Body a) {
            return a.IsDisposed;
        }
    }
}
