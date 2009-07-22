#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// Contains info about distance, normal and position.
    /// Used internal in collision detection.
    /// </summary>
    public struct Feature
    {
        public float Distance;
        public Vector2 Normal;
        public Vector2 Position;

        public Feature(ref Vector2 position, ref  Vector2 normal, float distance)
        {
            Position = position;
            Normal = normal;
            Distance = distance;
        }

        //NOTE: There might be a better way to generate the hashcode
        public override int GetHashCode()
        {
            return (int)(Normal.X + Normal.Y + Position.X + Position.Y + Distance);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Feature))
                return false;

            return Equals((Feature)obj);
        }

        /// <summary>
        /// Checks against another Features to see if they are equal.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(Feature other)
        {
            return ((Normal == other.Normal) && (Position == other.Position) && (Distance == other.Distance));
        }

        public bool Equals(ref Feature other)
        {
            return ((Normal == other.Normal) && (Position == other.Position) && (Distance == other.Distance));
        }

        public static bool operator ==(Feature first, Feature second)
        {
            return first.Equals(ref second);
        }

        public static bool operator !=(Feature first, Feature second)
        {
            return !first.Equals(ref second);
        }
    }
}