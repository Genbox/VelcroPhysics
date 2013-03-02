using System;
using System.Collections.Generic;
using System.Reflection;
using FarseerPhysics.Samples.MediaSystem;
using FarseerPhysics.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Samples
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the samples
        /// </summary>
        private static void Main(string[] args)
        {
            using (FarseerPhysicsSamples physicsSamples = new FarseerPhysicsSamples())
            {
                physicsSamples.Run();
            }
        }
    }

    /// <summary>
    /// This is the main type for the samples
    /// </summary>
    public class FarseerPhysicsSamples : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private LineBatch _lineBatch;
        private QuadRenderer _quadRenderer;

        private InputHelper _input;

        private List<GameScreen> _screens = new List<GameScreen>();
        private List<GameScreen> _screensToUpdate = new List<GameScreen>();

        private List<RenderTarget2D> _transitions = new List<RenderTarget2D>();
        private List<RenderTarget2D> _previews = new List<RenderTarget2D>();

        private MenuScreen _menuScreen;

        private bool _isExiting;

#if WINDOWS
        private FrameRateCounter _counter;
        private bool _showFPS;
#endif

        public FarseerPhysicsSamples()
        {
            Window.Title = "Farseer Physics Samples";
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferMultiSampling = true;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            ConvertUnits.SetDisplayUnitToSimUnitRatio(24f);
            IsFixedTimeStep = true;
#if WINDOWS
            _graphics.IsFullScreen = false;
#elif XBOX
      _graphics.IsFullScreen = true;
#endif

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            _input = new InputHelper();
#if WINDOWS
            _counter = new FrameRateCounter();
            _showFPS = false;
#endif
            _isExiting = false;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            ContentWrapper.Initialize(this);

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _lineBatch = new LineBatch(GraphicsDevice);
            _quadRenderer = new QuadRenderer(GraphicsDevice);

            _input.LoadContent(GraphicsDevice.Viewport);
#if WINDOWS
            _counter.LoadContent();
#endif

            // Create rendertarget for transitions
            PresentationParameters _pp = GraphicsDevice.PresentationParameters;
            _transitions.Add(new RenderTarget2D(GraphicsDevice, _pp.BackBufferWidth, _pp.BackBufferHeight, false,
                                                SurfaceFormat.Color, _pp.DepthStencilFormat, _pp.MultiSampleCount,
                                                RenderTargetUsage.DiscardContents));

            _menuScreen = new MenuScreen();

            Assembly SamplesFramework = Assembly.GetExecutingAssembly();
            foreach (Type SampleType in SamplesFramework.GetTypes())
            {
                if (SampleType.IsSubclassOf(typeof(PhysicsDemoScreen)))
                {
                    PhysicsDemoScreen DemoScreen = SamplesFramework.CreateInstance(SampleType.ToString()) as PhysicsDemoScreen;
#if WINDOWS
                    Console.WriteLine("Loading demo: " + DemoScreen.GetTitle());
#endif
                    RenderTarget2D preview = new RenderTarget2D(GraphicsDevice, _pp.BackBufferWidth / 2, _pp.BackBufferHeight / 2, false,
                                                                SurfaceFormat.Color, _pp.DepthStencilFormat, _pp.MultiSampleCount,
                                                                RenderTargetUsage.DiscardContents);

                    DemoScreen.Framework = this;
                    DemoScreen.IsExiting = false;

                    DemoScreen.Sprites = _spriteBatch;
                    DemoScreen.Lines = _lineBatch;
                    DemoScreen.Quads = _quadRenderer;

                    DemoScreen.LoadContent();

                    // "Abuse" transition rendertarget to render screen preview
                    GraphicsDevice.SetRenderTarget(_transitions[0]);
                    GraphicsDevice.Clear(Color.Transparent);

                    _quadRenderer.Begin();
                    _quadRenderer.Render(Vector2.Zero, new Vector2(_transitions[0].Width, _transitions[0].Height), null, true, ContentWrapper.Grey, Color.White * 0.3f);
                    _quadRenderer.End();
                    // Update ensures that the screen is fully visible, we "cover" it so that no physics are run
                    DemoScreen.Update(new GameTime(DemoScreen.TransitionOnTime, DemoScreen.TransitionOnTime), true, false);
                    DemoScreen.Draw(new GameTime());
                    DemoScreen.Draw(new GameTime());

                    GraphicsDevice.SetRenderTarget(preview);
                    GraphicsDevice.Clear(Color.Transparent);

                    _spriteBatch.Begin();
                    _spriteBatch.Draw(_transitions[0], preview.Bounds, Color.White);
                    _spriteBatch.End();

                    GraphicsDevice.SetRenderTarget(null);

                    DemoScreen.ExitScreen();
                    DemoScreen.Update(new GameTime(DemoScreen.TransitionOffTime, DemoScreen.TransitionOffTime), true, false);
                    _menuScreen.AddMenuItem(DemoScreen, preview);
                }
            }

            AddScreen(new BackgroundScreen());
            AddScreen(_menuScreen);
            AddScreen(new LogoScreen(TimeSpan.FromSeconds(5.0)));

            ResetElapsedTime();
        }

        protected override void UnloadContent()
        {
            foreach (GameScreen screen in _screens)
            {
                screen.UnloadContent();
            }
            base.UnloadContent();
        }

        /// <summary>
        /// Allows each screen to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Read the keyboard and gamepad.
            _input.Update(gameTime);
            // Update the framerate counter
