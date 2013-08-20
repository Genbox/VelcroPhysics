#region Using Statements

using System.Collections.Generic;
using System.Windows.Controls;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

#endregion

namespace FarseerPhysics.ScreenSystem
{
    /// <summary>
    /// The screen manager is a component which manages one or more <see cref="GameScreen"/>
    /// instances. It maintains a stack of _screens, calls their Update and Draw
    /// methods at the appropriate times, and automatically routes _input to the
    /// topmost active screen.
    /// </summary>
    public class ScreenManager : DrawableGameComponent
    {
        public MainMenuScreen MainMenuScreen = new MainMenuScreen();
        private Canvas _canvas;
        private Canvas _debugCanvas;
        private InputState _input = new InputState();
        private List<GameScreen> _screens = new List<GameScreen>();
        private List<GameScreen> _screensToUpdate = new List<GameScreen>();
        private TextBlock _txtDebug;

        /// <summary>
        /// Constructs a new screen manager component.
        /// </summary>
        public ScreenManager(Game game)
            : base(game)
        {
            _canvas = game.DrawingCanvas;
            _debugCanvas = game.DebugCanvas;
            _txtDebug = game.TxtDebug;

            _input.Attach(game.UserControl);

            if (_debugCanvas != null)
            {
                _debugCanvas.IsHitTestVisible = false;
            }
        }

        public Vector2 ScreenCenter
        {
            get
            {
                return new Vector2(50 / 2f,
                                   50 / 2f);
            }
        }

        public int ScreenWidth
        {
            get { return 50; }
        }

        public int ScreenHeight
        {
            get { return 50; }
        }

        /// <summary>
        /// Goes to main menu.
        /// Removes all active screens and add a main menu
        /// </summary>
        public void GoToMainMenu()
        {
            _screens.Clear();
            _screensToUpdate.Clear();
            _txtDebug.Text = "";

            AddScreen(MainMenuScreen);
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Tell each of the _screens to load their content.
            foreach (GameScreen screen in _screens)
            {
                screen.LoadContent();
            }
        }

        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            // Tell each of the _screens to unload their content.
            foreach (GameScreen screen in _screens)
            {
                screen.UnloadContent();
            }
        }

        /// <summary>
        /// Allows each screen to run logic.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            _screensToUpdate.Clear();

            for (int i = 0; i < _screens.Count; i++)
                _screensToUpdate.Add(_screens[i]);

            bool otherScreenHasFocus = !Game.IsActive;
            bool coveredByOtherScreen = false;

            // Loop as long as there are _screens waiting to be updated.
            while (_screensToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                GameScreen screen = _screensToUpdate[_screensToUpdate.Count - 1];

                _screensToUpdate.RemoveAt(_screensToUpdate.Count - 1);

                // Update the screen.
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.TransitionOn ||
                    screen.ScreenState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle _input.
                    if (!otherScreenHasFocus)
                    {
                        screen.HandleInput(_input);

                        otherScreenHasFocus = true;
                    }

                    // If this is an active non-popup, inform any subsequent
                    // _screens that they are covered by it.
                    if (!screen.IsPopup)
                        coveredByOtherScreen = true;
                }
            }

            //Process Input
            _input.Update(gameTime);
        }

        /// <summary>
        /// Tells each screen to draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            _debugCanvas.Children.Clear();

            for (int i = 0; i < _screens.Count; i++)
            {
                if (_screens[i].ScreenState == ScreenState.Hidden)
                    continue;

                _screens[i].Draw(gameTime);
            }
        }

        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        public void AddScreen(GameScreen screen)
        {
            screen.ScreenManager = this;
            screen.DebugCanvas = _debugCanvas;
            screen.TxtDebug = _txtDebug;
            screen.Initialize();

            //Tell the screen to load content.
            screen.LoadContent();
            
            _screens.Add(screen);

            IDemoScreen demoScreen = screen as IDemoScreen;
            if (demoScreen != null && screen.FirstRun)
            {
                AddScreen(new PauseScreen(demoScreen.GetTitle(), demoScreen.GetDetails()));
                screen.FirstRun = false;
            }
        }

        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use <see cref="GameScreen"/>.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(GameScreen screen)
        {
            screen.UnloadContent();
            
            _screens.Remove(screen);
            _screensToUpdate.Remove(screen);

            screen.Dispose();
        }

    }
}