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

using System;
using Box2D.XNA.TestBed.Framework;
using Box2D.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Box2D.XNA.TestBed.Tests
{
    public class Gears : Test
    {
        Gears()
	    {
		    Body ground = null;
		    {
			    BodyDef bd = new BodyDef();
			    ground = _world.CreateBody(bd);

			    PolygonShape shape = new PolygonShape();
			    shape.SetAsEdge(new Vector2(50.0f, 0.0f), new Vector2(-50.0f, 0.0f));
			    ground.CreateFixture(shape, 0.0f);
		    }

		    {
                CircleShape circle1 = new CircleShape();
			    circle1._radius = 1.0f;

                CircleShape circle2 = new CircleShape();
			    circle2._radius = 2.0f;

                PolygonShape box = new PolygonShape();
			    box.SetAsBox(0.5f, 5.0f);

                BodyDef bd1 = new BodyDef();
                bd1.type = BodyType.Dynamic;
			    bd1.position = new Vector2(-3.0f, 12.0f);
			    Body body1 = _world.CreateBody(bd1);
			    body1.CreateFixture(circle1, 5.0f);

                RevoluteJointDef jd1 = new RevoluteJointDef();
			    jd1.bodyA = ground;
			    jd1.bodyB = body1;
			    jd1.localAnchorA = ground.GetLocalPoint(bd1.position);
			    jd1.localAnchorB = body1.GetLocalPoint(bd1.position);
			    jd1.referenceAngle = body1.GetAngle() - ground.GetAngle();
			    _joint1 = (RevoluteJoint)_world.CreateJoint(jd1);

                BodyDef bd2 = new BodyDef();
                bd2.type = BodyType.Dynamic;
			    bd2.position = new Vector2(0.0f, 12.0f);
			    Body body2 = _world.CreateBody(bd2);
			    body2.CreateFixture(circle2, 5.0f);

                RevoluteJointDef jd2 = new RevoluteJointDef();
			    jd2.Initialize(ground, body2, bd2.position);
			    _joint2 = (RevoluteJoint)_world.CreateJoint(jd2);

                BodyDef bd3 = new BodyDef();
                bd3.type = BodyType.Dynamic;
			    bd3.position = new Vector2(2.5f, 12.0f);
			    Body body3 = _world.CreateBody(bd3);
			    body3.CreateFixture(box, 5.0f);

                PrismaticJointDef jd3 = new PrismaticJointDef();
			    jd3.Initialize(ground, body3, bd3.position, new Vector2(0.0f, 1.0f));
			    jd3.lowerTranslation = -5.0f;
			    jd3.upperTranslation = 5.0f;
			    jd3.enableLimit = true;

			    _joint3 = (PrismaticJoint)_world.CreateJoint(jd3);

                GearJointDef jd4 = new GearJointDef();
			    jd4.bodyA = body1;
			    jd4.bodyB = body2;
			    jd4.joint1 = _joint1;
			    jd4.joint2 = _joint2;
			    jd4.ratio = circle2._radius / circle1._radius;
			    _joint4 = (GearJoint)_world.CreateJoint(jd4);

                GearJointDef jd5 = new GearJointDef();
			    jd5.bodyA = body2;
			    jd5.bodyB = body3;
			    jd5.joint1 = _joint2;
			    jd5.joint2 = _joint3;
			    jd5.ratio = -1.0f / circle2._radius;
			    _joint5 = (GearJoint)_world.CreateJoint(jd5);
		    }
	    }

	    public override void Step(Framework.Settings settings)
	    {
		    base.Step(settings);

		    float ratio = 0.0f;
            float value = 0.0f;
    		
		    ratio = _joint4.GetRatio();
		    value = _joint1.GetJointAngle() + ratio * _joint2.GetJointAngle();
            _debugDraw.DrawString(50, _textLine, "theta1 + {0:n} * theta2 = {1:n}", (float)ratio, (float)value);
		    _textLine += 15;

		    ratio = _joint5.GetRatio();
		    value = _joint2.GetJointAngle() + ratio * _joint3.GetJointTranslation();
            _debugDraw.DrawString(50, _textLine, "theta2 + {0:n} * delta = {1:n}", (float)ratio, (float)value);
		    _textLine += 15;
	    }

	    internal static Test Create()
	    {
		    return new Gears();
	    }

	    RevoluteJoint _joint1;
	    RevoluteJoint _joint2;
	    PrismaticJoint _joint3;
	    GearJoint _joint4;
	    GearJoint _joint5;
    }
}