#if WINDOWS
            _counter.Update(gameTime);
#endif
            if ((_input.IsNewButtonPress(Buttons.Y) || _input.IsNewKeyPress(Keys.F5)) &&
                !(_screens[_screens.Count - 1] is OptionsScreen || _screens[_screens.Count - 1] is LogoScreen))
            {
                AddScreen(new OptionsScreen());
            }
#if WINDOWS
            if (_input.IsNewKeyPress(Keys.F11))
            {
                _showFPS = !_showFPS;
            }
            if (_input.IsNewKeyPress(Keys.F12))
            {
                _graphics.ToggleFullScreen();
            }
#endif
            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            _screensToUpdate.Clear();
            _screensToUpdate.AddRange(_screens);

            bool otherScreenHasFocus = !IsActive;
            bool coveredByOtherScreen = false;

            // Loop as long as there are screens waiting to be updated.
            while (_screensToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                GameScreen screen = _screensToUpdate[_screensToUpdate.Count - 1];

                _screensToUpdate.RemoveAt(_screensToUpdate.Count - 1);

                // Update the screen.
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle input.
                    if (!otherScreenHasFocus && !_isExiting)
                    {
                        _input.ShowCursor = screen.HasCursor;
                        screen.HandleInput(_input, gameTime);
                        otherScreenHasFocus = true;
                    }

                    // If this is an active non-popup, inform any subsequent
                    // screens that they are covered by it.
                    if (!screen.IsPopup)
                    {
                        coveredByOtherScreen = true;
                    }
                }
            }

            if (_isExiting && _screens.Count == 0)
            {
                Exit();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            int transitionCount = 0;
            foreach (GameScreen screen in _screens)
            {
                if (screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.TransitionOff)
                {
                    transitionCount++;
                    if (_transitions.Count < transitionCount)
                    {
                        PresentationParameters _pp = GraphicsDevice.PresentationParameters;
                        _transitions.Add(new RenderTarget2D(GraphicsDevice, _pp.BackBufferWidth, _pp.BackBufferHeight, false,
                                                            SurfaceFormat.Color, _pp.DepthStencilFormat, _pp.MultiSampleCount,
                                                            RenderTargetUsage.DiscardContents));
                    }
                    GraphicsDevice.SetRenderTarget(_transitions[transitionCount - 1]);
                    GraphicsDevice.Clear(Color.Transparent);
                    screen.Draw(gameTime);
                    GraphicsDevice.SetRenderTarget(null);
                }
            }

            GraphicsDevice.Clear(Color.Black);

            transitionCount = 0;
            foreach (GameScreen screen in _screens)
            {
                if (screen.ScreenState == ScreenState.Hidden)
                {
                    continue;
                }

                if (screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.TransitionOff)
                {
                    _spriteBatch.Begin(0, BlendState.AlphaBlend);
                    if (screen is PhysicsDemoScreen)
                    {
                        Vector2 position = Vector2.Lerp(_menuScreen.PreviewPosition, new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) / 2f, 1f - screen.TransitionPosition);
                        _spriteBatch.Draw(_transitions[transitionCount], position, null, Color.White * Math.Min(screen.TransitionAlpha / 0.2f, 1f), 0f,
                                          new Vector2(_transitions[transitionCount].Width, _transitions[transitionCount].Height) / 2f, 0.5f + 0.5f * (1f - screen.TransitionPosition), SpriteEffects.None, 0f);
                    }
                    else
                    {
                        _spriteBatch.Draw(_transitions[transitionCount], Vector2.Zero, Color.White * screen.TransitionAlpha);
                    }
                    _spriteBatch.End();

                    transitionCount++;
                }
                else
                {
                    screen.Draw(gameTime);
                }
            }

            _input.Draw(_spriteBatch);
#if WINDOWS
            if (_showFPS)
            {
                _counter.Draw(_spriteBatch);
            }
#endif
            base.Draw(gameTime);
        }

        public void ExitGame()
        {
            foreach (GameScreen screen in _screens)
            {
                screen.ExitScreen();
            }
            _isExiting = true;
        }

        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        public void AddScreen(GameScreen screen)
        {
            screen.Framework = this;
            screen.IsExiting = false;

            screen.Sprites = _spriteBatch;
            screen.Lines = _lineBatch;
            screen.Quads = _quadRenderer;

            // Tell the screen to load content.
            screen.LoadContent();
            // Loading my take a while so elapsed time is reset to prevent hick-ups
            ResetElapsedTime();
            _screens.Add(screen);
        }

        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(GameScreen screen)
        {
            // Tell the screen to unload content.
            screen.UnloadContent();
            _screens.Remove(screen);
            _screensToUpdate.Remove(screen);
        }
    }
}