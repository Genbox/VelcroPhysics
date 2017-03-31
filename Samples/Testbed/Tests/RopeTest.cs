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
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
{
    /// <summary>
    /// This test shows how a rope joint can be used to stabilize a chain of
    /// bodies with a heavy payload. Notice that the rope joint just prevents
    /// excessive stretching and has no other effect.
    /// By disabling the rope joint you can see that the Box2D solver has trouble
    /// supporting heavy bodies with light bodies. Try playing around with the
    /// densities, time step, and iterations to see how they affect stability.
    /// This test also shows how to use contact filtering. Filtering is configured
    /// so that the payload does not collide with the chain.
    /// </summary>
    public class RopeTest : Test
    {
        private RopeJoint _rj;
        private bool _useRopeJoint = true;

        private RopeTest()
        {
            Body ground = BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            {
                Body prevBody = ground;
                PolygonShape largeShape = new PolygonShape(PolygonTools.CreateRectangle(1.5f, 1.5f), 100);
                PolygonShape smallShape = new PolygonShape(PolygonTools.CreateRectangle(0.5f, 0.125f), 20);

                const int N = 10;
                const float y = 15;

                for (int i = 0; i < N; ++i)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(0.5f + 1.0f * i, y);

                    if (i == N - 1)
                    {
                        Fixture fixture = body.CreateFixture(largeShape);
                        fixture.Friction = 0.2f;
                        fixture.CollisionCategories = Category.Cat2;
                        fixture.CollidesWith = Category.All & ~Category.Cat2;
                        body.Position = new Vector2(1.0f * i, y);
                        body.AngularDamping = 0.4f;
                    }
                    else
                    {
                        Fixture fixture = body.CreateFixture(smallShape);
                        fixture.Friction = 0.2f;
                        fixture.CollisionCategories = Category.Cat1;
                        fixture.CollidesWith = Category.All & ~Category.Cat2;
                    }

                    Vector2 anchor = new Vector2(i, y);
                    RevoluteJoint jd = new RevoluteJoint(prevBody, body, anchor, true); 
                    jd.CollideConnected = false;

                    World.AddJoint(jd);

                    prevBody = body;
                }

                _rj = new RopeJoint(ground, prevBody, new Vector2(0, y), Vector2.Zero);

                //FPE: The two following lines are actually not needed as FPE sets the MaxLength to a default value
                const float extraLength = 0.01f;
                _rj.MaxLength = N - 1.0f + extraLength;

                World.AddJoint(_rj);
            }
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.J))
            {
                if (_useRopeJoint)
                {
                    _useRopeJoint = false;
                    World.RemoveJoint(_rj);
                }
                else
                {
                    _useRopeJoint = true;
                    World.AddJoint(_rj);
                }
            }

            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Press (j) to toggle the rope joint.");
            DrawString(_useRopeJoint ? "Rope ON" : "Rope OFF");
            
            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new RopeTest();
        }
    }
}