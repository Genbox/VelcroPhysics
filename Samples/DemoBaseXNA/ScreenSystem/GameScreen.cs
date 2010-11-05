using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.DemoBaseXNA.DemoShare;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.DemoBaseXNA.ScreenSystem
{
    /// <summary>
    /// Enum describes the screen transition state.
    /// </summary>
    public enum ScreenState
    {
        TransitionOn,
        Active,
        TransitionOff,
        Hidden,
    }

    /// <summary>
    /// A screen is a single layer that has update and draw logic, and which
    /// can be combined with other layers to build up a complex menu system.
    /// For instance the main menu, the options menu, the "are you sure you
    /// want to quit" message box, and the main game itself are all implemented
    /// as screens.
    /// </summary>
    public abstract class GameScreen : IDisposable
    {
        private bool _otherScreenHasFocus;
        public bool firstRun = true;
        private FixedMouseJoint _fixedMouseJoint;

        protected GameScreen()
        {
            ScreenState = ScreenState.TransitionOn;
            TransitionPosition = 1;
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
        }

        public World World { get; set; }

        public DebugViewXNA.DebugViewXNA DebugView { get; set; }

        public Matrix Projection;

        public Matrix View = Matrix.Identity;

        public bool DebugViewEnabled { get; set; }

        /// <summary>
        /// Normally when one screen is brought up over the top of another,
        /// the first screen will transition off to make room for the new
        /// one. This property indicates whether the screen is only a small
        /// popup, in which case screens underneath it do not need to bother
        /// transitioning off.
        /// </summary>
        public bool IsPopup { get; protected set; }

        /// <summary>
        /// Indicates how long the screen takes to
        /// transition on when it is activated.
        /// </summary>
        public TimeSpan TransitionOnTime { get; protected set; }

        /// <summary>
        /// Indicates how long the screen takes to
        /// transition off when it is deactivated.
        /// </summary>
        public TimeSpan TransitionOffTime { get; protected set; }

        /// <summary>
        /// Gets the current position of the screen transition, ranging
        /// from zero (fully active, no transition) to one (transitioned
        /// fully off to nothing).
        /// </summary>
        public float TransitionPosition { get; protected set; }

        /// <summary>
        /// Gets the current alpha of the screen transition, ranging
        /// from 255 (fully active, no transition) to 0 (transitioned
        /// fully off to nothing).
        /// </summary>
        public byte TransitionAlpha
        {
            get { return (byte)(255 - TransitionPosition * 255); }
        }

        /// <summary>
        /// Gets the current screen transition state.
        /// </summary>
        public ScreenState ScreenState { get; protected set; }

        /// <summary>
        /// There are two possible reasons why a screen might be transitioning
        /// off. It could be temporarily going away to make room for another
        /// screen that is on top of it, or it could be going away for good.
        /// This property indicates whether the screen is exiting for real:
        /// if set, the screen will automatically remove itself as soon as the
        /// transition finishes.
        /// </summary>
        public bool IsExiting { get; protected set; }

        /// <summary>
        /// Checks whether this screen is active and can respond to user input.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return !_otherScreenHasFocus &&
                       (ScreenState == ScreenState.TransitionOn ||
                        ScreenState == ScreenState.Active);
            }
        }

        /// <summary>
        /// Gets the manager that this screen belongs to.
        /// </summary>
        public ScreenManager ScreenManager { get; internal set; }

        #region IDisposable Members

        public virtual void Dispose()
        {
        }

        #endregion

        public virtual void Initialize()
        {
            if (World != null)
            {
                DebugView = new DebugViewXNA.DebugViewXNA(World);
                DebugView.DefaultShapeColor = Color.White;
                DebugView.SleepingShapeColor = Color.LightGray;
            }
        }

        private Vector2 _viewCenter = Vector2.Zero;

        public Vector2 ConvertScreenToWorld(int x, int y)
        {
            float viewportWidth = ScreenManager.GraphicsDevice.Viewport.Width;
            float viewportHeight = ScreenManager.GraphicsDevice.Viewport.Height;

            float aspectRatio = ScreenManager.GraphicsDevice.Viewport.AspectRatio;

            Vector2 extents = new Vector2(aspectRatio * 40, 40);

            Vector2 lower = _viewCenter - extents;
            Vector2 upper = _viewCenter + extents;

            float u = x / viewportWidth;
            float v = (viewportHeight - y) / viewportHeight;

            Vector2 p = new Vector2();
            p.X = (1.0f - u) * lower.X + u * upper.X;
            p.Y = (1.0f - v) * lower.Y + v * upper.Y;

            return p;
        }

        /// <summary>
        /// Load graphics content for the screen.
        /// </summary>
        public virtual void LoadContent()
        {
            if (World != null)
            {
                DebugViewXNA.DebugViewXNA.LoadContent(ScreenManager.GraphicsDevice, ScreenManager.ContentManager);

                Vector2 gameWorld = ConvertScreenToWorld(ScreenManager.ScreenWidth, ScreenManager.ScreenHeight);

                new Border(World, gameWorld.X, -gameWorld.Y, 2);
            }
        }

        /// <summary>
        /// Unload content for the screen.
        /// </summary>
        public virtual void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the screen to run logic, such as updating the transition position.
        /// Unlike <see cref="HandleInput"/>, this method is called regardless of whether the screen
        /// is active, hidden, or in the middle of a transition.
        /// </summary>
        public virtual void Update(GameTime gameTime, bool otherScreenHasFocus,
                                   bool coveredByOtherScreen)
        {
            _otherScreenHasFocus = otherScreenHasFocus;

            if (IsExiting)
            {
                // If the screen is going away to die, it should transition off.
                ScreenState = ScreenState.TransitionOff;

                if (!UpdateTransition(gameTime, TransitionOffTime, 1))
                {
                    // When the transition finishes, remove the screen.
                    ScreenManager.RemoveScreen(this);

                    IsExiting = false;
                }
            }
            else if (coveredByOtherScreen)
            {
                // If the screen is covered by another, it should transition off.
                if (UpdateTransition(gameTime, TransitionOffTime, 1))
                {
                    // Still busy transitioning.
                    ScreenState = ScreenState.TransitionOff;
                }
                else
                {
                    // Transition finished!
                    ScreenState = ScreenState.Hidden;
                }
            }
            else
            {
                // Otherwise the screen should transition on and become active.
                if (UpdateTransition(gameTime, TransitionOnTime, -1))
                {
                    // Still busy transitioning.
                    ScreenState = ScreenState.TransitionOn;
                }
                else
                {
                    // Transition finished!
                    ScreenState = ScreenState.Active;
                }
            }

            if (!coveredByOtherScreen && !otherScreenHasFocus)
            {
                if (World != null)
                {
                    // variable time step but never less then 30 Hz
                    World.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f,
                                        (1f / 30f)));
                    Settings.VelocityIterations = 5;
                    Settings.PositionIterations = 3;
                }
            }
        }

        /// <summary>
        /// Helper for updating the screen transition position.
        /// </summary>
        private bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
        {
            // How much should we move by?
            float transitionDelta;

            if (time == TimeSpan.Zero)
                transitionDelta = 1;
            else
                transitionDelta = (float)(gameTime.ElapsedGameTime.TotalMilliseconds /
                                           time.TotalMilliseconds);

            // Update the transition position.
            TransitionPosition += transitionDelta * direction;

            // Did we reach the end of the transition?
            if ((TransitionPosition <= 0) || (TransitionPosition >= 1))
            {
                TransitionPosition = MathHelper.Clamp(TransitionPosition, 0, 1);
                return false;
            }
            // Otherwise we are still busy transitioning.
            return true;
        }

        /// <summary>
        /// Allows the screen to handle user input. Unlike Update, this method
        /// is only called when the screen is active, and not when some other
        /// screen has taken the focus.
        /// </summary>
        public virtual void HandleInput(InputState input)
        {
            //Xbox
            if (input.LastGamePadState.Buttons.Y != ButtonState.Pressed &&
                input.CurrentGamePadState.Buttons.Y == ButtonState.Pressed)
            {
                DebugViewEnabled = !DebugViewEnabled;
                Settings.EnableDiagnostics = DebugViewEnabled;
            }

            if (input.LastGamePadState.Buttons.B != ButtonState.Pressed &&
                input.CurrentGamePadState.Buttons.B == ButtonState.Pressed)
            {
                ScreenManager.GoToMainMenu();
            }

            //Windows
            if (!input.LastKeyboardState.IsKeyDown(Keys.F1) && input.CurrentKeyboardState.IsKeyDown(Keys.F1))
            {
                DebugViewEnabled = !DebugViewEnabled;
                Settings.EnableDiagnostics = DebugViewEnabled;
            }

            if (!input.LastKeyboardState.IsKeyDown(Keys.Escape) && input.CurrentKeyboardState.IsKeyDown(Keys.Escape))
            {
                ScreenManager.GoToMainMenu();
            }

            if (DebugViewEnabled)
                DebugView.AppendFlags(DebugViewFlags.DebugPanel);
            else
                DebugView.RemoveFlags(DebugViewFlags.DebugPanel);


            //TODO: Create pause screen and presentation screen
            //if (input.PauseGame)
            //{
            //    ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
            //}

#if !XBOX
            if (World != null)
            {
                Mouse(input.CurrentMouseState, input.LastMouseState);
            }
#else
            if (World != null)
            {
                GamePad(input.CurrentGamePadState, input.LastGamePadState);
            }
#endif
        }

