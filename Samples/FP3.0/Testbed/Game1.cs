/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using FarseerPhysics.Common;
using FarseerPhysics.TestBed.Framework;
using FarseerPhysics.TestBed.Tests;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        private const float SettingsHz = 60.0f;
        public bool DebugViewEnabled = true;
        private TestEntry _entry;
        private GraphicsDeviceManager _graphics;
        private int _height = 480;
        private GamePadState _oldGamePad;
        private KeyboardState _oldKeyboardState;
        private MouseState _oldMouseState;
        private Matrix _projection;
        private GameSettings _settings = new GameSettings();
        private Test _test;
        private int _testCount;
        private int _testIndex;
        private int _testSelection;
        private int _viewportHeight;
        private int _viewportWidth;
        private Vector2 _viewCenter = new Vector2(0.0f, 20.0f);
        private float _viewZoom = 1.0f;
        private int _width = 640;
        private Vector2 _lower;
        private Vector2 _upper;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _graphics.PreferMultiSampling = true;
            IsMouseVisible = true;

            IsFixedTimeStep = true;

            _graphics.SynchronizeWithVerticalRetrace = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            //Set window defaults. Parent game can override in constructor
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += WindowClientSizeChanged;

            _testCount = 0;
            while (TestEntries.g_testEntries[_testCount].CreateFcn != null)
            {
                ++_testCount;
            }

            _testIndex = MathUtils.Clamp(_testIndex, 0, _testCount - 1);
            _testSelection = _testIndex;
            StartTest(_testIndex);

            //settings.drawAABBs = 1;
        }

        private void StartTest(int index)
        {
            _entry = TestEntries.g_testEntries[index];
            _test = _entry.CreateFcn();
            _test.GameInstance = this;
            _test.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            DebugViewXNA.DebugViewXNA.LoadContent(GraphicsDevice, Content);

            _oldKeyboardState = Keyboard.GetState();
            _oldMouseState = Mouse.GetState();
            _oldGamePad = GamePad.GetState(PlayerIndex.One);

            Resize(GraphicsDevice.PresentationParameters.BackBufferWidth,
                   GraphicsDevice.PresentationParameters.BackBufferHeight);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            KeyboardState newKeyboardState = Keyboard.GetState();
            GamePadState newGamePad = GamePad.GetState(PlayerIndex.One);
            MouseState newMouseState = Mouse.GetState();

            // Press 'z' to zoom out.
            if (newKeyboardState.IsKeyDown(Keys.Z))
            {
                _viewZoom = Math.Min(1.1f * _viewZoom, 20.0f);
                Resize(_width, _height);
            }
            // Press 'x' to zoom in.
            else if (newKeyboardState.IsKeyDown(Keys.X))
            {
                _viewZoom = Math.Max(0.9f * _viewZoom, 0.02f);
                Resize(_width, _height);
            }
            // Press 'r' to reset.
            else if (newKeyboardState.IsKeyDown(Keys.R) && _oldKeyboardState.IsKeyUp(Keys.R))
            {
                Restart();
            }
            else if ((newKeyboardState.IsKeyDown(Keys.P) && _oldKeyboardState.IsKeyUp(Keys.P)) ||
                     newGamePad.IsButtonDown(Buttons.Start) && _oldGamePad.IsButtonUp(Buttons.Start))
            {
                _settings.Pause = !_settings.Pause;
            }
            // Press I to prev test.
            else if ((newKeyboardState.IsKeyDown(Keys.I) && _oldKeyboardState.IsKeyUp(Keys.I)) ||
                     newGamePad.IsButtonDown(Buttons.LeftShoulder) && _oldGamePad.IsButtonUp(Buttons.LeftShoulder))
            {
                --_testSelection;
                if (_testSelection < 0)
                {
                    _testSelection = _testCount - 1;
                }
            }
            // Press O to next test.
            else if ((newKeyboardState.IsKeyDown(Keys.O) && _oldKeyboardState.IsKeyUp(Keys.O)) ||
                     newGamePad.IsButtonDown(Buttons.RightShoulder) && _oldGamePad.IsButtonUp(Buttons.RightShoulder))
            {
                ++_testSelection;
                if (_testSelection == _testCount)
                {
                    _testSelection = 0;
                }
            }
            // Press left to pan left.
            else if (newKeyboardState.IsKeyDown(Keys.Left))
            {
                _viewCenter.X -= 0.5f;
                Resize(_width, _height);
            }
            // Press right to pan right.
            else if (newKeyboardState.IsKeyDown(Keys.Right))
            {
                _viewCenter.X += 0.5f;
                Resize(_width, _height);
            }
            // Press down to pan down.
            else if (newKeyboardState.IsKeyDown(Keys.Down))
            {
                _viewCenter.Y -= 0.5f;
                Resize(_width, _height);
            }
            // Press up to pan up.
            else if (newKeyboardState.IsKeyDown(Keys.Up))
            {
                _viewCenter.Y += 0.5f;
                Resize(_width, _height);
            }
            // Press home to reset the view.
            else if (newKeyboardState.IsKeyDown(Keys.Home) && _oldKeyboardState.IsKeyUp(Keys.Home))
            {
                _viewZoom = 1.0f;
                _viewCenter = new Vector2(0.0f, 20.0f);
                Resize(_width, _height);
            }
            else if (newKeyboardState.IsKeyDown(Keys.F1) && _oldKeyboardState.IsKeyUp(Keys.F1))
            {
                DebugViewEnabled = !DebugViewEnabled;
            }
            else
            {
                if (_test != null)
                {
                    _test.Keyboard(newKeyboardState, _oldKeyboardState);
                }
            }

            if (_test != null)
                _test.Mouse(newMouseState, _oldMouseState);

            if (_test != null && newGamePad.IsConnected)
                _test.Gamepad(newGamePad, _oldGamePad);

            base.Update(gameTime);

            _oldKeyboardState = newKeyboardState;
            _oldMouseState = newMouseState;
            _oldGamePad = newGamePad;

            _settings.Hz = SettingsHz;

            if (_test != null)
            {
                _test.TextLine = 30;
                _test.Update(_settings, gameTime);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _test.DrawTitle(50, 15, _entry.Name);

            if (_testSelection != _testIndex)
            {
                _testIndex = _testSelection;
                StartTest(_testIndex);
                _viewZoom = 1.0f;
                _viewCenter = new Vector2(0.0f, 20.0f);
                Resize(_width, _height);
            }

            _test.Draw(gameTime);
            _test.DebugView.RenderDebugData(ref _projection);

            base.Draw(gameTime);
        }

        private void Resize(int w, int h)
        {
            _width = w;
            _height = h;

            _viewportWidth = GraphicsDevice.Viewport.Width;
            _viewportHeight = GraphicsDevice.Viewport.Height;

            float ratio = _viewportWidth / (float)_viewportHeight;

            Vector2 extents = new Vector2(ratio * 25.0f, 25.0f);
            extents *= _viewZoom;

            _lower = _viewCenter - extents;
            _upper = _viewCenter + extents;

            // L/R/B/T
            _projection = Matrix.CreateOrthographicOffCenter(_lower.X, _upper.X, _lower.Y, _upper.Y, -1, 1);
        }

        public Vector2 ConvertScreenToWorld(int x, int y)
        {
            float u = x / (float)_viewportWidth;
            float v = (_viewportHeight - y) / (float)_viewportHeight;

            Vector2 p = new Vector2();
            p.X = (1.0f - u) * _lower.X + u * _upper.X;
            p.Y = (1.0f - v) * _lower.Y + v * _upper.Y;

            return p;
        }

        private void Restart()
        {
            StartTest(_testIndex);
            Resize(_width, _height);
        }

        private void WindowClientSizeChanged(object sender, EventArgs e)
        {
            if (Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0)
            {
                _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            }
        }
    }
}