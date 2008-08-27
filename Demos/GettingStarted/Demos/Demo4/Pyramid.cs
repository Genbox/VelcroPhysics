using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Collisions;

using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysicsDemos.Demos.Demo4 {
    class Pyramid {
        Body referenceBody;
        Geom referenceGeom;

        Body[] blockBody;
        Geom[] blockGeom;
        
        private float horizontalSpacing;
        private float verticleSpacing;
        private float blockWidth;
        private float blockHeight;
        private int bottomRowBlockCount;
        private Vector2 bottomRightBlockPosition;


        public Pyramid(Body referenceBody, Geom referenceGeom, float horizontalSpacing, float verticleSpacing, float blockWidth, float blockHeight, int bottomRowBlockCount, Vector2 bottomRightBlockPosition) {
            this.referenceBody = referenceBody;
            this.referenceGeom = referenceGeom;
            this.horizontalSpacing = horizontalSpacing;
            this.verticleSpacing = verticleSpacing;
            this.blockWidth = blockWidth;
            this.blockHeight = blockHeight;
            this.bottomRowBlockCount = bottomRowBlockCount;
            this.bottomRightBlockPosition = bottomRightBlockPosition;
        }
        
        public void Load(PhysicsSimulator physicsSimulator) {
            int count = bottomRowBlockCount * (1 + bottomRowBlockCount) / 2;
            blockBody = new Body[count];
            blockGeom = new Geom[count];

            for (int i = 0; i < blockBody.Length; i++) {
                blockBody[i] = BodyFactory.Instance.CreateBody(physicsSimulator,referenceBody);
                blockGeom[i] = GeomFactory.Instance.CreateGeom(physicsSimulator,blockBody[i], referenceGeom);
            }

            CreatePyramid();
        }

        private void CreatePyramid() {
            Vector2 rowOffset = new Vector2((blockWidth / 2) + (horizontalSpacing / 2), -(blockHeight + verticleSpacing));
            Vector2 colOffset = new Vector2(horizontalSpacing + blockWidth, 0);
            Vector2 position = Vector2.Zero;
            int blockCounter = 0;
            for (int i = 0; i < bottomRowBlockCount; i++) {
                position = bottomRightBlockPosition + rowOffset * i;
                for (int j = 0; j < bottomRowBlockCount - i; j++) {
                    Vector2 rowPosition = position + colOffset * j;
                    blockBody[blockCounter].Position = rowPosition;
                    blockCounter += 1;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture) {
            for (int i = 0; i < blockBody.Length; i++) {
                spriteBatch.Draw(texture, blockBody[i].Position, null, Color.White, blockBody[i].Rotation, new Vector2(texture.Width/2, texture.Height/2), 1, SpriteEffects.None, 0f);
            } 
        }
    }
}
