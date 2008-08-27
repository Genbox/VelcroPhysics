using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Collisions;

using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;

namespace FarseerGames.FarseerPhysicsDemos.Demos.Demo4 {
    class Floor {
        Texture2D floorTexture;
        Vector2 floorOrigin;

        Body floorBody;
        Geom floorGeom;

        int width;
        int height;
        Vector2 position;

        public Floor(int width, int height, Vector2 position) {
            this.width = width;
            this.height = height;
            this.position = position;
        }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator) {
            floorTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, width, height,0,1,1, Color.White,Color.Black);
            floorOrigin = new Vector2(floorTexture.Width / 2f, floorTexture.Height / 2f);

            //use the body factory to create the physics body
            floorBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator,width,height, 1);
            floorBody.IsStatic = true;
            floorGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator,floorBody,width,height);
            floorGeom.RestitutionCoefficient = .4f;
            floorGeom.FrictionCoefficient = .4f;
            floorBody.Position = position;
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(floorTexture, floorBody.Position, null, Color.White, floorBody.Rotation, floorOrigin, 1, SpriteEffects.None, 0f);
        }


    }
}
