using System.Diagnostics;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class RayCastTest : Test
    {
        // This test demonstrates how to use the world ray-cast feature.
        // NOTE: we are intentionally filtering one of the polygons, therefore
        // the ray will always miss one type of polygon.

        // This callback finds the closest hit. Polygon 0 is filtered.
        private class RayCastClosestCallback
        {
            public bool _hit;
            public Vector2 _point;
            public Vector2 _normal;

            public RayCastClosestCallback()
            {
                _hit = false;
            }

            public float ReportFixture(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
            {
                int index = (int)fixture.UserData;
                if (index == 1)

                    // By returning -1, we instruct the calling code to ignore this fixture and
                    // continue the ray-cast to the next fixture.
                    return -1.0f;

                _hit = true;
                _point = point;
                _normal = normal;

                // By returning the current fraction, we instruct the calling code to clip the ray and
                // continue the ray-cast to the next fixture. WARNING: do not assume that fixtures
                // are reported in order. However, by clipping, we can always get the closest fixture.
                return fraction;
            }
        }

        // This callback finds any hit. Polygon 0 is filtered. For this type of query we are usually
        // just checking for obstruction, so the actual fixture and hit point are irrelevant. 
        private class RayCastAnyCallback
        {
            public bool _hit;
            public Vector2 _point;
            public Vector2 _normal;

            public RayCastAnyCallback()
            {
                _hit = false;
            }

            public float ReportFixture(Fixture fixture, Vector2 point, Vector2 normal, float _)
            {
                int index = (int)fixture.UserData;
                if (index == 1)

                    // By returning -1, we instruct the calling code to ignore this fixture and
                    // continue the ray-cast to the next fixture.
                    return -1.0f;

                _hit = true;
                _point = point;
                _normal = normal;

                // At this point we have a hit, so we know the ray is obstructed.
                // By returning 0, we instruct the calling code to terminate the ray-cast.
                return 0.0f;
            }
        }

        // This ray cast collects multiple hits along the ray. Polygon 0 is filtered.
        // The fixtures are not necessary reported in order, so we might not capture
        // the closest fixture.
        private class RayCastMultipleCallback
        {
            private const int _maxCount = 3;
            public readonly Vector2[] _points = new Vector2[_maxCount];
            public readonly Vector2[] _normals = new Vector2[_maxCount];
            public int _count;

            public RayCastMultipleCallback()
            {
                _count = 0;
            }

            public float ReportFixture(Fixture fixture, Vector2 point, Vector2 normal, float _)
            {
                int index = (int)fixture.UserData;
                if (index == 1)

                    // By returning -1, we instruct the calling code to ignore this fixture and
                    // continue the ray-cast to the next fixture.
                    return -1.0f;

                Debug.Assert(_count < _maxCount);

                _points[_count] = point;
                _normals[_count] = normal;
                ++_count;

                if (_count == _maxCount)

                    // At this point the buffer is full.
                    // By returning 0, we instruct the calling code to terminate the ray-cast.
                    return 0.0f;

                // By returning 1, we instruct the caller to continue without clipping the ray.
                return 1.0f;
            }
        }

        private const int _maxBodies = 256;

        private int _bodyIndex;
        private readonly Body[] _bodies = new Body[_maxBodies];
        private readonly PolygonShape[] _polygons = new PolygonShape[4];
        private CircleShape _circle;
        private EdgeShape _edge;
        private readonly float _degrees;
        private readonly Mode _mode;

        private enum Mode
        {
            Any = 0,
            Closest = 1,
            Multiple = 2
        }

        private RayCastTest()
        {
            // Ground body
            {
                BodyDef bd = new BodyDef();
                Body ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();
                shape.SetTwoSided(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.AddFixture(shape);
            }

            {
                Vertices vertices = new Vertices(3);
                vertices.Add(new Vector2(-0.5f, 0.0f));
                vertices.Add(new Vector2(0.5f, 0.0f));
                vertices.Add(new Vector2(0.0f, 1.5f));
                _polygons[0] = new PolygonShape(vertices, 1.0f);
            }

            {
                Vertices vertices = new Vertices(3);
                vertices.Add(new Vector2(-0.1f, 0.0f));
                vertices.Add(new Vector2(0.1f, 0.0f));
                vertices.Add(new Vector2(0.0f, 1.5f));
                _polygons[1] = new PolygonShape(vertices, 1.0f);
            }

            {
                float w = 1.0f;
                float b = w / (2.0f + MathUtils.Sqrt(2.0f));
                float s = MathUtils.Sqrt(2.0f) * b;

                Vertices vertices = new Vertices(8);
                vertices.Add(new Vector2(0.5f * s, 0.0f));
                vertices.Add(new Vector2(0.5f * w, b));
                vertices.Add(new Vector2(0.5f * w, b + s));
                vertices.Add(new Vector2(0.5f * s, w));
                vertices.Add(new Vector2(-0.5f * s, w));
                vertices.Add(new Vector2(-0.5f * w, b + s));
                vertices.Add(new Vector2(-0.5f * w, b));
                vertices.Add(new Vector2(-0.5f * s, 0.0f));

                _polygons[2] = new PolygonShape(vertices, 1.0f);
            }

            {
                _polygons[3] = new PolygonShape(1.0f);
                _polygons[3].SetAsBox(0.5f, 0.5f);
            }

            {
                _circle = new CircleShape(1.0f);
                _circle.Radius = 0.5f;
            }

            {
                _edge = new EdgeShape(new Vector2(-1.0f, 0.0f), new Vector2(1.0f, 0.0f));
            }

            _bodyIndex = 0;
            _degrees = 0.0f;

            _mode = Mode.Closest;
        }

        private void Create(int index)
        {
            if (_bodies[_bodyIndex] != null)
            {
                World.RemoveBody(_bodies[_bodyIndex]);
                _bodies[_bodyIndex] = null;
            }

            BodyDef bd = new BodyDef();

            float x = Rand.RandomFloat(-10.0f, 10.0f);
            float y = Rand.RandomFloat(0.0f, 20.0f);
            bd.Position = new Vector2(x, y);
            bd.Angle = Rand.RandomFloat(-MathConstants.Pi, MathConstants.Pi);

            if (index == 4)
                bd.AngularDamping = 0.02f;

            _bodies[_bodyIndex] = BodyFactory.CreateFromDef(World, bd);

            if (index < 4)
            {
                FixtureDef fd = new FixtureDef();
                fd.Shape = _polygons[index];
                fd.Friction = 0.3f;
                fd.UserData = index + 1;
                _bodies[_bodyIndex].AddFixture(fd);
            }
            else if (index < 5)
            {
                FixtureDef fd = new FixtureDef();
                fd.Shape = _circle;
                fd.Friction = 0.3f;
                fd.UserData = index + 1;
                _bodies[_bodyIndex].AddFixture(fd);
            }
            else
            {
                FixtureDef fd = new FixtureDef();
                fd.Shape = _edge;
                fd.Friction = 0.3f;
                fd.UserData = index + 1;

                _bodies[_bodyIndex].AddFixture(fd);
            }

            _bodyIndex = (_bodyIndex + 1) % _maxBodies;
        }

        private void DestroyBody()
        {
            for (int i = 0; i < _maxBodies; ++i)
                if (_bodies[i] != null)
                {
                    World.RemoveBody(_bodies[i]);
                    _bodies[i] = null;
                    return;
                }
        }

        //void UpdateUI()
        //{
        //    ImGui::SetNextWindowPos(ImVec2(10.0f, 100.0f));
        //    ImGui::SetNextWindowSize(ImVec2(210.0f, 285.0f));
        //    ImGui::Begin("Ray-cast Controls", nullptr, ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoResize);

        //    if (ImGui::Button("Shape 1"))
        //    {
        //        Create(0);
        //    }

        //    if (ImGui::Button("Shape 2"))
        //    {
        //        Create(1);
        //    }

        //    if (ImGui::Button("Shape 3"))
        //    {
        //        Create(2);
        //    }

        //    if (ImGui::Button("Shape 4"))
        //    {
        //        Create(3);
        //    }

        //    if (ImGui::Button("Shape 5"))
        //    {
        //        Create(4);
        //    }

        //    if (ImGui::Button("Shape 6"))
        //    {
        //        Create(5);
        //    }

        //    if (ImGui::Button("Destroy Shape"))
        //    {
        //        DestroyBody();
        //    }

        //    ImGui::RadioButton("Any", _mode, Any);
        //    ImGui::RadioButton("Closest", _mode, Closest);
        //    ImGui::RadioButton("Multiple", _mode, Multiple);

        //    ImGui::SliderFloat("Angle", _degrees, 0.0f, 360.0f, "%.0f");

        //    ImGui::End();
        //}

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            DrawString("Shape 1 is intentionally ignored by the ray");
            switch (_mode)
            {
                case Mode.Closest:
                    DrawString("Ray-cast mode: closest - find closest fixture along the ray");
                    break;

                case Mode.Any:
                    DrawString("Ray-cast mode: any - check for obstruction");
                    break;

                case Mode.Multiple:
                    DrawString("Ray-cast mode: multiple - gather multiple fixtures");
                    break;
            }

            float angle = MathConstants.Pi * _degrees / 180.0f;
            float L = 11.0f;
            Vector2 point1 = new Vector2(0.0f, 10.0f);
            Vector2 d = new Vector2(L * MathUtils.Cosf(angle), L * MathUtils.Sinf(angle));
            Vector2 point2 = point1 + d;

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);

            if (_mode == Mode.Closest)
            {
                RayCastClosestCallback callback = new RayCastClosestCallback();
                World.RayCast(callback.ReportFixture, point1, point2);

                if (callback._hit)
                {
                    DebugView.DrawPoint(callback._point, 5.0f, new Color(0.4f, 0.9f, 0.4f));
                    DebugView.DrawSegment(point1, callback._point, new Color(0.8f, 0.8f, 0.8f));
                    Vector2 head = callback._point + 0.5f * callback._normal;
                    DebugView.DrawSegment(callback._point, head, new Color(0.9f, 0.9f, 0.4f));
                }
                else
                {
                    DebugView.DrawSegment(point1, point2, new Color(0.8f, 0.8f, 0.8f));
                }
            }
            else if (_mode == Mode.Any)
            {
                RayCastAnyCallback callback = new RayCastAnyCallback();
                World.RayCast(callback.ReportFixture, point1, point2);

                if (callback._hit)
                {
                    DebugView.DrawPoint(callback._point, 5.0f, new Color(0.4f, 0.9f, 0.4f));
                    DebugView.DrawSegment(point1, callback._point, new Color(0.8f, 0.8f, 0.8f));
                    Vector2 head = callback._point + 0.5f * callback._normal;
                    DebugView.DrawSegment(callback._point, head, new Color(0.9f, 0.9f, 0.4f));
                }
                else
                {
                    DebugView.DrawSegment(point1, point2, new Color(0.8f, 0.8f, 0.8f));
                }
            }
            else if (_mode == Mode.Multiple)
            {
                RayCastMultipleCallback callback = new RayCastMultipleCallback();
                World.RayCast(callback.ReportFixture, point1, point2);
                DebugView.DrawSegment(point1, point2, new Color(0.8f, 0.8f, 0.8f));

                for (int i = 0; i < callback._count; ++i)
                {
                    Vector2 p = callback._points[i];
                    Vector2 n = callback._normals[i];
                    DebugView.DrawPoint(p, 5.0f, new Color(0.4f, 0.9f, 0.4f));
                    DebugView.DrawSegment(point1, p, new Color(0.8f, 0.8f, 0.8f));
                    Vector2 head = p + 0.5f * n;
                    DebugView.DrawSegment(p, head, new Color(0.9f, 0.9f, 0.4f));
                }
            }

            DebugView.EndCustomDraw();

#if false
		// This case was failing.
		{
			b2Vec2 vertices[4];
			//vertices[0].Set(-22.875f, -3.0f);
			//vertices[1].Set(22.875f, -3.0f);
			//vertices[2].Set(22.875f, 3.0f);
			//vertices[3].Set(-22.875f, 3.0f);

			PolygonShape shape = new PolygonShape();
			//shape.Set(vertices, 4);
			shape.SetAsBox(22.875f, 3.0f);

			b2RayCastInput input;
			input.p1.Set(10.2725f,1.71372f);
			input.p2.Set(10.2353f,2.21807f);
			//input.maxFraction = 0.567623f;
			input.maxFraction = 0.56762173f;

			Transform xf;
			xf.SetIdentity();
			xf.Position = new Vector2(23.0f, 5.0f);

			b2RayCastOutput output;
			bool hit;
			hit = shape.RayCast(&output, input, xf);
			hit = false;

			b2Color color(1.0f, 1.0f, 1.0f);
			b2Vec2 vs[4];
			for (int i = 0; i < 4; ++i)
			{
				vs[i] = MathUtils.Mul(xf, shape.m_vertices[i]);
			}

			DebugView.DrawPolygon(vs, 4, color);
			DebugView.DrawSegment(input.p1, input.p2, color);
		}
#endif
        }

        internal static Test Create()
        {
            return new RayCastTest();
        }
    }
}