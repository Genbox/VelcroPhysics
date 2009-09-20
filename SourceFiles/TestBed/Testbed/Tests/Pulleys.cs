/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2007 Erin Catto http://www.gphysics.com

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
    public class Pulleys : Test
    {
        PulleyJoint _joint1;

        public Pulleys()
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
                float a = 2.0f;
                float b = 4.0f;
                float y = 16.0f;
                float L = 12.0f;

                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(a, b);

                BodyDef bd = new BodyDef();

                bd.Position.Set(-10.0f, y);
                Body body1 = _world.CreateBody(bd);
                body1.CreateFixture(shape, 5.0f);

                bd.Position.Set(10.0f, y);
                Body body2 = _world.CreateBody(bd);
                body2.CreateFixture(shape, 5.0f);

                PulleyJointDef pulleyDef = new PulleyJointDef();
                Vec2 anchor1 = new Vec2(-10.0f, y + b);
                Vec2 anchor2 = new Vec2(10.0f, y + b);
                Vec2 groundAnchor1 = new Vec2(-10.0f, y + b + L);
                Vec2 groundAnchor2 = new Vec2(10.0f, y + b + L);
                pulleyDef.Initialize(body1, body2, groundAnchor1, groundAnchor2, anchor1, anchor2, 2.0f);

                _joint1 = (PulleyJoint)_world.CreateJoint(pulleyDef);
            }
        }

        public override void Keyboard(System.Windows.Forms.Keys key)
        {
            switch (key)
            {
                case 0:
                    break;
            }
        }

        public override void Step(Settings settings)
        {
            base.Step(settings);

            float ratio = _joint1.Ratio;
            float L = _joint1.Length1 + ratio * _joint1.Length2;
            OpenGLDebugDraw.DrawString(5, _textLine, string.Format("L1 + {0} * L2 = {1}", ratio, L));
            _textLine += 15;
        }

        public static Test Create()
        {
            return new Pulleys();
        }
    }
}
