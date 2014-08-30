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

            //for (int i = 0; i < 500; i++)
            //{
            //    //World.Fluid.AddParticle(new Vector2(-14.0f + 28.0f * (float)random.NextDouble(), 10.0f + 20.0f * (float)random.NextDouble()));
            //    World.Fluid.AddParticle(new Vec2(-14.0f + 28.0f * (float)random.NextDouble(), 10.0f + 20.0f * (float)random.NextDouble()));
            //}
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

            foreach (var fluidParticle in World.Fluid.Particles)
            {
                _spriteBatch.Draw(_pixel, GameInstance.ConvertWorldToScreen(new Vector2(fluidParticle.Position.X, fluidParticle.Position.Y)), Color.White);
            }
            _spriteBatch.End();

            base.Update(settings, gameTime);
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            if (state.LeftButton == ButtonState.Pressed)
            {
                Vector2 mousePosition = GameInstance.ConvertScreenToWorld(state.X, state.Y);

                for (int i = 0; i < 5; i++)
                {
                    World.Fluid.AddParticle(new Vec2(mousePosition.X + i, mousePosition.Y));
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