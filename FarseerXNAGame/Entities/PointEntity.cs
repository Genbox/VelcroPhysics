using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerXNAGame.Entities;
using FarseerGames.FarseerXNAGame.Sprites;

using FarseerGames.FarseerXNAPhysics;
using FarseerGames.FarseerXNAPhysics.Collisions;
using FarseerGames.FarseerXNAPhysics.Dynamics;

namespace FarseerGames.FarseerXNAGame.Entities {
    public class PointEntity : IEntity {
        private PointRigidBody rigidBody;

        public PointEntity(PhysicsSimulator physicsSimulator) {

            rigidBody = new PointRigidBody(.1f);

            //setup some default physics parameters for all rigid body sprites
            rigidBody.RotationalDragCoefficient = 50;
            rigidBody.LinearDragCoefficient = .00001f;
            rigidBody.FrictionCoefficient = .01f;
            rigidBody.RestitutionCoefficient = 0f;

            //add rigid body to physics simulator
            physicsSimulator.Add(rigidBody);
        }

        public Vector2 Position {
            get { return rigidBody.Position; }
            set { rigidBody.Position = value; }
        }

        public float Orientation {
            get { return rigidBody.Orientation; }
            set { rigidBody.Orientation = value; }
        }

        public float Mass {
            get { return rigidBody.Mass; }
            set { rigidBody.Mass = Mass; }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            //subclasses can override incase they need to dispose of resources
            //otherwise do nothing.
            if (disposing) {
                rigidBody.Dispose();
            }
        }
    }
}
