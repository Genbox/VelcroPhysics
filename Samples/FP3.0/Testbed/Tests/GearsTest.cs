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

                ground = World.CreateBody();

                Vertices edge = PolygonTools.CreateEdge(new Vector2(50.0f, 0.0f), new Vector2(-50.0f, 0.0f));
                PolygonShape shape = new PolygonShape(edge, 0);

                ground.CreateFixture(shape);
            }

            {
                CircleShape circle1 = new CircleShape(1.0f, 5);

                CircleShape circle2 = new CircleShape(2.0f, 5);

                Vertices box = PolygonTools.CreateBox(0.5f, 5.0f);

                PolygonShape polygonBox = new PolygonShape(box, 5);

                Body body1 = World.CreateBody();
                body1.BodyType = BodyType.Dynamic;
                body1.Position = new Vector2(-3.0f, 12.0f);

                body1.CreateFixture(circle1);

                _joint1 = new RevoluteJoint(ground, body1, ground.GetLocalPoint(body1.Position));
                _joint1.LocalAnchorB = body1.GetLocalPoint(body1.Position);
                _joint1.ReferenceAngle = body1.GetAngle() - ground.GetAngle();
                World.CreateJoint(_joint1);

                Body body2 = World.CreateBody();
                body2.BodyType = BodyType.Dynamic;
                body2.Position = new Vector2(0.0f, 12.0f);

                body2.CreateFixture(circle2);

                _joint2 = new RevoluteJoint(ground, body2, body2.Position);
                World.CreateJoint(_joint2);

                Body body3 = World.CreateBody();
                body3.BodyType = BodyType.Dynamic;
                body3.Position = new Vector2(2.5f, 12.0f);

                body3.CreateFixture(polygonBox);

                _joint3 = new PrismaticJoint(ground, body3, body3.Position, new Vector2(0.0f, 1.0f));
                _joint3.LowerLimit = -5.0f;
                _joint3.UpperLimit = 5.0f;
                _joint3.LimitEnabled = true;

                World.CreateJoint(_joint3);

                _joint4 = new GearJoint(_joint1, _joint2, circle2.Radius / circle1.Radius);
                _joint4.BodyA = body1;
                _joint4.BodyB = body2;
                World.CreateJoint(_joint4);

                _joint5 = new GearJoint(_joint2, _joint3, -1.0f / circle2.Radius);
                _joint5.BodyA = body2;
                _joint5.BodyB = body3;
                World.CreateJoint(_joint5);
            }
        }

        public override void Step(Framework.Settings settings)
        {
            base.Step(settings);

            float ratio = _joint4.Ratio;
            float value = _joint1.JointAngle + ratio * _joint2.JointAngle;
            _debugView.DrawString(50, TextLine, "theta1 + {0:n} * theta2 = {1:n}", ratio, value);
            TextLine += 15;

            ratio = _joint5.Ratio;
            value = _joint2.JointAngle + ratio * _joint3.JointTranslation;
            _debugView.DrawString(50, TextLine, "theta2 + {0:n} * delta = {1:n}", ratio, value);
            TextLine += 15;
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