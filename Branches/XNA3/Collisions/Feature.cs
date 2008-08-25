using System;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Collisions
{
    public struct Feature
    {
        public float Distance;
        public Vector2 Normal;
        public Vector2 Position;

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