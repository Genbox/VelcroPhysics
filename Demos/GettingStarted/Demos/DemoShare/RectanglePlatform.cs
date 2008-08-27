using System;
using System.Collections.Generic;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Collisions;

using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;

namespace FarseerGames.FarseerPhysicsDemos.Demos.DemoShare {
    public class RectanglePlatform {
        Texture2D platformTexture;

        Body platformBody;
        Geom platformGeom;

        int width;
        int height;
        Vector2 position;
        Color color;
        Color borderColor;
        int collisionGroup;

        Vector2 platformOrigin;

        public RectanglePlatform(int width, int height, Vector2 position, Color color, Color borderColor, int collisionGroup) {
            this.width = width;
            this.height = height;
             this.position = position;
            this.color = color;
            this.borderColor = borderColor;
            this.collisionGroup = collisionGroup;
        }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator) {
           
            platformTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice,width, height,2,0,0,color, borderColor);
            platformOrigin = new Vector2(platformTexture.Width / 2, platformTexture.Height / 2);
            //use the body factory to create the physics body
            platformBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator,width,height, 1);
            platformBody.IsStatic = true;
            platformBody.Position = position;

            platformGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator,platformBody, width, height);
            platformGeom.CollisionGroup = 100;
            platformGeom.CollisionGroup = collisionGroup;
            platformGeom.FrictionCoefficient = 1;
        }

        public void Draw(SpriteBatch spriteBatch) {
            for (int i = 0; i < 4; i++) {
                spriteBatch.Draw(platformTexture, platformGeom.Position, null, Color.White, platformGeom.Rotation, platformOrigin, 1, SpriteEffects.None, 0f);
            }

        }
    }
}