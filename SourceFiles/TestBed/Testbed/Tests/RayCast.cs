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

using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace TestBed
{

    public class MyRayCastCallback : RayCastCallback
    {
        public MyRayCastCallback()
        {
            _fixture = null;
        }

        public override float ReportFixture(Fixture fixture, Vec2 point, Vec2 normal, float fraction)
        {
            _fixture = fixture;
            _point = point;
            _normal = normal;

            return fraction;
        }

        public Fixture _fixture;
        public Vec2 _point;
        public Vec2 _normal;
    };

    public class RayCast : Test
    {
        public const int _maxBodies = 256;

        RayCast()
        {
            // Ground body
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vec2(-40.0f, 0.0f), new Vec2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0);
            }

            //Fill array with polygons:
            for (int i = 0; i < 4; i++)
            {
                _polygons[i] = new PolygonShape();
            }

            {
                Vec2[] vertices = new Vec2[3];
                vertices[0].Set(-0.5f, 0.0f);
                vertices[1].Set(0.5f, 0.0f);
                vertices[2].Set(0.0f, 1.5f);
                _polygons[0].Set(vertices, 3);
            }

            {
                Vec2[] vertices = new Vec2[3];
                vertices[0].Set(-0.1f, 0.0f);
                vertices[1].Set(0.1f, 0.0f);
                vertices[2].Set(0.0f, 1.5f);
                _polygons[1].Set(vertices, 3);
            }

            {
                float w = 1.0f;
                float b = w / (2.0f + Math.Sqrt(2.0f));
                float s = Math.Sqrt(2.0f) * b;

                Vec2[] vertices = new Vec2[8];
                vertices[0].Set(0.5f * s, 0.0f);
                vertices[1].Set(0.5f * w, b);
                vertices[2].Set(0.5f * w, b + s);
                vertices[3].Set(0.5f * s, w);
                vertices[4].Set(-0.5f * s, w);
                vertices[5].Set(-0.5f * w, b + s);
                vertices[6].Set(-0.5f * w, b);
                vertices[7].Set(-0.5f * s, 0.0f);

                _polygons[2].Set(vertices, 8);
            }

            {
                _polygons[3].SetAsBox(0.5f, 0.5f);
            }

            {
                _circle._radius = 0.5f;
            }

            _bodyIndex = 0;
            //memset(m_bodies, 0, sizeof(m_bodies));

            _angle = 0.0f;
        }

        void Create(int index)
        {
            if (_bodies[_bodyIndex] != null)
            {
                _world.DestroyBody(_bodies[_bodyIndex]);
                _bodies[_bodyIndex] = null;
            }

            BodyDef bd = new BodyDef();

            float x = Math.Random(-10.0f, 10.0f);
            float y = Math.Random(0.0f, 20.0f);
            bd.Position.Set(x, y);
            bd.Angle = Math.Random(-Box2DX.Common.Settings.PI, Box2DX.Common.Settings.PI);

            if (index == 4)
            {
                bd.AngularDamping = 0.02f;
            }

            _bodies[_bodyIndex] = _world.CreateBody(bd);

            if (index < 4)
            {
                FixtureDef fd = new FixtureDef();
                fd.Shape = _polygons[index];
                fd.Friction = 0.3f;
                _bodies[_bodyIndex].CreateFixture(fd);
            }
            else
            {
                FixtureDef fd = new FixtureDef();
                fd.Shape = _circle;
                fd.Friction = 0.3f;

                _bodies[_bodyIndex].CreateFixture(fd);
            }

            _bodyIndex = (_bodyIndex + 1) % _maxBodies;
        }

        void DestroyBody()
        {
            for (int i = 0; i < _maxBodies; ++i)
            {
                if (_bodies[i] != null)
                {
                    _world.DestroyBody(_bodies[i]);
                    _bodies[i] = null;
                    return;
                }
            }
        }

        public override void Keyboard(System.Windows.Forms.Keys key)
        {
            int keyKode = 0;
            switch (key)
            {
                case System.Windows.Forms.Keys.D1:
                    keyKode = 1;
                    break;
                case System.Windows.Forms.Keys.D2:
                    keyKode = 2;
                    break;
                case System.Windows.Forms.Keys.D3:
                    keyKode = 3;
                    break;
                case System.Windows.Forms.Keys.D4:
                    keyKode = 4;
                    break;
                case System.Windows.Forms.Keys.D5:
                    keyKode = 5;
                    break;
                case System.Windows.Forms.Keys.D:
                    DestroyBody();
                    return;
                default:
                    return;
            }
            Create(keyKode - 1);
            return;
        }

        public override void Step(Settings settings)
        {
            base.Step(settings);
            OpenGLDebugDraw.DrawString(5, _textLine, "Press 1-5 to drop stuff");
            _textLine += 15;

            float L = 11.0f;
            Vec2 point1 = new Vec2(0.0f, 10.0f);
            Vec2 d = new Vec2(L * (float)System.Math.Cos(_angle), L * (float)System.Math.Sin(_angle));
            Vec2 point2 = point1 + d;

            MyRayCastCallback callback = new MyRayCastCallback();

            _world.RayCast(callback, point1, point2);

            if (callback._fixture != null)
            {
                OpenGLDebugDraw.DrawPoint(callback._point, 5.0f, new Color(0.4f, 0.9f, 0.4f));

                _debugDraw.DrawSegment(point1, callback._point, new Color(0.8f, 0.8f, 0.8f));

                Vec2 head = callback._point + 0.5f * callback._normal;
                _debugDraw.DrawSegment(callback._point, head, new Color(0.9f, 0.9f, 0.4f));
            }
            else
            {
                _debugDraw.DrawSegment(point1, point2, new Color(0.8f, 0.8f, 0.8f));
            }

            _angle += 0.25f * Box2DX.Common.Settings.PI / 180.0f;
        }

        public static Test Create()
        {
            return new RayCast();
        }

        int _bodyIndex;
        Body[] _bodies = new Body[_maxBodies];
        PolygonShape[] _polygons = new PolygonShape[4];
        CircleShape _circle = new CircleShape();

        float _angle;
    }
}