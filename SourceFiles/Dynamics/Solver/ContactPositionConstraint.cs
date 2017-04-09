using Microsoft.Xna.Framework;
using VelcroPhysics.Collision;

namespace VelcroPhysics.Dynamics.Contacts
{
    public sealed class ContactPositionConstraint
    {
        public int indexA;
        public int indexB;
        public float invIA, invIB;
        public float invMassA, invMassB;
        public Vector2 localCenterA, localCenterB;
        public Vector2 localNormal;
        public Vector2 localPoint;
        public Vector2[] localPoints = new Vector2[Settings.MaxManifoldPoints];
        public int pointCount;
        public float radiusA, radiusB;
        public ManifoldType type;
    }
}