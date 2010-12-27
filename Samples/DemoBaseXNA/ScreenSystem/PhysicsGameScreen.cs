using System;
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
        public DebugViewXNA DebugView;

        private FixedMouseJoint _fixedMouseJoint;
        private Border _border;

        protected PhysicsGameScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.75);
            TransitionOffTime = TimeSpan.FromSeconds(0.75);
        }

        public override void LoadContent()
        {
            base.LoadContent();
            if (World == null)
                return;

            //We enable diagnostics to show get values for our performance counters.
            Settings.EnableDiagnostics = true;

            DebugView = new DebugViewXNA(World);
            DebugView.AppendFlags(DebugViewFlags.TexturedShape);
            DebugView.RemoveFlags(DebugViewFlags.Shape);
            DebugView.RemoveFlags(DebugViewFlags.Joint);

            DebugView.DefaultShapeColor = Color.White;
            DebugView.SleepingShapeColor = Color.LightGray;

            DebugView.LoadContent(ScreenManager.GraphicsDevice, ScreenManager.ContentManager);
            Vector2 gameWorld =
                Camera2D.ConvertScreenToWorld(new Vector2(ScreenManager.Camera.ScreenWidth,
                                                          ScreenManager.Camera.ScreenHeight));
            _border = new Border(World, gameWorld.X, gameWorld.Y, 1f);

            ScreenManager.Camera.ProjectionUpdated += UpdateScreen;

            // Loading may take a while... so prevent the game from "catching up" once we finished loading
            ScreenManager.Game.ResetElapsedTime();
        }

        private void UpdateScreen()
        {
            if (World != null)
            {
                Vector2 gameWorld =
                    Camera2D.ConvertScreenToWorld(new Vector2(ScreenManager.Camera.ScreenWidth,
                                                              ScreenManager.Camera.ScreenHeight));
                _border.ResetBorder(gameWorld.X, gameWorld.Y, 1f);
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!coveredByOtherScreen && !otherScreenHasFocus && World != null)
            {
                // variable time step but never less then 30 Hz
                World.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, (1f / 30f)));
                DebugView.Update(gameTime);
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(InputHelper input)
        {
            //Xbox
            if (input.IsNewButtonPress(Buttons.Start))
            {
                EnableOrDisableFlag(DebugViewFlags.Shape);
                EnableOrDisableFlag(DebugViewFlags.DebugPanel);
                EnableOrDisableFlag(DebugViewFlags.PerformanceGraph);
            }
            if (input.IsNewButtonPress(Buttons.Back))
            {
                ExitScreen();
            }

            //Windows
            if (input.IsNewKeyPress(Keys.F1))
            {
                EnableOrDisableFlag(DebugViewFlags.Shape);
            }
            else if (input.IsNewKeyPress(Keys.F2))
            {
                EnableOrDisableFlag(DebugViewFlags.DebugPanel);
            }
            else if (input.IsNewKeyPress(Keys.F3))
            {
                EnableOrDisableFlag(DebugViewFlags.PerformanceGraph);
            }
            else if (input.IsNewKeyPress(Keys.F4))
            {
                EnableOrDisableFlag(DebugViewFlags.AABB);
            }
            else if (input.IsNewKeyPress(Keys.F5))
            {
                EnableOrDisableFlag(DebugViewFlags.CenterOfMass);
            }
            else if (input.IsNewKeyPress(Keys.F6))
            {
                EnableOrDisableFlag(DebugViewFlags.Joint);
            }
            else if (input.IsNewKeyPress(Keys.F7))
            {
                EnableOrDisableFlag(DebugViewFlags.ContactPoints);
                EnableOrDisableFlag(DebugViewFlags.ContactNormals);
            }
            else if (input.IsNewKeyPress(Keys.F8))
            {
                EnableOrDisableFlag(DebugViewFlags.PolygonPoints);
            }

            if (input.IsNewKeyPress(Keys.Escape))
            {
                ExitScreen();
            }

            if (World != null)
            {
#if XBOX
                GamePad(input.CurrentGamePadState, input.LastGamePadState);
#else
                Mouse(input);
#endif
            }

            base.HandleInput(input);
        }

        private void EnableOrDisableFlag(DebugViewFlags flag)
        {
            if ((DebugView.Flags & flag) == flag)
                DebugView.RemoveFlags(flag);
            else
                DebugView.AppendFlags(flag);
        }

#if XBOX
        private void GamePad(GamePadState state, GamePadState oldState)
        {
            Vector2 worldPosition = Camera2D.ConvertScreenToWorld(new Vector2(state.ThumbSticks.Left.X, state.ThumbSticks.Left.Y));

            if (state.Buttons.A == ButtonState.Released && oldState.Buttons.A == ButtonState.Pressed)
            {
                MouseUp();
            }
            else if (state.Buttons.A == ButtonState.Pressed && oldState.Buttons.A == ButtonState.Released)
            {
                MouseDown(worldPosition);
            }

            GamePadMove(worldPosition);
        }

        private void GamePadMove(Vector2 p)
        {
            if (_fixedMouseJoint != null)
            {
                _fixedMouseJoint.WorldAnchorB = p;
            }
        }

#else
        private void Mouse(InputHelper state)
        {
            Vector2 position = Camera2D.ConvertScreenToWorld(state.MousePosition);

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
                _fixedMouseJoint.WorldAnchorB = position;
            }
        }
#endif

        private void MouseDown(Vector2 p)
        {
            if (_fixedMouseJoint != null)
            {
                return;
            }

            Fixture savedFixture = World.TestPoint(p);

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
                DebugView.RenderDebugData(ref Camera2D.Projection, ref Camera2D.View);
            }

            base.Draw(gameTime);
        }
    }
}