using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPerformanceTest
{
    public class Ground
    {
        private Body body;
        private Geom geom;

        private float height = 25f;
        private float width = 800f;

        public Ground(Vector2 position)
        {
            body = BodyFactory.Instance.CreateRectangleBody(Globals.Physics, width, height, 1);
            body.Position = position;
            body.IsStatic = true;

            geom = GeomFactory.Instance.CreateRectangleGeom(Globals.Physics, body, width, height);
            geom.FrictionCoefficient = 1;
        }
    }
}