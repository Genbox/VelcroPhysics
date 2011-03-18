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
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class PulleysTest : Test
    {
        private PulleyJoint _joint1;

        private PulleysTest()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            {
                const float a = 2.0f;
                const float b = 4.0f;
                const float y = 16.0f;
                const float l = 12.0f;

                PolygonShape shape = new PolygonShape(5);
                shape.SetAsBox(a, b);

                Body body1 = BodyFactory.CreateBody(World);
                body1.BodyType = BodyType.Dynamic;
                body1.Position = new Vector2(-10.0f, y);
                body1.CreateFixture(shape);

                Body body2 = BodyFactory.CreateBody(World);
                body2.BodyType = BodyType.Dynamic;
                body2.Position = new Vector2(10.0f, y);

                body2.CreateFixture(shape);

                Vector2 anchor1 = new Vector2(-10.0f, y + b);
                Vector2 anchor2 = new Vector2(10.0f, y + b);
                Vector2 groundAnchor1 = new Vector2(-10.0f, y + b + l);
                Vector2 groundAnchor2 = new Vector2(10.0f, y + b + l);
                _joint1 = new PulleyJoint(body1, body2, groundAnchor1, groundAnchor2, body1.GetLocalPoint(anchor1),
                                          body2.GetLocalPoint(anchor2), 2.0f);
                World.AddJoint(_joint1);
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            float ratio = _joint1.Ratio;
            float l = _joint1.LengthA + ratio * _joint1.LengthB;
            DebugView.DrawString(50, TextLine, "L1 + {0:n} * L2 = {1:n}", ratio, l);
            TextLine += 15;
        }

        internal static Test Create()
        {
            return new PulleysTest();
        }
    }
}