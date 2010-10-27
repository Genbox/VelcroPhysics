    /*
* Copyright (c) 2006-2010 Erin Catto http://www.gphysics.com
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
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{



/// This test shows how a rope joint can be used to stabilize a chain of
/// bodies with a heavy payload. Notice that the rope joint just prevents
/// excessive stretching and has no other effect.
/// By disabling the rope joint you can see that the Box2D solver has trouble
/// supporting heavy bodies with light bodies. Try playing around with the
/// densities, time step, and iterations to see how they affect stability.
/// This test also shows how to use contact filtering. Filtering is configured
/// so that the payload does not collide with the chain.
    public class RopeTest : Test
    {
        private const int Count = 10;
        RopeJoint _rj;
        bool _useRopeJoint = true;

        private RopeTest()
        {
            Body ground;
            {
                ground = World.CreateBody();

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0);
            }

            {
                Vertices box = PolygonTools.CreateRectangle(0.5f, 0.125f);
                PolygonShape shape = new PolygonShape(box);
                shape.Density = 20;

                const float y = 15;

                Body prevBody = ground;
                for (int i = 0; i < Count; ++i)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(0.5f + 1.0f * i, y);

                    if (i == Count - 1)
                    {
                        shape.SetAsBox(1.5f, 1.5f);
                        shape.Density = 100;
                        body.Position = new Vector2(1.0f * i, y);
                        body.AngularDamping = 0.4f;
                    }

                    Fixture fixture = body.CreateFixture(shape);
                    fixture.Friction = 0.2f;

                    Vector2 anchor = new Vector2(i, y);
                    RevoluteJoint jd = new RevoluteJoint(prevBody, body, prevBody.GetLocalPoint(anchor), body.GetLocalPoint(anchor));
                    jd.CollideConnected = false;

                    World.AddJoint(jd);

                    prevBody = body;
                }

                _rj = new RopeJoint(ground, prevBody, new Vector2(0, y), Vector2.Zero);

                const float extraLength = 0.01f;
                _rj.MaxLength = Count - 1.0f + extraLength;

                _rj.CollideConnected = true;

                World.AddJoint(_rj);
            }
        }
  
        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsKeyDown(Keys.J))
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
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DebugView.DrawString(50, TextLine, "Press (j) to toggle the rope joint.");
            TextLine += 15;

            if (_useRopeJoint)
            {
                DebugView.DrawString(50, TextLine, "Rope ON");
            }
            else
            {
                DebugView.DrawString(50, TextLine, "Rope OFF");
            }
            TextLine += 15;

            base.Update(settings, gameTime);
        }
        
        internal static Test Create()
        {
            return new RopeTest();
        }
    }
}
