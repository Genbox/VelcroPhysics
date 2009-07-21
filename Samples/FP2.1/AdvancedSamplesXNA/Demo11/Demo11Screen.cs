using System;
using System.Text;
using System.Collections.Generic;
using FarseerGames.AdvancedSamplesXNA.ScreenSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Collisions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.AdvancedSamplesXNA.Demos.Demo11
{
    public class Demo11Screen : GameScreen
    {
        Body bodyA, bodyB;
        Geom geomA, geomB;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 150));

            // Use the SAT narrow phase collider
            PhysicsSimulator.NarrowPhaseCollider = NarrowPhaseCollider.SAT;

            // setup our debug view
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);
            PhysicsSimulatorView.EnableEdgeView = true;
            PhysicsSimulatorView.EnableAABBView = false;

            Vertices polygon = new Vertices();

            polygon.Add(new Vector2(-50, -50));
            polygon.Add(new Vector2(0, -75));
            polygon.Add(new Vector2(50, -50));
            polygon.Add(new Vector2(25, 50));
            polygon.Add(new Vector2(-25, 50));

            bodyA = BodyFactory.Instance.CreateRectangleBody(PhysicsSimulator, 10, 10, 10);
            bodyB = BodyFactory.Instance.CreateRectangleBody(PhysicsSimulator, 10, 10, 10);

            geomA = GeomFactory.Instance.CreatePolygonGeom(PhysicsSimulator, bodyA,
                polygon, 1);
            geomA.FrictionCoefficient = 1f;
            geomA.RestitutionCoefficient = 0f;

            geomB = GeomFactory.Instance.CreatePolygonGeom(PhysicsSimulator, bodyB,
                Vertices.CreateSimpleRectangle(100, 100), 1);
            geomB.FrictionCoefficient = 0f;
            geomB.RestitutionCoefficient = 0.9f;

            bodyA.Position = new Vector2(400, 300);
            bodyA.Rotation = 0;
            bodyA.MomentOfInertia = 5000;
            bodyB.Position = new Vector2(400, 300);
            bodyB.MomentOfInertia = 5000;

            base.Initialize();
        }

        public override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);

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
            return "Demo10: Auto-divide geometry with SAT";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to use the");
            sb.AppendLine("Auto-divide geometry generator.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("It is important to note that");
            sb.AppendLine("decomposing geometry is only");
            sb.AppendLine("nessary with SAT narrow phase.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Move : left and right arrows");
            return sb.ToString();
        }
    }
}
