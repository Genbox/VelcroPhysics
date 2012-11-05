using Microsoft.Xna.Framework;

namespace FarseerPhysics.Physics.Collisions
{
    public struct Feature
    {
        /// <summary>
        /// Distance from the feature (negative if inside)
        /// </summary>
        public float Distance;

        /// <summary>
        /// Normal of the feature
        /// </summary>
        public Vector2 Normal;

        /// <summary>
        /// Target position on the feature
        /// </summary>
        public Vector2 Position;

        public static Feature Empty
        {
            get
            {
                return new Feature
                           {
                               Distance = float.PositiveInfinity,
                               Normal = Vector2.Zero,
                               Position = Vector2.Zero,
                           };
            }
        }
    }
}