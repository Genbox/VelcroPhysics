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
    public class PolygonEntity {
        private PolygonRigidBody rigidBody;

        public PolygonEntity(Vertices vertices, PhysicsSimulator physicsSimulator) {

            rigidBody = new PolygonRigidBody(1, vertices);

            //setup some default physics parameters for all rigid body sprites
            rigidBody.RotationalDragCoefficient = 50;
            rigidBody.LinearDragCoefficient = .001f;
            rigidBody.FrictionCoefficient = .8f;
            rigidBody.RestitutionCoefficient = .1f;

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
    }
}
