using System.Text;
using DemoBaseXNA;
using DemoBaseXNA.DemoShare;
using DemoBaseXNA.DrawingSystem;
using DemoBaseXNA.ScreenSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.SimpleSamplesXNA.Demo9
{
    public class Demo9Screen : GameScreen
    {
        private Ragdoll _ragdoll;

        private Body[] _obstacleBodies;
        private Geom[] _obstacleGeoms;
        private RectangleBrush _obstacleBrush;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            base.Initialize();
        }

        public override void LoadContent()
        {
            _ragdoll = new Ragdoll(new Vector2(ScreenManager.ScreenCenter.X + 100, 110));
            _ragdoll.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            LoadObstacles();

            base.LoadContent();
        }

        private void LoadObstacles()
        {

            _obstacleBrush = new RectangleBrush(128, 32, Color.White, Color.Black);
            _obstacleBrush.Load(ScreenManager.GraphicsDevice);

            _obstacleBodies = new Body[5];
            _obstacleGeoms = new Geom[5];
            for (int i = 0; i < _obstacleBodies.Length; i++)
            {
                _obstacleBodies[i] = BodyFactory.Instance.CreateRectangleBody(PhysicsSimulator, 128, 32, 1);
                _obstacleBodies[i].IsStatic = true;

                if (i == 0)
                {
                    _obstacleGeoms[i] = GeomFactory.Instance.CreateRectangleGeom(PhysicsSimulator, _obstacleBodies[i], 128,
                                                                                 32);
                    _obstacleGeoms[i].RestitutionCoefficient = .2f;
                    _obstacleGeoms[i].FrictionCoefficient = .2f;
                }
                else
                {
                    _obstacleGeoms[i] = GeomFactory.Instance.CreateGeom(PhysicsSimulator, _obstacleBodies[i],
                                                                        _obstacleGeoms[0]);
                }
            }

            _obstacleBodies[0].Position = ScreenManager.ScreenCenter + new Vector2(-50, -200);
            _obstacleBodies[1].Position = ScreenManager.ScreenCenter + new Vector2(150, -100);
            _obstacleBodies[2].Position = ScreenManager.ScreenCenter + new Vector2(100, 50);
            _obstacleBodies[3].Position = ScreenManager.ScreenCenter + new Vector2(-100, 200);
            _obstacleBodies[4].Position = ScreenManager.ScreenCenter + new Vector2(-170, 0);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            DrawObstacles();

            _ragdoll.Draw(ScreenManager.SpriteBatch);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawObstacles()
        {
            for (int i = 0; i < _obstacleBodies.Length; i++)
            {
                _obstacleBrush.Draw(ScreenManager.SpriteBatch, _obstacleBodies[i].Position, _obstacleBodies[i].Rotation);
            }
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

            base.HandleInput(input);
        }

        public static string GetTitle()
        {
            return "Demo9: Ragdoll";
        }

        private static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to combine physics objects");
            sb.AppendLine("to create a ragdoll");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }
    }
}