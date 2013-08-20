using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FarseerPhysics.Collision;
using FarseerPhysics.DebugView;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Border = FarseerPhysics.DemoShare.Border;

namespace FarseerPhysics.ScreenSystem
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
        private FixedMouseJoint _fixedMouseJoint;
        private bool _otherScreenHasFocus;
        public bool FirstRun = true;

        protected GameScreen()
        {
            ScreenState = ScreenState.TransitionOn;
            TransitionPosition = 1;
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            DebugCanvas = null;
            TxtDebug = null;
            Transform = new CompositeTransform();
        }

        public Canvas DebugCanvas { get; set; }
        public TextBlock TxtDebug { get; set; }
        public CompositeTransform Transform { get; set; }

        public World World { get; set; }

        public DebugViewSilverlight DebugView { get; set; }

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
            if (DebugCanvas != null)
            {
                Transform.ScaleX = DebugCanvas.ActualWidth / ScreenManager.ScreenWidth;
                Transform.ScaleY = -DebugCanvas.ActualHeight / ScreenManager.ScreenHeight;
                Transform.TranslateX = DebugCanvas.ActualWidth / 2;
                Transform.TranslateY = DebugCanvas.ActualHeight / 2;

                //DebugView
                DebugView = new DebugViewSilverlight(DebugCanvas, TxtDebug, World);
                DebugView.DefaultShapeColor = Colors.White;
                DebugView.SleepingShapeColor = Colors.LightGray;
                DebugView.Transform = Transform;
            }
        }

        /// <summary>
        /// Load graphics content for the screen.
        /// </summary>
        public virtual void LoadContent()
        {
            if (World != null)
            {
                new Border(World, ScreenManager.ScreenWidth, ScreenManager.ScreenHeight, 1);
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
                transitionDelta = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds);

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
            //Keyboard Input
            if (!input.LastKeyboardState.IsKeyDown(Key.F1) && input.CurrentKeyboardState.IsKeyDown(Key.F1))
            {
                DebugViewEnabled = !DebugViewEnabled;

                if (DebugViewEnabled == false)
                    TxtDebug.Text = "";
            }

            if (!input.LastKeyboardState.IsKeyDown(Key.Escape) && input.CurrentKeyboardState.IsKeyDown(Key.Escape))
            {
                ScreenManager.GoToMainMenu();
            }

            //Mouse
            Point p = Transform.Inverse.Transform(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y));
            Vector2 position = new Vector2((float)p.X, (float)p.Y);
            if (input.CurrentMouseState.IsLeftButtonDown == false && input.LastMouseState.IsLeftButtonDown)
            {
                MouseUp();
            }
            else if (input.CurrentMouseState.IsLeftButtonDown && input.LastMouseState.IsLeftButtonDown == false)
            {
                MouseDown(position);
            }

            MouseMove(position);

            //DebugView
            if (DebugView != null)
            {
                if (DebugViewEnabled)
                    DebugView.AppendFlags(DebugViewFlags.DebugPanel);
                else
                    DebugView.RemoveFlags(DebugViewFlags.DebugPanel);
            }
        }

        private void MouseMove(Vector2 p)
        {
            if (_fixedMouseJoint != null)
            {
                _fixedMouseJoint.WorldAnchorB = p;
            }
        }

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
                if (DebugView != null)
                {
                    DebugView.DrawDebugData();
                }
            }
        }

        /// <summary>
        /// Tells the screen to go away. Unlike <see cref="ScreenManager"/>.RemoveScreen, which
        /// instantly kills the screen, this method respects the transition timings
        /// and will give the screen a chance to gradually transition off.
        /// </summary>
        public void ExitScreen()
        {
            TxtDebug.Text = "";
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