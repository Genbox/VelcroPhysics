/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
{
    public class ConfinedTest : Test
    {
        private const int ColumnCount = 0;
        private const int RowCount = 0;

        private ConfinedTest()
        {
            {
                Body ground = BodyFactory.CreateBody(World);

                // Floor
                EdgeShape shape = new EdgeShape(new Vector2(-10.0f, 0.0f), new Vector2(10.0f, 0.0f));
                ground.CreateFixture(shape);

                // Left wall
                shape = new EdgeShape(new Vector2(-10.0f, 0.0f), new Vector2(-10.0f, 20.0f));
                ground.CreateFixture(shape);

                // Right wall
                shape = new EdgeShape(new Vector2(10.0f, 0.0f), new Vector2(10.0f, 20.0f));
                ground.CreateFixture(shape);

                // Roof
                shape = new EdgeShape(new Vector2(-10.0f, 20.0f), new Vector2(10.0f, 20.0f));
                ground.CreateFixture(shape);
            }

            const float radius = 0.5f;
            CircleShape shape2 = new CircleShape(radius, 1);
            shape2.Position = Vector2.Zero;

            for (int j = 0; j < ColumnCount; ++j)
            {
                for (int i = 0; i < RowCount; ++i)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(-10.0f + (2.1f * j + 1.0f + 0.01f * i) * radius,
                                                (2.0f * i + 1.0f) * radius);

                    Fixture fixture = body.CreateFixture(shape2);
                    fixture.Friction = 0.1f;
                }
            }

            World.Gravity = Vector2.Zero;
        }

        private void CreateCircle()
        {
            const float radius = 2f;
            CircleShape shape = new CircleShape(radius, 1);
            shape.Position = Vector2.Zero;

            Body body = BodyFactory.CreateBody(World);
            body.BodyType = BodyType.Dynamic;
            body.Position = new Vector2(Rand.RandomFloat(), 3.0f + Rand.RandomFloat());

            Fixture fixture = body.CreateFixture(shape);
            fixture.Friction = 0;
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsKeyDown(Keys.C))
            {
                CreateCircle();
            }

            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            foreach (Body b in World.BodyList)
            {
                if (b.BodyType != BodyType.Dynamic)
                {
                    continue;
                }

                if (b.Awake)
                {
                }
            }

            if (StepCount == 180)
            {
                StepCount += 0;
            }

            //if (sleeping)
            //{
            //	CreateCircle();
            //}

            base.Update(settings, gameTime);

            foreach (Body b in World.BodyList)
            {
                if (b.BodyType != BodyType.Dynamic)
                {
                    continue;
                }

                Vector2 p = b.Position;
                if (p.X <= -10.0f || 10.0f <= p.X || p.Y <= 0.0f || 20.0f <= p.Y)
                {
                    p.X += 0.0f;
                }
            }

            DrawString("Press 'c' to create a circle.");
            
        }

        internal static Test Create()
        {
            return new ConfinedTest();
        }
    }
}