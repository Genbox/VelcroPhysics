/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
*/

using System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Fluids;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class FluidsTest : Test
    {
        private FluidsTest()
        {
            World = new World(new Vector2(0f, -10f));

            Random random = new Random();

            for (int i = 0; i < 500; i++)
            {
                World.Fluid.AddParticle(new Vector2(-14.0f + 28.0f * (float)random.NextDouble(), 10.0f + 20.0f * (float)random.NextDouble()));
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);

            foreach (FluidParticle fluidParticle in World.Fluid.Particles)
            {
                DebugView.DrawCircle(fluidParticle.Position, World.Fluid.Definition.InfluenceRadius, Color.White * 0.1f);
                DebugView.DrawCircle(fluidParticle.Position, World.Fluid.Definition.InfluenceRadius / 10.0f, Color.Red);

            }

            //DebugView.DrawString(100, 100, World.Fluid.Particles[0].Position.X + " " + World.Fluid.Particles[0].Position.Y);

            DebugView.EndCustomDraw();

            base.Update(settings, gameTime);

            foreach (FluidParticle fluidParticle in World.Fluid.Particles)
            {
                WallCollision(fluidParticle);
            }
        }

        private const float WorldWidth = 60;
        private const float WorldHeight = 60;
        private const float CollisionForce = 0.1f;

        private void WallCollision(FluidParticle pi)
        {
            Vector2 correction = Vector2.Zero;

            if (pi.Position.X > WorldWidth / 2f)
                correction -= new Vector2((pi.Position.X - WorldWidth / 2f) / CollisionForce, 0);

            else if (pi.Position.X < -WorldWidth / 2f)
                correction += new Vector2((-WorldWidth / 2f - pi.Position.X) / CollisionForce, 0);

            if (pi.Position.Y > WorldHeight / 2f)
                correction -= new Vector2(0, (pi.Position.Y - WorldHeight / 2f) / CollisionForce);

            else if (pi.Position.Y < 0)
                correction += new Vector2(0, (0 - pi.Position.Y) / CollisionForce);

            pi.Velocity = new Vector2(pi.Velocity.X + correction.X, pi.Velocity.Y + correction.Y);
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            if (state.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
            {
                Vector2 mousePosition = GameInstance.ConvertScreenToWorld(state.X, state.Y);
                World.Fluid.AddParticle(mousePosition);
            }

            base.Mouse(state, oldState);
        }

        internal static Test Create()
        {
            return new FluidsTest();
        }
    }
}