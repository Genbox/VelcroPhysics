/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2008 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

using System;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace TestBed
{
    public class Prismatic : Test
    {
        private PrismaticJoint _joint;

        public Prismatic()
        {
            Body ground = null;
            {
                BodyDef bd = new BodyDef();
                ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vec2(-40.0f, 0.0f), new Vec2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(2.0f, 0.5f);

                BodyDef bd = new BodyDef();
                bd.Position.Set(-10.0f, 10.0f);
                bd.Angle = 0.5f * Box2DX.Common.Settings.PI;
                Body body = _world.CreateBody(bd);
                body.CreateFixture(shape, 5.0f);

                PrismaticJointDef pjd = new PrismaticJointDef();

                // Bouncy limit
                pjd.Initialize(ground, body, new Vec2(0.0f, 0.0f), new Vec2(1.0f, 0.0f));

                // Non-bouncy limit
                //pjd.Initialize(ground, body, b2Vec2(-10.0f, 10.0f), b2Vec2(1.0f, 0.0f));

                pjd.MotorSpeed = 10.0f;
                pjd.MaxMotorForce = 1000.0f;
                pjd.EnableMotor = true;
                pjd.LowerTranslation = 0.0f;
                pjd.UpperTranslation = 20.0f;
                pjd.EnableLimit = true;

                _joint = (PrismaticJoint)_world.CreateJoint(pjd);
            }
        }

        public override void Keyboard(System.Windows.Forms.Keys key)
        {
            switch (key)
            {
                case System.Windows.Forms.Keys.L:
                    _joint.EnableLimit(!_joint.IsLimitEnabled);
                    break;
                case System.Windows.Forms.Keys.M:
                    _joint.EnableMotor(!_joint.IsMotorEnabled);
                    break;
                case System.Windows.Forms.Keys.P:
                    _joint.MotorSpeed = (-_joint.MotorSpeed);
                    break;
                default:
                    return;
            }
        }

        public override void Step(Settings settings)
        {
            base.Step(settings);
            OpenGLDebugDraw.DrawString(5, _textLine, "Keys: (l) limits, (m) motors, (p) speed");
            _textLine += 15;
            float force = _joint.MotorForce;
            OpenGLDebugDraw.DrawString(5, _textLine, String.Format("Motor Force = {0}", force));
            _textLine += 15;

        }

        public static Test Create()
        {
            return new Prismatic();
        }
    }
}