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

namespace FarseerGames.SimpleSamplesXNA.Demo4
{
    public class Demo4Screen : GameScreen
    {
#if XBOX
        //Xbox360 can't handle as many geometries
        private const int _pyramidBaseBodyCount = 8;
#else
        private const int _pyramidBaseBodyCount = 14;
#endif

        private Agent _agent;
        private Pyramid _pyramid;
        private Body _rectangleBody;
        private Geom _rectangleGeom;
        private Texture2D _rectangleTexture;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 50));
            PhysicsSimulator.BiasFactor = .2f;
            //for stacked objects, simultaneous collision are the bottlenecks so limit them to 2 per geometric pair.
            PhysicsSimulator.MaxContactsToDetect = 2;

            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);
            PhysicsSimulatorView.EnableEdgeView = false;

            base.Initialize();
        }

        public override void LoadContent()
        {
            _rectangleTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 32, 32, 2, 0, 0,
                                                                     Color.White, Color.Black);

            _rectangleBody = BodyFactory.Instance.CreateRectangleBody(32, 32, 1f); //template              
            _rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(_rectangleBody, 32, 32); //template
            _rectangleGeom.FrictionCoefficient = .4f;
            _rectangleGeom.RestitutionCoefficient = 0f;


            //create the pyramid near the bottom of the screen.
            _pyramid = new Pyramid(_rectangleBody, _rectangleGeom, 32f / 3f, 32f / 3f, 32, 32, _pyramidBaseBodyCount,
                                   new Vector2(ScreenManager.ScreenCenter.X - _pyramidBaseBodyCount * .5f * (32 + 32 / 3),
                                               ScreenManager.ScreenHeight - 60));

            _pyramid.Load(PhysicsSimulator);

            _agent = new Agent(ScreenManager.ScreenCenter - new Vector2(320, 200));
            _agent.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            _pyramid.Draw(ScreenManager.SpriteBatch, _rectangleTexture);
            _agent.Draw(ScreenManager.SpriteBatch);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public override void HandleInput(InputState input)
        {
            if (firstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
                firstRun = false;
            }

            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
            }

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
            Vector2 force = 1000 * input.CurrentGamePadState.ThumbSticks.Left;
            force.Y = -force.Y;
            _agent.ApplyForce(force);

            float rotation = -14000 * input.CurrentGamePadState.Triggers.Left;
            _agent.ApplyTorque(rotation);

            rotation = 14000 * input.CurrentGamePadState.Triggers.Right;
            _agent.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
            const float forceAmount = 1000;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.A)) { force += new Vector2(-forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S)) { force += new Vector2(0, forceAmount); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D)) { force += new Vector2(forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W)) { force += new Vector2(0, -forceAmount); }

            _agent.ApplyForce(force);

            const float torqueAmount = 14000;
            float torque = 0;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left)) { torque -= torqueAmount; }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right)) { torque += torqueAmount; }

            _agent.ApplyTorque(torque);
        }

        public static string GetTitle()
        {
            return "Demo4: Stacked Objects";
        }

        private static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows the stacking stability of the engine.");
            sb.AppendLine("It shows a stack of rectangular bodies stacked in");
            sb.AppendLine("the shape of a pyramid.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate: left and right triggers");
            sb.AppendLine("  -Move: left thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate: left and right arrows");
            sb.AppendLine("  -Move: A,S,D,W");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }
    }
}