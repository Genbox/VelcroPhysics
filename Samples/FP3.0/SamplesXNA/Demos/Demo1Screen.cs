using System.Collections.Generic;
using System.Text;
using DemoBaseXNA.DrawingSystem;
using DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.DebugViewXNA;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SamplesXNA.Demo1
{
    internal class Demo1Screen : GameScreen
    {
        private Body _rectangleBody;
        private PolygonShape _rectangleShape;
        private Texture2D _rectangleTexture;

        public override void Initialize()
        {
            PhysicsSimulator = new World(new Vector2(0, -50), true);
            PhysicsSimulatorView = new DebugViewXNA(PhysicsSimulator);
            DebugViewEnabled = true;

            //PhysicsSimulatorView.AppendFlags(DebugViewFlags.CenterOfMass);

            base.Initialize();
        }

        public override void LoadContent()
        {
            //load texture that will visually represent the physics body
            //_rectangleTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 100, 100, Color.White,
            //                                                         Color.Black);

            //Vertices verts = PolygonTools.CreateGear(1, 12, 0.65f, 0.25f);
            Vertices verts = PolygonTools.CreateRoundedRectangle(2, 1f, 0.5f, 0.5f, 5);
            //Vertices verts = PolygonTools.CreateCapsule(4, 0.5f, 32, 0.5f, 32);
            //Vertices verts = PolygonTools.CreateRectangle(0.5f, 0.25f);

            verts = BooleanTools.Simplify(verts);

            List<Vertices> decomposedVerts = EarclipDecomposer.ConvexPartition(verts);

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    //use the body factory to create the physics body
                    _rectangleBody = BodyFactory.CreateBody(PhysicsSimulator);
                    _rectangleBody.BodyType = BodyType.Dynamic;
                    _rectangleBody.Position = new Vector2(i * 2f - 15f, j * 2f - 15f);
                    
                        //_rectangleShape = new CircleShape(0.5f, 1);
                    foreach (var item in decomposedVerts)
                    {
                        _rectangleShape = new PolygonShape(item, 1);
                        _rectangleBody.CreateFixture(_rectangleShape);
                        _rectangleBody.FixtureList[0].Friction = 0.5f;
                    }
                        
                }
            }

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            //_rectangleBrush.Draw(ScreenManager.SpriteBatch, _rectangleBody.Position, _rectangleBody.Rotation);
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
                //HandleGamePadInput(input);
            }
            else
            {
                //HandleKeyboardInput(input);
            }

            base.HandleInput(input);
        }

        private void HandleGamePadInput(InputState input)
        {
            Vector2 force = 50 * input.CurrentGamePadState.ThumbSticks.Left;
            force.Y = -force.Y;
            _rectangleBody.ApplyForce(force);

            float rotation = -1000 * input.CurrentGamePadState.Triggers.Left;
            _rectangleBody.ApplyTorque(rotation);

            rotation = 1000 * input.CurrentGamePadState.Triggers.Right;
            _rectangleBody.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
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

            _rectangleBody.ApplyForce(force);

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

            _rectangleBody.ApplyTorque(torque);
        }

        public static string GetTitle()
        {
            return "Demo1: A Single Body";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with no geometry");
            sb.AppendLine("attached. Note that it does not collide with the borders.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate: left and right triggers");
            sb.AppendLine("  -Move: left thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate: left and right arrows");
            sb.AppendLine("  -Move: A,S,D,W");
            return sb.ToString();
        }
    }
}
