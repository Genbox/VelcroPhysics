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

namespace FarseerGames.FarseerPhysicsDemos.Demos.Demo5
{
    public class Demo5Screen : GameScreen
    {
        private readonly LineBrush lineBrush = new LineBrush(1, Color.Black); //used to draw spring on mouse grab

        private readonly PhysicsSimulator physicsSimulator;
        private readonly PhysicsSimulatorView physicsSimulatorView;

        private Agent agent;
        private Circles blackCircles1;
        private Circles blackCircles2;
        private Circles blackCircles3;

        private Circles blueCircles1;
        private Circles blueCircles2;
        private Circles blueCircles3;
        private Border border;
        private ContentManager contentManager;
        private bool debugViewEnabled;
        private bool firstRun = true;

        private Circles greenCircles1;
        private Circles greenCircles2;
        private Circles greenCircles3;
        private FixedLinearSpring mousePickSpring;
        private Geom pickedGeom;
        private Circles redCircles1;
        private Circles redCircles2;
        private Circles redCircles3;
        public bool updatedOnce;

        public Demo5Screen()
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
            agent.CollisionCategory = Enums.CollisionCategories.Cat5;
            agent.CollidesWith = Enums.CollisionCategories.All & ~Enums.CollisionCategories.Cat4;
                //collide with all but Cat5(black)
            agent.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            LoadCircles();
        }

