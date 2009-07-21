using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DemoShare
{
    public class Box
    {
        private Texture2D _texture;
        private Vector2 _origin;
        private Vector2 _position;
        private int _mass;

        public Box(int width, int height, Vector2 position)
        {
            Width = width;
            Height = height;
            CollidesWith = CollisionCategory.All;
            CollisionCategory = CollisionCategory.All;
            _position = position;
        }

        public Box(int width, int height, Vector2 position, int mass)
        {
            Width = width;
            Height = height;
            CollidesWith = CollisionCategory.All;
            CollisionCategory = CollisionCategory.All;
            _position = position;
            _mass = mass;
        }

        public Body Body { get; private set; }

        public Geom Geom { get; private set; }

        public CollisionCategory CollisionCategory { get; set; }

        public CollisionCategory CollidesWith { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _texture = DrawingSystem.DrawingHelper.CreateRectangleTexture(graphicsDevice, Width, Height, Color.White, Color.Black);
            _origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);

            Body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, Width, Height, 1);
            Body.Position = _position;

            // Enable so that the can idle
            // and set the minimum velocity is 25
            Body.IsAutoIdle = true;
            Body.MinimumVelocity = 25;

            Geom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, Body, Width, Height);
            Geom.CollidesWith = CollidesWith;
            Geom.CollisionCategories = CollisionCategory;
            Geom.FrictionCoefficient = 1;
        }

        public void Draw(SpriteBatch spriteBatch, Color color)
        {
            spriteBatch.Draw(_texture, Body.Position, null, color, Body.Rotation, _origin, 1f, SpriteEffects.None, 0f);
        }
    }
}