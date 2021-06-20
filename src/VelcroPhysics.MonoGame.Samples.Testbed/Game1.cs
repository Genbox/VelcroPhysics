using System;
using Genbox.VelcroPhysics.Extensions.DebugView;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed
{
    public sealed class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly KeyboardManager _keyboardManager = new KeyboardManager();
        private readonly GamePadManager _gamePadManager = new GamePadManager();
        private readonly MouseManager _mouseManager = new MouseManager();
        private readonly GameSettings _settings = new GameSettings();
        private Vector2 _lower;
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

            Window.Title = "Velcro Physics Testbed";
            _graphics = new GraphicsDeviceManager(this);
            IsFixedTimeStep = true;
            IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        public float ViewZoom
        {
            get => _viewZoom;
            set
            {
                _viewZoom = value;
                ResizeViewMatrix();
            }
        }

        public Vector2 ViewCenter
        {
            get => _viewCenter;
            set
            {
                _viewCenter = value;
                ResizeViewMatrix();
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            //Set window defaults. Parent game can override in constructor
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += WindowClientSizeChanged;

            _graphics.PreferMultiSampling = true;
            _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width - 80;
            _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height - 120;

            _graphics.ApplyChanges();

            CreateProjection();

            _testCount = 0;
            while (TestEntries.TestList[_testCount] != null)
                ++_testCount;

            _testIndex = MathUtils.Clamp(_testIndex, 0, _testCount - 1);
            _testSelection = _testIndex;

            StartTest(_testIndex);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _graphics.Dispose();

            base.Dispose(disposing);
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
            _test = TestEntries.TestList[index]();
            _test.GameInstance = this;
            _test.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            // clear graphics here because some tests already draw during update
            GraphicsDevice.Clear(Color.Black);

            float maxZoom = 20f;
            float minZoom = 0.5f;

            float zoomScale = (ViewZoom - minZoom) / maxZoom;
            float moveScale = 1 / MathHelper.Lerp(minZoom, maxZoom, zoomScale);

            // Allows the game to exit
            if (_gamePadManager.IsNewButtonPress(Buttons.Back))
                Exit();

            if (_keyboardManager.IsNewKeyPress(Keys.R)) // Press 'r' to reset.
                Restart();
            else if (_keyboardManager.IsNewKeyPress(Keys.P) || _gamePadManager.IsNewButtonPress(Buttons.Start)) // Press I to go to prev test.
                _settings.Pause = !_settings.Pause;
            else if (_keyboardManager.IsNewKeyPress(Keys.I) || _gamePadManager.IsNewButtonPress(Buttons.LeftShoulder))
            {
                --_testSelection;
                if (_testSelection < 0)
                    _testSelection = _testCount - 1;
            }
            else if (_keyboardManager.IsNewKeyPress(Keys.O) || _gamePadManager.IsNewButtonPress(Buttons.RightShoulder)) // Press O to go to next test.
            {
                ++_testSelection;
                if (_testSelection == _testCount)
                    _testSelection = 0;
            }
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
                if (_keyboardManager.IsKeyDown(Keys.Left)) // Press left to pan left.
                    ViewCenter = new Vector2(ViewCenter.X - moveScale, ViewCenter.Y);
                if (_keyboardManager.IsKeyDown(Keys.Right)) // Press right to pan right.
                    ViewCenter = new Vector2(ViewCenter.X + moveScale, ViewCenter.Y);
                if (_keyboardManager.IsKeyDown(Keys.Down)) // Press down to pan down.
                    ViewCenter = new Vector2(ViewCenter.X, ViewCenter.Y - moveScale);
                if (_keyboardManager.IsKeyDown(Keys.Up)) // Press up to pan up.
                    ViewCenter = new Vector2(ViewCenter.X, ViewCenter.Y + moveScale);

                if (_keyboardManager.IsKeyDown(Keys.OemMinus) || _keyboardManager.IsKeyDown(Keys.Subtract)) // Press '-' to zoom out.
                    ViewZoom *= 0.9512294f;
                else if (_keyboardManager.IsKeyDown(Keys.OemPlus) || _keyboardManager.IsKeyDown(Keys.Add)) // Press '+' to zoom in.
                    ViewZoom *= 1.051271f;
                else if (_mouseManager.DeltaScrollValue != 0) // Zoom with mouse wheel
                    ViewZoom *= Math.Sign(_mouseManager.DeltaScrollValue) == -1 ? 0.85f : 1.15f;

                ViewZoom = MathUtils.Clamp(ViewZoom, minZoom, maxZoom);

                _test?.Keyboard(_keyboardManager);
            }

            //Support moving the camera around with right-click movement
            Vector2 currentPos = ConvertScreenToWorld(_mouseManager.NewPosition);

            if (_mouseManager.NewState.RightButton == ButtonState.Pressed)
                ViewCenter += ConvertScreenToWorld(_mouseManager.OldPosition) - currentPos;
            else
                _test?.Mouse(_mouseManager);

            if (_gamePadManager.IsConnected)
                _test?.Gamepad(_gamePadManager);

            base.Update(gameTime);

            _keyboardManager.Update();
            _gamePadManager.Update();
            _mouseManager.Update();

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
            _test.DebugView.DrawString(50, 15, _test.GetType().Name.Replace("Test", string.Empty, StringComparison.OrdinalIgnoreCase));

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
            ResizeViewMatrix();
        }

        private void ResizeViewMatrix()
        {
            View = Matrix.CreateTranslation(new Vector3(-ViewCenter.X, -ViewCenter.Y, 0)) * Matrix.CreateScale(ViewZoom);
        }

        public Vector2 ConvertWorldToScreen(Vector2 position)
        {
            Vector3 temp = GraphicsDevice.Viewport.Project(new Vector3(position, 0), Projection, View, Matrix.Identity);
            return new Vector2(temp.X, temp.Y);
        }

        public Vector2 ConvertScreenToWorld(Vector2 position)
        {
            Vector3 temp = GraphicsDevice.Viewport.Unproject(new Vector3(position, 0), Projection, View, Matrix.Identity);
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