using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
{
    internal class SimpleWindForceTest : Test
    {
        private SimpleWindForce _simpleWind;

        private float _strength;

        private SimpleWindForceTest()
        {
            _simpleWind = new SimpleWindForce();
            _simpleWind.Direction = new Vector2(0.7f, 0.2f);
            _simpleWind.Variation = 1.0f;
            _simpleWind.Strength = 5;
            _simpleWind.Position = new Vector2(0, 20);
            _simpleWind.DecayStart = 5f;
            _simpleWind.DecayEnd = 10f;
            _simpleWind.DecayMode = AbstractForceController.DecayModes.Step;
            _simpleWind.ForceType = AbstractForceController.ForceTypes.Point;

            _strength = 1.0f;

            World.AddController(_simpleWind);
            World.Gravity = Vector2.Zero;

            const int countX = 10;
            const int countY = 10;

            for (int x = 0; x < countX; x++)
            {
                for (int y = 0; y < countY; y++)
                {
                    Body currentFixture = BodyFactory.CreateRectangle(World, 1f, 1f, 5f,
                                                                      new Vector2(x * 2 - countX, y * 2 + 5));
                    //Fixture currentFixture = BodyFactory.CreateCircle(World, 0.2f, 10f, new Vector2(x - countX, y  + 5));
                    currentFixture.BodyType = BodyType.Dynamic;
                    currentFixture.Friction = 0.5f;
                    currentFixture.SetTransform(currentFixture.Position, 0.6f);
                    //currentFixture.CollidesWith = Category.Cat10;
                }
            }

            Body floor = BodyFactory.CreateRectangle(World, 100, 1, 1, new Vector2(0, 0));
            Body ceiling = BodyFactory.CreateRectangle(World, 100, 1, 1, new Vector2(0, 40));
            Body right = BodyFactory.CreateRectangle(World, 1, 100, 1, new Vector2(35, 0));
            Body left = BodyFactory.CreateRectangle(World, 1, 100, 1, new Vector2(-35, 0));

            floor.Friction = 0.2f;
            ceiling.Friction = 0.2f;
            right.Friction = 0.2f;
            left.Friction = 0.2f;
        }

        public void DrawPointForce()
        {
            DebugView.DrawPoint(_simpleWind.Position, 2, Color.Red);
            DebugView.DrawCircle(_simpleWind.Position, _simpleWind.DecayStart, Color.Green);
            DebugView.DrawCircle(_simpleWind.Position, _simpleWind.DecayEnd, Color.Red);
        }

        public void DrawLineForce()
        {
            Vector2 drawVector;
            drawVector = _simpleWind.Direction;
            drawVector.Normalize();
            drawVector *= _strength;
            DebugView.DrawArrow(_simpleWind.Position, _simpleWind.Position + drawVector, 2, 1f, true, Color.Red);
        }

        public void DrawNoneForce()
        {
            DebugView.DrawPoint(_simpleWind.Position, 2, Color.Red);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("SimpleWindForce | Mouse: Direction | Left-Click: Position | W/S: Variation");
            DrawString("Wind Strength:" + _simpleWind.Strength);
            DrawString("Variation:" + _simpleWind.Variation);

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            //DebugView.DrawSegment(SimpleWind.Position, SimpleWind.Direction-SimpleWind.Position, Color.Red);
            DrawPointForce();
            DebugView.EndCustomDraw();
            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsKeyDown(Keys.Q))
                _strength += 1f;

            if (keyboardManager.IsKeyDown(Keys.A))
                _strength -= 1f;

            if (keyboardManager.IsKeyDown(Keys.W))
                _simpleWind.Variation += 0.1f;

            if (keyboardManager.IsKeyDown(Keys.S))
                _simpleWind.Variation -= 0.1f;

            base.Keyboard(keyboardManager);
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            //base.Mouse(state, oldState);
            Vector2 mouseWorld = GameInstance.ConvertScreenToWorld(state.X, state.Y);
            _simpleWind.Direction = mouseWorld - _simpleWind.Position;
            _simpleWind.Strength = _strength;

            if (state.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
            {
                _simpleWind.Position = mouseWorld;
                _simpleWind.Direction = mouseWorld + new Vector2(0, 1);
                Microsoft.Xna.Framework.Input.Mouse.SetPosition(state.X, state.Y + 10);
            }
        }

        internal static Test Create()
        {
            return new SimpleWindForceTest();
        }
    }
}