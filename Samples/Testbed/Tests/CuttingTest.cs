using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
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

            Vertices box = PolygonTools.CreateRectangle(0.5f, 0.5f);
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
                    body.CreateFixture(shape);

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
            {
                DebugView.DrawPoint(entryPoint, 0.5f, Color.Yellow);
            }

            foreach (Vector2 exitPoint in exitPoints)
            {
                DebugView.DrawPoint(exitPoint, 0.5f, Color.PowderBlue);
            }
            DebugView.EndCustomDraw();

            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.Tab))
                _switched = !_switched;

            if (keyboardManager.IsNewKeyPress(Keys.Enter))
                CuttingTools.Cut(World, _start, _end);

            if (_switched)
            {
                if (keyboardManager.IsKeyDown(Keys.A))
                    _start.X -= MoveAmount;

                if (keyboardManager.IsKeyDown(Keys.S))
                    _start.Y -= MoveAmount;

                if (keyboardManager.IsKeyDown(Keys.W))
                    _start.Y += MoveAmount;

                if (keyboardManager.IsKeyDown(Keys.D))
                    _start.X += MoveAmount;
            }
            else
            {
                if (keyboardManager.IsKeyDown(Keys.A))
                    _end.X -= MoveAmount;

                if (keyboardManager.IsKeyDown(Keys.S))
                    _end.Y -= MoveAmount;

                if (keyboardManager.IsKeyDown(Keys.W))
                    _end.Y += MoveAmount;

                if (keyboardManager.IsKeyDown(Keys.D))
                    _end.X += MoveAmount;
            }

            base.Keyboard(keyboardManager);
        }

        public override void Gamepad(GamePadState state, GamePadState oldState)
        {
            _start.X += state.ThumbSticks.Left.X / 5;
            _start.Y += state.ThumbSticks.Left.Y / 5;

            _end.X += state.ThumbSticks.Right.X / 5;
            _end.Y += state.ThumbSticks.Right.Y / 5;

            if (state.Buttons.A == ButtonState.Pressed && oldState.Buttons.A == ButtonState.Released)
                CuttingTools.Cut(World, _start, _end);

            base.Gamepad(state, oldState);
        }

        public static CuttingTest Create()
        {
            return new CuttingTest();
        }
    }
}