#if !XBOX

        private void Mouse(MouseState state, MouseState oldState)
        {
            Vector3 worldPosition = ScreenManager.GraphicsDevice.Viewport.Unproject(new Vector3(state.X, state.Y, 0),
                                                                                    Projection, View, Matrix.Identity);
            Vector2 position = new Vector2(worldPosition.X, worldPosition.Y);

            if (state.LeftButton == ButtonState.Released && oldState.LeftButton == ButtonState.Pressed)
            {
                MouseUp();
            }
            else if (state.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
            {
                MouseDown(position);
            }

            MouseMove(position);
        }

        private void MouseMove(Vector2 p)
        {
            if (_fixedMouseJoint != null)
            {
                _fixedMouseJoint.Target = p;
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

        /// <summary>
        /// This is called when the screen should draw itself.
        /// </summary>
        public virtual void Draw(GameTime gameTime)
        {
            if (World != null)
            {
                float aspect = (float)ScreenManager.ScreenWidth / ScreenManager.ScreenHeight;

                Projection = Matrix.CreateOrthographic(40 * aspect, 40, 0, 1);

                DebugView.RenderDebugData(ref Projection, ref View);
            }
        }

        /// <summary>
        /// Tells the screen to go away. Unlike <see cref="ScreenManager"/>.RemoveScreen, which
        /// instantly kills the screen, this method respects the transition timings
        /// and will give the screen a chance to gradually transition off.
        /// </summary>
        public void ExitScreen()
        {
            if (TransitionOffTime == TimeSpan.Zero)
            {
                // If the screen has a zero transition time, remove it immediately.
                ScreenManager.RemoveScreen(this);
            }
            else
            {
                // Otherwise flag that it should transition off and then exit.
                IsExiting = true;
            }
        }
    }
}