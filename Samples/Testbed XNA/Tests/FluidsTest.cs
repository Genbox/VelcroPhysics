/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
*/

using System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Fluids;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class FluidsTest : Test
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;

        private FluidsTest()
        {
            World = new World(new Vector2(0f, -10f));

            Random random = new Random();

            for (int i = 0; i < 500; i++)
            {
                World.Fluid.AddParticle(new Vector2(-14.0f + 28.0f * (float)random.NextDouble(), 10.0f + 20.0f * (float)random.NextDouble()));
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            //DebugView.AppendFlags(DebugViewFlags.DebugPanel);
            //DebugView.AppendFlags(DebugViewFlags.PerformanceGraph);

            _spriteBatch = new SpriteBatch(GameInstance.GraphicsDevice);
            _pixel = GameInstance.Content.Load<Texture2D>("Pixel");
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            _spriteBatch.Begin();

            foreach (FluidParticle fluidParticle in World.Fluid.Particles)
            {
                _spriteBatch.Draw(_pixel, GameInstance.ConvertWorldToScreen(fluidParticle.Position), Color.White);
            }
            _spriteBatch.End();

            base.Update(settings, gameTime);

            foreach (FluidParticle fluidParticle in World.Fluid.Particles)
            {
                WallCollision(fluidParticle);
            }
        }

        private const float WorldWidth = 60;
        private const float WorldHeight = 60;
        private const float CollisionForce = 1f;

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
            if (state.LeftButton == ButtonState.Pressed)
            {
                Vector2 mousePosition = GameInstance.ConvertScreenToWorld(state.X, state.Y);

                for (int i = 0; i < 5; i++)
                {
                    World.Fluid.AddParticle(mousePosition + new Vector2(i,0));
                }
            }

            base.Mouse(state, oldState);
        }

        internal static Test Create()
        {
            return new FluidsTest();
        }
    }
}