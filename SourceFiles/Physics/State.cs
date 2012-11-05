using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Physics
{
    public struct State
    {
        public Vector2 Force;
        public float Torque;

        public Transform Xf;
        public Sweep Sweep;

        public Vector2 LinearVelocity;
        public float AngularVelocity;

        public bool Awake;

        public void Save(Body body)
        {
            Force = body.Force;
            Torque = body.Torque;
            Xf = body.Xf;
            Sweep = body.Sweep;
            LinearVelocity = body.LinearVelocity;
            AngularVelocity = body.AngularVelocity;
            Awake = body.Awake;
        }

        public void Restore(Body body)
        {
            body.Force = Force;
            body.Torque = Torque;
            body.Xf = Xf;
            body.Sweep = Sweep;
            body.LinearVelocity = LinearVelocity;
            body.AngularVelocity = AngularVelocity;
            body.Awake = Awake;
        }
    }
}