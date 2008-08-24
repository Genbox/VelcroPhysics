using System;
using System.Collections.Generic;
using System.Text;


using FarseerGames.FarseerPhysics.Dynamics;

namespace FarseerGames.FarseerPhysics.Collisions {
    public interface IBroadPhaseCollider { 
        void ProcessRemovedGeoms();
        void ProcessDisposedGeoms();
        void Add(Geom geom);
        void Update();
    }
}
