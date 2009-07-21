using System;
using System.Text;
using DemoBaseXNA;
using DemoBaseXNA.DemoShare;
using DemoBaseXNA.DrawingSystem;
using DemoBaseXNA.ScreenSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.WaterSampleXNA.Demo1
{
    public class Demo1Screen : GameScreen
    {
        private Box _platform;
        private Body _refBody;
        private Geom _refGeom;
        private Pyramid _pyramid;
        private Texture2D _refTexture;
        private const int _pyramidBaseBodyCount = 4;
        private Matrix _cameraMatrix;
        private Matrix _projectionMatrix;
        private Matrix _worldMatrix;
        private VertexPositionColor[] _vertices; // an array of vertices with position and color
        private WaterModel _waterModel;
        private SpriteFont _font;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 250));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            base.Initialize();
        }

        public override void LoadContent()
        {
            _cameraMatrix = Matrix.Identity;
            _projectionMatrix = Matrix.CreateOrthographicOffCenter(0, ScreenManager.ScreenWidth, ScreenManager.ScreenHeight, 0, -100, 100);
            _worldMatrix = Matrix.Identity;

            _waterModel = new WaterModel();
            _waterModel.Initialize(PhysicsSimulator, new Vector2(ScreenManager.ScreenHeight * .05f, 500), new Vector2(ScreenManager.ScreenWidth - ((ScreenManager.ScreenHeight * .05f) * 2.0f), ScreenManager.ScreenHeight - 500));
            _waterModel.WaveController.Enabled = true;  // if false waves will not be created

            _platform = new Box(300, 20, new Vector2(ScreenManager.ScreenWidth / 2f, 500));
            _platform.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            _refTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 32, 32, 2, 0, 0,
                                                               Color.White, Color.Black);

            _refBody = BodyFactory.Instance.CreateRectangleBody(32, 32, 3f);
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

            // load a font
            _font = ScreenManager.ContentManager.Load<SpriteFont>("Content/Fonts/detailsfont");

            base.LoadContent();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            _waterModel.Update(gameTime.ElapsedGameTime);


            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(InputState input)
        {
            if (input.CurrentGamePadState.IsConnected)
            {
                HandleGamePadInput(input);
            }
            else
            {
                HandleKeyboardInput(input);
            }

            base.HandleInput(input);
        }

        private void HandleGamePadInput(InputState input)
        {
            // TODO - add simple property control through gamepad
        }

        int count = 0;

        private void HandleKeyboardInput(InputState input)
        {
            if (count > 5)      // this is used to slow down the input a little
            {
                if (input.CurrentKeyboardState.IsKeyDown(Keys.Z)) { _waterModel.WaveController.WaveGeneratorMax -= 0.1f; }
                if (input.CurrentKeyboardState.IsKeyDown(Keys.X)) { _waterModel.WaveController.WaveGeneratorMax += 0.1f; }

                if (input.CurrentKeyboardState.IsKeyDown(Keys.C)) { _waterModel.WaveController.WaveGeneratorMin -= 0.1f; }
                if (input.CurrentKeyboardState.IsKeyDown(Keys.V)) { _waterModel.WaveController.WaveGeneratorMin += 0.1f; }

                if (input.CurrentKeyboardState.IsKeyDown(Keys.B)) { _waterModel.WaveController.WaveGeneratorStep -= 0.1f; }
                if (input.CurrentKeyboardState.IsKeyDown(Keys.N)) { _waterModel.WaveController.WaveGeneratorStep += 0.1f; }

                if (input.CurrentKeyboardState.IsKeyDown(Keys.A)) { _waterModel.FluidDragController.Density -= 0.0001f; }
                if (input.CurrentKeyboardState.IsKeyDown(Keys.S)) { _waterModel.FluidDragController.Density += 0.0001f; }

                if (input.CurrentKeyboardState.IsKeyDown(Keys.D)) { _waterModel.FluidDragController.LinearDragCoefficient -= 0.1f; }
                if (input.CurrentKeyboardState.IsKeyDown(Keys.F)) { _waterModel.FluidDragController.LinearDragCoefficient += 0.1f; }

                if (input.CurrentKeyboardState.IsKeyDown(Keys.G)) { _waterModel.FluidDragController.RotationalDragCoefficient -= 0.1f; }
                if (input.CurrentKeyboardState.IsKeyDown(Keys.H)) { _waterModel.FluidDragController.RotationalDragCoefficient += 0.1f; }

                if (input.CurrentKeyboardState.IsKeyDown(Keys.Q)) { _waterModel.WaveController.DampingCoefficient -= 0.001f; }
                if (input.CurrentKeyboardState.IsKeyDown(Keys.E)) { _waterModel.WaveController.DampingCoefficient += 0.001f; }

                count = 0;
            }
            count++;
        }


        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            _pyramid.Draw(ScreenManager.SpriteBatch, _refTexture);
            _platform.Draw(ScreenManager.SpriteBatch, Color.White);

            // draw text for properties
            ScreenManager.SpriteBatch.DrawString(_font, "Properties:", new Vector2(ScreenManager.ScreenWidth - 240, 50), Color.White);
            ScreenManager.SpriteBatch.DrawString(_font, "+Z -X Wave Max: " + Convert.ToString(_waterModel.WaveController.WaveGeneratorMax), new Vector2(ScreenManager.ScreenWidth - 230, 65), Color.White);
            ScreenManager.SpriteBatch.DrawString(_font, "+C -V Wave Min: " + Convert.ToString(_waterModel.WaveController.WaveGeneratorMin), new Vector2(ScreenManager.ScreenWidth - 230, 80), Color.White);
            ScreenManager.SpriteBatch.DrawString(_font, "+B -N Wave Step: " + Convert.ToString(_waterModel.WaveController.WaveGeneratorStep), new Vector2(ScreenManager.ScreenWidth - 230, 95), Color.White);
            ScreenManager.SpriteBatch.DrawString(_font, "+Q -E Damping: " + Convert.ToString(_waterModel.WaveController.DampingCoefficient), new Vector2(ScreenManager.ScreenWidth - 230, 110), Color.White);

            ScreenManager.SpriteBatch.DrawString(_font, "+A -S Density: " + Convert.ToString(_waterModel.FluidDragController.Density), new Vector2(ScreenManager.ScreenWidth - 230, 125), Color.White);
            ScreenManager.SpriteBatch.DrawString(_font, "+D -F Linear Drag: " + Convert.ToString(_waterModel.FluidDragController.LinearDragCoefficient), new Vector2(ScreenManager.ScreenWidth - 230, 140), Color.White);
            ScreenManager.SpriteBatch.DrawString(_font, "+G -H Angular Drag: " + Convert.ToString(_waterModel.FluidDragController.RotationalDragCoefficient), new Vector2(ScreenManager.ScreenWidth - 230, 155), Color.White);

            ScreenManager.SpriteBatch.End();

            ScreenManager.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            //ScreenManager.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;

            // create the triangle strip
            CreateWaterVertexBuffer();

            // set the devices vertex declaration NOTICE - the vertex type is the same as we use when we create the vertex buffer
            ScreenManager.GraphicsDevice.VertexDeclaration = new VertexDeclaration(ScreenManager.GraphicsDevice,
                                                                                   VertexPositionColor.VertexElements);

            // create a basic effect with no pool
            BasicEffect effect = new BasicEffect(ScreenManager.GraphicsDevice, null);

            // set the effects matrix's
            effect.World = _worldMatrix;
            effect.View = _cameraMatrix;
            effect.Projection = _projectionMatrix;

            effect.Alpha = 0.5f;    // this effect supports a blending mode

            effect.VertexColorEnabled = true;   // we must enable vertex coloring with this effect
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
            // here we create a vertex buffer to hold position and color...this could be changed to hold many other elements for special effects that use a custom shader
            _vertices = new VertexPositionColor[_waterModel.WaveController.NodeCount * 2];

            // start at bottom left... this just sets the first corner of the triangle strip to the bottom left
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
            position = new Vector3(_waterModel.WaveController.XPosition[_waterModel.WaveController.NodeCount - 1], _waterModel.WaveController.Position.Y, 0); // assumes bottom of screen is 600
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
            sb.AppendLine("This demo shows how to do water in XNA.");
            sb.AppendLine("Keys are listed next to their values.");
            sb.AppendLine(string.Empty);
            return sb.ToString();
        }
    }
}