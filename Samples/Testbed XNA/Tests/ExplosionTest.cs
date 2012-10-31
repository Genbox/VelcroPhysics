using System.Diagnostics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PhysicsLogic;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class ExplosionTest : Test
    {
        private const int ColumnCount = 5;
        private const int RowCount = 16;
        private Body[] _bodies = new Body[RowCount * ColumnCount];
        private Explosion _explosion;
        private int[] _indices = new int[RowCount * ColumnCount];
        private Vector2 _mousePos;
        private float _power;
        private float _radius;

        private ExplosionTest()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            float[] xs = new[] { -10.0f, -5.0f, 0.0f, 5.0f, 10.0f };

            for (int j = 0; j < ColumnCount; ++j)
            {
                PolygonShape shape = new PolygonShape(1);
                shape.Vertices = PolygonTools.CreateRectangle(0.5f, 0.5f);

                for (int i = 0; i < RowCount; ++i)
                {
                    int n = j * RowCount + i;
                    Debug.Assert(n < RowCount * ColumnCount);
                    _indices[n] = n;

                    const float x = 0.0f;
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(xs[j] + x, 0.752f + 1.54f * i);
                    body.UserData = _indices[n];
                    _bodies[n] = body;

                    Fixture fixture = body.CreateFixture(shape);
                    fixture.Friction = 0.3f;

                    //First column is unaffected by the explosion
                    if (j == 0)
                    {
                        body.PhysicsLogicFilter.IgnorePhysicsLogic(PhysicsLogicType.Explosion);
                    }
                }
            }

            _radius = 5;
            _power = 3;
            _explosion = new Explosion(World);
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            _mousePos = GameInstance.ConvertScreenToWorld(state.X, state.Y);
            base.Mouse(state, oldState);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.OemComma))
            {
                _explosion.Activate(_mousePos, _radius, _power);
            }
            if (keyboardManager.IsKeyDown(Keys.A))
            {
                _radius = MathHelper.Clamp(_radius - 0.1f, 0, 20);
            }
            if (keyboardManager.IsKeyDown(Keys.S))
            {
                _radius = MathHelper.Clamp(_radius + 0.1f, 0, 20);
            }
            if (keyboardManager.IsKeyDown(Keys.D))
            {
                _power = MathHelper.Clamp(_power - 0.1f, 0, 20);
            }
            if (keyboardManager.IsKeyDown(Keys.F))
            {
                _power = MathHelper.Clamp(_power + 0.1f, 0, 20);
            }

            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            DebugView.DrawString(50, TextLine, "Press: (,) to explode at mouse position.");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Press: (A) to decrease the explosion radius, (S) to increase it.");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Press: (D) to decrease the explosion power, (F) to increase it.");
            TextLine += 15;
            // Fighting against float decimals
            float powernumber = (float)((int)(_power * 10)) / 10;
            DebugView.DrawString(50, TextLine, "Power: " + powernumber);

            Color color = new Color(0.4f, 0.7f, 0.8f);
            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            DebugView.DrawCircle(_mousePos, _radius, color);
            DebugView.EndCustomDraw();
        }

        internal static Test Create()
        {
            return new ExplosionTest();
        }
    }
}