using System.Text;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.SimpleSamplesXNA.Demos.DemoShare;
using FarseerGames.SimpleSamplesXNA.DrawingSystem;
using FarseerGames.SimpleSamplesXNA.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.SimpleSamplesXNA.Demos.Demo10
{
    public class Demo10Screen : GameScreen
    {
        private Body bodyA, bodyB;
        private Geom geomA, geomB;
        private WeldJoint weldJoint;

        private PolygonBrush brush;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            base.Initialize();
        }

        public override void LoadContent()
        {
            bodyA = BodyFactory.Instance.CreateRectangleBody(100, 25, 5);
            bodyB = BodyFactory.Instance.CreateRectangleBody(100, 25, 5);

            bodyA.Position = new Vector2(250, 300);
            bodyB.Position = new Vector2(350, 300);

            geomA = GeomFactory.Instance.CreateRectangleGeom(bodyA, 100, 25);
            geomB = GeomFactory.Instance.CreateRectangleGeom(bodyB, 100, 25);

            weldJoint = new WeldJoint(bodyA, bodyB, new Vector2(300, 300));

            PhysicsSimulator.Add(bodyA);
            PhysicsSimulator.Add(bodyB);
            PhysicsSimulator.Add(geomA);
            PhysicsSimulator.Add(geomB);
            weldJoint.Add(PhysicsSimulator);

            brush = new PolygonBrush(Vertices.CreateRectangle(100, 25), Color.White, Color.Black, 2.0f, 0.5f);

            brush.Load(ScreenManager.GraphicsDevice);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            brush.Draw(bodyA.Position, bodyA.Rotation);
            brush.Draw(bodyB.Position, bodyB.Rotation);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public override void HandleInput(InputState input)
        {
            if (firstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
                firstRun = false;
            }
            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
            }

            base.HandleInput(input);
        }

        public static string GetTitle()
        {
            return "Demo10: WeldJoint and GearJoint";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to use the WeldJoint");
            sb.AppendLine("and the GearJoint.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }
    }
}