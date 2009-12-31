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
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace FarseerPhysics.TestBed.Tests
{
    /// This tests stacking. It also shows how to use b2World::Query
    /// and b2TestOverlap.
    /// 
    /// This callback is called by b2World::QueryAABB. We find all the fixtures
    /// that overlap an AABB. Of those, we use b2TestOverlap to determine which fixtures
    /// overlap a circle. Up to 4 overlapped fixtures will be highlighted with a yellow border.
    public class PolyShapesCallback
    {
        private const int MaxCount = 4;

        private void DrawFixture(Fixture fixture)
        {
            Color color = new Color(0.95f, 0.95f, 0.6f);
            Transform xf;
            fixture.GetBody().GetTransform(out xf);

            switch (fixture.ShapeType)
            {
                case ShapeType.Circle:
                    {
                        CircleShape circle = (CircleShape)fixture.GetShape();

                        Vector2 center = MathUtils.Multiply(ref xf, circle.Position);
                        float radius = circle.Radius;

                        _debugDraw.DrawCircle(center, radius, color);
                    }
                    break;

                case ShapeType.Polygon:
                    {
                        PolygonShape poly = (PolygonShape)fixture.GetShape();
                        int vertexCount = poly.VertexCount;
                        Debug.Assert(vertexCount <= Settings.MaxPolygonVertices);
                        FixedArray8<Vector2> vertices = new FixedArray8<Vector2>();

                        for (int i = 0; i < vertexCount; ++i)
                        {
                            vertices[i] = MathUtils.Multiply(ref xf, poly.Vertices[i]);
                        }

                        _debugDraw.DrawPolygon(ref vertices, vertexCount, color);
                    }
                    break;
            }
        }

        /// Called for each fixture found in the query AABB.
        /// @return false to terminate the query.
        public bool ReportFixture(Fixture fixture)
        {
            if (_count == MaxCount)
            {
                return false;
            }

            Body body = fixture.GetBody();
            Shape shape = fixture.GetShape();

            Transform xf;
            body.GetTransform(out xf);

            bool overlap = AABB.TestOverlap(shape, _circle, ref xf, ref _transform);

            if (overlap)
            {
                DrawFixture(fixture);
                ++_count;
            }

            return true;
        }

        internal CircleShape _circle = new CircleShape();
        internal Transform _transform;
        internal DebugViewXNA.DebugViewXNA _debugDraw;
        private int _count;
    }

    public class PolyShapesTest : Test
    {
        private const int MaxBodies = 256;

        private PolyShapesTest()
        {
            for (int i = 0; i < 4; i++)
            {
                _polygons[i] = new PolygonShape();
            }

            // Ground body
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0.0f);
            }

            {
                Vector2[] vertices = new Vector2[3];
                vertices[0] = new Vector2(-0.5f, 0.0f);
                vertices[1] = new Vector2(0.5f, 0.0f);
                vertices[2] = new Vector2(0.0f, 1.5f);
                _polygons[0].Set(vertices, 3);
            }

            {
                Vector2[] vertices3 = new Vector2[3];
                vertices3[0] = new Vector2(-0.1f, 0.0f);
                vertices3[1] = new Vector2(0.1f, 0.0f);
                vertices3[2] = new Vector2(0.0f, 1.5f);
                _polygons[1].Set(vertices3, 3);
            }

            {
                const float w = 1.0f;
                float b = w / (2.0f + (float)Math.Sqrt(2.0));
                float s = (float)Math.Sqrt(2.0) * b;

                Vector2[] vertices8 = new Vector2[8];
                vertices8[0] = new Vector2(0.5f * s, 0.0f);
                vertices8[1] = new Vector2(0.5f * w, b);
                vertices8[2] = new Vector2(0.5f * w, b + s);
                vertices8[3] = new Vector2(0.5f * s, w);
                vertices8[4] = new Vector2(-0.5f * s, w);
                vertices8[5] = new Vector2(-0.5f * w, b + s);
                vertices8[6] = new Vector2(-0.5f * w, b);
                vertices8[7] = new Vector2(-0.5f * s, 0.0f);

                _polygons[2].Set(vertices8, 8);
            }

            {
                _polygons[3].SetAsBox(0.5f, 0.5f);
            }

            {
                _circle.Radius = 0.5f;
            }

            _bodyIndex = 0;
        }

        private void Create(int index)
        {
            if (_bodies[_bodyIndex] != null)
            {
                _world.DestroyBody(_bodies[_bodyIndex]);
                _bodies[_bodyIndex] = null;
            }

            BodyDef bd = new BodyDef();
            bd.Type = BodyType.Dynamic;

            float x = Rand.RandomFloat(-2.0f, 2.0f);
            bd.Position = new Vector2(x, 10.0f);
            bd.Angle = Rand.RandomFloat(-Settings.Pi, Settings.Pi);

            if (index == 4)
            {
                bd.AngularDamping = 0.02f;
            }

            _bodies[_bodyIndex] = _world.CreateBody(bd);

            if (index < 4)
            {
                FixtureDef fd = new FixtureDef();
                fd.Shape = _polygons[index];
                fd.Density = 1.0f;
                fd.Friction = 0.3f;
                _bodies[_bodyIndex].CreateFixture(fd);
            }
            else
            {
                FixtureDef fd = new FixtureDef();
                fd.Shape = _circle;
                fd.Density = 1.0f;
                fd.Friction = 0.3f;

                _bodies[_bodyIndex].CreateFixture(fd);
            }

            _bodyIndex = (_bodyIndex + 1) % MaxBodies;
        }

        private void DestroyBody()
        {
            for (int i = 0; i < MaxBodies; ++i)
            {
                if (_bodies[i] != null)
                {
                    _world.DestroyBody(_bodies[i]);
                    _bodies[i] = null;
                    return;
                }
            }
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.D1) && oldState.IsKeyUp(Keys.D1))
            {
                Create(0);
            }
            if (state.IsKeyDown(Keys.D2) && oldState.IsKeyUp(Keys.D2))
            {
                Create(1);
            }
            if (state.IsKeyDown(Keys.D3) && oldState.IsKeyUp(Keys.D3))
            {
                Create(2);
            }
            if (state.IsKeyDown(Keys.D4) && oldState.IsKeyUp(Keys.D4))
            {
                Create(3);
            }
            if (state.IsKeyDown(Keys.D5) && oldState.IsKeyUp(Keys.D5))
            {
                Create(4);
            }
            if (state.IsKeyDown(Keys.A) && oldState.IsKeyUp(Keys.A))
            {
                for (int i = 0; i < MaxBodies; i += 2)
                {
                    if (_bodies[i] != null)
                    {
                        bool active = _bodies[i].IsActive();
                        _bodies[i].SetActive(!active);
                    }
                }
            }
            if (state.IsKeyDown(Keys.D) && oldState.IsKeyUp(Keys.D))
            {
                DestroyBody();
            }
        }

        public override void Step(Framework.Settings settings)
        {
            base.Step(settings);

            PolyShapesCallback callback = new PolyShapesCallback();
            callback._circle.Radius = 2.0f;
            callback._circle.Position = new Vector2(0.0f, 2.1f);
            callback._transform.SetIdentity();
            callback._debugDraw = _debugView;

            AABB aabb;
            callback._circle.ComputeAABB(out aabb, ref callback._transform);

            _world.QueryAABB(callback.ReportFixture, ref aabb);

            Color color = new Color(0.4f, 0.7f, 0.8f);
            _debugView.DrawCircle(callback._circle.Position, callback._circle.Radius, color);

            _debugView.DrawString(50, _textLine, "Press 1-5 to drop stuff");
            _textLine += 15;
            _debugView.DrawString(50, _textLine, "Press a to (de)activate some bodies");
            _textLine += 15;
            _debugView.DrawString(50, _textLine, "Press d to destroy a body");
            _textLine += 15;
        }

        internal static Test Create()
        {
            return new PolyShapesTest();
        }

        private int _bodyIndex;
        private Body[] _bodies = new Body[MaxBodies];
        private PolygonShape[] _polygons = new PolygonShape[4];
        private CircleShape _circle = new CircleShape();
    }
}