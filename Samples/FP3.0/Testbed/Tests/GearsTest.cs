/*
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

using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class GearsTest : Test
    {
        private GearsTest()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vector2(50.0f, 0.0f), new Vector2(-50.0f, 0.0f));
                ground.CreateFixture(shape, 0.0f);
            }

            {
                CircleShape circle1 = new CircleShape(1.0f);

                CircleShape circle2 = new CircleShape(2.0f);

                PolygonShape box = new PolygonShape();
                box.SetAsBox(0.5f, 5.0f);

                BodyDef bd1 = new BodyDef();
                bd1.type = BodyType.Dynamic;
                bd1.position = new Vector2(-3.0f, 12.0f);
                Body body1 = _world.CreateBody(bd1);
                body1.CreateFixture(circle1, 5.0f);

                RevoluteJointDef jd1 = new RevoluteJointDef();
                jd1.BodyA = ground;
                jd1.BodyB = body1;
                jd1.LocalAnchorA = ground.GetLocalPoint(bd1.position);
                jd1.LocalAnchorB = body1.GetLocalPoint(bd1.position);
                jd1.ReferenceAngle = body1.GetAngle() - ground.GetAngle();
                _joint1 = (RevoluteJoint) _world.CreateJoint(jd1);

                BodyDef bd2 = new BodyDef();
                bd2.type = BodyType.Dynamic;
                bd2.position = new Vector2(0.0f, 12.0f);
                Body body2 = _world.CreateBody(bd2);
                body2.CreateFixture(circle2, 5.0f);

                RevoluteJointDef jd2 = new RevoluteJointDef();
                jd2.Initialize(ground, body2, bd2.position);
                _joint2 = (RevoluteJoint) _world.CreateJoint(jd2);

                BodyDef bd3 = new BodyDef();
                bd3.type = BodyType.Dynamic;
                bd3.position = new Vector2(2.5f, 12.0f);
                Body body3 = _world.CreateBody(bd3);
                body3.CreateFixture(box, 5.0f);

                PrismaticJointDef jd3 = new PrismaticJointDef();
                jd3.Initialize(ground, body3, bd3.position, new Vector2(0.0f, 1.0f));
                jd3.LowerTranslation = -5.0f;
                jd3.UpperTranslation = 5.0f;
                jd3.EnableLimit = true;

                _joint3 = (PrismaticJoint) _world.CreateJoint(jd3);

                GearJointDef jd4 = new GearJointDef();
                jd4.BodyA = body1;
                jd4.BodyB = body2;
                jd4.Joint1 = _joint1;
                jd4.Joint2 = _joint2;
                jd4.Ratio = circle2.Radius/circle1.Radius;
                _joint4 = (GearJoint) _world.CreateJoint(jd4);

                GearJointDef jd5 = new GearJointDef();
                jd5.BodyA = body2;
                jd5.BodyB = body3;
                jd5.Joint1 = _joint2;
                jd5.Joint2 = _joint3;
                jd5.Ratio = -1.0f/circle2.Radius;
                _joint5 = (GearJoint) _world.CreateJoint(jd5);
            }
        }

        public override void Step(Framework.Settings settings)
        {
            base.Step(settings);

            float ratio = _joint4.GetRatio();
            float value = _joint1.GetJointAngle() + ratio*_joint2.GetJointAngle();
            _debugView.DrawString(50, _textLine, "theta1 + {0:n} * theta2 = {1:n}", ratio, value);
            _textLine += 15;

            ratio = _joint5.GetRatio();
            value = _joint2.GetJointAngle() + ratio*_joint3.GetJointTranslation();
            _debugView.DrawString(50, _textLine, "theta2 + {0:n} * delta = {1:n}", ratio, value);
            _textLine += 15;
        }

        internal static Test Create()
        {
            return new GearsTest();
        }

        private RevoluteJoint _joint1;
        private RevoluteJoint _joint2;
        private PrismaticJoint _joint3;
        private GearJoint _joint4;
        private GearJoint _joint5;
    }
}