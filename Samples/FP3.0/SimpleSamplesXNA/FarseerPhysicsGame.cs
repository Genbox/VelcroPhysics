using System;
using FarseerPhysics.DemoBaseXNA.Components;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleSamplesXNA.Demo1;
using SimpleSamplesXNA.Demo2;
using SimpleSamplesXNA.Demo3;
using SimpleSamplesXNA.Demo4;
using SimpleSamplesXNA.Demo5;
using SimpleSamplesXNA.Demo6;
using SimpleSamplesXNA.Demo7;
using SimpleSamplesXNA.Demo8;
using SimpleSamplesXNA.Demo9;

namespace SimpleSamplesXNA
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

            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 16);
            IsFixedTimeStep = true;

            Content.RootDirectory = "Content";

#if !XBOX
            //windowed
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
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

            Demo1Screen demo1 = new Demo1Screen();
            Demo2Screen demo2 = new Demo2Screen();
            Demo3Screen demo3 = new Demo3Screen();
            Demo4Screen demo4 = new Demo4Screen();
            Demo5Screen demo5 = new Demo5Screen();
            Demo6Screen demo6 = new Demo6Screen();
            Demo7Screen demo7 = new Demo7Screen();
            Demo8Screen demo8 = new Demo8Screen();
            Demo9Screen demo9 = new Demo9Screen();
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo1.GetTitle(), demo1);
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo2.GetTitle(), demo2);
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo3.GetTitle(), demo3);
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo4.GetTitle(), demo4);
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo5.GetTitle(), demo5);
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo6.GetTitle(), demo6);
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo7.GetTitle(), demo7);
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo8.GetTitle(), demo8);
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo9.GetTitle(), demo9);
            ScreenManager.MainMenuScreen.AddMainMenuItem("Exit", null, true);

            //ScreenManager.GoToMainMenu();
            ScreenManager.AddScreen(new Demo7Screen());
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