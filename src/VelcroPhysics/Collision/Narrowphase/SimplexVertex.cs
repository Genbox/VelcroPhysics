using Microsoft.Xna.Framework;

namespace VelcroPhysics.Collision.Narrowphase
{
    internal struct SimplexVertex
    {
        /// <summary>
        /// Barycentric coordinate for closest point
        /// </summary>
        public float A;

        /// <summary>
        /// wA index
        /// </summary>
        public int IndexA;

        /// <summary>
        /// wB index
        /// </summary>
        public int IndexB;

        /// <summary>
        /// wB - wA
        /// </summary>
        public Vector2 W;

        /// <summary>
        /// Support point in proxyA
        /// </summary>
        public Vector2 WA;

        /// <summary>
        /// Support point in proxyB
        /// </summary>
        public Vector2 WB;
    }
}