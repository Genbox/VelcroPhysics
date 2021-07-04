using System;
using System.Collections.Generic;
using System.Reflection;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem.Graphics;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.ScreenSystem
{
    public class ScreenManager
    {
        private readonly List<GameScreen> _screens = new List<GameScreen>();
        private readonly List<GameScreen> _screensToUpdate = new List<GameScreen>();
        private readonly List<RenderTarget2D> _transitions = new List<RenderTarget2D>();

        private readonly GraphicsDevice _graphics;
        private readonly InputHelper _input;
        private readonly Game1 _game;

        private readonly LineBatch _lineBatch;
        private readonly QuadRenderer _quadRenderer;
        private readonly SpriteBatch _spriteBatch;

        private MenuScreen _menuScreen;

        public ScreenManager(Game1 game, LineBatch lineBatch, QuadRenderer quadRenderer, SpriteBatch spriteBatch, InputHelper input)
        {
            _graphics = game.GraphicsDevice;
            _game = game;
            _lineBatch = lineBatch;
            _quadRenderer = quadRenderer;
            _spriteBatch = spriteBatch;
            _input = input;
        }

        public int ScreenCount => _screens.Count;
        public GameScreen LastScreen => _screens[^1];

        public void UnloadContent()
        {
            foreach (GameScreen screen in _screens)
            {
                screen.UnloadContent();
            }
        }

        public void LoadContent()
        {
            // Create rendertarget for transitions
            PresentationParameters pp = _graphics.PresentationParameters;
            _transitions.Add(new RenderTarget2D(_graphics, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.PreserveContents));

            _menuScreen = new MenuScreen();

            List<Type> demosToLoad = new List<Type>();
            Assembly samplesFramework = Assembly.GetExecutingAssembly();
            foreach (Type sampleType in samplesFramework.GetTypes())
            {
                if (sampleType.IsSubclassOf(typeof(PhysicsDemoScreen)))
                    demosToLoad.Add(sampleType);
            }

            foreach (Type sampleType in demosToLoad)
            {
                PhysicsDemoScreen demoScreen = (PhysicsDemoScreen)samplesFramework.CreateInstance(sampleType.ToString());
                demoScreen.Framework = _game;
                demoScreen.ScreenManager = this;
                demoScreen.Sprites = _spriteBatch;
                demoScreen.Lines = _lineBatch;
                demoScreen.Quads = _quadRenderer;

                demoScreen.LoadContent();

                // "Abuse" transition rendertarget to render screen preview
                _graphics.SetRenderTarget(_transitions[0]);
                _graphics.Clear(Color.Transparent);

                _quadRenderer.Begin();
                _quadRenderer.Render(Vector2.Zero, new Vector2(_transitions[0].Width, _transitions[0].Height), null, true, Colors.Grey, Color.White * 0.3f);
                _quadRenderer.End();

                // Update ensures that the screen is fully visible, we "cover" it so that no physics are run
                demoScreen.Draw();

                RenderTarget2D preview = new RenderTarget2D(_graphics, pp.BackBufferWidth / 2, pp.BackBufferHeight / 2, false, SurfaceFormat.Color, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
                _graphics.SetRenderTarget(preview);
                _graphics.Clear(Color.Transparent);

                _spriteBatch.Begin();
                _spriteBatch.Draw(_transitions[0], preview.Bounds, Color.White);
                _spriteBatch.End();

                _graphics.SetRenderTarget(null);

                demoScreen.ExitScreen();
                _menuScreen.AddMenuItem(demoScreen, preview);
            }

            AddScreen(new BackgroundScreen());
            AddScreen(_menuScreen);
        }

        public void Update(GameTime gameTime)
        {
            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            _screensToUpdate.Clear();
            _screensToUpdate.AddRange(_screens);

            bool otherScreenHasFocus = !_game.IsActive;
            bool coveredByOtherScreen = false;

            // Loop as long as there are screens waiting to be updated.
            while (_screensToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                GameScreen screen = _screensToUpdate[^1];

                _screensToUpdate.RemoveAt(_screensToUpdate.Count - 1);

                // Update the screen.
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle input.
                    if (!otherScreenHasFocus && !_game.IsExiting)
                    {
                        _input.ShowCursor = screen.HasCursor;
                        screen.HandleInput(_input, gameTime);
                        otherScreenHasFocus = true;
                    }

                    // If this is an active non-popup, inform any subsequent
                    // screens that they are covered by it.
                    if (!screen.IsPopup)
                        coveredByOtherScreen = true;
                }
            }
        }

        public void Draw(GameTime gameTime)
        {
            int transitionCount = 0;
            foreach (GameScreen screen in _screens)
            {
                if (screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.TransitionOff)
                {
                    transitionCount++;
                    if (_transitions.Count < transitionCount)
                    {
                        PresentationParameters pp = _graphics.PresentationParameters;
                        _transitions.Add(new RenderTarget2D(_graphics, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents));
                    }

                    _graphics.SetRenderTarget(_transitions[transitionCount - 1]);
                    _graphics.Clear(Color.Transparent);
                    screen.Draw();
                    _graphics.SetRenderTarget(null);
                }
            }

            _graphics.Clear(Color.Black);

            transitionCount = 0;
            foreach (GameScreen screen in _screens)
            {
                if (screen.ScreenState == ScreenState.Hidden)
                    continue;

                if (screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.TransitionOff)
                {
                    _spriteBatch.Begin(0, BlendState.AlphaBlend);
                    if (screen is PhysicsDemoScreen)
                    {
                        Vector2 position = Vector2.Lerp(_menuScreen.PreviewPosition, new Vector2(_graphics.Viewport.Width, _graphics.Viewport.Height) / 2f, 1f - screen.TransitionPosition);
                        _spriteBatch.Draw(_transitions[transitionCount], position, null, Color.White * Math.Min(screen.TransitionAlpha / 0.2f, 1f), 0f, new Vector2(_transitions[transitionCount].Width, _transitions[transitionCount].Height) / 2f, 0.5f + 0.5f * (1f - screen.TransitionPosition), SpriteEffects.None, 0f);
                    }
                    else
                        _spriteBatch.Draw(_transitions[transitionCount], Vector2.Zero, Color.White * screen.TransitionAlpha);

                    _spriteBatch.End();

                    transitionCount++;
                }
                else
                    screen.Draw();
            }
        }

        public void ExitScreens()
        {
            foreach (GameScreen screen in _screens)
            {
                screen.ExitScreen();
            }
        }

        /// <summary>Adds a new screen to the screen manager.</summary>
        public void AddScreen(GameScreen screen)
        {
            screen.Framework = _game;
            screen.IsExiting = false;
            screen.ScreenManager = this;

            screen.Sprites = _spriteBatch;
            screen.Lines = _lineBatch;
            screen.Quads = _quadRenderer;

            // Tell the screen to load content.
            screen.LoadContent();

            // Loading my take a while so elapsed time is reset to prevent hick-ups
            //TODO: Can't call this in MonoGame at the moment
            //_game.ResetElapsedTime();

            _screens.Add(screen);
        }

        /// <summary>Removes a screen from the screen manager. You should normally use GameScreen.ExitScreen instead of calling
        /// this directly, so the screen can gradually transition off rather than just being instantly removed.</summary>
        public void RemoveScreen(GameScreen screen)
        {
            // Tell the screen to unload content.
            screen.UnloadContent();
            _screens.Remove(screen);
            _screensToUpdate.Remove(screen);
        }
    }
}