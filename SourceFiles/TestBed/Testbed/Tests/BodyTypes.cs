/*
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

using Box2DX.Dynamics;
using Box2DX.Collision;
using System.Windows.Forms;
using Box2DX.Common;
using TestBed;

public class BodyTypes : TestBed.Test
{
    public BodyTypes()
    {
        Body ground = null;
        {
            BodyDef bd = new BodyDef();
            ground = _world.CreateBody(bd);

            PolygonShape shape = new PolygonShape();
            shape.SetAsEdge(new Vec2(-20.0f, 0.0f), new Vec2(20.0f, 0.0f));

            FixtureDef fd = new FixtureDef();
            fd.Shape = shape;

            ground.CreateFixture(fd);
        }

        // Define attachment
        {
            BodyDef bd = new BodyDef();
            bd.Type = Body.BodyType.Dynamic;
            bd.Position.Set(0.0f, 3.0f);
            _attachment = _world.CreateBody(bd);

            PolygonShape shape = new PolygonShape();
            shape.SetAsBox(0.5f, 2.0f);
            _attachment.CreateFixture(shape, 2.0f);
        }

        // Define platform
        {
            BodyDef bd = new BodyDef();
            bd.Type = Body.BodyType.Dynamic;
            bd.Position.Set(0.0f, 5.0f);
            _platform = _world.CreateBody(bd);

            PolygonShape shape = new PolygonShape();
            shape.SetAsBox(4.0f, 0.5f);

            FixtureDef fd = new FixtureDef();
            fd.Shape = shape;
            fd.Friction = 0.6f;
            fd.Density = 2.0f;
            _platform.CreateFixture(fd);

            RevoluteJointDef rjd = new RevoluteJointDef();
            rjd.Initialize(_attachment, _platform, new Vec2(0.0f, 5.0f));
            rjd.MaxMotorTorque = 50.0f;
            rjd.EnableMotor = true;
            _world.CreateJoint(rjd);

            PrismaticJointDef pjd = new PrismaticJointDef();
            pjd.Initialize(ground, _platform, new Vec2(0.0f, 5.0f), new Vec2(1.0f, 0.0f));

            pjd.MaxMotorForce = 1000.0f;
            pjd.EnableMotor = true;
            pjd.LowerTranslation = -10.0f;
            pjd.UpperTranslation = 10.0f;
            pjd.EnableLimit = true;

            _world.CreateJoint(pjd);

            _speed = 3.0f;
        }

        // Create a payload
        {
            BodyDef bd = new BodyDef();
            bd.Type = Body.BodyType.Dynamic;
            bd.Position.Set(0.0f, 8.0f);
            Body body = _world.CreateBody(bd);

            PolygonShape shape = new PolygonShape();
            shape.SetAsBox(0.75f, 0.75f);

            FixtureDef fd = new FixtureDef();
            fd.Shape = shape;
            fd.Friction = 0.6f;
            fd.Density = 2.0f;

            body.CreateFixture(fd);
        }
    }

    public override void Keyboard(System.Windows.Forms.Keys key)
    {
        switch (key)
        {
            case Keys.D:
                _platform.SetType(Body.BodyType.Dynamic);
                break;

            case Keys.S:
                _platform.SetType(Body.BodyType.Static);
                break;

            case Keys.K:
                _platform.SetType(Body.BodyType.Kinematic);
                _platform.SetLinearVelocity(new Vec2(-_speed, 0.0f));
                _platform.SetAngularVelocity(0.0f);
                break;
        }
    }

    public override void Step(TestBed.Settings settings)
    {
        // Drive the kinematic body.
        if (_platform.GetType() == Body.BodyType.Kinematic)
        {
            Vec2 p = _platform.GetTransform().Position;
            Vec2 v = _platform.GetLinearVelocity();

            if ((p.X < -10.0f && v.X < 0.0f) ||
                (p.X > 10.0f && v.X > 0.0f))
            {
                v.X = -v.X;
                _platform.SetLinearVelocity(v);
            }
        }

        base.Step(settings);
        _debugDraw.DrawString(5, _textLine, "Keys: (d) dynamic, (s) static, (k) kinematic");
        _textLine += 15;
    }

    public static Test Create()
    {
        return new BodyTypes();
    }

    Body _attachment;
    Body _platform;
    float _speed;
};