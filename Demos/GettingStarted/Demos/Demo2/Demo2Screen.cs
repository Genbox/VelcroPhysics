using System.Text;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.FarseerPhysicsDemos.Demos.Demo2
{
    public class Demo2Screen : GameScreen
    {
        //declare variables
        private readonly LineBrush lineBrush = new LineBrush(1, Color.Black); //used to draw spring on mouse grab

        private readonly PhysicsSimulator physicsSimulator;
        private readonly PhysicsSimulatorView physicsSimulatorView;

        private Body circleBody;

        private Geom circleGeom;
        private Vector2 circleOrigin;
        private Texture2D circleTexture;
        private ContentManager contentManager;
        private bool debugViewEnabled;
        private bool firstRun = true;
        private FixedLinearSpring mousePickSpring;
        private Geom pickedGeom;
        private Body rectangleBody;
        private Geom rectangleGeom;
        private Vector2 rectangleOrigin;
        private Texture2D rectangleTexture;
        public bool updatedOnce;


        public Demo2Screen()
        {
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            physicsSimulatorView = new PhysicsSimulatorView(physicsSimulator);
        }

        public override void LoadContent()
        {
            if (contentManager == null) contentManager = new ContentManager(ScreenManager.Game.Services);
            lineBrush.Load(ScreenManager.GraphicsDevice);
            physicsSimulatorView.LoadContent(ScreenManager.GraphicsDevice, contentManager);

            rectangleTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 128, 128, Color.Gold,
                                                                    Color.Black);
            rectangleOrigin = new Vector2(rectangleTexture.Width/2f, rectangleTexture.Height/2f);

            circleTexture = DrawingHelper.CreateCircleTexture(ScreenManager.GraphicsDevice, 64, Color.White, Color.Black);
            circleOrigin = new Vector2(circleTexture.Width/2f, circleTexture.Height/2f);

            rectangleBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody.Position = new Vector2(256, 384);
            rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody, 128, 128);

            circleBody = BodyFactory.Instance.CreateCircleBody(physicsSimulator, 64, 1); //fix 
            circleBody.Position = new Vector2(725, 384);
            circleGeom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, circleBody, 64, 20);
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
            ScreenManager.SpriteBatch.Draw(circleTexture, circleGeom.Position, null, Color.White, circleGeom.Rotation,
                                           circleOrigin, 1, SpriteEffects.None, 0);
            ScreenManager.SpriteBatch.Draw(rectangleTexture, rectangleGeom.Position, null, Color.White,
                                           rectangleGeom.Rotation, rectangleOrigin, 1, SpriteEffects.None, 0);

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

            Vector2 force = 50*input.CurrentGamePadState.ThumbSticks.Left;
            force.Y = -force.Y;
            rectangleBody.ApplyForce(force);

            float rotation = -1000*input.CurrentGamePadState.Triggers.Left;
            rectangleBody.ApplyTorque(rotation);

            rotation = 1000*input.CurrentGamePadState.Triggers.Right;
            rectangleBody.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
            if (!input.LastKeyboardState.IsKeyDown(Keys.F1) && input.CurrentKeyboardState.IsKeyDown(Keys.F1))
            {
                debugViewEnabled = !debugViewEnabled;
                physicsSimulator.EnableDiagnostics = debugViewEnabled;
            }
            const float forceAmount = 50;
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

            rectangleBody.ApplyForce(force);

            const float torqueAmount = 1000;
            float torque = 0;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                torque -= torqueAmount;
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right))
            {
                torque += torqueAmount;
            }
            rectangleBody.ApplyTorque(torque);
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
            return "Two Bodies With Geom";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows two bodies each with a single geometry");
            sb.AppendLine("object attached.");
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