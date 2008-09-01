using System.Text;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysicsDemos.Demos.DemoShare;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.FarseerPhysicsDemos.Demos.Demo4
{
    public class Demo4Screen : GameScreen
    {
        private const int pyramidBaseBodyCount = 16;
        private readonly LineBrush lineBrush = new LineBrush(1, Color.Black); //used to draw spring on mouse grab

        private readonly PhysicsSimulator physicsSimulator;
        private readonly PhysicsSimulatorView physicsSimulatorView;

        private Agent agent;
        private ContentManager contentManager;
        private bool debugViewEnabled;
        private bool firstRun;
        private Floor floor;
        private FixedLinearSpring mousePickSpring;
        private Geom pickedGeom;
        private Pyramid pyramid;

#if XBOX
        private int pyramidBaseBodyCount = 8;
#else
        private Body rectangleBody;
        private Geom rectangleGeom;
        private Texture2D rectangleTexture;
        public bool updatedOnce;
#endif

        public Demo4Screen()
        {
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 50));
            physicsSimulator.BiasFactor = .4f;
            physicsSimulator.MaxContactsToDetect = 2;
            //for stacked objects, simultaneous collision are the bottlenecks so limit them to 2 per geometric pair.
            physicsSimulatorView = new PhysicsSimulatorView(physicsSimulator);
        }

        public override void LoadContent()
        {
            if (contentManager == null) contentManager = new ContentManager(ScreenManager.Game.Services);
            lineBrush.Load(ScreenManager.GraphicsDevice);
            physicsSimulatorView.LoadContent(ScreenManager.GraphicsDevice, contentManager);

            rectangleTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 32, 32, 2, 0, 0,
                                                                    Color.White, Color.Black);

            rectangleBody = BodyFactory.Instance.CreateRectangleBody(32, 32, 1f); //template              
            rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(rectangleBody, 32, 32); //template
            rectangleGeom.FrictionCoefficient = .4f;
            rectangleGeom.RestitutionCoefficient = 0f;

            //create the pyramid near the bottom of the screen.
            pyramid = new Pyramid(rectangleBody, rectangleGeom, 32f/3f, 32f/3f, 32, 32, pyramidBaseBodyCount,
                                  new Vector2(ScreenManager.ScreenCenter.X - pyramidBaseBodyCount*.5f*(32 + 32/3),
                                              ScreenManager.ScreenHeight - 125));
            pyramid.Load(physicsSimulator);

            floor = new Floor(ScreenManager.ScreenWidth, 100,
                              new Vector2(ScreenManager.ScreenCenter.X, ScreenManager.ScreenHeight - 50));
            floor.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            agent = new Agent(ScreenManager.ScreenCenter - new Vector2(320, 300));
            agent.Load(ScreenManager.GraphicsDevice, physicsSimulator);
        }

        public override void UnloadContent()
        {
            contentManager.Unload();
            physicsSimulator.Clear();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (IsActive)
            {
                physicsSimulator.Update(gameTime.ElapsedGameTime.Milliseconds*.001f);
                updatedOnce = true;
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!updatedOnce)
            {
                return;
            }
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            pyramid.Draw(ScreenManager.SpriteBatch, rectangleTexture);
            floor.Draw(ScreenManager.SpriteBatch);
            agent.Draw(ScreenManager.SpriteBatch);

            if (mousePickSpring != null)
            {
                lineBrush.Draw(ScreenManager.SpriteBatch,
                               mousePickSpring.Body.GetWorldPosition(mousePickSpring.BodyAttachPoint),
                               mousePickSpring.WorldAttachPoint);
            }
            if (debugViewEnabled)
            {
                physicsSimulatorView.Draw(ScreenManager.SpriteBatch);
            }
            ScreenManager.SpriteBatch.End();
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
#if !XBOX
                HandleMouseInput(input);
#endif
            }
            base.HandleInput(input);
        }

        private void HandleGamePadInput(InputState input)
        {
            if (input.LastGamePadState.Buttons.Y != ButtonState.Pressed &&
                input.CurrentGamePadState.Buttons.Y == ButtonState.Pressed)
            {
                debugViewEnabled = !debugViewEnabled;
                physicsSimulator.EnableDiagnostics = debugViewEnabled;
            }

            Vector2 force = 1000*input.CurrentGamePadState.ThumbSticks.Left;
            force.Y = -force.Y;
            agent.ApplyForce(force);

            float rotation = -14000*input.CurrentGamePadState.Triggers.Left;
            agent.ApplyTorque(rotation);

            rotation = 14000*input.CurrentGamePadState.Triggers.Right;
            agent.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
            if (!input.LastKeyboardState.IsKeyDown(Keys.F1) && input.CurrentKeyboardState.IsKeyDown(Keys.F1))
            {
                debugViewEnabled = !debugViewEnabled;
                physicsSimulator.EnableDiagnostics = debugViewEnabled;
            }

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

            agent.ApplyForce(force);

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
            agent.ApplyTorque(torque);
        }

#if !XBOX
        private void HandleMouseInput(InputState input)
        {
            Vector2 point = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y);
            if (input.LastMouseState.LeftButton == ButtonState.Released &&
                input.CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                //create mouse spring
                pickedGeom = physicsSimulator.Collide(point);
                if (pickedGeom != null)
                {
                    mousePickSpring = ControllerFactory.Instance.CreateFixedLinearSpring(physicsSimulator,
                                                                                         pickedGeom.Body,
                                                                                         pickedGeom.Body.
                                                                                             GetLocalPosition(point),
                                                                                         point, 20, 10);
                }
            }
            else if (input.LastMouseState.LeftButton == ButtonState.Pressed &&
                     input.CurrentMouseState.LeftButton == ButtonState.Released)
            {
                //destroy mouse spring
                if (mousePickSpring != null && mousePickSpring.IsDisposed == false)
                {
                    mousePickSpring.Dispose();
                    mousePickSpring = null;
                }
            }

            //move anchor point
            if (input.CurrentMouseState.LeftButton == ButtonState.Pressed && mousePickSpring != null)
            {
                mousePickSpring.WorldAttachPoint = point;
            }
        }
#endif

        public string GetTitle()
        {
            return "Stacked Objects";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows the stacking stability of the engine.");
            sb.AppendLine("It shows a stack of rectangular bodies stacked in");
            sb.AppendLine("the shape of a pyramid.");
            sb.AppendLine("");
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate : left and right triggers");
            sb.AppendLine("  -Move : left thumbstick");
            sb.AppendLine("");
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate : left and right arrows");
            sb.AppendLine("  -Move : A,S,D,W");
            sb.AppendLine("");
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }
    }
}