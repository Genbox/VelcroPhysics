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

using System.Windows.Forms;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace TestBed
{
    public class Bridge : Test
    {
        private static int _count = 20;
        private Body _middle;

        public Bridge()
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
                shape.SetAsBox(0.5f, 0.125f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Density = 20.0f;
                fd.Friction = 0.2f;

                RevoluteJointDef jd = new RevoluteJointDef();
                const int numPlanks = 30;

                Body prevBody = ground;
                for (int i = 0; i < _count; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Position.Set(-14.5f + 1.0f * i, 5.0f);
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(fd);

                    Vec2 anchor = new Vec2(-15.0f + 1.0f * i, 5.0f);
                    jd.Initialize(prevBody, body, anchor);
                    _world.CreateJoint(jd);

                    if (i == (_count >> 1))
                    {
                        _middle = body;
                    }

                    prevBody = body;
                }

                Vec2 anchor2 = new Vec2(-15.0f + 1.0f * _count, 5.0f);
                jd.Initialize(prevBody, ground, anchor2);
                _world.CreateJoint(jd);
            }

            for (int i = 0; i < 2; ++i)
            {
                Vec2[] vertices = new Vec2[3];
                vertices[0].Set(-0.5f, 0.0f);
                vertices[1].Set(0.5f, 0.0f);
                vertices[2].Set(0.0f, 1.5f);

                PolygonShape shape = new PolygonShape();
                shape.Set(vertices, 3);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Density = 1.0f;

                BodyDef bd = new BodyDef();
                bd.Position.Set(-8.0f + 8.0f * i, 12.0f);
                Body body = _world.CreateBody(bd);
                body.CreateFixture(fd);
            }

            for (int i = 0; i < 3; ++i)
            {
                CircleShape shape = new CircleShape();
                shape._radius = 0.5f;

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Density = 1.0f;

                BodyDef bd = new BodyDef();
                bd.Position.Set(-6.0f + 6.0f * i, 10.0f);
                Body body = _world.CreateBody(bd);
                body.CreateFixture(fd);
            }
        }

        public override void Keyboard(Keys key)
        {
            switch (key)
            {
                case Keys.S:
                    {
                        MassData data = new MassData();
                        data.Center.SetZero();
                        data.I = 0.0f;
                        data.Mass = 0.0f;
                        _middle.SetMassData(data);
                    }
                    break;
            }
        }

        public override void Step(Settings settings)
        {
            base.Step(settings);
            OpenGLDebugDraw.DrawString(5, _textLine, "Press (s) to make a body static");
            _textLine += 15;
        }

        public static Test Create()
        {
            return new Bridge();
        }
    }
}
