using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.Demos.DemoShare
{
    internal class Border
    {
        private readonly int borderWidth;
        private readonly int height;
        private readonly Vector2 position;
        private readonly int width;
        private Body borderBody;
        private Geom[] borderGeom;
        private Texture2D[] borderTexture;

        public Border(int width, int height, int borderWidth, Vector2 position)
        {
            this.width = width;
            this.height = height;
            this.borderWidth = borderWidth;
            this.position = position;
        }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            borderTexture = new Texture2D[4];

            borderTexture[0] = DrawingHelper.CreateRectangleTexture(graphicsDevice, borderWidth, height, 2, 0, 0,
                                                                    new Color(200, 200, 200, 150), Color.White);
            borderTexture[1] = DrawingHelper.CreateRectangleTexture(graphicsDevice, borderWidth, height, 2, 0, 0,
                                                                    new Color(200, 200, 200, 150), Color.White);
            borderTexture[2] = DrawingHelper.CreateRectangleTexture(graphicsDevice, width, borderWidth, 2, 0, 0,
                                                                    new Color(200, 200, 200, 150), Color.White);
            borderTexture[3] = DrawingHelper.CreateRectangleTexture(graphicsDevice, width, borderWidth, 2, 0, 0,
                                                                    new Color(200, 200, 200, 150), Color.White);

            //use the body factory to create the physics body
            borderBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, width, height, 1);
            borderBody.IsStatic = true;
            borderBody.Position = position;

            LoadBorderGeom(physicsSimulator);
        }

        public void LoadBorderGeom(PhysicsSimulator physicsSimulator)
        {
            borderGeom = new Geom[4];
            //left border
            Vector2 geometryOffset = new Vector2(-(width*.5f - borderWidth*.5f), 0);
            borderGeom[0] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, borderBody, borderWidth, height,
                                                                     geometryOffset, 0);
            borderGeom[0].RestitutionCoefficient = .2f;
            borderGeom[0].FrictionCoefficient = .5f;
            borderGeom[0].CollisionGroup = 100;

            //right border (clone left border since geometry is same size)
            geometryOffset = new Vector2(width*.5f - borderWidth*.5f, 0);
            borderGeom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, borderBody, borderGeom[0], geometryOffset,
                                                            0);


            //top border
            geometryOffset = new Vector2(0, -(height*.5f - borderWidth*.5f));
            borderGeom[2] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, borderBody, width, borderWidth,
                                                                     geometryOffset, 0);
            borderGeom[2].RestitutionCoefficient = .2f;
            borderGeom[2].FrictionCoefficient = .2f;
            borderGeom[2].CollisionGroup = 100;
            borderGeom[2].CollisonGridCellSize = 20;
            borderGeom[2].ComputeCollisonGrid();

            //bottom border (clone top border since geometry is same size)
            geometryOffset = new Vector2(0, height*.5f - borderWidth*.5f);
            borderGeom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, borderBody, borderGeom[2], geometryOffset,
                                                            0);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 borderOrigin;
            for (int i = 0; i < 4; i++)
            {
                borderOrigin = new Vector2(borderTexture[i].Width/2f, borderTexture[i].Height/2f);
                spriteBatch.Draw(borderTexture[i], borderGeom[i].Position, null, Color.White, borderGeom[i].Rotation,
                                 borderOrigin, 1, SpriteEffects.None, 0f);
            }
        }
    }
}