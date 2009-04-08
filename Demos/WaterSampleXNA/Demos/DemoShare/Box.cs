using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.SimpleSamples.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.SimpleSamples.Demos.DemoShare
{
    public class Box
    {
        private Color _borderColor;
        private Color _color;
        private int _width;
        private int _height;
        public Body Body;
        public Geom Geom;
        private int _collisionGroup;
        private RectangleBrush _brush;
        private Vector2 _position;
        private float _mass;

        public Box(int width, int height, float mass, Vector2 position, Color color, Color borderColor,
                                 int collisionGroup)
        {
            _width = width;
            _height = height;
            _mass = mass;
            _position = position;
            _color = color;
            _borderColor = borderColor;
            _collisionGroup = collisionGroup;
        }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _brush = new RectangleBrush(_width, _height, _color, _borderColor);
            _brush.Load(graphicsDevice);

            //use the body factory to create the physics body
            Body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _width, _height, _mass);
            Body.Position = _position;

            Geom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, Body, _width, _height);
            Geom.CollisionGroup = 100;
            Geom.CollisionGroup = _collisionGroup;
            Geom.FrictionCoefficient = .2f;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < 4; i++)
            {
                _brush.Draw(spriteBatch, Body.Position, Body.Rotation);
            }
        }
    }
}