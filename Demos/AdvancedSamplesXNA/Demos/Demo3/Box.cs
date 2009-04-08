using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.AdvancedSamplesXNA.Demos.Demo3
{
    public class Box
    {
        private Body _body;
        private Geom _geom;
        private Texture2D _texture;
        private Vector2 _origin;
        private CollisionCategory _collidesWith = CollisionCategory.All;
        private CollisionCategory _collisionCategory = CollisionCategory.All;
        private Vector2 _position;
        private const int _width = 25;
        private const int _height = 25;

        public Box(Vector2 position)
        {
            _position = position;
        }

        public Body Body
        {
            get { return _body; }
        }

        public Geom Geom
        {
            get { return _geom; }
        }

        public CollisionCategory CollisionCategory
        {
            get { return _collisionCategory; }
            set { _collisionCategory = value; }
        }

        public CollisionCategory CollidesWith
        {
            get { return _collidesWith; }
            set { _collidesWith = value; }
        }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _texture = DrawingSystem.DrawingHelper.CreateRectangleTexture(graphicsDevice, _width, _height, Color.White, Color.Black);
            _origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);

            _body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _width, _height, 1);
            _body.Position = _position;

            // Enable so that the can idle
            // and set the minimum velocity is 25
            _body.IsAutoIdle = true;
            _body.MinimumVelocity = 25;

            _geom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _body, _width, _height);
            //_geom.CollisionGroup = 1;
            _geom.CollidesWith = _collidesWith;
            _geom.CollisionCategories = _collisionCategory;
            _geom.FrictionCoefficient = 1;
        }

        public void Draw(SpriteBatch spriteBatch, Color color)
        {
            spriteBatch.Draw(_texture, _body.Position, null, color, _body.Rotation, _origin, 1f, SpriteEffects.None, 0f);
        }
    }
}