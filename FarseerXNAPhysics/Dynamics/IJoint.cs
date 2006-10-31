using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public interface IJoint {
        void PreStep(float inverseDt);
        void ApplyImpulse();
    }
}
