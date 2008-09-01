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

namespace FarseerGames.FarseerPhysicsDemos.Demos.Demo8
{
    public class Demo8Screen : GameScreen
    {
        private readonly LineBrush lineBrush = new LineBrush(1, Color.Black); //used to draw spring on mouse grab

        private readonly PhysicsSimulator physicsSimulator;
        private readonly PhysicsSimulatorView physicsSimulatorView;

        private Agent agent;
        private Circles[] blackCircles;

        private Circles[] blueCircles;
        private Border border;
        private ContentManager contentManager;
        private bool debugViewEnabled;
        private bool firstRun = true;
        private Circles[] greenCircles;
        private FixedLinearSpring mousePickSpring;
        private Geom pickedGeom;
        private Circles[] redCircles;
        public bool updatedOnce;

        public Demo8Screen()
        {
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            physicsSimulator.MaxContactsToDetect = 2;
            //for stacked objects, simultaneous collision are the bottlenecks so limit them to 2 per geometric pair.
            physicsSimulatorView = new PhysicsSimulatorView(physicsSimulator);
        }

        public override void LoadContent()
        {
            if (contentManager == null) contentManager = new ContentManager(ScreenManager.Game.Services);
            lineBrush.Load(ScreenManager.GraphicsDevice);
            physicsSimulatorView.LoadContent(ScreenManager.GraphicsDevice, contentManager);

            int borderWidth = (int) (ScreenManager.ScreenHeight*.05f);
            border = new Border(ScreenManager.ScreenWidth, ScreenManager.ScreenHeight, borderWidth,
                                ScreenManager.ScreenCenter);
            border.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            agent = new Agent(ScreenManager.ScreenCenter);
            agent.CollisionCategory = CollisionCategories.Cat5;
            agent.CollidesWith = CollisionCategories.All & ~CollisionCategories.Cat4;
                //collide with all but Cat5(black)
            agent.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            LoadCircles();
        }

        private void LoadCircles()
        {
            redCircles = new Circles[10];
            blueCircles = new Circles[10];
            blackCircles = new Circles[10];
            greenCircles = new Circles[10];


            Vector2 startPosition = new Vector2(50, 50);
            Vector2 endPosition = new Vector2(ScreenManager.ScreenWidth - 50, 50);

#if XBOX
            int balls = 20;
#else
            const int balls = 40;
#endif
            const float ySpacing = 12;
            for (int i = 0; i < redCircles.Length; i++)
            {
                redCircles[i] = new Circles(startPosition, endPosition, balls, 5, new Color(200, 0, 0, 175), Color.Black);
                redCircles[i].CollisionCategories = (CollisionCategories.Cat1);
                redCircles[i].CollidesWith = (CollisionCategories.Cat5);
                redCircles[i].Load(ScreenManager.GraphicsDevice, physicsSimulator);
                startPosition.Y += ySpacing;
                endPosition.Y += ySpacing;
            }

            startPosition.Y += 2*ySpacing;
            endPosition.Y += 2*ySpacing;

            for (int i = 0; i < blueCircles.Length; i++)
            {
                blueCircles[i] = new Circles(startPosition, endPosition, balls, 5, new Color(0, 0, 200, 175),
                                             Color.Black);
                blueCircles[i].CollisionCategories = (CollisionCategories.Cat3);
                blueCircles[i].CollidesWith = (CollisionCategories.Cat5);
                blueCircles[i].Load(ScreenManager.GraphicsDevice, physicsSimulator);
                startPosition.Y += ySpacing;
                endPosition.Y += ySpacing;
            }

            startPosition.Y += 12*ySpacing;
            endPosition.Y += 12*ySpacing;

            for (int i = 0; i < greenCircles.Length; i++)
            {
                greenCircles[i] = new Circles(startPosition, endPosition, balls, 5, new Color(0, 200, 0, 175),
                                              Color.Black);
                greenCircles[i].CollisionCategories = (CollisionCategories.Cat2);
                greenCircles[i].CollidesWith = (CollisionCategories.Cat5);
                greenCircles[i].Load(ScreenManager.GraphicsDevice, physicsSimulator);
                startPosition.Y += ySpacing;
                endPosition.Y += ySpacing;
            }

            startPosition.Y += 2*ySpacing;
            endPosition.Y += 2*ySpacing;

            for (int i = 0; i < blackCircles.Length; i++)
            {
                blackCircles[i] = new Circles(startPosition, endPosition, balls, 5, new Color(0, 0, 0, 175), Color.Black);
                blackCircles[i].CollisionCategories = (CollisionCategories.Cat4);
                blackCircles[i].CollidesWith = (CollisionCategories.Cat5);
                blackCircles[i].Load(ScreenManager.GraphicsDevice, physicsSimulator);
                startPosition.Y += ySpacing;
                endPosition.Y += ySpacing;
            }
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
            border.Draw(ScreenManager.SpriteBatch);
            agent.Draw(ScreenManager.SpriteBatch);

            for (int i = 0; i < redCircles.Length; i++)
            {
                redCircles[i].Draw(ScreenManager.SpriteBatch);
            }

            for (int i = 0; i < blueCircles.Length; i++)
            {
                blueCircles[i].Draw(ScreenManager.SpriteBatch);
            }


            for (int i = 0; i < blueCircles.Length; i++)
            {
                greenCircles[i].Draw(ScreenManager.SpriteBatch);
            }


            for (int i = 0; i < blueCircles.Length; i++)
            {
                blackCircles[i].Draw(ScreenManager.SpriteBatch);
            }

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
            return "Broad Phase Collision Stress Test";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo simply stress tests broad phase collision");
            sb.AppendLine("In this demo:");
            sb.AppendLine("Narrow phase collision is disabled between");
            sb.AppendLine(" all balls.");
            sb.AppendLine("");
            sb.AppendLine("");
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