using Microsoft.Xna.Framework;

namespace VelcroPhysics.Collision.Narrowphase
{
    /// <summary>
    /// Reference face used for clipping
    /// </summary>
    public struct ReferenceFace
    {
        public int i1, i2;

        public Vector2 v1, v2;

        public Vector2 normal;

        public Vector2 sideNormal1;
        public float sideOffset1;

        public Vector2 sideNormal2;
        public float sideOffset2;
    }
}