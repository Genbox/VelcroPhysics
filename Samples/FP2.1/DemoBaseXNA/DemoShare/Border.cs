using DemoBaseXNA.DrawingSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DemoShare
{
    public class Border
    {
        private Body _borderBody;
        private Geom[] _borderGeom;
        private Texture2D[] _borderTexture;
        private int _borderWidth;
        private int _height;
        private Vector2 _position;
        private int _width;

        public Border(int width, int height, int borderWidth, Vector2 position)
        {
            _width = width;
            _height = height;
            _borderWidth = borderWidth;
            _position = position;
        }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _borderTexture = new Texture2D[4];

            _borderTexture[0] = DrawingHelper.CreateRectangleTexture(graphicsDevice, _borderWidth, _height, 2, 0, 0,
                                                                     new Color(200, 200, 200, 150), Color.White);
            _borderTexture[1] = DrawingHelper.CreateRectangleTexture(graphicsDevice, _borderWidth, _height, 2, 0, 0,
                                                                     new Color(200, 200, 200, 150), Color.White);
            _borderTexture[2] = DrawingHelper.CreateRectangleTexture(graphicsDevice, _width, _borderWidth, 2, 0, 0,
                                                                     new Color(200, 200, 200, 150), Color.White);
            _borderTexture[3] = DrawingHelper.CreateRectangleTexture(graphicsDevice, _width, _borderWidth, 2, 0, 0,
                                                                     new Color(200, 200, 200, 150), Color.White);

            //use the body factory to create the physics body
            _borderBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _width, _height, 1);
            _borderBody.IsStatic = true;
            _borderBody.Position = _position;

            LoadBorderGeom(physicsSimulator);
        }

        private void LoadBorderGeom(PhysicsSimulator physicsSimulator)
        {
            _borderGeom = new Geom[4];
            //left border
            Vector2 geometryOffset = new Vector2(-(_width * .5f - _borderWidth * .5f), 0);
            _borderGeom[0] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _borderBody, _borderWidth,
                                                                      _height,
                                                                      geometryOffset, 0);
            _borderGeom[0].RestitutionCoefficient = .0001f;
            _borderGeom[0].FrictionCoefficient = .5f;
            _borderGeom[0].CollisionGroup = 100;

            //right border (clone left border since geometry is same size)
            geometryOffset = new Vector2(_width * .5f - _borderWidth * .5f, 0);
            _borderGeom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, _borderBody, _borderGeom[0],
                                                             geometryOffset,
                                                             0);


            //top border
            geometryOffset = new Vector2(0, -(_height * .5f - _borderWidth * .5f));
            

            _borderGeom[2] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _borderBody, _width,
                                                                      _borderWidth,
                                                                      geometryOffset, 0, 20);
            _borderGeom[2].RestitutionCoefficient = .2f;
            _borderGeom[2].FrictionCoefficient = .2f;
            _borderGeom[2].CollisionGroup = 100;

            //bottom border (clone top border since geometry is same size)
            geometryOffset = new Vector2(0, _height * .5f - _borderWidth * .5f);
            _borderGeom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, _borderBody, _borderGeom[2],
                                                             geometryOffset,
                                                             0);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 borderOrigin = new Vector2(_borderTexture[i].Width / 2f, _borderTexture[i].Height / 2f);
                spriteBatch.Draw(_borderTexture[i], _borderGeom[i].Position, null, Color.White, _borderGeom[i].Rotation,
                                 borderOrigin, 1, SpriteEffects.None, 0f);
            }
        }
    }
}