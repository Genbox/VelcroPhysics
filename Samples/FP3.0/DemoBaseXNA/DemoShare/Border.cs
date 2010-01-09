using DemoBaseXNA.DrawingSystem;
using DemoBaseXNA.ScreenSystem;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DemoShare
{
    public class Border
    {
        private Body _borderBody;
        private Fixture[] _borderGeom;
        private Texture2D[] _borderTexture;
        private float _borderWidth;
        private float _height;
        private Vector2 _position;
        private float _width;
        private QuadRenderEngine _quadRenderEngine;
        private Quad[] _quads;

        public Border(float width, float height, float borderWidth, Vector2 position)
        {
            _width = width;
            _height = height;
            _borderWidth = borderWidth;
            _position = position;
        }

        public void Load(GraphicsDevice graphicsDevice, World physicsSimulator)
        {
            _quadRenderEngine = new QuadRenderEngine(graphicsDevice);
            _quads = new Quad[4];
            
            _borderTexture = new Texture2D[4];

            _borderTexture[0] = DrawingHelper.CreateRectangleTexture(graphicsDevice, (int)(_borderWidth * 10), (int)(_height * 10), 2, 0, 0,
                                                                     new Color(200, 200, 200, 150), Color.White);
            _borderTexture[1] = DrawingHelper.CreateRectangleTexture(graphicsDevice, (int)(_borderWidth * 100), (int)(_height * 10), 2, 0, 0,
                                                                     new Color(200, 200, 200, 150), Color.White);
            _borderTexture[2] = DrawingHelper.CreateRectangleTexture(graphicsDevice, (int)(_width * 10), (int)(_borderWidth * 10), 2, 0, 0,
                                                                     new Color(200, 200, 200, 150), Color.White);
            _borderTexture[3] = DrawingHelper.CreateRectangleTexture(graphicsDevice, (int)(_width * 10), (int)(_borderWidth * 10), 2, 0, 0,
                                                                     new Color(200, 200, 200, 150), Color.White);

            _quadRenderEngine.Submit(_borderTexture[0], false);
            _quadRenderEngine.Submit(_borderTexture[1], false);
            _quadRenderEngine.Submit(_borderTexture[2], false);
            _quadRenderEngine.Submit(_borderTexture[3], false);

            //use the body factory to create the physics body
            _borderBody = physicsSimulator.CreateBody();
            _borderBody.BodyType = BodyType.Static;
            _borderBody.Position = _position;

            LoadBorderGeom(physicsSimulator);
        }

        private void LoadBorderGeom(World physicsSimulator)
        {
            _borderGeom = new Fixture[4];
            //left border
            Vector2 geometryOffset = new Vector2(-(_width * .5f - (_borderWidth * .5f)), 0);
            Vertices box = PolygonTools.CreateBox(_borderWidth / 2f, _height / 2f, geometryOffset, 0);
            PolygonShape shape = new PolygonShape(box, 5);
            _borderGeom[0] = _borderBody.CreateFixture(shape);
            _borderGeom[0].Restitution = .0f;
            _borderGeom[0].Friction = .5f;
            _borderGeom[0].GroupIndex = 100;
            _borderGeom[0].UserData = geometryOffset;
            _quads[0] = new Quad(geometryOffset, 0, _borderWidth, _height, 0);

            //right border (clone left border since geometry is same size)
            geometryOffset = new Vector2((_width * .5f - _borderWidth * .5f), 0);
            box = PolygonTools.CreateBox(_borderWidth / 2f, _height / 2f, geometryOffset, 0);
            shape = new PolygonShape(box, 5);
            _borderGeom[1] = _borderBody.CreateFixture(shape);
            _borderGeom[1].Restitution = .0f;
            _borderGeom[1].Friction = .5f;
            _borderGeom[1].GroupIndex = 100;
            _borderGeom[1].UserData = geometryOffset;
            _quads[1] = new Quad(geometryOffset, 0, _borderWidth, _height, 1);

            //top border
            geometryOffset = new Vector2(0, -(_height * .5f - _borderWidth * .5f));
            box = PolygonTools.CreateBox(_width / 2f, _borderWidth / 2f, geometryOffset, 0);
            shape = new PolygonShape(box, 5);
            _borderGeom[2] = _borderBody.CreateFixture(shape);
            _borderGeom[2].Restitution = .0f;
            _borderGeom[2].Friction = .5f;
            _borderGeom[2].GroupIndex = 100;
            _borderGeom[2].UserData = geometryOffset;
            _quads[2] = new Quad(geometryOffset, 0, _width, _borderWidth, 2);

            //bottom border (clone top border since geometry is same size)
            geometryOffset = new Vector2(0, _height * .5f - _borderWidth * .5f);
            box = PolygonTools.CreateBox(_width / 2f, _borderWidth / 2f, geometryOffset, 0);
            shape = new PolygonShape(box, 5);
            _borderGeom[3] = _borderBody.CreateFixture(shape);
            _borderGeom[3].Restitution = .0f;
            _borderGeom[3].Friction = .5f;
            _borderGeom[3].GroupIndex = 100;
            _borderGeom[3].UserData = geometryOffset;
            _quads[3] = new Quad(geometryOffset, 0, _width, _borderWidth, 3);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < 4; i++)
            {
                //Vector2 borderOrigin = new Vector2(_borderTexture[i].Width / 2f, _borderTexture[i].Height / 2f);
                //spriteBatch.Draw(_borderTexture[i], ConvertUnits.ToDisplayUnits(_borderBody.Position + (Vector2)_borderGeom[i].UserData), null, Color.White, 0,
                //                 borderOrigin, 1, SpriteEffects.None, 0f);
                _quadRenderEngine.Submit(_quads[i]);
            }
            _quadRenderEngine.Render();
        }
    }
}