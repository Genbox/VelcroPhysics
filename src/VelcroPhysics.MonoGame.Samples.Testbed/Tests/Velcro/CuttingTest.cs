using System.Collections.Generic;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Tools.Cutting.Simple;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests.Velcro
{
    public class CuttingTest : Test
    {
        private const float MoveAmount = 0.1f;

        private const int Count = 20;
        private Vector2 _end = new Vector2(6, 5);
        private Vector2 _start = new Vector2(-6, 5);
        private bool _switched;

        private CuttingTest()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            Vertices box = PolygonUtils.CreateRectangle(0.5f, 0.5f);
            PolygonShape shape = new PolygonShape(box, 5);

            Vector2 x = new Vector2(-7.0f, 0.75f);
            Vector2 deltaX = new Vector2(0.5625f, 1.25f);
            Vector2 deltaY = new Vector2(1.125f, 0.0f);

            for (int i = 0; i < Count; ++i)
            {
                Vector2 y = x;

                for (int j = i; j < Count; ++j)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = y;
                    body.AddFixture(shape);

                    y += deltaY;
                }

                x += deltaX;
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Press A,S,W,D move endpoint");

            DrawString("Press Enter to cut");

            DrawString("Press TAB to change endpoint");

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            DebugView.DrawSegment(_start, _end, Color.Red);
            DebugView.EndCustomDraw();

            List<Fixture> fixtures = new List<Fixture>();
            List<Vector2> entryPoints = new List<Vector2>();
            List<Vector2> exitPoints = new List<Vector2>();

            //Get the entry points
            World.RayCast((f, p, n, fr) =>
            {
                fixtures.Add(f);
                entryPoints.Add(p);
                return 1;
            }, _start, _end);

            //Reverse the ray to get the exitpoints
            World.RayCast((f, p, n, fr) =>
            {
                exitPoints.Add(p);
                return 1;
            }, _end, _start);

            DrawString("Fixtures: " + fixtures.Count);

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            foreach (Vector2 entryPoint in entryPoints)
                DebugView.DrawPoint(entryPoint, 0.5f, Color.Yellow);

            foreach (Vector2 exitPoint in exitPoints)
                DebugView.DrawPoint(exitPoint, 0.5f, Color.PowderBlue);
            DebugView.EndCustomDraw();

            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.Tab))
                _switched = !_switched;

            if (keyboard.IsNewKeyPress(Keys.Enter))
                CuttingTools.Cut(World, _start, _end);

            if (_switched)
            {
                if (keyboard.IsKeyDown(Keys.A))
                    _start.X -= MoveAmount;

                if (keyboard.IsKeyDown(Keys.S))
                    _start.Y -= MoveAmount;

                if (keyboard.IsKeyDown(Keys.W))
                    _start.Y += MoveAmount;

                if (keyboard.IsKeyDown(Keys.D))
                    _start.X += MoveAmount;
            }
            else
            {
                if (keyboard.IsKeyDown(Keys.A))
                    _end.X -= MoveAmount;

                if (keyboard.IsKeyDown(Keys.S))
                    _end.Y -= MoveAmount;

                if (keyboard.IsKeyDown(Keys.W))
                    _end.Y += MoveAmount;

                if (keyboard.IsKeyDown(Keys.D))
                    _end.X += MoveAmount;
            }

            base.Keyboard(keyboard);
        }

        public static CuttingTest Create()
        {
            return new CuttingTest();
        }
    }
}