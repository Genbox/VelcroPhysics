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

                ground = World.AddBody();

                Vertices edge = PolygonTools.CreateEdge(new Vector2(50.0f, 0.0f), new Vector2(-50.0f, 0.0f));
                PolygonShape shape = new PolygonShape(edge, 0);

                ground.CreateFixture(shape);
            }

            {
                CircleShape circle1 = new CircleShape(1.0f, 5);

                CircleShape circle2 = new CircleShape(2.0f, 5);

                Vertices box = PolygonTools.CreateRectangle(0.5f, 5.0f);

                PolygonShape polygonBox = new PolygonShape(box, 5);

                Body body1 = World.AddBody();
                body1.BodyType = BodyType.Dynamic;
                body1.Position = new Vector2(-3.0f, 12.0f);

                body1.CreateFixture(circle1);

                _joint1 = new FixedRevoluteJoint(/*ground, */body1, body1.Position/*ground.GetLocalPoint(body1.Position)*/);
                //_joint1.LocalAnchorB = body1.GetLocalPoint(body1.Position);
                //_joint1.ReferenceAngle = body1.GetAngle() - ground.GetAngle();
                World.AddJoint(_joint1);

                Body body2 = World.AddBody();
                body2.BodyType = BodyType.Dynamic;
                body2.Position = new Vector2(0.0f, 12.0f);

                body2.CreateFixture(circle2);

                _joint2 = new FixedRevoluteJoint(/*ground,*/ body2, body2.Position);
                World.AddJoint(_joint2);

                Body body3 = World.AddBody();
                body3.BodyType = BodyType.Dynamic;
                body3.Position = new Vector2(2.5f, 12.0f);

                body3.CreateFixture(polygonBox);

                _joint3 = new FixedPrismaticJoint(body3, body3.Position, new Vector2(0.0f, 1.0f));
                _joint3.LowerLimit = -5.0f;
                _joint3.UpperLimit = 5.0f;
                _joint3.LimitEnabled = true;

                World.AddJoint(_joint3);

                _joint4 = new GearJoint(_joint1, _joint2, circle2.Radius / circle1.Radius);
                _joint4.BodyA = body1;
                _joint4.BodyB = body2;
                World.AddJoint(_joint4);

                _joint5 = new GearJoint(_joint2, _joint3, -1.0f / circle2.Radius);
                _joint5.BodyA = body2;
                _joint5.BodyB = body3;
                World.AddJoint(_joint5);
            }
        }

        public override void Update(Framework.Settings settings)
        {
            base.Update(settings);

            float ratio = _joint4.Ratio;
            float value = _joint1.JointAngle + ratio * _joint2.JointAngle;
            DebugView.DrawString(50, TextLine, "theta1 + {0:n} * theta2 = {1:n}", ratio, value);
            TextLine += 15;

            ratio = _joint5.Ratio;
            value = _joint2.JointAngle + ratio * _joint3.JointTranslation;
            DebugView.DrawString(50, TextLine, "theta2 + {0:n} * delta = {1:n}", ratio, value);
            TextLine += 15;
        }

        internal static Test Create()
        {
            return new GearsTest();
        }

        private FixedRevoluteJoint _joint1;
        private FixedRevoluteJoint _joint2;
        private FixedPrismaticJoint _joint3;
        private GearJoint _joint4;
        private GearJoint _joint5;
    }
}