#region File Description

//-----------------------------------------------------------------------------
// ScreenManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace DemoBaseXNA.ScreenSystem
{
    /// <summary>
    /// The screen manager is a component which manages one or more <see cref="GameScreen"/>
    /// instances. It maintains a stack of _screens, calls their Update and Draw
    /// methods at the appropriate times, and automatically routes _input to the
    /// topmost active screen.
    /// </summary>
    public class ScreenManager : DrawableGameComponent
    {
        private Texture2D _blankTexture;
        private IGraphicsDeviceService _graphicsDeviceService;
        private InputState _input = new InputState();
        private List<GameScreen> _screens = new List<GameScreen>();
        private List<GameScreen> _screensToUpdate = new List<GameScreen>();
        private SpriteFonts _spriteFonts;
        public MainMenuScreen MainMenuScreen = new MainMenuScreen();

        /// <summary>
        /// Constructs a new screen manager component.
        /// </summary>
        /// <exception cref="InvalidOperationException">No graphics device service.</exception>
        public ScreenManager(Game game)
            : base(game)
        {
            ContentManager = new ContentManager(game.Services);
            _graphicsDeviceService = (IGraphicsDeviceService) game.Services.GetService(
                                                                  typeof (IGraphicsDeviceService));
            game.Exiting += Game_Exiting;

            if (_graphicsDeviceService == null)
                throw new InvalidOperationException("No graphics device service.");
        }

        public SpriteFonts SpriteFonts
        {
            get { return _spriteFonts; }
        }

        /// <summary>
        /// A content manager used to load data that is shared between multiple
        /// screens. This is never unloaded, so if a screen requires a large amount
        /// of temporary data, it should create a local content manager instead.
        /// </summary>
        public ContentManager ContentManager { get; private set; }

        /// <summary>
        /// A default SpriteBatch shared by all the screens. This saves
        /// each screen having to bother creating their own local instance.
        /// </summary>
        public SpriteBatch SpriteBatch { get; private set; }

        public Vector2 ScreenCenter
        {
            get
            {
                return new Vector2(_graphicsDeviceService.GraphicsDevice.Viewport.Width/2f,
                                   _graphicsDeviceService.GraphicsDevice.Viewport.Height/2f);
            }
        }

        public int ScreenWidth
        {
            get { return _graphicsDeviceService.GraphicsDevice.Viewport.Width; }
        }

        public int ScreenHeight
        {
            get { return _graphicsDeviceService.GraphicsDevice.Viewport.Height; }
        }

        /// <summary>
        /// If true, the manager prints out a list of all the screens
        /// each time it is updated. This can be useful for making sure
        /// everything is being added and removed at the right times.
        /// </summary>
        public bool TraceEnabled { get; set; }

        /// <summary>
        /// Goes to main menu.
        /// Removes all active screens and add a main menu
        /// </summary>
        public void GoToMainMenu()
        {
            _screens.Clear();
            _screensToUpdate.Clear();

            AddScreen(MainMenuScreen);
        }

        private void Game_Exiting(object sender, EventArgs e)
        {
            //Make sure to dispose ALL screens when the game is forcefully closed
            //We do this to ensure that open resources and threads created by screens are closed.
            foreach (GameScreen screen in _screens)
            {
                screen.Dispose();
            }

            _screens.Clear();
            _screensToUpdate.Clear();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            _spriteFonts = new SpriteFonts(ContentManager);

            foreach (GameScreen screen in _screens)
            {
                screen.Initialize();
            }
            base.Initialize();
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load content belonging to the screen manager.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            _blankTexture = ContentManager.Load<Texture2D>("Content/Common/blank");

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
            ContentManager.Unload();

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
            // Read the keyboard and gamepad.
            _input.Update();

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

            // Print debug trace?
            if (TraceEnabled)
                TraceScreens();
        }

        /// <summary>
        /// Prints a list of all the screens, for debugging.
        /// </summary>
        private void TraceScreens()
        {
            List<string> screenNames = new List<string>();

            foreach (GameScreen screen in _screens)
                screenNames.Add(screen.GetType().Name);

            Trace.WriteLine(string.Join(", ", screenNames.ToArray()));
        }

        /// <summary>
        /// Tells each screen to draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
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
            screen.Initialize();
            // If we have a graphics device, tell the screen to load content.
            if ((_graphicsDeviceService != null) &&
                (_graphicsDeviceService.GraphicsDevice != null))
            {
                screen.LoadContent();
            }

            _screens.Add(screen);
        }

        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use <see cref="GameScreen"/>.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(GameScreen screen)
        {
            // If we have a graphics device, tell the screen to unload content.
            if ((_graphicsDeviceService != null) &&
                (_graphicsDeviceService.GraphicsDevice != null))
            {
                screen.UnloadContent();
            }

            _screens.Remove(screen);
            _screensToUpdate.Remove(screen);

            screen.Dispose();
        }

        /// <summary>
        /// Helper draws a translucent black full screen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeBackBufferToBlack(int alpha)
        {
            Viewport viewport = GraphicsDevice.Viewport;

            SpriteBatch.Begin();

            SpriteBatch.Draw(_blankTexture,
                             new Rectangle(0, 0, viewport.Width, viewport.Height),
                             new Color(0, 0, 0, (byte) alpha));

            SpriteBatch.End();
        }
    }
}