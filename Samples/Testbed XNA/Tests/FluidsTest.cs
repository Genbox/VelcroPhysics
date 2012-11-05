/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
*/

using System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Fluids;
using FarseerPhysics.Physics;
using FarseerPhysics.Physics.Collisions;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class FluidsTest : Test
    {
        private FluidManager _fluidsManager;

        private FluidsTest()
        {
            World = new World(new Vector2(0f, -5f));

            Body ground = BodyFactory.CreateRectangle(World, 40, 1, 0);
            FixtureFactory.AttachRectangle(1, 30, 0, new Vector2(-20 + 0.5f, 15 - 0.5f), ground);
            FixtureFactory.AttachRectangle(1, 30, 0, new Vector2(20 - 0.5f, 15 - 0.5f), ground);

            foreach (Fixture fixture in ground.FixtureList)
            {
                fixture.FluidProperties = new FluidCollisionProperties();
                fixture.RigidBody = new RigidBody(ground);
            }

            // Circle
            //{
            //    Body body = BodyFactory.CreateBody(World, new Vector2(4.0f, 30.0f));
            //    body.BodyType = BodyType.Dynamic;

            //    Fixture fix = FixtureFactory.AttachCircle(3.0f, 1.0f, body);
            //    fix.FluidProperties = new FluidCollisionProperties();
            //    fix.RigidBody = new RigidBody(body);
            //}

            // Rectangle
            {
                Body body = BodyFactory.CreateBody(World, new Vector2(-4.0f, 30.0f));
                body.BodyType = BodyType.Dynamic;

                Fixture fix = FixtureFactory.AttachRectangle(3.0f, 3.0f, 1.0f, Vector2.Zero, body);
                fix.FluidProperties = new FluidCollisionProperties();
                fix.RigidBody = new RigidBody(body);
            }

            _fluidsManager = new FluidManager(World);
            //Random random = new Random();

            //for (int i = 0; i < 500; ++i)
            //{
            //    _fluidsManager.Fluid.AddParticle(new Vector2(-14.0f + 28.0f * (float)random.NextDouble(), 10.0f + 20.0f * (float)random.NextDouble()));
            //}


            _fluidsManager.Fluid.AddParticle(new Vector2(-4.0f, 30.0f));
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);

            foreach (FluidParticle fluidParticle in _fluidsManager.Fluid.Particles)
            {
                DebugView.DrawCircle(fluidParticle.Position, _fluidsManager.Fluid.Definition.InfluenceRadius, Color.White * 0.1f);
                DebugView.DrawCircle(fluidParticle.Position, _fluidsManager.Fluid.Definition.InfluenceRadius / 10.0f, Color.Red);
            }

            DebugView.EndCustomDraw();

            //base.Update(settings, gameTime);

            if ((float)gameTime.ElapsedGameTime.TotalSeconds > 0)
                _fluidsManager.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            if (state.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
            {
                Vector2 mousePosition = GameInstance.ConvertScreenToWorld(state.X, state.Y);
                _fluidsManager.Fluid.AddParticle(mousePosition);
            }

            base.Mouse(state, oldState);
        }

        internal static Test Create()
        {
            return new FluidsTest();
        }
    }
}