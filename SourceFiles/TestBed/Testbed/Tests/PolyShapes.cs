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

/// This tests stacking. It also shows how to use World.Query
/// and TestOverlap.
namespace TestBed
{
    /// This callback is called by b2World::QueryAABB. We find all the fixtures
    /// that overlap an AABB. Of those, we use b2TestOverlap to determine which fixtures
    /// overlap a circle. Up to 4 overlapped fixtures will be highlighted with a yellow border.
    public class PolyShapesCallback : QueryCallback
    {
        public const int _maxCount = 4;

        public PolyShapesCallback()
        {
            _count = 0;
        }

        public void DrawFixture(Fixture fixture)
        {
            Color color = new Color(0.95f, 0.95f, 0.6f);
            Transform xf = fixture.GetBody().GetTransform();

            switch (fixture.GetType())
            {
                case ShapeType.CircleShape:
                    {
                        CircleShape circle = (CircleShape)fixture.GetShape();

                        Vec2 center = Math.Mul(xf, circle._p);
                        float radius = circle._radius;

                        _debugDraw.DrawCircle(center, radius, color);
                    }
                    break;

                case ShapeType.PolygonShape:
                    {
                        PolygonShape poly = (PolygonShape)fixture.GetShape();
                        int vertexCount = poly._vertexCount;
                        Box2DX.Box2DXDebug.Assert(vertexCount <= Box2DX.Common.Settings.MaxPolygonVertices);
                        Vec2[] vertices = new Vec2[Box2DX.Common.Settings.MaxPolygonVertices];

                        for (int i = 0; i < vertexCount; ++i)
                        {
                            vertices[i] = Math.Mul(xf, poly._vertices[i]);
                        }

                        _debugDraw.DrawPolygon(vertices, vertexCount, color);
                    }
                    break;
            }
        }

        /// Called for each fixture found in the query AABB.
        /// @return false to terminate the query.
        public override bool ReportFixture(Fixture fixture)
        {
            if (_count == _maxCount)
            {
                return false;
            }

            Body body = fixture.GetBody();
            Shape shape = fixture.GetShape();

            bool overlap = Collision.TestOverlap(shape, _circle, body.GetTransform(), _transform);

            if (overlap)
            {
                DrawFixture(fixture);
                ++_count;
            }

            return true;
        }

        public CircleShape _circle = new CircleShape();
        public Transform _transform;
        public DebugDraw _debugDraw;
        public int _count;
    }

    public class PolyShapes : Test
    {
        const int _maxBodies = 256;

        int bodyIndex;
        Body[] bodies = new Body[_maxBodies];
        PolygonShape[] polygons = new PolygonShape[4];
        CircleShape circle;

        public PolyShapes()
        {
            // Ground body
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vec2(-40.0f, 0.0f), new Vec2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0);
            }

            for (int i = 0; i < 4; i++)
            {
                polygons[i] = new PolygonShape();
            }

            {
                Vec2[] vertices = new Vec2[3];
                vertices[0].Set(-0.5f, 0.0f);
                vertices[1].Set(0.5f, 0.0f);
                vertices[2].Set(0.0f, 1.5f);
                polygons[0].Set(vertices, 3);
            }

            {
                Vec2[] vertices = new Vec2[3];
                vertices[0].Set(-0.1f, 0.0f);
                vertices[1].Set(0.1f, 0.0f);
                vertices[2].Set(0.0f, 1.5f);
                polygons[1].Set(vertices, 3);
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

                polygons[2].Set(vertices, 8);
            }

            {
                polygons[3].SetAsBox(0.5f, 0.5f);
            }

            {
                circle = new CircleShape();
                circle._radius = 0.5f;
            }

            bodyIndex = 0;
            //memset(bodies, 0, sizeof(bodies));
        }

        public void Create(int index)
        {
            if (bodies[bodyIndex] != null)
            {
                _world.DestroyBody(bodies[bodyIndex]);
                bodies[bodyIndex] = null;
            }

            BodyDef bd = new BodyDef();
            bd.Type = Body.BodyType.Dynamic;

            float x = Math.RandomFloat(-2.0f, 2.0f);
            bd.Position.Set(x, 10.0f);
            bd.Angle = Math.RandomFloat(-Box2DX.Common.Settings.pi, Box2DX.Common.Settings.pi);

            if (index == 4)
            {
                bd.AngularDamping = 0.02f;
            }

            bodies[bodyIndex] = _world.CreateBody(bd);

            if (index < 4)
            {
                FixtureDef fd = new FixtureDef();
                fd.Shape = polygons[index];
                fd.Density = 1.0f;
                fd.Friction = 0.3f;
                bodies[bodyIndex].CreateFixture(fd);
            }
            else
            {
                FixtureDef fd = new FixtureDef();
                fd.Shape = circle;
                fd.Density = 1.0f;
                fd.Friction = 0.3f;

                bodies[bodyIndex].CreateFixture(fd);
            }

            bodyIndex = (bodyIndex + 1) % _maxBodies;
        }

        public void DestroyBody()
        {
            for (int i = 0; i < _maxBodies; ++i)
            {
                if (bodies[i] != null)
                {
                    _world.DestroyBody(bodies[i]);
                    bodies[i] = null;
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

            PolyShapesCallback callback = new PolyShapesCallback();
            callback._circle._radius = 2.0f;
            callback._circle._p.Set(0.0f, 2.1f);
            callback._transform.SetIdentity();
            callback._debugDraw = _debugDraw;

            AABB aabb;
            callback._circle.ComputeAABB(out aabb, ref callback._transform);

            _world.QueryAABB(callback, aabb);

            Color color = new Color(0.4f, 0.7f, 0.8f);
            _debugDraw.DrawCircle(callback._circle._p, callback._circle._radius, color);

            OpenGLDebugDraw.DrawString(5, _textLine, "Press 1-5 to drop stuff");
            _textLine += 15;
            OpenGLDebugDraw.DrawString(5, _textLine, "Press 'a' to (de)activate some bodies");
            _textLine += 15;
            OpenGLDebugDraw.DrawString(5, _textLine, "Press 'd' to destroy a body");
            _textLine += 15;
        }

        public static Test Create()
        {
            return new PolyShapes();
        }
    }
}