using System;
using System.Text;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysicsDemos.Demos.DemoShare;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.FarseerPhysicsDemos.Demos.Demo6
{
    public class Demo6Screen : GameScreen
    {
        private readonly LineBrush lineBrush = new LineBrush(1, Color.Black); //used to draw spring on mouse grab

        private readonly PhysicsSimulator physicsSimulator;
        private readonly PhysicsSimulatorView physicsSimulatorView;

        private Agent agent;

        private AngularSpringLever angularSpringLever1;
        private AngularSpringLever angularSpringLever2;
        private AngularSpringLever angularSpringLever3;
        private AngularSpringLever angularSpringLever4;
        private Border border;
        private ContentManager contentManager;
        private bool debugViewEnabled;
        private bool firstRun = true;
        private RectanglePlatform floor;
        private Body hangingBody;
        private Geom hangingGeom;
        private Texture2D hangingTexture;
        private FixedLinearSpring mousePickSpring;
        private Geom pickedGeom;
        private RectanglePlatform platform1;
        private const float platform1HeightRatio = .6f;
        private const float platform1WidthRatio = .1f;

        private RectanglePlatform platform2;
        private const float platform2HeightRatio = .7f;
        private const float platform2WidthRatio = .1f;

        private RectanglePlatform platform3;
        private const float platform3HeightRatio = .6f;
        private const float platform3WidthRatio = .1f;
        private SpringRectangleRope springRectangleRope1;
        private SpringRectangleRope springRectangleRope2;
        public bool updatedOnce;

        public Demo6Screen()
        {
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 200));
            physicsSimulator.MaxContactsToDetect = 2;
                //for stacked objects, simultaneous collision are the bottlenecks so limit them to 2 per geometric pair.
            physicsSimulatorView = new PhysicsSimulatorView(physicsSimulator);
            physicsSimulatorView.EnableGridView = false;
            physicsSimulatorView.EnableEdgeView = false;
            physicsSimulatorView.EnableVerticeView = false;
            physicsSimulatorView.EnableAABBView = true;
            physicsSimulatorView.EnableCoordinateAxisView = true;
        }

        public override void LoadContent()
        {
            if (contentManager == null) contentManager = new ContentManager(ScreenManager.Game.Services);
            lineBrush.Load(ScreenManager.GraphicsDevice);

            physicsSimulatorView.LoadContent(ScreenManager.GraphicsDevice, contentManager);

            int borderWidth = (int) (ScreenManager.ScreenHeight*.05f);
            border = new Border(ScreenManager.ScreenWidth + borderWidth*2, ScreenManager.ScreenHeight + borderWidth*2,
                                borderWidth, ScreenManager.ScreenCenter);
            border.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            agent = new Agent(new Vector2(ScreenManager.ScreenCenter.X, 100));
            agent.CollisionCategory = CollisionCategories.Cat5;
            agent.CollidesWith = CollisionCategories.All & ~CollisionCategories.Cat4; //collide with all but Cat5(black)
            agent.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            LoadPlatforms();
        }

        public void LoadPlatforms()
        {
            //platform1
            int width = Convert.ToInt32(ScreenManager.ScreenWidth*platform1WidthRatio);
            int height = Convert.ToInt32(ScreenManager.ScreenHeight*platform1HeightRatio);
            Vector2 position = new Vector2(-5 + width/2f, 5 + ScreenManager.ScreenHeight - height/2f);

            platform1 = new RectanglePlatform(width, height, position, Color.White, Color.Black, 100);
            platform1.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            angularSpringLever1 = new AngularSpringLever();
            angularSpringLever1.AttachPoint = 0;
            angularSpringLever1.RectangleWidth = 200;
            angularSpringLever1.RectangleHeight = 20;
            angularSpringLever1.SpringConstant = 1000000;
            angularSpringLever1.DampningConstant = 5000;
            angularSpringLever1.CollisionGroup = 100;
            Vector2 springPosition = position + new Vector2(width/2f, -height/2f) +
                                     new Vector2(-angularSpringLever1.RectangleHeight - 5, .4f*height);
            angularSpringLever1.Position = springPosition;
            angularSpringLever1.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            //platform 2
            width = Convert.ToInt32(ScreenManager.ScreenWidth*platform2WidthRatio);
            height = Convert.ToInt32(ScreenManager.ScreenHeight*platform2HeightRatio);
            position = new Vector2(ScreenManager.ScreenCenter.X, 5 + ScreenManager.ScreenHeight - height/2f);

            platform2 = new RectanglePlatform(width, height, position, Color.White, Color.Black, 100);
            platform2.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            angularSpringLever2 = new AngularSpringLever();
            angularSpringLever2.AttachPoint = 2;
            angularSpringLever2.RectangleWidth = 200;
            angularSpringLever2.RectangleHeight = 20;
            angularSpringLever2.SpringConstant = 1000000;
            angularSpringLever2.DampningConstant = 5000;
            angularSpringLever2.CollisionGroup = 100;
            springPosition = position + new Vector2(-width/2f, -height/2f) +
                             new Vector2(angularSpringLever2.RectangleHeight + 5, .2f*height);
            angularSpringLever2.Position = springPosition;
            angularSpringLever2.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            angularSpringLever3 = new AngularSpringLever();
            angularSpringLever3.AttachPoint = 0;
            angularSpringLever3.RectangleWidth = 150;
            angularSpringLever3.RectangleHeight = 20;
            angularSpringLever3.SpringConstant = 10000000;
            angularSpringLever3.DampningConstant = 1000;
            angularSpringLever3.CollisionGroup = 100;
            springPosition = position + new Vector2(width/2f, -height/2f) +
                             new Vector2(-angularSpringLever3.RectangleHeight - 5, .1f*height);
            angularSpringLever3.Position = springPosition;
            angularSpringLever3.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            springRectangleRope1 = new SpringRectangleRope();
            springRectangleRope1.Position = springPosition + new Vector2(angularSpringLever3.RectangleWidth - 5, 25);
            springRectangleRope1.RectangleCount = 20;
            springRectangleRope1.RectangleWidth = 15;
            springRectangleRope1.RectangleHeight = 15;
            springRectangleRope1.RectangleMass = .5f;
            springRectangleRope1.SpringLength = 15;
            springRectangleRope1.SpringConstant = 400;
            springRectangleRope1.DampningConstant = 3f;
            springRectangleRope1.Load(ScreenManager.GraphicsDevice, physicsSimulator);
            ControllerFactory.Instance.CreateLinearSpring(physicsSimulator, angularSpringLever3.Body,
                                                                          new Vector2(
                                                                              angularSpringLever3.RectangleWidth/2f, 0),
                                                                          springRectangleRope1.FirstBody, Vector2.Zero,
                                                                          400, 3);

            //platform 3
            width = Convert.ToInt32(ScreenManager.ScreenWidth*platform3WidthRatio);
            height = Convert.ToInt32(ScreenManager.ScreenHeight*platform3HeightRatio);
            position = new Vector2(ScreenManager.ScreenWidth + 5 - width/2f, 5 + ScreenManager.ScreenHeight - height/2f);
            platform3 = new RectanglePlatform(width, height, position, Color.White, Color.Black, 100);
            platform3.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            hangingTexture = DrawingHelper.CreateCircleTexture(ScreenManager.GraphicsDevice, 40, Color.White,
                                                               Color.Black);
            hangingBody = BodyFactory.Instance.CreateCircleBody(physicsSimulator, 40, 1);
            hangingBody.Position = new Vector2(position.X - 200, 200);
            hangingGeom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, hangingBody, 40, 20);
            ControllerFactory.Instance.CreateFixedLinearSpring(physicsSimulator, hangingBody,
                                                                                    new Vector2(0, -35),
                                                                                    new Vector2(position.X - 200, 100),
                                                                                    2, .1f);

            angularSpringLever4 = new AngularSpringLever();
            angularSpringLever4.AttachPoint = 2;
            angularSpringLever4.RectangleWidth = 200;
            angularSpringLever4.RectangleHeight = 20;
            angularSpringLever4.SpringConstant = 1000000;
            angularSpringLever4.DampningConstant = 1000;
            angularSpringLever4.CollisionGroup = 100;
            springPosition = position + new Vector2(-width/2f, -height/2f) +
                             new Vector2(angularSpringLever4.RectangleHeight + 5, .7f*height);
            angularSpringLever4.Position = springPosition;
            angularSpringLever4.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            height = (int) (ScreenManager.ScreenHeight*.05f);
            floor = new RectanglePlatform(ScreenManager.ScreenWidth + 10, height,
                                          new Vector2(ScreenManager.ScreenCenter.X,
                                                      ScreenManager.ScreenHeight + 5 - height/2), Color.White,
                                          Color.Black, 0);
            floor.Load(ScreenManager.GraphicsDevice, physicsSimulator);

            springRectangleRope2 = new SpringRectangleRope();
            springRectangleRope2.Position = new Vector2(ScreenManager.ScreenCenter.X, 100);
            springRectangleRope2.RectangleCount = 20;
            springRectangleRope2.RectangleWidth = 10;
            springRectangleRope2.RectangleHeight = 10;
            springRectangleRope2.RectangleMass = .2f;
            springRectangleRope2.SpringLength = 10;
            springRectangleRope2.SpringConstant = 200;
            springRectangleRope2.DampningConstant = 4f;
            springRectangleRope2.CollisionGroup = 1; //same as agent collision group
            springRectangleRope2.Load(ScreenManager.GraphicsDevice, physicsSimulator);
            ControllerFactory.Instance.CreateLinearSpring(physicsSimulator, agent.Body, Vector2.Zero,
                                                                          springRectangleRope2.FirstBody, Vector2.Zero,
                                                                          200, 4f);
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

            agent.Draw(ScreenManager.SpriteBatch);
            springRectangleRope1.Draw(ScreenManager.SpriteBatch);
            springRectangleRope2.Draw(ScreenManager.SpriteBatch);
            platform1.Draw(ScreenManager.SpriteBatch);
            platform2.Draw(ScreenManager.SpriteBatch);
            platform3.Draw(ScreenManager.SpriteBatch);
            floor.Draw(ScreenManager.SpriteBatch);
            angularSpringLever1.Draw(ScreenManager.SpriteBatch);
            angularSpringLever2.Draw(ScreenManager.SpriteBatch);
            angularSpringLever3.Draw(ScreenManager.SpriteBatch);
            angularSpringLever4.Draw(ScreenManager.SpriteBatch);

            ScreenManager.SpriteBatch.Draw(hangingTexture, hangingGeom.Position, null, Color.White, hangingGeom.Rotation,
                                           new Vector2(hangingTexture.Width/2f, hangingTexture.Height/2f), 1,
                                           SpriteEffects.None, 0);

            border.Draw(ScreenManager.SpriteBatch);

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

            Vector2 force = 3000*input.CurrentGamePadState.ThumbSticks.Left;
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

            const float forceAmount = 3000;
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
            return "Linear and Angular Springs";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows the use of angular and linear");
            sb.AppendLine("springs");
            sb.AppendLine("");
            sb.AppendLine("The levers are connected to the walls using");
            sb.AppendLine("revolute joints and they each have an angular");
            sb.AppendLine("spring attached.");
            sb.AppendLine("The hanging squares are connected by linear");
            sb.AppendLine("springs.");
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