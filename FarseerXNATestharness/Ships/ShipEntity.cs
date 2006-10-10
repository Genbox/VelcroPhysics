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

namespace FarseerGames.FarseerXNATestharness.Ships {
    public class ShipEntity : IEntity{
        private PolygonRigidBody rigidBody;
        private float thrustMagnitude = 20;
        private float turnMagnitude = 80;

        public ShipEntity(PhysicsSimulator physicsSimulator){
            Vertices vertices = new Vertices();
            vertices.Add(new Vector2(-2, -27));
            vertices.Add(new Vector2(-2, -22));
            vertices.Add(new Vector2(-11, -13));
            vertices.Add(new Vector2(-11, 2));
            vertices.Add(new Vector2(-11, 21));
            vertices.Add(new Vector2(-5, 28));
            vertices.Add(new Vector2(4, 28));
            vertices.Add(new Vector2(9, 21));
            vertices.Add(new Vector2(9, 2));
            vertices.Add(new Vector2(9, -13));
            vertices.Add(new Vector2(2, -22));
            vertices.Add(new Vector2(2, -27));
            rigidBody = new PolygonRigidBody(.2f, vertices);

            //setup some default physics parameters for all rigid body sprites
            rigidBody.RotationalDragCoefficient = 50;
            rigidBody.LinearDragCoefficient = .001f;
            rigidBody.FrictionCoefficient = .1f;
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
            set {rigidBody.Mass = Mass; }
        }

        public void Thrust() {
            Vector2 thrust = new Vector2(-rigidBody.BodyMatrix.Up.X, -rigidBody.BodyMatrix.Up.Y);
            thrust *= thrustMagnitude;
            rigidBody.ApplyForceAtLocalPoint(thrust, new Vector2(0, 0));
        }

        public void TurnLeft() {
            rigidBody.ApplyTorque(-turnMagnitude);
        }

        public void TurnRight() {
            rigidBody.ApplyTorque(turnMagnitude);
        }

        public void ApplyTorque(float torque) {
            rigidBody.ApplyTorque(torque);
        }
    }
}
