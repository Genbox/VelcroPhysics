using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DemoBaseXNA;
using DemoBaseXNA.DemoShare;
using DemoBaseXNA.DrawingSystem;
using DemoBaseXNA.ScreenSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.AdvancedSamplesXNA.Demo1
{
    public class Demo1Screen : GameScreen
    {
        private Pool<Ball> _pool;
        private static bool _usePool;
        private List<Ball> _ballsToDraw = new List<Ball>();
        private Stopwatch _stopWatch = new Stopwatch();
        private Vector2 _timerPosition = new Vector2(100, 150);
        private Vector2 _ballCountPosition = new Vector2(100, 170);
        private Vector2 _poolEnabledPosition = new Vector2(100, 190);
        private Texture2D _panelTexture;
        private Vector2 _panelPosition = new Vector2(80, 130);
        private const int _maxBalls = 120;

        public Demo1Screen(bool usePool)
        {
            _usePool = usePool;
        }

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 150));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            if (_usePool)
                LoadPool();

            base.Initialize();
        }

        private void LoadPool()
        {
            //Create the empty pool
            _pool = new Pool<Ball>();

            //Preload balls
            for (int i = 0; i < _maxBalls; i++)
            {
                Ball ball = new Ball();
                ball.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);
                ball.Geom.Tag = ball;

                _pool.Insert(ball);
            }
        }

        public override void LoadContent()
        {
            _panelTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 290, 100, new Color(0, 0, 0, 155));

            base.LoadContent();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                while (_ballsToDraw.Count < _maxBalls)
                {
                    _stopWatch.Reset();
                    _stopWatch.Start();

                    if (_usePool)
                    {
                        Ball ball = _pool.Fetch();

                        //We need to reset the dynamics of the body.
                        ball.Body.ResetDynamics();

                        ball.Body.Position = new Vector2(Calculator.RandomNumber(50, ScreenManager.ScreenWidth - 50),
                                                         Calculator.RandomNumber(50, ScreenManager.ScreenHeight - 50));
                        ball.Geom.OnCollision += OnCollision;

                        //Reactivate the body
                        ball.Geom.Body.Enabled = true;

                        _ballsToDraw.Add(ball);
                    }
                    else
                    {
                        Ball ball = new Ball();
                        ball.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);
                        ball.Body.Position = new Vector2(Calculator.RandomNumber(50, ScreenManager.ScreenWidth - 50),
                                                         Calculator.RandomNumber(50, ScreenManager.ScreenHeight - 50));
                        ball.Geom.OnCollision += OnCollision;
                        ball.Geom.Tag = ball;
                        _ballsToDraw.Add(ball);
                    }

                    _stopWatch.Stop();

                }
            }
        }

        private bool OnCollision(Geom geom1, Geom geom2, ContactList contactList)
        {
            Ball ball = (Ball)geom1.Tag;

            //Remove the collision event
            geom1.OnCollision -= OnCollision;

            //Disable the body
            geom1.Body.Enabled = false;

            if (_usePool)
            {
                //Insert the ball back to the pool
                _pool.Insert(ball);
            }
            else
            {
                //Remove from physics simulator
                geom1.Tag = null;
                PhysicsSimulator.Remove(geom1.Body);
                PhysicsSimulator.Remove(geom1);
            }

            //Remove it from drawing list
            _ballsToDraw.Remove(ball);

            //Cancel the collision since we are removing the geom from simulation.
            return false;
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            foreach (Ball ball in _ballsToDraw)
            {
                ball.Draw(ScreenManager.SpriteBatch);
            }

            ScreenManager.SpriteBatch.Draw(_panelTexture, _panelPosition, Color.Black);

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DiagnosticSpriteFont, "Ticks per ball creation: " + _stopWatch.ElapsedTicks, _timerPosition, Color.White);
            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DiagnosticSpriteFont, "Current number of balls: " + _ballsToDraw.Count, _ballCountPosition, Color.White);
            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DiagnosticSpriteFont, "Using pool: " + _usePool + " (press R to toggle)", _poolEnabledPosition, Color.White);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public override void HandleInput(InputState input)
        {
            if (firstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
                firstRun = false;
            }

            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
            }

            HandleKeyboardInput(input);
            base.HandleInput(input);
        }

        private void HandleKeyboardInput(InputState input)
        {
            if (input.LastKeyboardState.IsKeyUp(Keys.R) && input.CurrentKeyboardState.IsKeyDown(Keys.R))
            {
                ExitScreen();
                ScreenManager.AddScreen(new Demo1Screen(!_usePool));
            }
        }

        public static string GetTitle()
        {
            return "Demo1: Object pre-loading/caching";
        }

        private static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Shows the performance improvement using pools.");
            sb.AppendLine("Please have patience while the demo loads.");
            sb.AppendLine("In a real game, you should create a pause screen to");
            sb.AppendLine("cover up that the game loads.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("Press R to reload the demo and toggle the use of a pool");
            sb.AppendLine(string.Empty);
            sb.AppendLine(string.Empty);
            sb.AppendLine(string.Empty);

            if (_usePool)
                sb.AppendLine("Pool is ENABLED");
            else
                sb.AppendLine("Pool is DISABLED");

            return sb.ToString();
        }
    }
}