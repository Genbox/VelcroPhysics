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

using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace TestBed
{
    public class Revolute : Test
    {
        private RevoluteJoint _joint;

        public Revolute()
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
                CircleShape shape = new CircleShape();
                shape._radius = 0.5f;

                BodyDef bd = new BodyDef();

                RevoluteJointDef rjd = new RevoluteJointDef();

                bd.Position.Set(0.0f, 20.0f);
                Body body = _world.CreateBody(bd);
                body.CreateFixture(shape, 5.0f);

                float w = 100.0f;
                body.SetAngularVelocity(w);
                body.SetLinearVelocity(new Vec2(-8.0f * w, 0.0f));

                rjd.Initialize(ground, body, new Vec2(0.0f, 12.0f));
                rjd.MotorSpeed = 1.0f * Box2DX.Common.Settings.PI;
                rjd.MaxMotorTorque = 10000.0f;
                rjd.EnableMotor = false;
                rjd.LowerAngle = -0.25f * Box2DX.Common.Settings.PI;
                rjd.UpperAngle = 0.5f * Box2DX.Common.Settings.PI;
                rjd.EnableLimit = true;
                rjd.CollideConnected = true;

                _joint = (RevoluteJoint)_world.CreateJoint(rjd);
            }
        }

        public override void Keyboard(System.Windows.Forms.Keys key)
        {
            switch (key)
            {
                case System.Windows.Forms.Keys.L:
                    _joint.EnableLimit(_joint.IsLimitEnabled);
                    break;
                case System.Windows.Forms.Keys.S:
                    _joint.EnableMotor(false);
                    break;
                default:
                    return;
            }
        }

        public override void Step(Settings settings)
        {
            base.Step(settings);
            OpenGLDebugDraw.DrawString(5, _textLine, "Keys: (l) limits, (s) motor");
            _textLine += 15;
            //float32 torque1 = m_joint1->GetMotorTorque();
            //m_debugDraw.DrawString(5, m_textLine, "Motor Torque = %4.0f, %4.0f : Motor Force = %4.0f", (float) torque1, (float) torque2, (float) force3);
            //m_textLine += 15;
        }

        public static Test Create()
        {
            return new Revolute();
        }
    }
}