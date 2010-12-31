using System;
using FarseerPhysics.DemoBaseXNA.Components;
using FarseerPhysics.DemoBaseXNA.Screens;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.SimpleSamplesXNA
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
            _graphics.PreferMultiSampling = true;
            IsFixedTimeStep = true;

#if WINDOWS
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.IsFullScreen = false;
#endif
#if WINDOWS_PHONE
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 480;
            _graphics.IsFullScreen = true;
            IsFixedTimeStep = false;
#endif
#if XBOX
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.IsFullScreen = true;
#endif
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            //new-up components and add to Game.Components
            ScreenManager = new ScreenManager(this);
            Components.Add(ScreenManager);

            FrameRateCounter frameRateCounter = new FrameRateCounter(ScreenManager);
            frameRateCounter.DrawOrder = 101;
            Components.Add(frameRateCounter);
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
            base.Initialize();

#if WINDOWS
            //Set window defaults. Parent game can override in constructor
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += WindowClientSizeChanged;
#endif

            Demo1Screen demo1 = new Demo1Screen();
            Demo2Screen demo2 = new Demo2Screen();
            Demo3Screen demo3 = new Demo3Screen();
            Demo4Screen demo4 = new Demo4Screen();
            Demo5Screen demo5 = new Demo5Screen();
            Demo6Screen demo6 = new Demo6Screen();
            Demo7Screen demo7 = new Demo7Screen();
            Demo8Screen demo8 = new Demo8Screen();
            Demo9Screen demo9 = new Demo9Screen();
            MainMenuScreen mainMenuScreen = new MainMenuScreen();
            mainMenuScreen.AddMainMenuItem(demo1.GetTitle(), demo1);
            mainMenuScreen.AddMainMenuItem(demo2.GetTitle(), demo2);
            mainMenuScreen.AddMainMenuItem(demo3.GetTitle(), demo3);
            mainMenuScreen.AddMainMenuItem(demo4.GetTitle(), demo4);
            mainMenuScreen.AddMainMenuItem(demo5.GetTitle(), demo5);
            mainMenuScreen.AddMainMenuItem(demo6.GetTitle(), demo6);
            mainMenuScreen.AddMainMenuItem(demo7.GetTitle(), demo7);
            mainMenuScreen.AddMainMenuItem(demo8.GetTitle(), demo8);
            mainMenuScreen.AddMainMenuItem(demo9.GetTitle(), demo9);
            mainMenuScreen.AddMainMenuItem("Exit", null, true);

            ScreenManager.AddScreen(new BackgroundScreen(), null);
            ScreenManager.AddScreen(mainMenuScreen, null);
            ScreenManager.AddScreen(new LogoScreen(TimeSpan.FromSeconds(2.0)), null);
        }

#if WINDOWS
        private void WindowClientSizeChanged(object sender, EventArgs e)
        {
            if (Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0)
            {
                _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            }

            //We recreate the projection matrix to keep aspect ratio.
            ScreenManager.Camera.CreateProjection();

            //Reset transition render targets
            ScreenManager.ResetTargets();
        }
#endif
    }
}