        private void LoadCircles()
        {
            //Cat1=Red, Cat2=Green, Cat3=Blue, Cat4=Black, Cat5=Agent
            Vector2 startPosition = new Vector2(100, 100);
            Vector2 endPosition = new Vector2(100, ScreenManager.ScreenHeight - 100);
            redCircles1 = new Circles(startPosition, endPosition, 15, 15, new Color(200, 0, 0, 175), Color.Black);
            redCircles1.CollisionCategories = (Enums.CollisionCategories.Cat1);
            redCircles1.CollidesWith = (Enums.CollisionCategories.Cat1 | Enums.CollisionCategories.Cat4 |
                                        Enums.CollisionCategories.Cat5);
            redCircles1.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            startPosition = new Vector2(200, 200);
            endPosition = new Vector2(200, ScreenManager.ScreenHeight - 200);
            redCircles2 = new Circles(startPosition, endPosition, 15, 12, new Color(200, 0, 0, 175), Color.Black);
            redCircles2.CollisionCategories = (Enums.CollisionCategories.Cat1);
            redCircles2.CollidesWith = (Enums.CollisionCategories.Cat1 | Enums.CollisionCategories.Cat4 |
                                        Enums.CollisionCategories.Cat5);
            redCircles2.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            startPosition = new Vector2(300, 300);
            endPosition = new Vector2(300, ScreenManager.ScreenHeight - 300);
            redCircles3 = new Circles(startPosition, endPosition, 10, 9, new Color(200, 0, 0, 175), Color.Black);
            redCircles3.CollisionCategories = (Enums.CollisionCategories.Cat1);
            redCircles3.CollidesWith = (Enums.CollisionCategories.Cat1 | Enums.CollisionCategories.Cat4 |
                                        Enums.CollisionCategories.Cat5);
            redCircles3.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            startPosition = new Vector2(200, 100);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 200, 100);
            greenCircles1 = new Circles(startPosition, endPosition, 15, 15, new Color(0, 200, 0, 175), Color.Black);
            greenCircles1.CollisionCategories = (Enums.CollisionCategories.Cat2);
            greenCircles1.CollidesWith = (Enums.CollisionCategories.Cat2 | Enums.CollisionCategories.Cat4 |
                                          Enums.CollisionCategories.Cat5);
            greenCircles1.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            startPosition = new Vector2(300, 200);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 300, 200);
            greenCircles2 = new Circles(startPosition, endPosition, 15, 12, new Color(0, 200, 0, 175), Color.Black);
            greenCircles2.CollisionCategories = (Enums.CollisionCategories.Cat2);
            greenCircles2.CollidesWith = (Enums.CollisionCategories.Cat2 | Enums.CollisionCategories.Cat4 |
                                          Enums.CollisionCategories.Cat5);
            greenCircles2.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            startPosition = new Vector2(400, 300);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 400, 300);
            greenCircles3 = new Circles(startPosition, endPosition, 10, 9, new Color(0, 200, 0, 175), Color.Black);
            greenCircles3.CollisionCategories = (Enums.CollisionCategories.Cat2);
            greenCircles3.CollidesWith = (Enums.CollisionCategories.Cat2 | Enums.CollisionCategories.Cat4 |
                                          Enums.CollisionCategories.Cat5);
            greenCircles3.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            startPosition = new Vector2(ScreenManager.ScreenWidth - 100, 100);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 100, ScreenManager.ScreenHeight - 100);
            blueCircles1 = new Circles(startPosition, endPosition, 15, 15, new Color(0, 0, 200, 175), Color.Black);
            blueCircles1.CollisionCategories = (Enums.CollisionCategories.Cat3);
            blueCircles1.CollidesWith = (Enums.CollisionCategories.Cat3 | Enums.CollisionCategories.Cat4 |
                                         Enums.CollisionCategories.Cat5);
            blueCircles1.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            startPosition = new Vector2(ScreenManager.ScreenWidth - 200, 200);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 200, ScreenManager.ScreenHeight - 200);
            blueCircles2 = new Circles(startPosition, endPosition, 15, 12, new Color(0, 0, 200, 175), Color.Black);
            blueCircles2.CollisionCategories = (Enums.CollisionCategories.Cat3);
            blueCircles2.CollidesWith = (Enums.CollisionCategories.Cat3 | Enums.CollisionCategories.Cat4 |
                                         Enums.CollisionCategories.Cat5);
            blueCircles2.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            startPosition = new Vector2(ScreenManager.ScreenWidth - 300, 300);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 300, ScreenManager.ScreenHeight - 300);
            blueCircles3 = new Circles(startPosition, endPosition, 10, 9, new Color(0, 0, 200, 175), Color.Black);
            blueCircles3.CollisionCategories = (Enums.CollisionCategories.Cat3);
            blueCircles3.CollidesWith = (Enums.CollisionCategories.Cat3 | Enums.CollisionCategories.Cat4 |
                                         Enums.CollisionCategories.Cat5);
            blueCircles3.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            startPosition = new Vector2(200, ScreenManager.ScreenHeight - 100);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 200, ScreenManager.ScreenHeight - 100);
            blackCircles1 = new Circles(startPosition, endPosition, 15, 15, new Color(0, 0, 0, 200), Color.Black);
            blackCircles1.CollisionCategories = Enums.CollisionCategories.Cat4;
            blackCircles1.CollidesWith = Enums.CollisionCategories.All & ~Enums.CollisionCategories.Cat5;
            //Collide with all but Cat5
            blackCircles1.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            startPosition = new Vector2(300, ScreenManager.ScreenHeight - 200);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 300, ScreenManager.ScreenHeight - 200);
            blackCircles2 = new Circles(startPosition, endPosition, 15, 12, new Color(0, 0, 0, 200), Color.Black);
            blackCircles2.CollisionCategories = Enums.CollisionCategories.Cat4;
            blackCircles2.CollidesWith = Enums.CollisionCategories.All & ~Enums.CollisionCategories.Cat5;
            //Collide with all but Cat5
            blackCircles2.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            startPosition = new Vector2(400, ScreenManager.ScreenHeight - 300);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 400, ScreenManager.ScreenHeight - 300);
            blackCircles3 = new Circles(startPosition, endPosition, 10, 9, new Color(0, 0, 0, 200), Color.Black);
            blackCircles3.CollisionCategories = Enums.CollisionCategories.Cat4;
            blackCircles3.CollidesWith = Enums.CollisionCategories.All & ~Enums.CollisionCategories.Cat5;
            //Collide with all but Cat5
            blackCircles3.Load(ScreenManager.GraphicsDevice, physicsSimulator);
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

            redCircles1.Draw(ScreenManager.SpriteBatch);
            redCircles2.Draw(ScreenManager.SpriteBatch);
            redCircles3.Draw(ScreenManager.SpriteBatch);

            greenCircles1.Draw(ScreenManager.SpriteBatch);
            greenCircles2.Draw(ScreenManager.SpriteBatch);
            greenCircles3.Draw(ScreenManager.SpriteBatch);

            blueCircles1.Draw(ScreenManager.SpriteBatch);
            blueCircles2.Draw(ScreenManager.SpriteBatch);
            blueCircles3.Draw(ScreenManager.SpriteBatch);

            blackCircles1.Draw(ScreenManager.SpriteBatch);
            blackCircles2.Draw(ScreenManager.SpriteBatch);
            blackCircles3.Draw(ScreenManager.SpriteBatch);

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
            return "Collision Categories";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to setup complex collision");
            sb.AppendLine("scenerios.");
            sb.AppendLine("In this demo:");
            sb.AppendLine("-Red, Green, and Blue are set to only collide with");
            sb.AppendLine(" their own color.");
            sb.AppendLine("-Black is set to collide with itself, Red, Green, ");
            sb.AppendLine(" and Blue.");
            sb.AppendLine("-The 'Agent' (the cross thing) is set to collide");
            sb.AppendLine(" with all but Black");
            sb.AppendLine("");
            sb.AppendLine("NOTE: If two objects define conflicting");
            sb.AppendLine("collision status, collide wins over not colliding.");
            sb.AppendLine("This is the case with Black vs. the Red, Green, ");
            sb.AppendLine("and Blue circles");
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