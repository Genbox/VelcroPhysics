using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.HelloWorld
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _batch;
        private KeyboardState _oldState;
        private SpriteFont _font;
        private bool _showDebug;

        // Toogle DebugView with F1
        private World _world;
        private DebugViewXNA _debugView;

        private Body _circleBody;
        private Body _groundBody;

        private Texture2D _circleSprite;
        private Texture2D _groundSprite;
        private Matrix _projection;

        // Simple camera controls
        private Matrix _view;
        private Matrix _viewDebug;
        private Vector2 _cameraPosition;
        private Vector2 _screenCenter;
        private float _cameraRotation;

        const string Text = "Press F1 to toogle the DebugView\n" +
                            "Press A or D to rotate the ball\n" +
                            "Press Space to jump\n" +
                            "Press Shift + W/S/A/D to move the camera\n" +
                            "Press Shift + Q/E to rotate the camera";

        // Farseer expects objects to be scaled to MKS (meters, kilos, seconds)
        // 1m equals 64px here
        // (Objects should be scaled to be between 0.1 and 10 meters in size)
        private const float MeterInPx = 64f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 480;

            Content.RootDirectory = "Content";

            _world = new World(new Vector2(0, 20));
            _debugView = new DebugViewXNA(_world);
            _showDebug = false;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // The DebugView needs a projection matrix with the screen size in meters
            _projection = Matrix.CreateOrthographicOffCenter(0f, _graphics.GraphicsDevice.Viewport.Width / MeterInPx,
                                                             _graphics.GraphicsDevice.Viewport.Height / MeterInPx, 0f, 0f, 1f);
            // Initialize camera controls
            _view = Matrix.Identity;
            _viewDebug = Matrix.Identity;
            _cameraPosition = Vector2.Zero;
            _cameraRotation = 0f;

            _screenCenter = new Vector2(_graphics.GraphicsDevice.Viewport.Width / 2f,
                                                _graphics.GraphicsDevice.Viewport.Height / 2f);

            _batch = new SpriteBatch(_graphics.GraphicsDevice);
            _font = Content.Load<SpriteFont>("font");

            _debugView.LoadContent(_graphics.GraphicsDevice, Content);

            // Load sprites
            _circleSprite = Content.Load<Texture2D>("circleSprite"); //  96px x 96px => 1.5m x 1.5m
            _groundSprite = Content.Load<Texture2D>("groundSprite"); // 512px x 64px =>   8m x 1m

            /* Circle Fixture: */
            // Convert screen center from pixels to meters
            Vector2 circlePosition = _screenCenter / MeterInPx;
            // Add circle 1.5 meters above the screen center
            circlePosition -= Vector2.UnitY * 1.5f;

            // Create the circle fixture
            _circleBody = BodyFactory.CreateCircle(_world, 96f / (2f * MeterInPx), 1f, circlePosition);
            _circleBody.BodyType = BodyType.Dynamic;

            // Give it some bounce and friction
            _circleBody.Restitution = 0.3f;
            _circleBody.Friction = 0.5f;

            /* Ground Fixture: */
            Vector2 groundPosition = _screenCenter / MeterInPx + Vector2.UnitY * 1.25f;

            // Create the ground fixture
            _groundBody = BodyFactory.CreateRectangle(_world, 512f / MeterInPx, 64f / MeterInPx, 1f, groundPosition);
            _groundBody.IsStatic = true;

            _groundBody.Restitution = 0.3f;
            _groundBody.Friction = 0.5f;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();

            // Switch between circle body and camera control
            if (state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift))
            {
                // Move camera
                if (state.IsKeyDown(Keys.A))
                    _cameraPosition.X += 1.5f;

                if (state.IsKeyDown(Keys.D))
                    _cameraPosition.X -= 1.5f;

                if (state.IsKeyDown(Keys.W))
                    _cameraPosition.Y += 1.5f;

                if (state.IsKeyDown(Keys.S))
                    _cameraPosition.Y -= 1.5f;

                // Rotate camera
                if (state.IsKeyDown(Keys.Q))
                    _cameraRotation -= 0.05f;

                if (state.IsKeyDown(Keys.E))
                    _cameraRotation += 0.05f;

                _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) *
                        Matrix.CreateRotationZ(_cameraRotation) *
                        Matrix.CreateTranslation(new Vector3(_screenCenter, 0f));
                _viewDebug = Matrix.CreateTranslation(new Vector3((_cameraPosition - _screenCenter) / MeterInPx, 0f)) *
                             Matrix.CreateRotationZ(_cameraRotation) *
                             Matrix.CreateTranslation(new Vector3(_screenCenter / MeterInPx, 0f));
            }
            else
            {
                // We make it possible to rotate the circle body
                if (state.IsKeyDown(Keys.A))
                    _circleBody.ApplyTorque(-10);

                if (state.IsKeyDown(Keys.D))
                    _circleBody.ApplyTorque(10);

                if (state.IsKeyDown(Keys.Space) && _oldState.IsKeyUp(Keys.Space))
                    _circleBody.ApplyLinearImpulse(new Vector2(0, -10));
            }

            // Toggle DebugView
            if (state.IsKeyUp(Keys.F1) && _oldState.IsKeyDown(Keys.F1))
                _showDebug = !_showDebug;

            if (state.IsKeyDown(Keys.Escape))
                Exit();

            _oldState = state;

            //We update the world
            _world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            /* Circle position and rotation */
            // Convert physics position to screen coordinates
            Vector2 circlePos = _circleBody.Position * MeterInPx;
            float circleRotation = _circleBody.Rotation;

            /* Ground position and origin */
            Vector2 groundPos = _groundBody.Position * MeterInPx;
            Vector2 groundOrigin = new Vector2(_groundSprite.Width / 2f, _groundSprite.Height / 2f);

            // Align sprite center to body position
            Vector2 circleOrigin = new Vector2(_circleSprite.Width / 2f, _circleSprite.Height / 2f);

            _batch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _view);

            //Draw circle
            _batch.Draw(_circleSprite, circlePos, null, Color.White, circleRotation, circleOrigin, 1f, SpriteEffects.None, 0f);

            //Draw ground
            _batch.Draw(_groundSprite, groundPos, null, Color.White, 0f, groundOrigin, 1f, SpriteEffects.None, 0f);

            _batch.End();

            _batch.Begin();

            // Display instructions
            _batch.DrawString(_font, Text, new Vector2(14f, 14f), Color.Black);
            _batch.DrawString(_font, Text, new Vector2(12f, 12f), Color.White);

            _batch.End();

            if (_showDebug)
            {
                _debugView.RenderDebugData(ref _projection, ref _viewDebug);
            }

            base.Draw(gameTime);
        }
    }
}