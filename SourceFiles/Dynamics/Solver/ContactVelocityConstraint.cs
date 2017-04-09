using Microsoft.Xna.Framework;
using VelcroPhysics.Common;

namespace VelcroPhysics.Dynamics.Contacts
{
    public sealed class ContactVelocityConstraint
    {
        public int contactIndex;
        public float friction;
        public int indexA;
        public int indexB;
        public float invIA, invIB;
        public float invMassA, invMassB;
        public Mat22 K;
        public Vector2 normal;
        public Mat22 normalMass;
        public int pointCount;
        public VelocityConstraintPoint[] points = new VelocityConstraintPoint[Settings.MaxManifoldPoints];
        public float restitution;
        public float tangentSpeed;

        public ContactVelocityConstraint()
        {
            for (int i = 0; i < Settings.MaxManifoldPoints; i++)
            {
                points[i] = new VelocityConstraintPoint();
            }
        }
    }
}