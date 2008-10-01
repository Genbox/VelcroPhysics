using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerPerformanceTest;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace InactivityPerformanceXNA
{
    public class Box
    {
        private Body _body;
        private Geom _geom;
        private Texture2D _texture;
        private const float _height = 25f;
        private const float _width = 25f;

        public Box(Game game, Vector2 position)
        {
            _body = BodyFactory.Instance.CreateRectangleBody(Globals.Physics, _width, _height, 1);
            _body.Position = position;

            // turn automatic deactivation on and specify the minimum velocity.
            // if you are using a object pool which deactivates bodies to reuse them later, you should
            // additionally set "IsAutoIdle" to false when assigning it back to the pool. 
            // Otherwise it could be reactivated by the InactivityController
            _body.IsAutoIdle = true;
            _body.MinimumVelocity = 25;

            _geom = GeomFactory.Instance.CreateRectangleGeom(Globals.Physics, _body, _width, _height);
            _geom.FrictionCoefficient = 1;

            _texture = game.Content.Load<Texture2D>("box");
        }

        public Body Body
        {
            get { return _body; }
        }

        public Texture2D Texture
        {
            get { return _texture; }
        }

        public Vector2 Position
        {
            get { return _body.Position; }
        }

        public Vector2 Center
        {
            get { return new Vector2(_width/2, _height/2); }
        }
    }
}