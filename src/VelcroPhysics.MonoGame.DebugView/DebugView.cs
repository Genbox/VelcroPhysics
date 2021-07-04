using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Genbox.VelcroPhysics.Collision.Broadphase;
using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Collision.Narrowphase;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Dynamics.Joints.Misc;
using Genbox.VelcroPhysics.Extensions.Controllers.Buoyancy;
using Genbox.VelcroPhysics.Extensions.Controllers.ControllerBase;
using Genbox.VelcroPhysics.Extensions.DebugView;
using Genbox.VelcroPhysics.Interfaces;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Shared.Optimization;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.DebugView
{
    /// <summary>
    /// A debug view shows you what happens inside the physics engine. You can view
    /// bodies, joints, fixtures and more.
    /// </summary>
    public class DebugView : DebugViewBase, IDisposable, IDebugView
    {
        //Drawing
        private PrimitiveBatch _primitiveBatch;
        private SpriteBatch _batch;
        private SpriteFont _font;
        private GraphicsDevice _device;
        private readonly Vector2[] _tempVertices = new Vector2[Settings.MaxPolygonVertices];
        private List<StringData> _stringData;

        private Matrix _localProjection;
        private Matrix _localView;

        private readonly LinkedList<float> _graphValues = new LinkedList<float>();
        private readonly Vector2[] _background = new Vector2[4];

        //Contacts
        private int _pointCount;

        private const int _maxContactPoints = 2048;
        private readonly ContactPoint[] _points = new ContactPoint[_maxContactPoints];

        //Performance graph
        private float _min;
        private float _max;
        private float _avg;

        private readonly StringBuilder _debugPanelSb = new StringBuilder();

        //Shape colors
        public Color DefaultShapeColor = new Color(0.9f, 0.7f, 0.7f);

        public Color InactiveShapeColor = new Color(0.5f, 0.5f, 0.3f);
        public Color KinematicShapeColor = new Color(0.5f, 0.5f, 0.9f);
        public Color SleepingShapeColor = new Color(0.6f, 0.6f, 0.6f);
        public Color StaticShapeColor = new Color(0.5f, 0.9f, 0.5f);
        public Color TextColor = Color.White;

        //Debug panel
        public Vector2 DebugPanelPosition = new Vector2(55, 100);

        public bool AdaptiveLimits = true;
        public int ValuesToGraph = 500;
        public float MinimumValue;
        public float MaximumValue = 10;
        public bool Enabled = true;
        public Rectangle PerformancePanelBounds = new Rectangle(430, 100, 200, 100);

        public bool HighPrecisionCounters;
        public const int CircleSegments = 32;

        private const float _circleIncrement = MathConstants.TwoPi / CircleSegments;

        public DebugView(World world)
            : base(world)
        {
            world.ContactManager.PreSolve += PreSolve;

            //Default flags
            AppendFlags(DebugViewFlags.Shape);
            AppendFlags(DebugViewFlags.Controllers);
            AppendFlags(DebugViewFlags.Joint);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                World.ContactManager.PreSolve -= PreSolve;
                _primitiveBatch.Dispose();
                _batch.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void PreSolve(Contact contact, ref Manifold oldManifold)
        {
            if ((Flags & DebugViewFlags.ContactPoints) == DebugViewFlags.ContactPoints)
            {
                Manifold manifold = contact.Manifold;

                if (manifold.PointCount == 0)
                    return;

                Fixture fixtureA = contact.FixtureA;

                Collision.Narrowphase.Collision.GetPointStates(out _, out FixedArray2<PointState> state2, ref oldManifold, ref manifold);

                contact.GetWorldManifold(out Vector2 normal, out FixedArray2<Vector2> points);

                for (int i = 0; i < manifold.PointCount && _pointCount < _maxContactPoints; ++i)
                {
                    if (fixtureA == null)
                        _points[i] = new ContactPoint();

                    ContactPoint cp = _points[_pointCount];
                    cp.Position = points[i];
                    cp.Normal = normal;
                    cp.State = state2[i];
                    _points[_pointCount] = cp;
                    ++_pointCount;
                }
            }
        }

        /// <summary>
        /// Call this to draw shapes and other debug draw data.
        /// </summary>
        private void DrawDebugData()
        {
            if ((Flags & DebugViewFlags.Shape) == DebugViewFlags.Shape)
            {
                foreach (Body b in World.BodyList)
                {
                    b.GetTransform(out Transform xf);
                    foreach (Fixture f in b.FixtureList)
                    {
                        Shape shape = f.Shape;

                        if (!b.Enabled)
                            DrawShape(shape, ref xf, InactiveShapeColor);
                        else if (b.BodyType == BodyType.Static)
                            DrawShape(shape, ref xf, StaticShapeColor);
                        else if (b.BodyType == BodyType.Kinematic)
                            DrawShape(shape, ref xf, KinematicShapeColor);
                        else if (!b.Awake)
                            DrawShape(shape, ref xf, SleepingShapeColor);
                        else
                            DrawShape(shape, ref xf, DefaultShapeColor);
                    }
                }
            }

            if ((Flags & DebugViewFlags.ContactPoints) == DebugViewFlags.ContactPoints)
            {
                const float axisScale = 0.3f;

                for (int i = 0; i < _pointCount; ++i)
                {
                    ContactPoint point = _points[i];

                    if (point.State == PointState.Add)
                        DrawPoint(point.Position, 0.1f, new Color(0.3f, 0.95f, 0.3f));
                    else if (point.State == PointState.Persist)
                        DrawPoint(point.Position, 0.1f, new Color(0.3f, 0.3f, 0.95f));

                    if ((Flags & DebugViewFlags.ContactNormals) == DebugViewFlags.ContactNormals)
                    {
                        Vector2 p1 = point.Position;
                        Vector2 p2 = p1 + axisScale * point.Normal;
                        DrawSegment(p1, p2, new Color(0.4f, 0.9f, 0.4f));
                    }
                }

                _pointCount = 0;
            }

            if ((Flags & DebugViewFlags.PolygonPoints) == DebugViewFlags.PolygonPoints)
            {
                foreach (Body body in World.BodyList)
                {
                    foreach (Fixture f in body.FixtureList)
                    {
                        if (f.Shape is PolygonShape polygon)
                        {
                            body.GetTransform(out Transform xf);

                            for (int i = 0; i < polygon.Vertices.Count; i++)
                            {
                                Vector2 tmp = MathUtils.Mul(ref xf, polygon.Vertices[i]);
                                DrawPoint(tmp, 0.1f, Color.Red);
                            }
                        }
                    }
                }
            }

            if ((Flags & DebugViewFlags.Joint) == DebugViewFlags.Joint)
            {
                foreach (Joint j in World.JointList)
                {
                    DrawJoint(j);
                }
            }

            if ((Flags & DebugViewFlags.AABB) == DebugViewFlags.AABB)
            {
                Color color = new Color(0.9f, 0.3f, 0.9f);
                IBroadPhase bp = World.ContactManager.BroadPhase;

                foreach (Body body in World.BodyList)
                {
                    if (!body.Enabled)
                        continue;

                    foreach (Fixture f in body.FixtureList)
                    {
                        for (int t = 0; t < f.ProxyCount; ++t)
                        {
                            FixtureProxy proxy = f.Proxies[t];
                            bp.GetFatAABB(proxy.ProxyId, out AABB aabb);

                            DrawAABB(ref aabb, color);
                        }
                    }
                }
            }

            if ((Flags & DebugViewFlags.CenterOfMass) == DebugViewFlags.CenterOfMass)
            {
                foreach (Body b in World.BodyList)
                {
                    b.GetTransform(out Transform xf);
                    xf.p = b.WorldCenter;
                    DrawTransform(ref xf);
                }
            }

            if ((Flags & DebugViewFlags.Controllers) == DebugViewFlags.Controllers)
            {
                for (int i = 0; i < World.ControllerList.Count; i++)
                {
                    Controller controller = World.ControllerList[i];

                    if (controller is BuoyancyController buoyancy)
                    {
                        AABB container = buoyancy.Container;
                        DrawAABB(ref container, Color.LightBlue);
                    }
                }
            }

            if ((Flags & DebugViewFlags.DebugPanel) == DebugViewFlags.DebugPanel)
                DrawDebugPanel();
        }

        private void DrawPerformanceGraph()
        {
            _graphValues.AddLast(World.Profile.Step / (float)TimeSpan.TicksPerMillisecond);

            if (_graphValues.Count > ValuesToGraph + 1)
                _graphValues.RemoveFirst();

            float x = PerformancePanelBounds.X;
            float deltaX = PerformancePanelBounds.Width / (float)ValuesToGraph;
            float yScale = PerformancePanelBounds.Bottom - (float)PerformancePanelBounds.Top;

            // we must have at least 2 values to start rendering
            if (_graphValues.Count > 2)
            {
                _min = float.MaxValue;
                _max = 0;
                _avg = 0;

                foreach (float val in _graphValues)
                {
                    _min = MathUtils.Min(_min, val);
                    _max = MathUtils.Max(_max, val);
                    _avg += val;
                }

                _avg /= _graphValues.Count;

                if (AdaptiveLimits)
                {
                    MaximumValue = _max;
                    MinimumValue = 0;
                }

                // start at the last value (newest value added) and go back until no values are left
                LinkedListNode<float> current = _graphValues.Last;
                LinkedListNode<float> previous = _graphValues.Last.Previous;

                while (previous != null)
                {
                    float y1 = PerformancePanelBounds.Bottom - current.Value / (MaximumValue - MinimumValue) * yScale;
                    float y2 = PerformancePanelBounds.Bottom - previous.Value / (MaximumValue - MinimumValue) * yScale;

                    Vector2 x1 = new Vector2(MathHelper.Clamp(x, PerformancePanelBounds.Left, PerformancePanelBounds.Right), MathHelper.Clamp(y1, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));
                    Vector2 x2 = new Vector2(MathHelper.Clamp(x + deltaX, PerformancePanelBounds.Left, PerformancePanelBounds.Right), MathHelper.Clamp(y2, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));

                    DrawSegment(x1, x2, Color.LightGreen);

                    x += deltaX;

                    current = current.Previous;
                    previous = current?.Previous;
                }
            }

            DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Top, $"Max: {_max} ms");
            DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Center.Y - 7, $"Avg: {_avg} ms");
            DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Bottom - 15, $"Min: {_min} ms");

            //Draw background.
            _background[0] = new Vector2(PerformancePanelBounds.X, PerformancePanelBounds.Y);
            _background[1] = new Vector2(PerformancePanelBounds.X, PerformancePanelBounds.Y + PerformancePanelBounds.Height);
            _background[2] = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width, PerformancePanelBounds.Y + PerformancePanelBounds.Height);
            _background[3] = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width, PerformancePanelBounds.Y);

            DrawSolidPolygon(_background, 4, Color.DarkGray);
        }

        private void DrawDebugPanel()
        {
            int fixtureCount = 0;
            for (int i = 0; i < World.BodyList.Count; i++)
            {
                fixtureCount += World.BodyList[i].FixtureList.Count;
            }

            int x = (int)DebugPanelPosition.X;
            int y = (int)DebugPanelPosition.Y;

            _debugPanelSb.Clear();
            _debugPanelSb.AppendLine("Objects:");
            _debugPanelSb.Append("- Bodies: ").AppendLine(World.BodyList.Count.ToString());
            _debugPanelSb.Append("- Fixtures: ").AppendLine(fixtureCount.ToString());
            _debugPanelSb.Append("- Contacts: ").AppendLine(World.ContactManager.ContactCount.ToString());
            _debugPanelSb.Append("- Joints: ").AppendLine(World.JointList.Count.ToString());
            _debugPanelSb.Append("- Controllers: ").AppendLine(World.ControllerList.Count.ToString());
            _debugPanelSb.Append("- Proxies: ").AppendLine(World.ProxyCount.ToString());
            DrawString(x, y, _debugPanelSb.ToString());

            _debugPanelSb.Clear();
            _debugPanelSb.AppendLine("Update time:");

            string msStr = " ms";

            ref Profile profile = ref World.Profile;

            if (HighPrecisionCounters)
            {
                _debugPanelSb.Append("- Solve init: ").Append(profile.SolveInit / (double)TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Solve: ").Append(profile.Solve / (double)TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Solve velocity: ").Append(profile.SolveVelocity / (double)TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Solve position: ").Append(profile.SolvePosition / (double)TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- New contacts: ").Append(profile.NewContactsTime / (double)TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Broadphase: ").Append(profile.Broadphase / (double)TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Collide: ").Append(profile.Collide / (double)TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- CCD: ").Append(profile.SolveTOI / (double)TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Add/Remove: ").Append(profile.AddRemoveTime / (double)TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Controller: ").Append(profile.ControllersUpdateTime / (double)TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Breakable bodies: ").Append(profile.BreakableBodies / (double)TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Total: ").Append(profile.Step / (double)TimeSpan.TicksPerMillisecond).AppendLine(msStr);
            }
            else
            {
                _debugPanelSb.Append("- Solve init: ").Append(profile.SolveInit / TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Solve: ").Append(profile.Solve / TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Solve velocity: ").Append(profile.SolveVelocity / TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Solve position: ").Append(profile.SolvePosition / TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- New contacts: ").Append(profile.NewContactsTime / TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Broadphase: ").Append(profile.Broadphase / TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Collide: ").Append(profile.Collide / TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- CCD: ").Append(profile.SolveTOI / TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Add/Remove: ").Append(profile.AddRemoveTime / TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Controller: ").Append(profile.ControllersUpdateTime / TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Breakable bodies: ").Append(profile.BreakableBodies / TimeSpan.TicksPerMillisecond).AppendLine(msStr);
                _debugPanelSb.Append("- Total: ").Append(profile.Step / TimeSpan.TicksPerMillisecond).AppendLine(msStr);
            }

            DrawString(x + 140, y, _debugPanelSb.ToString());
        }

        public void DrawAABB(ref AABB aabb, Color color)
        {
            Vector2[] verts = new Vector2[4];
            verts[0] = new Vector2(aabb.LowerBound.X, aabb.LowerBound.Y);
            verts[1] = new Vector2(aabb.UpperBound.X, aabb.LowerBound.Y);
            verts[2] = new Vector2(aabb.UpperBound.X, aabb.UpperBound.Y);
            verts[3] = new Vector2(aabb.LowerBound.X, aabb.UpperBound.Y);

            DrawPolygon(verts, 4, color);
        }

        public void DrawJoint(Joint joint)
        {
            if (!joint.Enabled)
                return;

            Body bA = joint.BodyA;
            Body bB = joint.BodyB;
            bA.GetTransform(out Transform xfA);
            Vector2 x1 = xfA.p;

            Vector2 x2 = Vector2.Zero;
            Transform xfB = default;

            if (!joint.IsFixedType())
            {
                bB.GetTransform(out xfB);
                x2 = xfB.p;
            }

            Vector2 p1 = joint.WorldAnchorA;
            Vector2 p2 = joint.WorldAnchorB;

            Color color = new Color(0.5f, 0.8f, 0.8f);

            if (joint is PrismaticJoint pj)
            {
                Vector2 pA = MathUtils.Mul(ref xfA, pj.LocalAnchorA);
                Vector2 pB = MathUtils.Mul(ref xfB, pj.LocalAnchorB);

                Vector2 axis = MathUtils.Mul(ref xfA.q, pj.LocalXAxisA);

                Color c1 = new Color(0.7f, 0.7f, 0.7f);
                Color c2 = new Color(0.3f, 0.9f, 0.3f);
                Color c3 = new Color(0.9f, 0.3f, 0.3f);
                Color c4 = new Color(0.3f, 0.3f, 0.9f);
                Color c5 = new Color(0.4f, 0.4f, 0.4f);

                DrawSegment(pA, pB, c5);

                if (pj.LimitEnabled)
                {
                    Vector2 lower = pA + pj.LowerLimit * axis;
                    Vector2 upper = pA + pj.UpperLimit * axis;
                    Vector2 perp = MathUtils.Mul(xfA.q, pj.LocalYAxisA);
                    DrawSegment(lower, upper, c1);
                    DrawSegment(lower - 0.5f * perp, lower + 0.5f * perp, c2);
                    DrawSegment(upper - 0.5f * perp, upper + 0.5f * perp, c3);
                }
                else
                {
                    DrawSegment(pA - 1.0f * axis, pA + 1.0f * axis, c1);
                }

                DrawPoint(pA, 1.0f, c1);
                DrawPoint(pB, 1.0f, c4);
            }
            else if (joint is WheelJoint wj)
            {
                Vector2 pA = MathUtils.Mul(ref xfA, wj.LocalAnchorA);
                Vector2 pB = MathUtils.Mul(ref xfB, wj.LocalAnchorB);

                Vector2 axis = MathUtils.Mul(xfA.q, wj.LocalXAxisA);

                Color c1 = new Color(0.7f, 0.7f, 0.7f);
                Color c2 = new Color(0.3f, 0.9f, 0.3f);
                Color c3 = new Color(0.9f, 0.3f, 0.3f);
                Color c4 = new Color(0.3f, 0.3f, 0.9f);
                Color c5 = new Color(0.4f, 0.4f, 0.4f);

                DrawSegment(pA, pB, c5);

                if (wj.EnableLimit)
                {
                    Vector2 lower = pA + wj.LowerLimit * axis;
                    Vector2 upper = pA + wj.UpperLimit * axis;
                    Vector2 perp = MathUtils.Mul(xfA.q, wj.LocalYAxisA);
                    DrawSegment(lower, upper, c1);
                    DrawSegment(lower - 0.5f * perp, lower + 0.5f * perp, c2);
                    DrawSegment(upper - 0.5f * perp, upper + 0.5f * perp, c3);
                }
                else
                {
                    DrawSegment(pA - 1.0f * axis, pA + 1.0f * axis, c1);
                }

                DrawPoint(pA, 0.1f, c1);
                DrawPoint(pB, 0.1f, c4);
            }
            else
            {
                switch (joint.JointType)
                {
                    case JointType.Distance:
                        DrawSegment(p1, p2, color);
                        break;
                    case JointType.Pulley:
                        PulleyJoint pulley = (PulleyJoint)joint;
                        Vector2 s1 = bA.GetWorldPoint(pulley.LocalAnchorA);
                        Vector2 s2 = bB.GetWorldPoint(pulley.LocalAnchorB);
                        DrawSegment(p1, p2, color);
                        DrawSegment(p1, s1, color);
                        DrawSegment(p2, s2, color);
                        break;
                    case JointType.FixedMouse:
                        DrawPoint(p1, 0.5f, new Color(0.0f, 1.0f, 0.0f));
                        DrawSegment(p1, p2, new Color(0.8f, 0.8f, 0.8f));
                        break;
                    case JointType.Revolute:
                        DrawSegment(x1, p1, color);
                        DrawSegment(p1, p2, color);
                        DrawSegment(x2, p2, color);

                        DrawSolidCircle(p2, 0.1f, Vector2.Zero, Color.Red);
                        DrawSolidCircle(p1, 0.1f, Vector2.Zero, Color.Blue);
                        break;
                    case JointType.FixedAngle:

                        //Should not draw anything.
                        break;
                    case JointType.FixedRevolute:
                        DrawSegment(x1, p1, color);
                        DrawSolidCircle(p1, 0.1f, Vector2.Zero, Color.Pink);
                        break;
                    case JointType.FixedLine:
                        DrawSegment(x1, p1, color);
                        DrawSegment(p1, p2, color);
                        break;
                    case JointType.FixedDistance:
                        DrawSegment(x1, p1, color);
                        DrawSegment(p1, p2, color);
                        break;
                    case JointType.FixedPrismatic:
                        DrawSegment(x1, p1, color);
                        DrawSegment(p1, p2, color);
                        break;
                    case JointType.Gear:
                        DrawSegment(x1, x2, color);
                        break;
                    default:
                        DrawSegment(x1, p1, color);
                        DrawSegment(p1, p2, color);
                        DrawSegment(x2, p2, color);
                        break;
                }
            }
        }

        public void DrawShape(Shape shape, ref Transform transform, Color color)
        {
            switch (shape.ShapeType)
            {
                case ShapeType.Circle:
                    {
                        CircleShape circle = (CircleShape)shape;

                        Vector2 center = MathUtils.Mul(ref transform, circle.Position);
                        float radius = circle.Radius;
                        Vector2 axis = MathUtils.Mul(transform.q, new Vector2(1.0f, 0.0f));

                        DrawSolidCircle(center, radius, axis, color);
                    }
                    break;

                case ShapeType.Polygon:
                    {
                        PolygonShape poly = (PolygonShape)shape;
                        int vertexCount = poly.Vertices.Count;
                        Debug.Assert(vertexCount <= Settings.MaxPolygonVertices);

                        for (int i = 0; i < vertexCount; ++i)
                        {
                            _tempVertices[i] = MathUtils.Mul(ref transform, poly.Vertices[i]);
                        }

                        DrawSolidPolygon(_tempVertices, vertexCount, color);
                    }
                    break;

                case ShapeType.Edge:
                    {
                        EdgeShape edge = (EdgeShape)shape;
                        Vector2 v1 = MathUtils.Mul(ref transform, edge.Vertex1);
                        Vector2 v2 = MathUtils.Mul(ref transform, edge.Vertex2);
                        DrawSegment(v1, v2, color);
                    }
                    break;

                case ShapeType.Chain:
                    {
                        ChainShape chain = (ChainShape)shape;

                        for (int i = 0; i < chain.Vertices.Count - 1; ++i)
                        {
                            Vector2 v1 = MathUtils.Mul(ref transform, chain.Vertices[i]);
                            Vector2 v2 = MathUtils.Mul(ref transform, chain.Vertices[i + 1]);
                            DrawSegment(v1, v2, color);
                        }
                    }
                    break;
            }
        }

        public override void DrawPolygon(Vector2[] vertices, int count, Color color, bool closed = true)
        {
            if (!_primitiveBatch.IsReady())
                throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");

            for (int i = 0; i < count - 1; i++)
            {
                _primitiveBatch.AddVertex(vertices[i], color, PrimitiveType.LineList);
                _primitiveBatch.AddVertex(vertices[i + 1], color, PrimitiveType.LineList);
            }
            if (closed)
            {
                _primitiveBatch.AddVertex(vertices[count - 1], color, PrimitiveType.LineList);
                _primitiveBatch.AddVertex(vertices[0], color, PrimitiveType.LineList);
            }
        }

        public override void DrawSolidPolygon(Vector2[] vertices, int count, Color color, bool outline = true)
        {
            if (!_primitiveBatch.IsReady())
                throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");

            if (count == 2)
            {
                DrawPolygon(vertices, count, color);
                return;
            }

            Color colorFill = color * (outline ? 0.5f : 1.0f);

            for (int i = 1; i < count - 1; i++)
            {
                _primitiveBatch.AddVertex(vertices[0], colorFill, PrimitiveType.TriangleList);
                _primitiveBatch.AddVertex(vertices[i], colorFill, PrimitiveType.TriangleList);
                _primitiveBatch.AddVertex(vertices[i + 1], colorFill, PrimitiveType.TriangleList);
            }

            if (outline)
                DrawPolygon(vertices, count, color);
        }

        public override void DrawCircle(Vector2 center, float radius, Color color)
        {
            if (!_primitiveBatch.IsReady())
                throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");

            Rot rot = new Rot(_circleIncrement);
            Vector2 v2 = Vector2.UnitX * radius;

            for (int i = 0; i < CircleSegments; i++)
            {
                Vector2 v1 = v2;
                v2 = MathUtils.Mul(ref rot, v1);

                _primitiveBatch.AddVertex(center + v1, color, PrimitiveType.LineList);
                _primitiveBatch.AddVertex(center + v2, color, PrimitiveType.LineList);
            }
        }

        public override void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, Color color)
        {
            if (!_primitiveBatch.IsReady())
                throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");

            Rot rot = new Rot(_circleIncrement);
            Vector2 v0 = Vector2.UnitX * radius;
            Vector2 v2 = MathUtils.Mul(ref rot, v0);

            Color colorFill = color * 0.5f;

            for (int i = 1; i < CircleSegments - 1; i++)
            {
                Vector2 v1 = v2;
                v2 = MathUtils.Mul(ref rot, v1);

                _primitiveBatch.AddVertex(center + v0, colorFill, PrimitiveType.TriangleList);
                _primitiveBatch.AddVertex(center + v1, colorFill, PrimitiveType.TriangleList);
                _primitiveBatch.AddVertex(center + v2, colorFill, PrimitiveType.TriangleList);
            }

            DrawCircle(center, radius, color);
            DrawSegment(center, center + axis * radius, color);
        }

        public override void DrawSegment(Vector2 start, Vector2 end, Color color)
        {
            if (!_primitiveBatch.IsReady())
                throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");

            _primitiveBatch.AddVertex(start, color, PrimitiveType.LineList);
            _primitiveBatch.AddVertex(end, color, PrimitiveType.LineList);
        }

        public override void DrawTransform(ref Transform transform)
        {
            const float axisScale = 0.4f;
            Vector2 p1 = transform.p;

            Vector2 p2 = p1 + axisScale * transform.q.GetXAxis();
            DrawSegment(p1, p2, Color.Red);

            p2 = p1 + axisScale * transform.q.GetYAxis();
            DrawSegment(p1, p2, Color.Green);
        }

        public void DrawPoint(Vector2 position, float size, Color color)
        {
            Vector2[] verts = new Vector2[4];
            float hs = size / 20.0f;
            verts[0] = position + new Vector2(-hs, -hs);
            verts[1] = position + new Vector2(hs, -hs);
            verts[2] = position + new Vector2(hs, hs);
            verts[3] = position + new Vector2(-hs, hs);

            DrawSolidPolygon(verts, 4, color);
        }

        public void DrawString(int x, int y, string text)
        {
            DrawString(new Vector2(x, y), text);
        }

        public void DrawString(Vector2 position, string text)
        {
            _stringData.Add(new StringData(position, text, TextColor));
        }

        public void DrawArrow(Vector2 start, Vector2 end, float length, float width, bool drawStartIndicator, Color color)
        {
            // Draw connection segment between start- and end-point
            DrawSegment(start, end, color);

            // Precalculate halfwidth
            float halfWidth = width / 2;

            // Create directional reference
            Vector2 rotation = start - end;
            rotation.Normalize();

            // Calculate angle of directional vector
            float angle = (float)Math.Atan2(rotation.X, -rotation.Y);

            // Create matrix for rotation
            Matrix rotMatrix = Matrix.CreateRotationZ(angle);

            // Create translation matrix for end-point
            Matrix endMatrix = Matrix.CreateTranslation(end.X, end.Y, 0);

            // Setup arrow end shape
            Vector2[] verts = new Vector2[3];
            verts[0] = new Vector2(0, 0);
            verts[1] = new Vector2(-halfWidth, -length);
            verts[2] = new Vector2(halfWidth, -length);

            // Rotate end shape
            Vector2.Transform(verts, ref rotMatrix, verts);

            // Translate end shape
            Vector2.Transform(verts, ref endMatrix, verts);

            // Draw arrow end shape
            DrawSolidPolygon(verts, 3, color, false);

            if (drawStartIndicator)
            {
                // Create translation matrix for start
                Matrix startMatrix = Matrix.CreateTranslation(start.X, start.Y, 0);

                // Setup arrow start shape
                Vector2[] baseVerts = new Vector2[4];
                baseVerts[0] = new Vector2(-halfWidth, length / 4);
                baseVerts[1] = new Vector2(halfWidth, length / 4);
                baseVerts[2] = new Vector2(halfWidth, 0);
                baseVerts[3] = new Vector2(-halfWidth, 0);

                // Rotate start shape
                Vector2.Transform(baseVerts, ref rotMatrix, baseVerts);

                // Translate start shape
                Vector2.Transform(baseVerts, ref startMatrix, baseVerts);

                // Draw start shape
                DrawSolidPolygon(baseVerts, 4, color, false);
            }
        }

        public void BeginCustomDraw(Matrix projection, Matrix view)
        {
            BeginCustomDraw(ref projection, ref view);
        }

        public void BeginCustomDraw(ref Matrix projection, ref Matrix view)
        {
            _primitiveBatch.Begin(ref projection, ref view);
        }

        public void EndCustomDraw()
        {
            _primitiveBatch.End();
        }

        public void RenderDebugData(Matrix projection, Matrix view)
        {
            RenderDebugData(ref projection, ref view);
        }

        public void RenderDebugData(ref Matrix projection, ref Matrix view)
        {
            if (!Enabled)
                return;

            //Nothing is enabled - don't draw the debug view.
            if (Flags == 0)
                return;

            _device.RasterizerState = RasterizerState.CullNone;
            _device.DepthStencilState = DepthStencilState.Default;

            _primitiveBatch.Begin(ref projection, ref view);
            DrawDebugData();
            _primitiveBatch.End();

            if ((Flags & DebugViewFlags.PerformanceGraph) == DebugViewFlags.PerformanceGraph)
            {
                _primitiveBatch.Begin(ref _localProjection, ref _localView);
                DrawPerformanceGraph();
                _primitiveBatch.End();
            }

            // begin the sprite batch effect
            _batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // draw any strings we have
            for (int i = 0; i < _stringData.Count; i++)
            {
                _batch.DrawString(_font, _stringData[i].Text, _stringData[i].Position, _stringData[i].Color);
            }

            // end the sprite batch effect
            _batch.End();

            _stringData.Clear();
        }

        public void RenderDebugData(ref Matrix projection)
        {
            if (!Enabled)
                return;

            Matrix view = Matrix.Identity;
            RenderDebugData(ref projection, ref view);
        }

        public void LoadContent(GraphicsDevice device, ContentManager content)
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _device = device;
            _batch = new SpriteBatch(_device);
            _primitiveBatch = new PrimitiveBatch(_device, 1000);
            _font = content.Load<SpriteFont>("Font");
            _stringData = new List<StringData>();

            _localProjection = Matrix.CreateOrthographicOffCenter(0f, _device.Viewport.Width, _device.Viewport.Height, 0f, 0f, 1f);
            _localView = Matrix.Identity;
        }

        private struct ContactPoint
        {
            public Vector2 Normal;
            public Vector2 Position;
            public PointState State;
        }

        private struct StringData
        {
            public readonly Color Color;
            public readonly string Text;
            public readonly Vector2 Position;

            public StringData(Vector2 position, string text, Color color)
            {
                Position = position;
                Text = text;
                Color = color;
            }
        }
    }
}