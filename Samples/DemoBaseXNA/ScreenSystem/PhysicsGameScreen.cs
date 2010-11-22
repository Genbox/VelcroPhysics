using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.DebugViews;
using FarseerPhysics.DemoBaseXNA.DemoShare;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.DemoBaseXNA.ScreenSystem
{
    public class PhysicsGameScreen : GameScreen
    {
        public World World;
        private FixedMouseJoint _fixedMouseJoint;
        public DebugViewXNA DebugView;
        public bool DebugViewEnabled;

        public override void LoadContent()
        {
            base.LoadContent();

            if (World == null)
                return;

            DebugView = new DebugViewXNA(World);
            DebugView.DefaultShapeColor = Color.White;
            DebugView.SleepingShapeColor = Color.LightGray;

            DebugViewXNA.LoadContent(ScreenManager.GraphicsDevice, ScreenManager.ContentManager);
            Vector2 gameWorld =
                ScreenManager.Camera.ConvertScreenToWorld(new Vector2(ScreenManager.Camera.ScreenWidth,
                                                                      ScreenManager.Camera.ScreenHeight));
            new Border(World, gameWorld.X, -gameWorld.Y, 1);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!coveredByOtherScreen && !otherScreenHasFocus)
            {
                if (World != null)
                {
                    // variable time step but never less then 30 Hz
                    World.Step(Math.Min((float) gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f,
                                        (1f / 30f)));
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(InputHelper input)
        {
            //Xbox
            if (input.IsNewButtonPress(Buttons.Y))
            {
                DebugViewEnabled = !DebugViewEnabled;
                Settings.EnableDiagnostics = DebugViewEnabled;
            }

            if (input.IsNewButtonPress(Buttons.B))
            {
                ScreenManager.RemoveScreen(this);
            }

            //Windows
            if (input.IsNewKeyPress(Keys.F1))
            {
                DebugViewEnabled = !DebugViewEnabled;
                Settings.EnableDiagnostics = DebugViewEnabled;
            }

            if (input.IsNewKeyPress(Keys.Escape))
            {
                ScreenManager.RemoveScreen(this);
            }

            if (World != null)
            {
#if !XBOX
                Mouse(input);
#else
                GamePad(input.CurrentGamePadState, input.LastGamePadState);
#endif
            }

            base.HandleInput(input);
        }

#if !XBOX
        private void Mouse(InputHelper state)
        {
            Vector2 position = ScreenManager.Camera.ConvertScreenToWorld(state.MousePosition);

            if (state.IsOldButtonPress(MouseButtons.LeftButton))
            {
                MouseUp();
            }
            else if (state.IsNewButtonPress(MouseButtons.LeftButton))
            {
                MouseDown(position);
            }

            if (_fixedMouseJoint != null)
            {
                _fixedMouseJoint.Target = position;
            }
        }
#else
        private void GamePad(GamePadState state, GamePadState oldState)
        {
            Vector3 worldPosition = ScreenManager.GraphicsDevice.Viewport.Unproject(new Vector3(state.ThumbSticks.Left.X, state.ThumbSticks.Left.Y, 0),
                                                                                    Projection, View, Matrix.Identity);
            Vector2 position = new Vector2(worldPosition.X, worldPosition.Y);

            if (state.Buttons.A == ButtonState.Released && oldState.Buttons.A == ButtonState.Pressed)
            {
                MouseUp();
            }
            else if (state.Buttons.A == ButtonState.Pressed && oldState.Buttons.A == ButtonState.Released)
            {
                MouseDown(position);
            }

            GamePadMove(position);
        }

        private void GamePadMove(Vector2 p)
        {
            if (_fixedMouseJoint != null)
            {
                _fixedMouseJoint.Target = p;
            }
        }
#endif

        private void MouseDown(Vector2 p)
        {
            if (_fixedMouseJoint != null)
            {
                return;
            }

            // Make a small box.
            AABB aabb;
            Vector2 d = new Vector2(0.001f, 0.001f);
            aabb.LowerBound = p - d;
            aabb.UpperBound = p + d;

            Fixture savedFixture = null;

            // Query the world for overlapping shapes.
            World.QueryAABB(
                fixture =>
                    {
                        Body body = fixture.Body;
                        if (body.BodyType == BodyType.Dynamic)
                        {
                            bool inside = fixture.TestPoint(ref p);
                            if (inside)
                            {
                                savedFixture = fixture;

                                // We are done, terminate the query.
                                return false;
                            }
                        }

                        // Continue the query.
                        return true;
                    }, ref aabb);

            if (savedFixture != null)
            {
                Body body = savedFixture.Body;
                _fixedMouseJoint = new FixedMouseJoint(body, p);
                _fixedMouseJoint.MaxForce = 1000.0f * body.Mass;
                World.AddJoint(_fixedMouseJoint);
                body.Awake = true;
            }
        }

        private void MouseUp()
        {
            if (_fixedMouseJoint != null)
            {
                World.RemoveJoint(_fixedMouseJoint);
                _fixedMouseJoint = null;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (World != null)
            {
                DebugView.RenderDebugData(ref ScreenManager.Camera.ProjectionMatrix, ref ScreenManager.Camera.ViewMatrix);
            }

            base.Draw(gameTime);
        }
    }
}