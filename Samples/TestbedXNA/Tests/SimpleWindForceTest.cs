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

        public float Strength;

        private SimpleWindForceTest()
        {
            SimpleWind = new SimpleWindForce();
            SimpleWind.Direction = new Vector2(0.7f, 0.2f);
            SimpleWind.Variation = 1.0f;
            SimpleWind.Strength = 5;
            SimpleWind.Position = new Vector2(0, 20);
            SimpleWind.DecayStart = 5f;
            SimpleWind.DecayEnd = 10f;
            SimpleWind.DecayMode = AbstractForceController.DecayModes.Step;
            SimpleWind.ForceType = AbstractForceController.ForceTypes.Point;

            Strength = 1.0f;

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

        public void DrawPointForce()
        {
            DebugView.DrawPoint(SimpleWind.Position, 2, Color.Red);
            DebugView.DrawCircle(SimpleWind.Position, SimpleWind.DecayStart, Color.Green);
            DebugView.DrawCircle(SimpleWind.Position, SimpleWind.DecayEnd, Color.Red);
        }

        public void DrawLineForce()
        {
            Vector2 DrawVector;
            DrawVector = SimpleWind.Direction;
            DrawVector.Normalize();
            DrawVector *= Strength;
            DebugView.DrawArrow(SimpleWind.Position, SimpleWind.Position + DrawVector, 2, 1f, true, Color.Red);
        }

        public void DrawNoneForce()
        {
            DebugView.DrawPoint(SimpleWind.Position, 2, Color.Red);

        }


        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DebugView.DrawString(50, TextLine, "SimpleWindForce | Mouse: Direction | Left-Click: Position | W/S: Variation");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Wind Strength:"+SimpleWind.Strength.ToString());
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Variation:" + SimpleWind.Variation.ToString());
            //DebugView.DrawSegment(SimpleWind.Position, SimpleWind.Direction-SimpleWind.Position, Color.Red);

            DrawPointForce();
            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsKeyDown(Keys.Q))
            {
                Strength += 1f;
            }
            if (keyboardManager.IsKeyDown(Keys.A))
            {
                Strength -= 1f;
            }
            if (keyboardManager.IsKeyDown(Keys.W))
            {
                SimpleWind.Variation += 0.1f;
            }
            if (keyboardManager.IsKeyDown(Keys.S))
            {
                SimpleWind.Variation -= 0.1f;
            }
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            //base.Mouse(state, oldState);
            Vector2 MouseWorld =GameInstance.ConvertScreenToWorld(state.X, state.Y);
            //SimpleWind.Position = MouseWorld;
            SimpleWind.Direction = MouseWorld-SimpleWind.Position;
            SimpleWind.Strength = Strength;

            if (state.LeftButton == ButtonState.Pressed && oldState.LeftButton==ButtonState.Released) 
            {
                SimpleWind.Position = MouseWorld;
                SimpleWind.Direction = MouseWorld + new Vector2(0, 1);
                Microsoft.Xna.Framework.Input.Mouse.SetPosition(state.X, state.Y + 10);
            }
        }

        internal static Test Create()
        {
            return new SimpleWindForceTest();
        }

    }
}
