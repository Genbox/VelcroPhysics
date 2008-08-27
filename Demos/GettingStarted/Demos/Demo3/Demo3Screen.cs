using System.Text;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.FarseerPhysicsDemos.Demos.Demo3
{
    public class Demo3Screen : GameScreen
    {
        private readonly LineBrush lineBrush = new LineBrush(1, Color.Black); //used to draw spring on mouse grab

        private readonly PhysicsSimulator physicsSimulator;
        private readonly PhysicsSimulatorView physicsSimulatorView;
        private Body agentBody;
        private Vector2 agentCrossBeamOrigin;
        private Texture2D agentCrossBeamTexture;
        private Geom[] agentGeom;
        private Vector2 agentOrigin;
        private Texture2D agentTexture;
        private ContentManager contentManager;
        private bool debugViewEnabled;
        private bool firstRun = true;

        private Body floorBody;
        private Geom floorGeom;
        private Vector2 floorOrigin;
        private Texture2D floorTexture;
        private FixedLinearSpring mousePickSpring;

        private Body[] obstacleBody;
        private Geom[] obstacleGeom;
        private Vector2 obstacleOrigin;
        private Texture2D obstacleTexture;
        private Geom pickedGeom;
        public float simTime;
        public bool updatedOnce;

        public Demo3Screen()
        {
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            physicsSimulatorView = new PhysicsSimulatorView(physicsSimulator);
            physicsSimulatorView.EnableGridView = false;
        }

        public override void LoadContent()
        {
            if (contentManager == null) contentManager = new ContentManager(ScreenManager.Game.Services);
            lineBrush.Load(ScreenManager.GraphicsDevice);
            physicsSimulatorView.LoadContent(ScreenManager.GraphicsDevice, contentManager);

            LoadAgent();
            LoadFloor();
            LoadObstacles();
        }

        public void LoadAgent()
        {
            agentTexture = DrawingHelper.CreateCircleTexture(ScreenManager.GraphicsDevice, 16, Color.Gold, Color.Black);
            agentOrigin = new Vector2(agentTexture.Width/2f, agentTexture.Height/2f);

            agentCrossBeamTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 16, 120,
                                                                         Color.DarkGray, Color.Black);
            agentCrossBeamOrigin = new Vector2(agentCrossBeamTexture.Width/2f, agentCrossBeamTexture.Height/2f);

            agentBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 80, 80, 5);
            agentBody.Position = new Vector2(ScreenManager.ScreenCenter.X, 110);

            agentGeom = new Geom[7];
            agentGeom[0] = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, agentBody, 16, 10,
                                                                 new Vector2(-40, -40), 0);
            agentGeom[0].RestitutionCoefficient = .2f;
            agentGeom[0].FrictionCoefficient = .2f;
            agentGeom[0].CollisionGroup = 1;
            agentGeom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0],
                                                           new Vector2(-40, 40), 0);
            agentGeom[2] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0],
                                                           new Vector2(40, -40), 0);
            agentGeom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0],
                                                           new Vector2(40, 40), 0);
            agentGeom[4] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0], new Vector2(0, 0),
                                                           0);

            agentGeom[5] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, agentBody, 16, 130, Vector2.Zero,
                                                                    MathHelper.PiOver4);
            agentGeom[5].CollisionGroup = 1;
            agentGeom[6] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, agentBody, 16, 130, Vector2.Zero,
                                                                    -MathHelper.PiOver4);
            agentGeom[6].CollisionGroup = 1;
        }

        public void LoadFloor()
        {
            //load texture that will visually represent the physics body
            floorTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, ScreenManager.ScreenWidth,
                                                                100, Color.White, Color.Black);
            floorOrigin = new Vector2(floorTexture.Width/2f, floorTexture.Height/2f);

            //use the body factory to create the physics body
            floorBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, ScreenManager.ScreenWidth, 100, 1);
            floorBody.IsStatic = true;
            floorGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, floorBody, ScreenManager.ScreenWidth,
                                                                 100);
            floorGeom.RestitutionCoefficient = .2f;
            floorGeom.FrictionCoefficient = .2f;
            floorBody.Position = new Vector2(ScreenManager.ScreenCenter.X, ScreenManager.ScreenHeight - 50);
        }

        public void LoadObstacles()
        {
            obstacleTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 128, 32, Color.White,
                                                                   Color.Black);
            obstacleOrigin = new Vector2(obstacleTexture.Width/2f, obstacleTexture.Height/2f);

            obstacleBody = new Body[5];
            obstacleGeom = new Geom[5];
            for (int i = 0; i < obstacleBody.Length; i++)
            {
                obstacleBody[i] = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 32, 1);
                obstacleBody[i].IsStatic = true;

                if (i == 0)
                {
                    obstacleGeom[i] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, obstacleBody[i], 128,
                                                                               32);
                    obstacleGeom[i].RestitutionCoefficient = .2f;
                    obstacleGeom[i].FrictionCoefficient = .2f;
                }
                else
                {
                    obstacleGeom[i] = GeomFactory.Instance.CreateGeom(physicsSimulator, obstacleBody[i], obstacleGeom[0]);
                }
            }

            obstacleBody[0].Position = ScreenManager.ScreenCenter + new Vector2(-50, -200);
            obstacleBody[1].Position = ScreenManager.ScreenCenter + new Vector2(150, -100);
            obstacleBody[2].Position = ScreenManager.ScreenCenter + new Vector2(100, 50);
            obstacleBody[3].Position = ScreenManager.ScreenCenter + new Vector2(-100, 200);
            obstacleBody[4].Position = ScreenManager.ScreenCenter + new Vector2(-170, 0);
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
            ScreenManager.SpriteBatch.Draw(floorTexture, floorBody.Position, null, Color.White, floorBody.Rotation,
                                           floorOrigin, 1, SpriteEffects.None, 0f);
            DrawObstacles();
            DrawAgent();

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

        private void DrawAgent()
        {
            for (int i = 5; i < 7; i++)
            {
                ScreenManager.SpriteBatch.Draw(agentCrossBeamTexture, agentGeom[i].Position, null, Color.White,
                                               agentGeom[i].Rotation, agentCrossBeamOrigin, 1, SpriteEffects.None, 0f);
            }
            for (int i = 0; i < 5; i++)
            {
                ScreenManager.SpriteBatch.Draw(agentTexture, agentGeom[i].Position, null, Color.White,
                                               agentGeom[i].Rotation, agentOrigin, 1, SpriteEffects.None, 0f);
            }
        }

        private void DrawObstacles()
        {
            for (int i = 0; i < obstacleBody.Length; i++)
            {
                ScreenManager.SpriteBatch.Draw(obstacleTexture, obstacleBody[i].Position, null, Color.White,
                                               obstacleBody[i].Rotation, obstacleOrigin, 1, SpriteEffects.None, 0f);
            }
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

            Vector2 force = 800*input.CurrentGamePadState.ThumbSticks.Left;
            force.Y = -force.Y;
            agentBody.ApplyForce(force);

            float rotation = -8000*input.CurrentGamePadState.Triggers.Left;
            agentBody.ApplyTorque(rotation);

            rotation = 8000*input.CurrentGamePadState.Triggers.Right;
            agentBody.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
            if (!input.LastKeyboardState.IsKeyDown(Keys.F1) && input.CurrentKeyboardState.IsKeyDown(Keys.F1))
            {
                debugViewEnabled = !debugViewEnabled;
                physicsSimulator.EnableDiagnostics = debugViewEnabled;
            }

            const float forceAmount = 800;
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

            agentBody.ApplyForce(force);

            const float torqueAmount = 8000;
            float torque = 0;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                torque -= torqueAmount;
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right))
            {
                torque += torqueAmount;
            }
            agentBody.ApplyTorque(torque);
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
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Static Bodies And ");
            sb.AppendLine("Offset Geometries");
            return sb.ToString();
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with multiple geometry");
            sb.AppendLine("objects attached.  The yellow circles are offset");
            sb.AppendLine("from the bodies center. The body itself is created");
            sb.AppendLine("using 'CreateRectangleBody' so that it's moment of");
            sb.AppendLine("inertia is that of a rectangle.");
            sb.AppendLine("");
            sb.AppendLine("This demo also shows the use of static bodies.");
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