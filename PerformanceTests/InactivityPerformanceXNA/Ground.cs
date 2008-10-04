using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace InactivityPerformanceXNA
{
    public class Ground
    {
        private Body _body;
        private Geom _geom;

        private const float _height = 25f;
        private const float _width = 800f;

        public Ground(Vector2 position)
        {
            _body = BodyFactory.Instance.CreateRectangleBody(Globals.Physics, _width, _height, 1);
            _body.Position = position;
            _body.IsStatic = true;

            _geom = GeomFactory.Instance.CreateRectangleGeom(Globals.Physics, _body, _width, _height);
            _geom.FrictionCoefficient = 1;
        }
    }
}