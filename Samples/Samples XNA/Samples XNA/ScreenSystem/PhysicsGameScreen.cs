using System;
using FarseerPhysics.Common;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.SamplesFramework
{
    public class PhysicsGameScreen : GameScreen
    {
        protected World World;
        protected DebugViewXNA DebugView;

        protected Camera2D Camera;

        private FixedMouseJoint _fixedMouseJoint;

        protected PhysicsGameScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.75);
            TransitionOffTime = TimeSpan.FromSeconds(0.75);
            HasCursor = true;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            //We enable diagnostics to show get values for our performance counters.
            Settings.EnableDiagnostics = true;

            World = new World();

            DebugView = new DebugViewXNA(World);
            //DebugView.RemoveFlags(DebugViewFlags.Shape);  removed for debugging purposes
            //DebugView.RemoveFlags(DebugViewFlags.Joint);  removed for debugging purposes
            DebugView.DefaultShapeColor = Color.White;
            DebugView.SleepingShapeColor = Color.LightGray;
            DebugView.LoadContent(ScreenManager.GraphicsDevice, ScreenManager.Content);

            Camera = new Camera2D(ScreenManager.GraphicsDevice);

            // Loading may take a while... so prevent the game from "catching up" once we finished loading
            ScreenManager.Game.ResetElapsedTime();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!coveredByOtherScreen && !otherScreenHasFocus)
            {
                // variable time step but never less then 30 Hz
                World.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, (1f / 30f)));
            }
            Camera.Update(gameTime);
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            // Control debug view
            if (input.IsNewButtonPress(Buttons.Start))
            {
                EnableOrDisableFlag(DebugViewFlags.Shape);
                EnableOrDisableFlag(DebugViewFlags.DebugPanel);
                EnableOrDisableFlag(DebugViewFlags.PerformanceGraph);
                EnableOrDisableFlag(DebugViewFlags.Joint);
                EnableOrDisableFlag(DebugViewFlags.ContactPoints);
                EnableOrDisableFlag(DebugViewFlags.ContactNormals);
                EnableOrDisableFlag(DebugViewFlags.Controllers);
            }

            if (input.IsNewKeyPress(Keys.F1))
            {
                EnableOrDisableFlag(DebugViewFlags.Shape);
            }
            if (input.IsNewKeyPress(Keys.F2))
            {
                EnableOrDisableFlag(DebugViewFlags.DebugPanel);
                EnableOrDisableFlag(DebugViewFlags.PerformanceGraph);
            }
            if (input.IsNewKeyPress(Keys.F3))
            {
                EnableOrDisableFlag(DebugViewFlags.Joint);
            }
            if (input.IsNewKeyPress(Keys.F4))
            {
                EnableOrDisableFlag(DebugViewFlags.ContactPoints);
                EnableOrDisableFlag(DebugViewFlags.ContactNormals);
            }
            if (input.IsNewKeyPress(Keys.F5))
            {
                EnableOrDisableFlag(DebugViewFlags.PolygonPoints);
            }
            if (input.IsNewKeyPress(Keys.F6))
            {
                EnableOrDisableFlag(DebugViewFlags.Controllers);
            }
            if (input.IsNewKeyPress(Keys.F7))
            {
                EnableOrDisableFlag(DebugViewFlags.CenterOfMass);
            }
            if (input.IsNewKeyPress(Keys.F8))
            {
                EnableOrDisableFlag(DebugViewFlags.AABB);
            }

            Vector2 position = Camera.ConvertScreenToWorld(input.Cursor);

            if ((input.IsNewButtonPress(Buttons.A) ||
                input.IsNewMouseButtonPress(MouseButtons.LeftButton)) &&
                _fixedMouseJoint == null)
            {
                Fixture savedFixture = World.TestPoint(position);
                if (savedFixture != null)
                {
                    Body body = savedFixture.Body;
                    _fixedMouseJoint = new FixedMouseJoint(body, position);
                    _fixedMouseJoint.MaxForce = 1000.0f * body.Mass;
                    World.AddJoint(_fixedMouseJoint);
                    body.Awake = true;
                }
            }

            if ((input.IsNewButtonRelease(Buttons.A) ||
                input.IsNewMouseButtonRelease(MouseButtons.LeftButton)) &&
                _fixedMouseJoint != null)
            {
                World.RemoveJoint(_fixedMouseJoint);
                _fixedMouseJoint = null;
            }

            if (_fixedMouseJoint != null)
            {
                _fixedMouseJoint.WorldAnchorB = position;
            }

            if (input.IsNewButtonPress(Buttons.Back) || input.IsNewKeyPress(Keys.Escape))
            {
                ExitScreen();
            }

            base.HandleInput(input, gameTime);
        }

        private void EnableOrDisableFlag(DebugViewFlags flag)
        {
            if ((DebugView.Flags & flag) == flag)
            {
                DebugView.RemoveFlags(flag);
            }
            else
            {
                DebugView.AppendFlags(flag);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix projection = Camera.Projection;
            Matrix view = Camera.View;

            DebugView.RenderDebugData(ref projection, ref view);
            base.Draw(gameTime);
        }
    }
}