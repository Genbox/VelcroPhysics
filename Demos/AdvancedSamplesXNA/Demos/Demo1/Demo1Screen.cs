using System.Text;
using System.Threading;
using FarseerGames.AdvancedSamplesXNA.Demos.DemoShare;
using FarseerGames.AdvancedSamplesXNA.DrawingSystem;
using FarseerGames.AdvancedSamplesXNA.ScreenSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.AdvancedSamplesXNA.Demos.Demo1
{
    public class Demo1Screen : GameScreen
    {
        private const int pyramidBaseBodyCount = 16;
        private Agent _agent;
        private PhysicsProcessor _physicsProcessor;
        private Thread _physicsThread;
        private MultithreadedPyramid _multithreadedPyramid;
        private Body _rectangleBody;
        private Geom _rectangleGeom;
        private Texture2D _rectangleTexture;

        // POINT OF INTEREST
        // This is the processor used to communicate with the physics thread

        public override void Dispose()
        {
            // POINT OF INTEREST
            // On screen destroy Dispose the physics processor and use Join to wait the thread to exit
            _physicsProcessor.Dispose();
            _physicsProcessor = null;
            //_physicsThread.Abort();
            _physicsThread.Join();
            _physicsThread = null;

            base.Dispose();
        }

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 50));
            PhysicsSimulator.BiasFactor = .3f;
            //for stacked objects, simultaneous collision are the bottlenecks so limit them to 2 per geometric pair.
            PhysicsSimulator.MaxContactsToDetect = 1;

            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);
            PhysicsSimulatorView.EnableEdgeView = false;

            // POINT OF INTEREST
            // Create a physics processor based on the physics processor
            _physicsProcessor = new PhysicsProcessor(PhysicsSimulator);
            // POINT OF INTEREST
            // Create the physics thread with the StartThinking function as the entry point.
            // The StartThinking is going to be the main function of the physics thread.
            _physicsThread = new Thread(_physicsProcessor.StartThinking);
            // POINT OF INTEREST
            // Name the thread for debugging purposes
            _physicsThread.Name = "PhysicsThread";
            // POINT OF INTEREST
            // And now start the thread
            _physicsThread.Start();

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
            _multithreadedPyramid = new MultithreadedPyramid(_rectangleBody, _rectangleGeom, 32f / 8f, 32f / 8f, 32, 32, pyramidBaseBodyCount,
                                   new Vector2(ScreenManager.ScreenCenter.X - pyramidBaseBodyCount * .5f * (32 + 32 / 3),
                                               ScreenManager.ScreenHeight - 80));
            // POINT OF INTEREST
            // It needs the processor to register the links
            _multithreadedPyramid.Load(PhysicsSimulator, _physicsProcessor);

            _agent = new Agent(ScreenManager.ScreenCenter - new Vector2(320, 300));
            _agent.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);
            // POINT OF INTEREST
            // Link the agent to the processor
            _agent.LinkToProcessor(_physicsProcessor);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            _multithreadedPyramid.Draw(ScreenManager.SpriteBatch, _rectangleTexture);
            _agent.Draw(ScreenManager.SpriteBatch);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public override void HandleInput(InputState input)
        {
            if (firstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
                firstRun = false;
            }

            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
            }

            HandleKeyboardInput(input);
            base.HandleInput(input);
        }

        private void HandleKeyboardInput(InputState input)
        {
            const float forceAmount = 1000;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.A))
            {
                force += new Vector2(-forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S))
            {
                force += new Vector2(0, forceAmount);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D))
            {
                force += new Vector2(forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W))
            {
                force += new Vector2(0, -forceAmount);
            }

            _agent.ApplyForce(force);

            const float torqueAmount = 14000;
            float torque = 0;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                torque -= torqueAmount;
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right))
            {
                torque += torqueAmount;
            }
            _agent.ApplyTorque(torque);
        }

        // POINT OF INTEREST
        // We override this to be able to wait for the physics simulator to finish a frame
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // POINT OF INTEREST
            // Wait for the physics thread to finish the physics frame. Without this
            // this thread and the physics thread can read and modify the simulator
            // at the same time. That can cause incorrect updates and even crashes.
            if (_physicsProcessor != null)
                _physicsProcessor.BlockUntilIdle();

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        // POINT OF INTEREST
        // We are going to update the physiscs through the processor, so override this function and do this ourselves
        public override void UpdatePhysics(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (otherScreenHasFocus || coveredByOtherScreen)
                return;

            // POINT OF INTEREST
            // Signal the physiscs processor to calculate the next physics frame
            // It isn't advance the simulator, just tell the physics processor
            // to do so on the other thread.
            // But if the DebugViewEnabled, we force the simulator to update instantly
            // as the debug view doesn't use the links.
            if (_physicsProcessor != null)
                _physicsProcessor.Iterate(gameTime, DebugViewEnabled);
        }

        public static string GetTitle()
        {
            return "Demo1: Multithreaded Stacked Objects";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows the stacking stability of the engine.");
            sb.AppendLine("It shows a stack of rectangular bodies stacked in");
            sb.AppendLine("the shape of a pyramid.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate : left and right arrows");
            sb.AppendLine("  -Move : A,S,D,W");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }
    }
}