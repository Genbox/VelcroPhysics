using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    internal class JointList : List<Joint> {
        internal static bool IsDisposed(Joint joint) {
            //if either of the joint's connected bodies are disposed then dispose the joint.
            if (joint.Body1.IsDisposed || joint.Body2.IsDisposed) {
                joint.Dispose();
            }
            return joint.IsDisposed;            
        }
    }
}
