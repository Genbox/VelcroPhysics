using DemoBaseXNA.DrawingSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DemoShare
{
    public class RectanglePlatform
    {
        private Color _borderColor;
        private Color _color;
        private int _width;
        private int _height;
        private Body _platformBody;
        private Geom _platformGeom;
        private int _collisionGroup;
        private RectangleBrush _platformBrush;
        private Vector2 _position;

        public RectanglePlatform(int width, int height, Vector2 position, Color color, Color borderColor,
                                 int collisionGroup)
        {
            _width = width;
            _height = height;
            _position = position;
            _color = color;
            _borderColor = borderColor;
            _collisionGroup = collisionGroup;
        }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _platformBrush = new RectangleBrush(_width, _height, _color, _borderColor);
            _platformBrush.Load(graphicsDevice);

            //use the body factory to create the physics body
            _platformBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _width, _height, 1);
            _platformBody.IsStatic = true;
            _platformBody.Position = _position;

            _platformGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _platformBody, _width, _height);
            _platformGeom.CollisionGroup = 100;
            _platformGeom.CollisionGroup = _collisionGroup;
            _platformGeom.FrictionCoefficient = 1;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < 4; i++)
            {
                _platformBrush.Draw(spriteBatch, _platformBody.Position, _platformBody.Rotation);
            }
        }
    }
}