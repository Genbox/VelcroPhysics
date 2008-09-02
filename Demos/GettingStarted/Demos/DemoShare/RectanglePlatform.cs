using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.Demos.DemoShare
{
    public class RectanglePlatform
    {
        private readonly Color _borderColor;
        private readonly int _collisionGroup;
        private readonly Color _color;
        private readonly int _height;
        private readonly Vector2 _position;
        private readonly int _width;
        private Body _platformBody;
        private Geom _platformGeom;

        private Vector2 _platformOrigin;
        private Texture2D _platformTexture;

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
            _platformTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, _width, _height, 2, 0, 0, _color,
                                                                   _borderColor);
            _platformOrigin = new Vector2(_platformTexture.Width/2f, _platformTexture.Height/2f);
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
                spriteBatch.Draw(_platformTexture, _platformGeom.Position, null, Color.White, _platformGeom.Rotation,
                                 _platformOrigin, 1, SpriteEffects.None, 0f);
            }
        }
    }
}