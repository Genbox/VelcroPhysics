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
        private readonly Color borderColor;
        private readonly int collisionGroup;
        private readonly Color color;
        private readonly int height;
        private readonly Vector2 position;
        private readonly int width;
        private Body platformBody;
        private Geom platformGeom;

        private Vector2 platformOrigin;
        private Texture2D platformTexture;

        public RectanglePlatform(int width, int height, Vector2 position, Color color, Color borderColor,
                                 int collisionGroup)
        {
            this.width = width;
            this.height = height;
            this.position = position;
            this.color = color;
            this.borderColor = borderColor;
            this.collisionGroup = collisionGroup;
        }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            platformTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, width, height, 2, 0, 0, color,
                                                                   borderColor);
            platformOrigin = new Vector2(platformTexture.Width/2f, platformTexture.Height/2f);
            //use the body factory to create the physics body
            platformBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, width, height, 1);
            platformBody.IsStatic = true;
            platformBody.Position = position;

            platformGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, platformBody, width, height);
            platformGeom.CollisionGroup = 100;
            platformGeom.CollisionGroup = collisionGroup;
            platformGeom.FrictionCoefficient = 1;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(platformTexture, platformGeom.Position, null, Color.White, platformGeom.Rotation,
                                 platformOrigin, 1, SpriteEffects.None, 0f);
            }
        }
    }
}