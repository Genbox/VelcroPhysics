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
    public class CircleEntity : IEntity {
        private CircleRigidBody rigidBody;

        public CircleEntity(float radius, int edgeCount,  PhysicsSimulator physicsSimulator) {

            rigidBody = new CircleRigidBody(radius, edgeCount, 1f);

            //setup some default physics parameters for all rigid body sprites
            rigidBody.RotationalDragCoefficient = 50;
            rigidBody.LinearDragCoefficient = .001f;
            rigidBody.FrictionCoefficient = .8f;
            rigidBody.RestitutionCoefficient = .91f;

            //add rigid body to physics simulator
            physicsSimulator.Add(rigidBody);
        }

        public RigidBody RigidBody {
            get { return rigidBody; }
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
            if (disposing) {
                rigidBody.Dispose();
            }
        }
    }
}
