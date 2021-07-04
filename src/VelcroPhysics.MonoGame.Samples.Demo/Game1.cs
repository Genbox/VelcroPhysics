using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem.Graphics;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.Screens;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.ScreenSystem;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo
{
    /// <summary>This is the main type for the samples</summary>
    public sealed class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private InputHelper _input;
        private ScreenManager _screenManager;

        private bool _isExiting;
        private LineBatch _lineBatch;
        private QuadRenderer _quadRenderer;
        private SpriteBatch _spriteBatch;

#if WINDOWS
        private FrameRateCounter _counter;
        private bool _showFps;
#endif

        public Game1()
        {
            Window.Title = "Velcro Physics Samples";
            _graphics = new GraphicsDeviceManager(this);
            ConvertUnits.SetDisplayUnitToSimUnitRatio(24f);
            IsFixedTimeStep = true;
            Content.RootDirectory = "Content";
        }

        public bool IsExiting { get; set; }

        protected override void Initialize()
        {
            _graphics.PreferMultiSampling = true;
            _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width - 80;
            _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height - 120;

#if WINDOWS
            _graphics.IsFullScreen = false;
#elif XBOX
            _graphics.IsFullScreen = true;
#endif

            _graphics.ApplyChanges();

            _input = new InputHelper();
            _isExiting = false;

#if WINDOWS
            _counter = new FrameRateCounter();
            _showFps = false;
#endif

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Managers.TextureManager = new TextureManager(Content, GraphicsDevice);
            Managers.FontManager = new FontManager(Content);
            Managers.SoundManager = new SoundManager(Content);

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _lineBatch = new LineBatch(GraphicsDevice);
            _quadRenderer = new QuadRenderer(GraphicsDevice);

            _input.LoadContent(GraphicsDevice.Viewport);

            _screenManager = new ScreenManager(this, _lineBatch, _quadRenderer, _spriteBatch, _input);

#if WINDOWS
            _counter.LoadContent();
#endif

            _screenManager.LoadContent();

            //TODO: Can't call this in MonoGame at the moment
            //ResetElapsedTime();
        }

        protected override void UnloadContent()
        {
            _screenManager.UnloadContent();

            base.UnloadContent();
        }

        /// <summary>Allows each screen to run logic.</summary>
        protected override void Update(GameTime gameTime)
        {
            // Read the keyboard and gamepad.
            _input.Update(gameTime);

#if WINDOWS

            // Update the framerate counter
            _counter.Update(gameTime);
#endif

            if ((_input.IsNewButtonPress(Buttons.Y) || _input.IsNewKeyPress(Keys.F5)) && _screenManager.LastScreen is not OptionsScreen)
                _screenManager.AddScreen(new OptionsScreen());

#if WINDOWS
            if (_input.IsNewKeyPress(Keys.F11))
                _showFps = !_showFps;

            if (_input.IsNewKeyPress(Keys.F12))
                _graphics.ToggleFullScreen();
#endif

            _screenManager.Update(gameTime);

            if (_isExiting && _screenManager.ScreenCount == 0)
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _screenManager.Draw(gameTime);

            _input.Draw(_spriteBatch);

#if WINDOWS
            if (_showFps)
                _counter.Draw(_spriteBatch);
#endif
            base.Draw(gameTime);
        }

        public void ExitGame()
        {
            _screenManager.ExitScreens();
            _isExiting = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _graphics.Dispose();
                _lineBatch.Dispose();
                _quadRenderer.Dispose();
                _spriteBatch.Dispose();
                Managers.TextureManager.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}