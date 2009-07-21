using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DemoShare
{
    public class Ball
    {
        private Body _body;
        private Geom _geom;
        private Texture2D _texture;
        private Vector2 _origin;
        private CollisionCategory _collidesWith = CollisionCategory.All;
        private CollisionCategory _collisionCategory = CollisionCategory.All;
        private const int _radius = 12;

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
            _texture = DrawingSystem.DrawingHelper.CreateCircleTexture(graphicsDevice, _radius, Color.Yellow, Color.Black);
            _origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);

            _body = BodyFactory.Instance.CreateCircleBody(physicsSimulator, _radius, 1);

            //NOTICE how the grid cell size is 0.5f, this causes the physics engine to take a long time
            //to calculate the collision grid. This is only a demonstration!
            _geom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, _body, _radius, 10, 0.5f);
            _geom.CollisionGroup = 1;
            _geom.CollidesWith = _collidesWith;
            _geom.CollisionCategories = _collisionCategory;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _body.Position, null, Color.White, _body.Rotation, _origin, 1f, SpriteEffects.None, 0f);
        }
    }
}