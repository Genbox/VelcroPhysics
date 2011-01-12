using System;
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

        // Usage:
        // Press A or D to rotate the ball
        // Alternative controls: Left or Right cursor
        //                       Left or Right trigger on Xbox360 controller
        private Fixture _circleFixture;
        private Fixture _groundFixture;

        private Texture2D _circleSprite;
        private Texture2D _groundSprite;

        // Farseer expects objects to be scaled to MKS (meters, kilos, seconds)
        // 1m equals 64px here
        // (Objects should be scaled to be between 0.1 and 10 meters in size)
        private const float _meterInPx = 64f;

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
            Vector2 _screenCenter = new Vector2(_graphics.GraphicsDevice.Viewport.Width / 2f,
                                                _graphics.GraphicsDevice.Viewport.Height / 2f);

            _batch = new SpriteBatch(_graphics.GraphicsDevice);
            _font = Content.Load<SpriteFont>("font");

            _debugView.LoadContent(_graphics.GraphicsDevice, Content);

            // Load sprites
            _circleSprite = Content.Load<Texture2D>("circleSprite"); //  96px x 96px => 1.5m x 1.5m
            _groundSprite = Content.Load<Texture2D>("groundSprite"); // 512px x 64px =>   8m x 1m

            /* Circle Fixture: */
            // Convert screen center from pixels to meters
            Vector2 _circlePosition = _screenCenter / _meterInPx;
            // Add circle 1.5 meters above the screen center
            _circlePosition -= Vector2.UnitY * 1.5f;

            // Create the circle fixture
            _circleFixture = FixtureFactory.CreateCircle(_world, 96f / (2f * _meterInPx), 1f, _circlePosition);
            _circleFixture.Body.BodyType = BodyType.Dynamic;

            // Give it some bounce and friction
            _circleFixture.Restitution = 0.3f;
            _circleFixture.Friction = 0.5f;

            /* Ground Fixture: */
            Vector2 _groundPosition = _screenCenter / _meterInPx + Vector2.UnitY * 1.25f;

            // Create the ground fixture
            _groundFixture = FixtureFactory.CreateRectangle(_world, 512f / _meterInPx, 64f / _meterInPx, 1f, _groundPosition);
            _groundFixture.Body.IsStatic = true;

            _groundFixture.Restitution = 0.3f;
            _groundFixture.Friction = 0.5f;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState s = Keyboard.GetState();

            // We make it possible to rotate the circle body
            if (s.IsKeyDown(Keys.A)|| s.IsKeyDown(Keys.Left))
                _circleFixture.Body.ApplyTorque(-10);

            if (s.IsKeyDown(Keys.D) || s.IsKeyDown(Keys.Right))
                _circleFixture.Body.ApplyTorque(10);

            // Toggle DebugView
            if (_oldState.IsKeyDown(Keys.F1) && s.IsKeyUp(Keys.F1))
                _showDebug = !_showDebug;

            if (s.IsKeyDown(Keys.Escape))
                Exit();

            // You can rotate the circle using the triggers on the Xbox360 controller.
            GamePadState gamepad = GamePad.GetState(PlayerIndex.One);

            if (gamepad.IsConnected)
            {
                if (gamepad.Buttons.Back == ButtonState.Pressed)
                    Exit();

                float rotation = -4 * gamepad.Triggers.Left;
                _circleFixture.Body.ApplyTorque(rotation);

                rotation = 4 * gamepad.Triggers.Right;
                _circleFixture.Body.ApplyTorque(rotation);
            }

            _oldState = s;

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

            _batch.Begin();

            /* Circle */
            // Convert physics position to screen coordinates
            Vector2 _circlePos = _circleFixture.Body.Position * _meterInPx;
            float _circleRotation = _circleFixture.Body.Rotation;
            
            // Align sprite center to body position
            Vector2 _circleOrigin = new Vector2(_circleSprite.Width / 2f, _circleSprite.Height / 2f);

            _batch.Draw(_circleSprite, _circlePos, null, Color.White, _circleRotation, _circleOrigin, 1f, SpriteEffects.None, 0f);

            /* Ground */
            Vector2 _groundPos = _groundFixture.Body.Position * _meterInPx;
            Vector2 _groundOrigin = new Vector2(_groundSprite.Width / 2f, _groundSprite.Height / 2f);

            _batch.Draw(_groundSprite, _groundPos, null, Color.White, 0f, _groundOrigin, 1f, SpriteEffects.None, 0f);

            // Display instructions
            String _text = "Press F1 to toogle the DebugView\n"+
                           "Press A or D to rotate the ball";
            _batch.DrawString(_font, _text, new Vector2(14f, 14f), Color.Black);
            _batch.DrawString(_font, _text, new Vector2(12f, 12f), Color.White);

            _batch.End();

            // The DebugView needs a projection matrix with the screen size in meters
            Matrix proj = Matrix.CreateOrthographicOffCenter(0f, _graphics.GraphicsDevice.Viewport.Width / _meterInPx,
                                                             _graphics.GraphicsDevice.Viewport.Height / _meterInPx, 0f, 0f, 1f);
            Matrix view = Matrix.Identity;

            if (_showDebug)
            {
                _debugView.RenderDebugData(ref proj, ref view);
            }

            base.Draw(gameTime);
        }
    }
}