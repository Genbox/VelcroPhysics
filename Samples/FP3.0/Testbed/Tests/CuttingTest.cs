using System.Collections.Generic;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class CuttingTest : Test
    {
        public CuttingTest()
        {
            Body ground;
            {
                ground = World.CreateBody();

                Vertices edge = PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                PolygonShape shape = new PolygonShape(edge, 0.0f);
                ground.CreateFixture(shape);
            }

            Vector2 offset = new Vector2(3, 5);

            for (int i = 0; i < 1; i++)
            {
                Body boxBody = World.CreateBody();
                boxBody.BodyType = BodyType.Static;
                boxBody.Position = i * offset + new Vector2(0, 5);
                PolygonShape boxShape = new PolygonShape(PolygonTools.CreateRectangle(1, 1));
                boxBody.CreateFixture(boxShape);
            }

        }

        Vector2 start = new Vector2(-2, 5);
        Vector2 end = new Vector2(15, 5);

        public override void Update(Framework.Settings settings)
        {
            DebugView.DrawSegment(start, end, Color.Red);

            List<Fixture> fixtures = new List<Fixture>();
            List<Vector2> entryPoints = new List<Vector2>();
            List<Vector2> exitPoints = new List<Vector2>();

            //Get the entry points
            World.RayCast((f, p, n, fr) =>
            {
                fixtures.Add(f);
                entryPoints.Add(p);
                return 1;
            }, start, end);

            //Reverse the ray to get the exitpoints
            World.RayCast((f, p, n, fr) =>
            {
                exitPoints.Add(p);
                return 1;
            }, end, start);

            DebugView.DrawString(100, 50, "Fixtures: " + fixtures.Count);

            foreach (Vector2 entryPoint in entryPoints)
            {
                DebugView.DrawPoint(entryPoint, 0.5f, Color.Yellow);
            }

            foreach (Vector2 exitPoint in exitPoints)
            {
                DebugView.DrawPoint(exitPoint, 0.5f, Color.PowderBlue);
            }

            base.Update(settings);
        }

        public static CuttingTest Create()
        {
            return new CuttingTest();
        }

        private float moveAmount = 0.1f;
        private bool switched = false;

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.Tab) && oldState.IsKeyUp(Keys.Tab))
                switched = !switched;

            if (state.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter))
                CuttingTools.Cut(World, start, end);

            if (switched)
            {
                if (state.IsKeyDown(Keys.A))
                    start.X -= moveAmount;

                if (state.IsKeyDown(Keys.S))
                    start.Y -= moveAmount;

                if (state.IsKeyDown(Keys.W))
                    start.Y += moveAmount;

                if (state.IsKeyDown(Keys.D))
                    start.X += moveAmount;
            }
            else
            {
                if (state.IsKeyDown(Keys.A))
                    end.X -= moveAmount;

                if (state.IsKeyDown(Keys.S))
                    end.Y -= moveAmount;

                if (state.IsKeyDown(Keys.W))
                    end.Y += moveAmount;

                if (state.IsKeyDown(Keys.D))
                    end.X += moveAmount;
            }

            base.Keyboard(state, oldState);
        }

        public override void Gamepad(GamePadState state, GamePadState oldState)
        {
            start.X += state.ThumbSticks.Left.X / 5;
            start.Y += state.ThumbSticks.Left.Y / 5;

            end.X += state.ThumbSticks.Right.X / 5;
            end.Y += state.ThumbSticks.Right.Y / 5;

            if (state.Buttons.A == ButtonState.Pressed && oldState.Buttons.A == ButtonState.Released)
                CuttingTools.Cut(World, start, end);

            base.Gamepad(state, oldState);
        }
    }
}
