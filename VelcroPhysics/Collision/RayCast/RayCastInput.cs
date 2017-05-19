using Microsoft.Xna.Framework;

namespace VelcroPhysics.Collision.RayCast
{
    /// <summary>
    /// Ray-cast input data.
    /// </summary>
    public struct RayCastInput
    {
        /// <summary>
        /// The ray extends from p1 to p1 + maxFraction * (p2 - p1).
        /// If you supply a max fraction of 1, the ray extends from p1 to p2.
        /// A max fraction of 0.5 makes the ray go from p1 and half way to p2.
        /// </summary>
        public float MaxFraction;

        /// <summary>
        /// The starting point of the ray.
        /// </summary>
        public Vector2 Point1;

        /// <summary>
        /// The ending point of the ray.
        /// </summary>
        public Vector2 Point2;
    }
}