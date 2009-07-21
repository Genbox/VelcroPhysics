using System;
using DemoBaseXNA.Components;
using DemoBaseXNA.ScreenSystem;
using FarseerGames.SimpleSamplesXNA.Demo1;
using FarseerGames.SimpleSamplesXNA.Demo10;
using FarseerGames.SimpleSamplesXNA.Demo2;
using FarseerGames.SimpleSamplesXNA.Demo3;
using FarseerGames.SimpleSamplesXNA.Demo4;
using FarseerGames.SimpleSamplesXNA.Demo5;
using FarseerGames.SimpleSamplesXNA.Demo6;
using FarseerGames.SimpleSamplesXNA.Demo7;
using FarseerGames.SimpleSamplesXNA.Demo8;
using FarseerGames.SimpleSamplesXNA.Demo9;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.SimpleSamplesXNA
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class FarseerPhysicsGame : Game
    {
        private GraphicsDeviceManager _graphics;

        public FarseerPhysicsGame()
        {
            Window.Title = "Farseer Physics Engine Samples Framework";
            _graphics = new GraphicsDeviceManager(this);

            _graphics.SynchronizeWithVerticalRetrace = false;

            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 10);
            IsFixedTimeStep = true;

#if !XBOX
            //windowed
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.IsFullScreen = false;

            //fullscreen
            //_graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            //_graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            //_graphics.IsFullScreen = true;

            IsMouseVisible = true;
#else
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.IsFullScreen = true;
#endif

            //Set window defaults. Parent game can override in constructor
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;

            //new-up components and add to Game.Components
            ScreenManager = new ScreenManager(this);
            Components.Add(ScreenManager);

            FrameRateCounter frameRateCounter = new FrameRateCounter(ScreenManager);
            frameRateCounter.DrawOrder = 101;
            Components.Add(frameRateCounter);

            ScreenManager.MainMenuScreen.AddMainMenuItem(Demo1Screen.GetTitle(), new Demo1Screen());
            ScreenManager.MainMenuScreen.AddMainMenuItem(Demo2Screen.GetTitle(), new Demo2Screen());
            ScreenManager.MainMenuScreen.AddMainMenuItem(Demo3Screen.GetTitle(), new Demo3Screen());
            ScreenManager.MainMenuScreen.AddMainMenuItem(Demo4Screen.GetTitle(), new Demo4Screen());
            ScreenManager.MainMenuScreen.AddMainMenuItem(Demo5Screen.GetTitle(), new Demo5Screen());
            ScreenManager.MainMenuScreen.AddMainMenuItem(Demo6Screen.GetTitle(), new Demo6Screen());
            ScreenManager.MainMenuScreen.AddMainMenuItem(Demo7Screen.GetTitle(), new Demo7Screen());
            ScreenManager.MainMenuScreen.AddMainMenuItem(Demo8Screen.GetTitle(), new Demo8Screen());
            ScreenManager.MainMenuScreen.AddMainMenuItem(Demo9Screen.GetTitle(), new Demo9Screen());
            ScreenManager.MainMenuScreen.AddMainMenuItem(Demo10Screen.GetTitle(), new Demo10Screen());
            ScreenManager.MainMenuScreen.AddMainMenuItem("Exit", null, true);

            ScreenManager.GoToMainMenu();
        }

        public ScreenManager ScreenManager { get; set; }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _graphics.ApplyChanges();
            base.Initialize();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.SteelBlue);

            base.Draw(gameTime);
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            if (Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0)
            {
                _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            }
        }
    }
}