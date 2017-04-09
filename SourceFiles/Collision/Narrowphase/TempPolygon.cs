using Microsoft.Xna.Framework;

namespace VelcroPhysics.Collision.Narrowphase
{
    /// <summary>
    /// This holds polygon B expressed in frame A.
    /// </summary>
    public class TempPolygon
    {
        public int Count;
        public Vector2[] Normals = new Vector2[Settings.MaxPolygonVertices];
        public Vector2[] Vertices = new Vector2[Settings.MaxPolygonVertices];
    }
}