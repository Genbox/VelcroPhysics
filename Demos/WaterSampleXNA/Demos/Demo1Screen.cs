using System;
using System.Text;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.WaterSampleXNA.Demos.DemoShare;
using FarseerGames.WaterSampleXNA.DrawingSystem;
using FarseerGames.WaterSampleXNA.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.WaterSampleXNA.Demos
{
    public class Demo1Screen : GameScreen
    {
        private Box _platform;
        private Body _refBody;
        private Geom _refGeom;
        private Pyramid _pyramid;
        private Texture2D _refTexture;
        private const int _pyramidBaseBodyCount = 6;
        private Matrix _cameraMatrix;
        private Matrix _projectionMatrix;
        private Matrix _worldMatrix;
        private VertexPositionColor[] _vertices; // an array of vertices with position and color
        private WaterModel _waterModel;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 250));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            _cameraMatrix = Matrix.Identity;
            _projectionMatrix = Matrix.CreateOrthographicOffCenter(0, 1024, 768, 0, -100, 100);
            _worldMatrix = Matrix.Identity;

            _waterModel = new WaterModel();
            _waterModel.Initialize(PhysicsSimulator);
            _waterModel.WaveController.Enabled = true;

            base.Initialize();
        }

        public override void LoadContent()
        {
            _platform = new Box(300, 20, 5, new Vector2(ScreenManager.ScreenWidth / 2f, 500), Color.White, Color.Black, 1);
            _platform.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            _refTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 32, 32, 2, 0, 0,
                                                               Color.White, Color.Black);

            _refBody = BodyFactory.Instance.CreateRectangleBody(32, 32, 1f);
            _refGeom = GeomFactory.Instance.CreateRectangleGeom(_refBody, 32, 32);
            _refGeom.FrictionCoefficient = .2f;

            //create the _pyramid near the bottom of the screen.
            _pyramid = new Pyramid(_refBody, _refGeom, 32f / 3f, 32f / 3f, 32, 32, _pyramidBaseBodyCount,
                                   new Vector2(ScreenManager.ScreenCenter.X - _pyramidBaseBodyCount * .5f * (32 + 32 / 3) + 20,
                                               ScreenManager.ScreenHeight - 300));

            _pyramid.Load(PhysicsSimulator);

            //Add the geometries to the watermodel's fluiddragcontroller to make them float.
            _waterModel.FluidDragController.AddGeom(_platform.Geom);

            foreach (Geom geom in _pyramid.Geoms)
            {
                _waterModel.FluidDragController.AddGeom(geom);
            }

            base.LoadContent();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            _waterModel.Update(gameTime.ElapsedGameTime);


            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }


        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            _pyramid.Draw(ScreenManager.SpriteBatch, _refTexture);
            _platform.Draw(ScreenManager.SpriteBatch);
            ScreenManager.SpriteBatch.End();

            ScreenManager.GraphicsDevice.RenderState.CullMode = CullMode.None;

            // create the triangle strip
            CreateWaterVertexBuffer();

            // set the devices vertex declaration
            ScreenManager.GraphicsDevice.VertexDeclaration = new VertexDeclaration(ScreenManager.GraphicsDevice,
                                                                                   VertexPositionColor.VertexElements);

            // create a basic effect with no pool
            BasicEffect effect = new BasicEffect(ScreenManager.GraphicsDevice, null);

            // set the effects matrix's
            effect.World = _worldMatrix;
            effect.View = _cameraMatrix;
            effect.Projection = _projectionMatrix;

            effect.Alpha = 0.5f;

            effect.VertexColorEnabled = true;
            effect.Begin();

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                ScreenManager.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _vertices, 0,
                                                                _waterModel.WaveController.NodeCount);
                pass.End();
            }

            base.Draw(gameTime);
        }

        public void CreateWaterVertexBuffer()
        {
            _vertices = new VertexPositionColor[_waterModel.WaveController.NodeCount * 2];

            // start at bottom left
            Vector3 position = new Vector3(_waterModel.WaveController.Position.X, _waterModel.WaveController.Position.Y + _waterModel.WaveController.Height, 0);
            _vertices[0] = new VertexPositionColor(position, Color.Aquamarine); // save it to the buffer

            // for each point on the wave
            for (int i = 0; i < _waterModel.WaveController.NodeCount; i += 2)
            {
                position = new Vector3(_waterModel.WaveController.XPosition[i],
                                       _waterModel.WaveController.Position.Y + _waterModel.WaveController.CurrentWave[i], 0);
                // bottom of screen - waveHeight + that nodes offset
                _vertices[i + 1] = new VertexPositionColor(position, Color.Aquamarine); // save it to the buffer

                position = new Vector3(_waterModel.WaveController.XPosition[i + 1], _waterModel.WaveController.Position.Y + _waterModel.WaveController.Height, 0); // bottom of screen
                _vertices[i + 2] = new VertexPositionColor(position, Color.Aquamarine); // save it to the buffer
            }

            // start at bottom left
            position = new Vector3(_waterModel.WaveController.XPosition[_waterModel.WaveController.NodeCount-1], _waterModel.WaveController.Position.Y, 0); // assumes bottom of screen is 600
            _vertices[_waterModel.WaveController.NodeCount + 1] = new VertexPositionColor(position, Color.Aquamarine);
            // save it to the buffer
        }

        public static string GetTitle()
        {
            return "Start water sample";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to do water in XNA");
            sb.AppendLine(string.Empty);
            return sb.ToString();
        }
    }
}