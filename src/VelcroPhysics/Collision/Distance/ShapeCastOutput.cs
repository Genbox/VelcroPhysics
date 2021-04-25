using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Collision.Distance
{
    /// <summary>Output results for b2ShapeCast</summary>
    public struct ShapeCastOutput
    {
        public Vector2 Point;
        public Vector2 Normal;
        public float Lambda;
        public int Iterations;
    }
}