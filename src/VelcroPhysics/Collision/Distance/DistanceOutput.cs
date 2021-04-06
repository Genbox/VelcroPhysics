using Microsoft.Xna.Framework;

namespace VelcroPhysics.Collision.Distance
{
    /// <summary>
    /// Output for Distance.ComputeDistance().
    /// </summary>
    public struct DistanceOutput
    {
        public float Distance;

        /// <summary>
        /// Number of GJK iterations used
        /// </summary>
        public int Iterations;

        /// <summary>
        /// Closest point on shapeA
        /// </summary>
        public Vector2 PointA;

        /// <summary>
        /// Closest point on shapeB
        /// </summary>
        public Vector2 PointB;
    }
}