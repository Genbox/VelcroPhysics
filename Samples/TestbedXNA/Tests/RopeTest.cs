/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
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
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
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
        private const int Count = 10;
        private RopeJoint _rj;
        private bool _useRopeJoint = true;

        private RopeTest()
        {
            Body ground;
            {
                ground = new Body(World);

                EdgeShape shape = new EdgeShape(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.CreateFixture(shape);
            }

            {
                const float y = 15;

                Body prevBody = ground;
                PolygonShape largeShape = new PolygonShape(PolygonTools.CreateRectangle(1.5f, 1.5f), 100);
                PolygonShape smallShape = new PolygonShape(PolygonTools.CreateRectangle(0.5f, 0.125f), 20);

                for (int i = 0; i < Count; ++i)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(0.5f + 1.0f * i, y);

                    if (i == Count - 1)
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
                    RevoluteJoint jd = new RevoluteJoint(prevBody, body, prevBody.GetLocalPoint(ref anchor),
                                                         body.GetLocalPoint(ref anchor));
                    jd.CollideConnected = false;

                    World.AddJoint(jd);

                    prevBody = body;
                }

                _rj = new RopeJoint(ground, prevBody, new Vector2(0, y), Vector2.Zero);
                const float extraLength = 0.01f;
                _rj.MaxLength = Count - 1.0f + extraLength;

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
            DebugView.DrawString(50, TextLine, "Press (j) to toggle the rope joint.");
            TextLine += 15;

            DebugView.DrawString(50, TextLine, _useRopeJoint ? "Rope ON" : "Rope OFF");
            TextLine += 15;

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new RopeTest();
        }
    }
}