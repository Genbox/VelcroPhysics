using System;
using FarseerGames.FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.GettingStarted.ScreenSystem
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
        private bool _debugViewEnabled;
        private bool _isExiting;
        private bool _otherScreenHasFocus;
        //Note: This should not really be here. It should be in an engine instead that takes care of physics
        private PhysicsSimulator _physicsSimulator;
        private PhysicsSimulatorView _physicsSimulatorView;
        private ScreenState _screenState = ScreenState.TransitionOn;
        private TimeSpan _transitionOffTime = TimeSpan.Zero;
        private TimeSpan _transitionOnTime = TimeSpan.Zero;
        private float _transitionPosition = 1;
        protected bool FirstRun = true;

        protected GameScreen()
        {
            _physicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            _physicsSimulatorView = new PhysicsSimulatorView(_physicsSimulator);
        }

        public virtual void Dispose()
        {
        }

        public PhysicsSimulator PhysicsSimulator
        {
            get { return _physicsSimulator; }
            set { _physicsSimulator = value; }
        }

        public PhysicsSimulatorView PhysicsSimulatorView
        {
            get { return _physicsSimulatorView; }
            set { _physicsSimulatorView = value; }
        }

        public bool DebugViewEnabled
        {
            get { return _debugViewEnabled; }
        }

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
        public TimeSpan TransitionOnTime
        {
            get { return _transitionOnTime; }
            protected set { _transitionOnTime = value; }
        }

        /// <summary>
        /// Indicates how long the screen takes to
        /// transition off when it is deactivated.
        /// </summary>
        public TimeSpan TransitionOffTime
        {
            get { return _transitionOffTime; }
            protected set { _transitionOffTime = value; }
        }

        /// <summary>
        /// Gets the current position of the screen transition, ranging
        /// from zero (fully active, no transition) to one (transitioned
        /// fully off to nothing).
        /// </summary>
        public float TransitionPosition
        {
            get { return _transitionPosition; }
            protected set { _transitionPosition = value; }
        }

        /// <summary>
        /// Gets the current alpha of the screen transition, ranging
        /// from 255 (fully active, no transition) to 0 (transitioned
        /// fully off to nothing).
        /// </summary>
        public byte TransitionAlpha
        {
            get { return (byte) (255 - TransitionPosition*255); }
        }

        /// <summary>
        /// Gets the current screen transition state.
        /// </summary>
        public ScreenState ScreenState
        {
            get { return _screenState; }
            protected set { _screenState = value; }
        }

        /// <summary>
        /// There are two possible reasons why a screen might be transitioning
        /// off. It could be temporarily going away to make room for another
        /// screen that is on top of it, or it could be going away for good.
        /// This property indicates whether the screen is exiting for real:
        /// if set, the screen will automatically remove itself as soon as the
        /// transition finishes.
        /// </summary>
        public bool IsExiting
        {
            get { return _isExiting; }
            protected set { _isExiting = value; }
        }

        /// <summary>
        /// Checks whether this screen is active and can respond to user input.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return !_otherScreenHasFocus &&
                       (_screenState == ScreenState.TransitionOn ||
                        _screenState == ScreenState.Active);
            }
        }

        /// <summary>
        /// Gets the manager that this screen belongs to.
        /// </summary>
        public ScreenManager ScreenManager { get; internal set; }

        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Load graphics content for the screen.
        /// </summary>
        public virtual void LoadContent()
        {
            _physicsSimulatorView.LoadContent(ScreenManager.GraphicsDevice, ScreenManager.ContentManager);
        }

        /// <summary>
        /// Unload content for the screen.
        /// </summary>
        public virtual void UnloadContent()
        {
            _physicsSimulator.Clear();
        }

        /// <summary>
        /// Allows the screen to run logic, such as updating the transition position.
        /// Unlike HandleInput, this method is called regardless of whether the screen
        /// is active, hidden, or in the middle of a transition.
        /// </summary>
        public virtual void Update(GameTime gameTime, bool otherScreenHasFocus,
                                   bool coveredByOtherScreen)
        {
            _otherScreenHasFocus = otherScreenHasFocus;

            if (_isExiting)
            {
                // If the screen is going away to die, it should transition off.
                _screenState = ScreenState.TransitionOff;

                if (!UpdateTransition(gameTime, _transitionOffTime, 1))
                {
                    // When the transition finishes, remove the screen.
                    ScreenManager.RemoveScreen(this);

                    _isExiting = false;
                }
            }
            else if (coveredByOtherScreen)
            {
                // If the screen is covered by another, it should transition off.
                if (UpdateTransition(gameTime, _transitionOffTime, 1))
                {
                    // Still busy transitioning.
                    _screenState = ScreenState.TransitionOff;
                }
                else
                {
                    // Transition finished!
                    _screenState = ScreenState.Hidden;
                }
            }
            else
            {
                // Otherwise the screen should transition on and become active.
                if (UpdateTransition(gameTime, _transitionOnTime, -1))
                {
                    // Still busy transitioning.
                    _screenState = ScreenState.TransitionOn;
                }
                else
                {
                    // Transition finished!
                    _screenState = ScreenState.Active;
                }
            }

            if (!coveredByOtherScreen && !otherScreenHasFocus)
            {
                PhysicsSimulator.Update(gameTime.ElapsedGameTime.Milliseconds * .001f);
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
                transitionDelta = (float) (gameTime.ElapsedGameTime.TotalMilliseconds/
                                           time.TotalMilliseconds);

            // Update the transition position.
            _transitionPosition += transitionDelta*direction;

            // Did we reach the end of the transition?
            if ((_transitionPosition <= 0) || (_transitionPosition >= 1))
            {
                _transitionPosition = MathHelper.Clamp(_transitionPosition, 0, 1);
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
            if (!input.LastKeyboardState.IsKeyDown(Keys.F1) && input.CurrentKeyboardState.IsKeyDown(Keys.F1))
            {
                _debugViewEnabled = !_debugViewEnabled;
                _physicsSimulator.EnableDiagnostics = _debugViewEnabled;
            }
        }

        /// <summary>
        /// This is called when the screen should draw itself.
        /// </summary>
        public virtual void Draw(GameTime gameTime)
        {
            if (_debugViewEnabled)
            {
                ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
                _physicsSimulatorView.Draw(ScreenManager.SpriteBatch);
                ScreenManager.SpriteBatch.End();
            }
        }

        /// <summary>
        /// Tells the screen to go away. Unlike ScreenManager.RemoveScreen, which
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
                _isExiting = true;
            }
        }
    }
}