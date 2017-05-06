using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Samples.HelloWorld
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _batch;
        private KeyboardState _oldKeyState;
        private GamePadState _oldPadState;
        private SpriteFont _font;

        private readonly World _world;

        private Body _circleBody;
        private Body _groundBody;

        private Texture2D _circleSprite;
        private Texture2D _groundSprite;

        // Simple camera controls
        private Matrix _view;

        private Vector2 _cameraPosition;
        private Vector2 _screenCenter;
        private Vector2 _groundOrigin;
        private Vector2 _circleOrigin;

#if !XBOX360

        private const string Text = "Press A or D to rotate the ball\n" +
                                    "Press Space to jump\n" +
                                    "Use arrow keys to move the camera";

#else
                const string Text = "Use left stick to move\n" +
                                    "Use right stick to move camera\n" +
                                    "Press A to jump\n";
#endif

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 480;

            Content.RootDirectory = "Content";

            //Create a world with gravity.
            _world = new World(new Vector2(0, 9.82f));
        }

        protected override void LoadContent()
        {
            // Initialize camera controls
            _view = Matrix.Identity;
            _cameraPosition = Vector2.Zero;
            _screenCenter = new Vector2(_graphics.GraphicsDevice.Viewport.Width / 2f, _graphics.GraphicsDevice.Viewport.Height / 2f);
            _batch = new SpriteBatch(_graphics.GraphicsDevice);

            _font = Content.Load<SpriteFont>("font");

            // Load sprites
            _circleSprite = Content.Load<Texture2D>("CircleSprite"); //  96px x 96px => 1.5m x 1.5m
            _groundSprite = Content.Load<Texture2D>("GroundSprite"); // 512px x 64px =>   8m x 1m

            /* We need XNA to draw the ground and circle at the center of the shapes */
            _groundOrigin = new Vector2(_groundSprite.Width / 2f, _groundSprite.Height / 2f);
            _circleOrigin = new Vector2(_circleSprite.Width / 2f, _circleSprite.Height / 2f);

            // Velcro Physics expects objects to be scaled to MKS (meters, kilos, seconds)
            // 1 meters equals 64 pixels here
            ConvertUnits.SetDisplayUnitToSimUnitRatio(64f);

            /* Circle */
            // Convert screen center from pixels to meters
            Vector2 circlePosition = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(0, -1.5f);

            // Create the circle fixture
            _circleBody = BodyFactory.CreateCircle(_world, ConvertUnits.ToSimUnits(96 / 2f), 1f, circlePosition, BodyType.Dynamic);

            // Give it some bounce and friction
            _circleBody.Restitution = 0.3f;
            _circleBody.Friction = 0.5f;

            /* Ground */
            Vector2 groundPosition = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(0, 1.25f);

            // Create the ground fixture
            _groundBody = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(512f), ConvertUnits.ToSimUnits(64f), 1f, groundPosition);
            _groundBody.BodyType = BodyType.Static;
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
            HandleGamePad();
            HandleKeyboard();

            //We update the world
            _world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f);

            base.Update(gameTime);
        }

        private void HandleGamePad()
        {
            GamePadState padState = GamePad.GetState(0);

            if (padState.IsConnected)
            {
                if (padState.Buttons.Back == ButtonState.Pressed)
                    Exit();

                if (padState.Buttons.A == ButtonState.Pressed && _oldPadState.Buttons.A == ButtonState.Released)
                    _circleBody.ApplyLinearImpulse(new Vector2(0, -10));

                _circleBody.ApplyForce(padState.ThumbSticks.Left);
                _cameraPosition.X -= padState.ThumbSticks.Right.X;
                _cameraPosition.Y += padState.ThumbSticks.Right.Y;

                _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) * Matrix.CreateTranslation(new Vector3(_screenCenter, 0f));

                _oldPadState = padState;
            }
        }

        private void HandleKeyboard()
        {
            KeyboardState state = Keyboard.GetState();

            // Move camera
            if (state.IsKeyDown(Keys.Left))
                _cameraPosition.X += 1.5f;

            if (state.IsKeyDown(Keys.Right))
                _cameraPosition.X -= 1.5f;

            if (state.IsKeyDown(Keys.Up))
                _cameraPosition.Y += 1.5f;

            if (state.IsKeyDown(Keys.Down))
                _cameraPosition.Y -= 1.5f;

            _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) * Matrix.CreateTranslation(new Vector3(_screenCenter, 0f));

            // We make it possible to rotate the circle body
            if (state.IsKeyDown(Keys.A))
                _circleBody.ApplyTorque(-10);

            if (state.IsKeyDown(Keys.D))
                _circleBody.ApplyTorque(10);

            if (state.IsKeyDown(Keys.Space) && _oldKeyState.IsKeyUp(Keys.Space))
                _circleBody.ApplyLinearImpulse(new Vector2(0, -10));

            if (state.IsKeyDown(Keys.Escape))
                Exit();

            _oldKeyState = state;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Draw circle and ground
            _batch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _view);
            _batch.Draw(_circleSprite, ConvertUnits.ToDisplayUnits(_circleBody.Position), null, Color.White, _circleBody.Rotation, _circleOrigin, 1f, SpriteEffects.None, 0f);
            _batch.Draw(_groundSprite, ConvertUnits.ToDisplayUnits(_groundBody.Position), null, Color.White, 0f, _groundOrigin, 1f, SpriteEffects.None, 0f);
            _batch.End();

            // Display instructions
            _batch.Begin();
            _batch.DrawString(_font, Text, new Vector2(14f, 14f), Color.Black);
            _batch.DrawString(_font, Text, new Vector2(12f, 12f), Color.White);
            _batch.End();

            base.Draw(gameTime);
        }
    }
}