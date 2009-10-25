using System;
using System.Text;
using System.Collections.Generic;
using DemoBaseXNA;
using DemoBaseXNA.DemoShare;
using DemoBaseXNA.DrawingSystem;
using DemoBaseXNA.ScreenSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.SimpleSamplesXNA.Demo10
{
    public class Demo10Screen : GameScreen
    {
        private Body bodyA, bodyB, bodyC, bodyD;
        private Geom geomA, geomB, geomC, geomD;
        private WeldJoint weldJoint;
        private bool broke;
        private FixedRevoluteJoint revJointA;
        private FixedRevoluteJoint revJointB;

        private PolygonBrush brush;
        private PolygonBrush gearBrushA;
        private PolygonBrush gearBrushB;

        private List<Table> table;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            PhysicsSimulatorView.EnableEdgeView = true;
            PhysicsSimulatorView.EnableCoordinateAxisView = true;
            PhysicsSimulatorView.EnableAABBView = false;

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
            geomA.IgnoreCollisionWith(geomB);

            weldJoint = new WeldJoint(bodyA, bodyB, new Vector2(300, 300));
            weldJoint.Broke += weldJoint_Broke;
            weldJoint.Breakpoint = 5.0f;

            PhysicsSimulator.Add(bodyA);
            PhysicsSimulator.Add(bodyB);
            PhysicsSimulator.Add(geomA);
            PhysicsSimulator.Add(geomB);
            PhysicsSimulator.Add(weldJoint);

            brush = new PolygonBrush(Vertices.CreateRectangle(100, 25), Color.White, Color.Black, 2.0f, 0.5f);

            brush.Load(ScreenManager.GraphicsDevice);

            bodyC = BodyFactory.Instance.CreatePolygonBody(Vertices.CreateGear(50, 20, .50f, 10), 10);
            bodyC.Position = new Vector2(500, 200);

            geomC = GeomFactory.Instance.CreatePolygonGeom(bodyC, Vertices.CreateGear(50, 20, .50f, 10), 1.5f);

            bodyD = BodyFactory.Instance.CreatePolygonBody(Vertices.CreateGear(50, 20, .50f, 10), 10);
            bodyD.Position = new Vector2(613, 200);

            geomD = GeomFactory.Instance.CreatePolygonGeom(bodyD, Vertices.CreateGear(50, 20, .50f, 10), 1.5f);

            geomC.CollisionGroup = 2;
            geomD.CollisionGroup = 3;
            geomC.FrictionCoefficient = 0f;
            geomD.FrictionCoefficient = 0f;

            PhysicsSimulator.Add(bodyC);
            PhysicsSimulator.Add(geomC);
            PhysicsSimulator.Add(bodyD);
            PhysicsSimulator.Add(geomD);

            gearBrushA = new PolygonBrush(Vertices.CreateGear(50, 20, .50f, 10), Color.White, Color.Black, 0.5f, 0.5f);

            gearBrushA.Load(ScreenManager.GraphicsDevice);

            gearBrushB = new PolygonBrush(Vertices.CreateGear(50, 20, .50f, 10), Color.White, Color.Black, 0.5f, 0.5f);

            gearBrushB.Load(ScreenManager.GraphicsDevice);

            revJointA = JointFactory.Instance.CreateFixedRevoluteJoint(bodyC, bodyC.Position);
            revJointB = JointFactory.Instance.CreateFixedRevoluteJoint(bodyD, bodyD.Position);

            PhysicsSimulator.Add(revJointA);
            PhysicsSimulator.Add(revJointB);

            table = new List<Table>();

            table.Add(new Table(new Vector2(200, 450), 200, 50));

            table[0].Load(PhysicsSimulator, ScreenManager.GraphicsDevice);

            base.LoadContent();
        }

        void weldJoint_Broke(object sender, EventArgs e)
        {
            broke = true;
            PhysicsSimulator.Remove(weldJoint);
            weldJoint.Dispose();
        }

        public override void Draw(GameTime gameTime)
        {
            brush.Draw(bodyA.Position, bodyA.Rotation);
            brush.Draw(bodyB.Position, bodyB.Rotation);

            gearBrushA.Draw(bodyC.Position, bodyC.Rotation);
            gearBrushB.Draw(bodyD.Position, bodyD.Rotation);

            foreach (Table t in table)
                t.Draw();

            base.Draw(gameTime);
        }

        int count;

        public override void HandleInput(InputState input)
        {
            Random rand = new Random();

            if (firstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
                firstRun = false;
            }
            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
            }

            if (input.CurrentMouseState.RightButton == ButtonState.Pressed && broke && count > 10)
            {
                broke = false;

                weldJoint = new WeldJoint(bodyA, bodyB, new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y));

                weldJoint.Breakpoint = 5.0f;
                weldJoint.Broke += weldJoint_Broke;

                PhysicsSimulator.Add(weldJoint);
                count = 0;
            }
            else if (input.CurrentMouseState.MiddleButton == ButtonState.Pressed && input.LastMouseState.MiddleButton == ButtonState.Released && count > 10)
            {
                table.Add(new Table(new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y), rand.Next(100, 300), rand.Next(20, 50)));
                table[table.Count - 1].Load(PhysicsSimulator, ScreenManager.GraphicsDevice);
                count = 0;
            }
            count++;

            base.HandleInput(input);
        }

        public static string GetTitle()
        {
            return "Demo10: WeldJoint and gears";
        }

        private static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to use the WeldJoint");
            sb.AppendLine("and how to create gears");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            sb.AppendLine("  -Click right button to reweld bodies");
            sb.AppendLine("  -Click middle button to add tables");
            return sb.ToString();
        }
    }
}
