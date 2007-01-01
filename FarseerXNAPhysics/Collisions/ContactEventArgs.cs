using System;
using System.Collections.Generic;
using System.Text;

using FarseerGames.FarseerXNAPhysics.Dynamics;

namespace FarseerGames.FarseerXNAPhysics.Collisions {
    public class ContactEventArgs : EventArgs  {
        private RigidBody _contactedRigidBody;
        private float _seperation;

        public ContactEventArgs(Contact contact) {
            
        }

        
    }
}
