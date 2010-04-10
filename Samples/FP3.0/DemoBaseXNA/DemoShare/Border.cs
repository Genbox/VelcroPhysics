using DemoBaseXNA.DrawingSystem;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DemoShare
{
    public class Border
    {
        private Body _borderBody;
        private Fixture[] _borderGeom;
        private Texture2D[] _borderTextures;
        private float _borderWidth;
        private float _height;
        private Vector2 _position;
        private float _width;

        public Border(float width, float height, float borderWidth, Vector2 position)
        {
            _width = width;
            _height = height;
            _borderWidth = borderWidth;
            _position = position;
        }

        public void Load(GraphicsDevice graphicsDevice, World physicsSimulator)
        {
            _borderTextures = new Texture2D[4];

            _borderTextures[0] = DrawingHelper.CreateRectangleTexture(graphicsDevice, (int) (_borderWidth * 10),
                                                                      (int) (_height * 10), 2, 0, 0,
                                                                      new Color(200, 200, 200, 150), Color.White);
            _borderTextures[1] = DrawingHelper.CreateRectangleTexture(graphicsDevice, (int) (_borderWidth * 10),
                                                                      (int) (_height * 10), 2, 0, 0,
                                                                      new Color(200, 200, 200, 150), Color.White);
            _borderTextures[2] = DrawingHelper.CreateRectangleTexture(graphicsDevice, (int) (_width * 10),
                                                                      (int) (_borderWidth * 10), 2, 0, 0,
                                                                      new Color(200, 200, 200, 150), Color.White);
            _borderTextures[3] = DrawingHelper.CreateRectangleTexture(graphicsDevice, (int) (_width * 10),
                                                                      (int) (_borderWidth * 10), 2, 0, 0,
                                                                      new Color(200, 200, 200, 150), Color.White);

            //use the body factory to create the physics body
            _borderBody = physicsSimulator.Add();
            _borderBody.BodyType = BodyType.Static;
            _borderBody.Position = _position;

            LoadBorderGeom(physicsSimulator);
        }

        private void LoadBorderGeom(World physicsSimulator)
        {
            _borderGeom = new Fixture[4];
            //left border
            Vector2 geometryOffset = new Vector2(-(_width * .5f - (_borderWidth * .5f)), 0);
            Vertices box = PolygonTools.CreateRectangle(_borderWidth / 2f, _height / 2f, geometryOffset, 0);
            PolygonShape shape = new PolygonShape(box, 5);
            _borderGeom[0] = _borderBody.CreateFixture(shape);
            _borderGeom[0].Restitution = .0f;
            _borderGeom[0].Friction = .5f;
            _borderGeom[0].CollisionGroup = 100;
            _borderGeom[0].UserData = geometryOffset;

            //right border (clone left border since geometry is same size)
            geometryOffset = new Vector2((_width * .5f - _borderWidth * .5f), 0);
            box = PolygonTools.CreateRectangle(_borderWidth / 2f, _height / 2f, geometryOffset, 0);
            shape = new PolygonShape(box, 5);
            _borderGeom[1] = _borderBody.CreateFixture(shape);
            _borderGeom[1].Restitution = .0f;
            _borderGeom[1].Friction = .5f;
            _borderGeom[1].CollisionGroup = 100;
            _borderGeom[1].UserData = geometryOffset;

            //top border
            geometryOffset = new Vector2(0, -(_height * .5f - _borderWidth * .5f));
            box = PolygonTools.CreateRectangle(_width / 2f, _borderWidth / 2f, geometryOffset, 0);
            shape = new PolygonShape(box, 5);
            _borderGeom[2] = _borderBody.CreateFixture(shape);
            _borderGeom[2].Restitution = .0f;
            _borderGeom[2].Friction = .5f;
            _borderGeom[2].CollisionGroup = 100;
            _borderGeom[2].UserData = geometryOffset;

            //bottom border (clone top border since geometry is same size)
            geometryOffset = new Vector2(0, _height * .5f - _borderWidth * .5f);
            box = PolygonTools.CreateRectangle(_width / 2f, _borderWidth / 2f, geometryOffset, 0);
            shape = new PolygonShape(box, 5);
            _borderGeom[3] = _borderBody.CreateFixture(shape);
            _borderGeom[3].Restitution = .0f;
            _borderGeom[3].Friction = .5f;
            _borderGeom[3].CollisionGroup = 100;
            _borderGeom[3].UserData = geometryOffset;
        }

        public void Draw()
        {
        }
    }
}