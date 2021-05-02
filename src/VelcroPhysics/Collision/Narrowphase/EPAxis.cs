using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Collision.Narrowphase
{
    /// <summary>This structure is used to keep track of the best separating axis.</summary>
    public struct EPAxis
    {
        public Vector2 Normal;
        public int Index;
        public float Separation;
        public EPAxisType Type;
    }
}