using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerXNAPhysics {
    public abstract class PhysicsSimulatorBase {
        protected Vector2 _gravity = Vector2.Zero;

        public Vector2 Gravity {
            get { return _gravity; }
            set { _gravity = value; }
        }

        public abstract void Update(float dt);
    }
}
