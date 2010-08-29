using FarseerPhysics.DebugViewXNA;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HelloWorld
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        // Usage:
        //
        // Press A or D to rotate the ball

        private GraphicsDeviceManager _graphics;

        //The most important object of them all; the world.
        private World _world = new World(new Vector2(0, -20));

        private Fixture _rectangleFixture;
        private Fixture _circleFixture;

        private DebugViewXNA _debugView;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            //Content.RootDirectory = "Content";
            _debugView = new DebugViewXNA(_world);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            DebugViewXNA.LoadContent(_graphics.GraphicsDevice, Content);

            _rectangleFixture = FixtureFactory.CreateRectangle(_world, 50, 5, 1, new Vector2(0, 0));
            _rectangleFixture.Body.IsStatic = true;

            //Give it some bounce and friction
            _rectangleFixture.Restitution = 0.3f;
            _rectangleFixture.Friction = 0.5f;

            //Create the circle fixture
            _circleFixture = FixtureFactory.CreateCircle(_world, 2, 1, new Vector2(10, 10));
            _circleFixture.Body.BodyType = BodyType.Dynamic;

            _circleFixture.Restitution = 0.3f;
            _circleFixture.Friction = 0.5f;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState s = Keyboard.GetState();

            //We make it possible to rotate the circle body
            if (s.IsKeyDown(Keys.A))
                _circleFixture.Body.ApplyTorque(100);

            if (s.IsKeyDown(Keys.D))
                _circleFixture.Body.ApplyTorque(-100);

            //You can rotate the circle using the triggers on the Xbox360 controller.
            GamePadState gamepad = GamePad.GetState(PlayerIndex.One);

            if (gamepad.IsConnected)
            {
                float rotation = 40 * gamepad.Triggers.Left;
                _circleFixture.Body.ApplyTorque(rotation);

                rotation = -40 * gamepad.Triggers.Right;
                _circleFixture.Body.ApplyTorque(rotation);
            }

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

            Matrix proj = Matrix.CreateOrthographic(50 * _graphics.GraphicsDevice.Viewport.AspectRatio, 50, 0, 1);
            Matrix view = Matrix.Identity;

            _debugView.RenderDebugData(ref proj, ref view);

            base.Draw(gameTime);
        }
    }
}
