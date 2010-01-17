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
            fixture.Body.GetTransform(out xf);

            switch (fixture.ShapeType)
            {
                case ShapeType.Circle:
                    {
                        CircleShape circle = (CircleShape)fixture.Shape;

                        Vector2 center = MathUtils.Multiply(ref xf, circle.Position);
                        float radius = circle.Radius;

                        _debugDraw.DrawCircle(center, radius, color);
                    }
                    break;

                case ShapeType.Polygon:
                    {
                        PolygonShape poly = (PolygonShape)fixture.Shape;
                        int vertexCount = poly.Vertices.Count;
                        Debug.Assert(vertexCount <= Settings.MaxPolygonVertices);
                        Vector2[] vertices = new Vector2[Settings.MaxPolygonVertices];

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

            Body body = fixture.Body;
            Shape shape = fixture.Shape;

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

        internal CircleShape _circle = new CircleShape(0, 0);
        internal Transform _transform;
        internal DebugViewXNA.DebugViewXNA _debugDraw;
        private int _count;
    }

    public class PolyShapesTest : Test
    {
        private const int MaxBodies = 256;

        private PolyShapesTest()
        {
            // Ground body
            {

                Body ground = World.CreateBody();

                Vertices edge = PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                PolygonShape shape = new PolygonShape(edge, 0);
                ground.CreateFixture(shape);
            }

            {
                Vertices vertices = new Vertices(3);
                vertices.Add(new Vector2(-0.5f, 0.0f));
                vertices.Add(new Vector2(0.5f, 0.0f));
                vertices.Add(new Vector2(0.0f, 1.5f));
                _polygons[0] = new PolygonShape(vertices, 0);
            }

            {
                Vertices vertices3 = new Vertices(3);
                vertices3.Add(new Vector2(-0.1f, 0.0f));
                vertices3.Add(new Vector2(0.1f, 0.0f));
                vertices3.Add(new Vector2(0.0f, 1.5f));
                _polygons[1] = new PolygonShape(vertices3, 0);
            }

            {
                const float w = 1.0f;
                float b = w / (2.0f + (float)Math.Sqrt(2.0));
                float s = (float)Math.Sqrt(2.0) * b;

                Vertices vertices8 = new Vertices(8);
                vertices8.Add(new Vector2(0.5f * s, 0.0f));
                vertices8.Add(new Vector2(0.5f * w, b));
                vertices8.Add(new Vector2(0.5f * w, b + s));
                vertices8.Add( new Vector2(0.5f * s, w));
                vertices8.Add( new Vector2(-0.5f * s, w));
                vertices8.Add( new Vector2(-0.5f * w, b + s));
                vertices8.Add(new Vector2(-0.5f * w, b));
                vertices8.Add( new Vector2(-0.5f * s, 0.0f));

                _polygons[2] = new PolygonShape(vertices8, 0);
            }

            {
                Vertices box = PolygonTools.CreateRectangle(0.5f, 0.5f);
                _polygons[3] = new PolygonShape(box, 0);
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
                World.DestroyBody(_bodies[_bodyIndex]);
                _bodies[_bodyIndex] = null;
            }

            _bodies[_bodyIndex] = World.CreateBody();
            _bodies[_bodyIndex].BodyType = BodyType.Dynamic;

            float x = Rand.RandomFloat(-2.0f, 2.0f);

            _bodies[_bodyIndex].Position = new Vector2(x, 10.0f);
            _bodies[_bodyIndex].Rotation = Rand.RandomFloat(-Settings.Pi, Settings.Pi);

            if (index == 4)
            {
                _bodies[_bodyIndex].AngularDamping = 0.02f;
            }


            if (index < 4)
            {
                Fixture fixture = _bodies[_bodyIndex].CreateFixture(_polygons[index]);
                fixture.Friction = 0.3f;
            }
            else
            {
                Fixture fixture = _bodies[_bodyIndex].CreateFixture(_circle);
                fixture.Friction = 0.3f;
            }

            _bodyIndex = (_bodyIndex + 1) % MaxBodies;
        }

        private void DestroyBody()
        {
            for (int i = 0; i < MaxBodies; ++i)
            {
                if (_bodies[i] != null)
                {
                    World.DestroyBody(_bodies[i]);
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
                        bool active = _bodies[i].Enabled;
                        _bodies[i].Enabled = !active;
                    }
                }
            }
            if (state.IsKeyDown(Keys.D) && oldState.IsKeyUp(Keys.D))
            {
                DestroyBody();
            }
        }

        public override void Update(Framework.Settings settings)
        {
            base.Update(settings);

            PolyShapesCallback callback = new PolyShapesCallback();
            callback._circle.Radius = 2.0f;
            callback._circle.Position = new Vector2(0.0f, 2.1f);
            callback._transform.SetIdentity();
            callback._debugDraw = DebugView;

            AABB aabb;
            callback._circle.ComputeAABB(out aabb, ref callback._transform);

            World.QueryAABB(callback.ReportFixture, ref aabb);

            Color color = new Color(0.4f, 0.7f, 0.8f);
            DebugView.DrawCircle(callback._circle.Position, callback._circle.Radius, color);

            DebugView.DrawString(50, TextLine, "Press 1-5 to drop stuff");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Press a to (de)activate some bodies");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Press d to destroy a body");
            TextLine += 15;
        }

        internal static Test Create()
        {
            return new PolyShapesTest();
        }

        private int _bodyIndex;
        private Body[] _bodies = new Body[MaxBodies];
        private PolygonShape[] _polygons = new PolygonShape[4];
        private CircleShape _circle = new CircleShape(0, 0);
    }
}