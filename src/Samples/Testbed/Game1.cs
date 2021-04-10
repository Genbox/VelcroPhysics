using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using VelcroPhysics.Extensions.DebugView;
using VelcroPhysics.Samples.Testbed.Framework;
using VelcroPhysics.Samples.Testbed.Tests;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Samples.Testbed
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly KeyboardManager _keyboardManager = new KeyboardManager();
        private readonly GameSettings _settings = new GameSettings();
        private TestEntry _entry;
        private Vector2 _lower;
        private GamePadState _oldGamePad;
        private MouseState _oldMouseState;
        private Test _test;
        private int _testCount;
        private int _testIndex;
        private int _testSelection;
        private Vector2 _upper;
        private Vector2 _viewCenter;

        private float _viewZoom;
        public Matrix Projection;
        public Matrix View;

        public Game1()
        {
            //Default view
            ResetView();

            _graphics = new GraphicsDeviceManager(this);

            //_graphics.PreferMultiSampling = true; // https://github.com/MonoGame/MonoGame/issues/5532
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            IsFixedTimeStep = true;

            _graphics.SynchronizeWithVerticalRetrace = false;
        }

        public float ViewZoom
        {
            get { return _viewZoom; }
            set
            {
                _viewZoom = value;
                Resize();
            }
        }

        public Vector2 ViewCenter
        {
            get { return _viewCenter; }
            set
            {
                _viewCenter = value;
                Resize();
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            //Set window defaults. Parent game can override in constructor
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += WindowClientSizeChanged;

            CreateProjection();

            _testCount = 0;
            while (TestEntries.TestList[_testCount].CreateTest != null)
            {
                ++_testCount;
            }

            _testIndex = MathUtils.Clamp(_testIndex, 0, _testCount - 1);
            _testSelection = _testIndex;
            StartTest(_testIndex);
        }

        private void CreateProjection()
        {
            _lower = -new Vector2(25.0f * GraphicsDevice.Viewport.AspectRatio, 25.0f);
            _upper = new Vector2(25.0f * GraphicsDevice.Viewport.AspectRatio, 25.0f);

            // L/R/B/T
            Projection = Matrix.CreateOrthographicOffCenter(_lower.X, _upper.X, _lower.Y, _upper.Y, -1, 1);
        }

        private void StartTest(int index)
        {
            _entry = TestEntries.TestList[index];
            _test = _entry.CreateTest();
            _test.GameInstance = this;
            _test.Initialize();
        }

        protected override void LoadContent()
        {
            _keyboardManager._oldKeyboardState = Keyboard.GetState();
            _oldMouseState = Mouse.GetState();
            _oldGamePad = GamePad.GetState(PlayerIndex.One);
        }

        protected override void Update(GameTime gameTime)
        {
            // clear graphics here because some tests already draw during update
            GraphicsDevice.Clear(Color.Black);

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            _keyboardManager._newKeyboardState = Keyboard.GetState();
            GamePadState newGamePad = GamePad.GetState(PlayerIndex.One);
            MouseState newMouseState = Mouse.GetState();

            if (_keyboardManager.IsKeyDown(Keys.Z)) // Press 'z' to zoom out.
                ViewZoom = Math.Min(1.1f * ViewZoom, 20.0f);
            else if (_keyboardManager.IsKeyDown(Keys.X)) // Press 'x' to zoom in.
                ViewZoom = Math.Max(0.9f * ViewZoom, 0.02f);
            else if (_keyboardManager.IsNewKeyPress(Keys.R)) // Press 'r' to reset.
                Restart();
            else if (_keyboardManager.IsNewKeyPress(Keys.P) || newGamePad.IsButtonDown(Buttons.Start) && _oldGamePad.IsButtonUp(Buttons.Start)) // Press I to prev test.
                _settings.Pause = !_settings.Pause;
            else if (_keyboardManager.IsNewKeyPress(Keys.I) || newGamePad.IsButtonDown(Buttons.LeftShoulder) && _oldGamePad.IsButtonUp(Buttons.LeftShoulder))
            {
                --_testSelection;
                if (_testSelection < 0)
                    _testSelection = _testCount - 1;
            }
            else if (_keyboardManager.IsNewKeyPress(Keys.O) || newGamePad.IsButtonDown(Buttons.RightShoulder) && _oldGamePad.IsButtonUp(Buttons.RightShoulder)) // Press O to next test.
            {
                ++_testSelection;
                if (_testSelection == _testCount)
                    _testSelection = 0;
            }
            else if (_keyboardManager.IsKeyDown(Keys.Left)) // Press left to pan left.
                ViewCenter = new Vector2(ViewCenter.X - 0.5f, ViewCenter.Y);
            else if (_keyboardManager.IsKeyDown(Keys.Right)) // Press right to pan right.
                ViewCenter = new Vector2(ViewCenter.X + 0.5f, ViewCenter.Y);
            else if (_keyboardManager.IsKeyDown(Keys.Down)) // Press down to pan down.
                ViewCenter = new Vector2(ViewCenter.X, ViewCenter.Y - 0.5f);
            else if (_keyboardManager.IsKeyDown(Keys.Up)) // Press up to pan up.
                ViewCenter = new Vector2(ViewCenter.X, ViewCenter.Y + 0.5f);
            else if (_keyboardManager.IsNewKeyPress(Keys.Home)) // Press home to reset the view.
                ResetView();
            else if (_keyboardManager.IsNewKeyPress(Keys.F1))
                EnableOrDisableFlag(DebugViewFlags.Shape);
            else if (_keyboardManager.IsNewKeyPress(Keys.F2))
                EnableOrDisableFlag(DebugViewFlags.DebugPanel);
            else if (_keyboardManager.IsNewKeyPress(Keys.F3))
                EnableOrDisableFlag(DebugViewFlags.PerformanceGraph);
            else if (_keyboardManager.IsNewKeyPress(Keys.F4))
                EnableOrDisableFlag(DebugViewFlags.AABB);
            else if (_keyboardManager.IsNewKeyPress(Keys.F5))
                EnableOrDisableFlag(DebugViewFlags.CenterOfMass);
            else if (_keyboardManager.IsNewKeyPress(Keys.F6))
                EnableOrDisableFlag(DebugViewFlags.Joint);
            else if (_keyboardManager.IsNewKeyPress(Keys.F7))
            {
                EnableOrDisableFlag(DebugViewFlags.ContactPoints);
                EnableOrDisableFlag(DebugViewFlags.ContactNormals);
            }
            else if (_keyboardManager.IsNewKeyPress(Keys.F8))
                EnableOrDisableFlag(DebugViewFlags.PolygonPoints);
            else if (_keyboardManager.IsNewKeyPress(Keys.F9))
                EnableOrDisableFlag(DebugViewFlags.PolygonPoints);
            else
            {
                if (_test != null)
                    _test.Keyboard(_keyboardManager);
            }

            if (_test != null)
                _test.Mouse(newMouseState, _oldMouseState);

            if (_test != null && newGamePad.IsConnected)
                _test.Gamepad(newGamePad, _oldGamePad);

            base.Update(gameTime);

            _keyboardManager._oldKeyboardState = _keyboardManager._newKeyboardState;
            _oldMouseState = newMouseState;
            _oldGamePad = newGamePad;

            if (_test != null)
            {
                _test.TextLine = 30;
                _test.Update(_settings, gameTime);
            }
        }

        private void EnableOrDisableFlag(DebugViewFlags flag)
        {
            if ((_test.DebugView.Flags & flag) == flag)
                _test.DebugView.RemoveFlags(flag);
            else
                _test.DebugView.AppendFlags(flag);
        }

        protected override void Draw(GameTime gameTime)
        {
            _test.DrawTitle(50, 15, _entry.Name);

            if (_testSelection != _testIndex)
            {
                _testIndex = _testSelection;
                StartTest(_testIndex);
                ResetView();
            }

            _test.DebugView.RenderDebugData(ref Projection, ref View);

            base.Draw(gameTime);
        }

        private void ResetView()
        {
            _viewZoom = 0.8f;
            _viewCenter = new Vector2(0.0f, 20.0f);
            Resize();
        }

        private void Resize()
        {
            View = Matrix.CreateTranslation(new Vector3(-ViewCenter.X, -ViewCenter.Y, 0)) * Matrix.CreateScale(ViewZoom);
        }

        public Vector2 ConvertWorldToScreen(Vector2 position)
        {
            Vector3 temp = GraphicsDevice.Viewport.Project(new Vector3(position, 0), Projection, View, Matrix.Identity);
            return new Vector2(temp.X, temp.Y);
        }

        public Vector2 ConvertScreenToWorld(int x, int y)
        {
            Vector3 temp = GraphicsDevice.Viewport.Unproject(new Vector3(x, y, 0), Projection, View, Matrix.Identity);
            return new Vector2(temp.X, temp.Y);
        }

        private void Restart()
        {
            StartTest(_testIndex);
        }

        private void WindowClientSizeChanged(object sender, EventArgs e)
        {
            if (Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0)
            {
                _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            }

            //We want to keep aspec ratio. Recalcuate the projection matrix.
            CreateProjection();
        }
    }
}