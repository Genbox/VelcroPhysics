using System;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Collisions
{
    public struct Feature
    {
        public float Distance; // = float.MaxValue;
        public Vector2 Normal; // = Vector2.Zero;
        public Vector2 Position; // = Vector2.Zero;

        public Feature(Vector2 position)
        {
            Position = position;
            Normal = new Vector2(0, 0);
            Distance = float.MaxValue;
        }

        public Feature(Vector2 position, Vector2 normal, Single distance)
        {
            Position = position;
            Normal = normal;
            Distance = distance;
        }
    }
}