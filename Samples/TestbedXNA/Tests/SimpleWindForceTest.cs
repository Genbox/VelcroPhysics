using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    class SimpleWindForceTest:Test
    {
        SimpleWindForce SimpleWind;

        private SimpleWindForceTest()
        {
            SimpleWind = new SimpleWindForce();
            SimpleWind.Direction = new Vector2(0.7f, 0.2f);
            SimpleWind.Variation = 1.0f;
            SimpleWind.Strength = 5;
            SimpleWind.Position = new Vector2(0, 20);

            World.AddController(SimpleWind);
            World.Gravity = Vector2.Zero;

            int countX = 10;
            int countY = 10;

            for (int x = 0; x < countX; x++)
            {
                for (int y = 0; y < countY; y++)
                {
                    Fixture currentFixture = FixtureFactory.CreateRectangle(World, 1f, 1f, 5f, new Vector2(x * 2 - countX, y * 2 + 5));
                    //Fixture currentFixture = FixtureFactory.CreateCircle(World, 0.2f, 10f, new Vector2(x - countX, y  + 5));
                    currentFixture.Body.BodyType = BodyType.Dynamic;
                    currentFixture.Friction = 0.5f;
                    currentFixture.Body.SetTransform(currentFixture.Body.Position, 0.6f);
                    //currentFixture.CollidesWith = CollisionCategory.Cat10;
                }
            }

            Fixture floor = FixtureFactory.CreateRectangle(World, 100, 1, 1, new Vector2(0, 0));
            Fixture ceiling = FixtureFactory.CreateRectangle(World, 100, 1, 1, new Vector2(0, 40));
            Fixture right = FixtureFactory.CreateRectangle(World, 1, 100, 1, new Vector2(35, 0));
            Fixture left = FixtureFactory.CreateRectangle(World, 1, 100, 1, new Vector2(-35, 0));

            floor.Friction = 0.2f;
            ceiling.Friction = 0.2f;
            right.Friction = 0.2f;
            left.Friction = 0.2f;
            
            
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DebugView.DrawString(50, TextLine, "Note: The left side of the ship has a different density than the right side of the ship");
            TextLine += 15;
            DebugView.DrawSegment(SimpleWind.Position, SimpleWind.Direction-SimpleWind.Position, Color.Red);
            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            //if (keyboardManager.IsKeyDown(Keys.W))
            //{
            //    Vector2 f = _body.GetWorldVector(new Vector2(0.0f, -200.0f));
            //    _body.ApplyForce(f);
            //}
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            //base.Mouse(state, oldState);
            Vector2 MouseWorld =GameInstance.ConvertScreenToWorld(state.X, state.Y);
            //SimpleWind.Position = MouseWorld;
            SimpleWind.Direction = SimpleWind.Position+MouseWorld;
            SimpleWind.Strength = 50;
        }

        internal static Test Create()
        {
            return new SimpleWindForceTest();
        }

    }
}
