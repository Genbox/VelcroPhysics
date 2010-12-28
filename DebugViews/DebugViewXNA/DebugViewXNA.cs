using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.DebugViews
{
    /// <summary>
    /// A debug view that works in XNA.
    /// A debug view shows you what happens inside the physics engine. You can view
    /// bodies, joints, fixtures and more.
    /// </summary>
    public class DebugViewXNA : DebugView, IDisposable
    {
        //Drawing
        private VertexPositionColorTexture[] _vertsLines;
        private VertexPositionColorTexture[] _vertsFill;
        private VertexPositionColor[] _localVertsLines;
        private VertexPositionColor[] _localVertsFill;
        private int _lineCount;
        private int _fillCount;
        private int _localLineCount;
        private int _localFillCount;
        private SpriteBatch _batch;
        private SpriteFont _font;
        private GraphicsDevice _device;
        private Vector2[] _tempVertices = new Vector2[Settings.MaxPolygonVertices];
        private int _maxPrimitiveCount;
        private List<StringData> _stringData;
        private BasicEffect _effect;
        private BasicEffect _localEffect;
        private BasicEffect _texturedEffect;

        //Textured drawing
        private Dictionary<MaterialType, List<Fixture>> _texturedObjects;
        private List<Fixture> _texturedEdges;
        private Texture2D _lineTexture;
        private MaterialManager _materialManager;

        //Shapes
        public Color DefaultShapeColor = new Color(0.9f, 0.7f, 0.7f);
        public Color InactiveShapeColor = new Color(0.5f, 0.5f, 0.3f);
        public Color KinematicShapeColor = new Color(0.5f, 0.5f, 0.9f);
        public Color SleepingShapeColor = new Color(0.6f, 0.6f, 0.6f);
        public Color StaticShapeColor = new Color(0.5f, 0.9f, 0.5f);
        public Color TextColor = Color.White;
        public float DefaultLineWidth = 0.25f;

        //Contacts
        private int _pointCount;
        private const int MaxContactPoints = 2048;
        private ContactPoint[] _points = new ContactPoint[MaxContactPoints];

        //Debug panel
#if XBOX
        public Vector2 DebugPanelPosition = new Vector2(55, 100);
#else
        public Vector2 DebugPanelPosition = new Vector2(40, 100);
#endif
        private int _max;
        private int _avg;
        private int _min;

        //Performance graph
        public bool AdaptiveLimits = true;
        public int ValuesToGraph = 500;
        public int MinimumValue;
        public int MaximumValue = 1000;
        private List<float> _graphValues = new List<float>();

#if XBOX
        public Rectangle PerformancePanelBounds = new Rectangle(265, 100, 200, 100);
#else
        public Rectangle PerformancePanelBounds = new Rectangle(250, 100, 200, 100);
#endif
        private Vector2[] _background = new Vector2[4];

#if XBOX || WINDOWS_PHONE
        public const int CircleSegments = 16;
        public const bool simpleLines = true;
#else
        public const int CircleSegments = 32;
        public const bool simpleLines = false;
#endif

        public DebugViewXNA(World world)
            : base(world)
        {
            world.ContactManager.PreSolve += PreSolve;
            world.FixtureAdded += AddFixture;
            world.FixtureRemoved += RemoveFixture;

            //Default flags
            AppendFlags(DebugViewFlags.Shape);
            AppendFlags(DebugViewFlags.Joint);
        }

        #region IDisposable Members

        public void Dispose()
        {
            World.ContactManager.PreSolve -= PreSolve;
        }

        #endregion

        private void PreSolve(Contact contact, ref Manifold oldManifold)
        {
            if ((Flags & DebugViewFlags.ContactPoints) == DebugViewFlags.ContactPoints)
            {
                Manifold manifold = contact.Manifold;

                if (manifold.PointCount == 0)
                {
                    return;
                }

                Fixture fixtureA = contact.FixtureA;

                FixedArray2<PointState> state1, state2;
                Collision.Collision.GetPointStates(out state1, out state2, ref oldManifold, ref manifold);

                FixedArray2<Vector2> points;
                Vector2 normal;
                contact.GetWorldManifold(out normal, out points);

                for (int i = 0; i < manifold.PointCount && _pointCount < MaxContactPoints; ++i)
                {
                    if (fixtureA == null)
                    {
                        _points[i] = new ContactPoint();
                    }
                    ContactPoint cp = _points[_pointCount];
                    cp.Position = points[i];
                    cp.Normal = normal;
                    cp.State = state2[i];
                    _points[_pointCount] = cp;
                    ++_pointCount;
                }
            }
        }

        private void AddFixture(Fixture fixture)
        {
            if (fixture.ShapeType == ShapeType.Edge || fixture.ShapeType == ShapeType.Loop)
            {
                _texturedEdges.Add(fixture);
            }
            else if (fixture.ShapeType == ShapeType.Circle || fixture.ShapeType == ShapeType.Polygon)
            {
                if (fixture.UserData is DebugMaterial)
                {
                    DebugMaterial tempMat = (DebugMaterial)fixture.UserData;
                    if (!_texturedObjects.ContainsKey(tempMat.Type))
                    {
                        _texturedObjects[tempMat.Type] = new List<Fixture>();
                    }
                    _texturedObjects[tempMat.Type].Add(fixture);
                }
            }
        }

        private void RemoveFixture(Fixture fixture)
        {
            _texturedEdges.Remove(fixture);
            foreach (KeyValuePair<MaterialType, List<Fixture>> p in _texturedObjects)
            {
                p.Value.Remove(fixture);
            }
        }

        /// <summary>
        /// Call this to draw shapes and other debug draw data.
        /// </summary>
        private void DrawDebugData()
        {
            if ((Flags & DebugViewFlags.ContactPoints) == DebugViewFlags.ContactPoints)
            {
                const float axisScale = 0.3f;

                for (int i = 0; i < _pointCount; ++i)
                {
                    ContactPoint point = _points[i];

                    if (point.State == PointState.Add)
                    {
                        // Add
                        DrawPoint(point.Position, 0.1f, new Color(0.3f, 0.95f, 0.3f));
                    }
                    else if (point.State == PointState.Persist)
                    {
                        // Persist
                        DrawPoint(point.Position, 0.1f, new Color(0.3f, 0.3f, 0.95f));
                    }

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
                        PolygonShape polygon = f.Shape as PolygonShape;
                        if (polygon != null)
                        {
                            Transform xf;
                            body.GetTransform(out xf);

                            for (int i = 0; i < polygon.Vertices.Count; i++)
                            {
                                Vector2 tmp = MathUtils.Multiply(ref xf, polygon.Vertices[i]);
                                DrawPoint(tmp, 0.1f, Color.Red);
                            }
                        }
                    }
                }
            }

            if ((Flags & DebugViewFlags.DebugPanel) == DebugViewFlags.DebugPanel)
            {
                DrawDebugPanel();
            }

            if ((Flags & DebugViewFlags.PerformanceGraph) == DebugViewFlags.PerformanceGraph)
            {
                DrawPerformanceGraph();
            }

            if ((Flags & DebugViewFlags.Shape) == DebugViewFlags.Shape)
            {
                foreach (Body b in World.BodyList)
                {
                    Transform xf;
                    b.GetTransform(out xf);
                    foreach (Fixture f in b.FixtureList)
                    {
                        if (b.Enabled == false)
                        {
                            DrawShape(f, xf, InactiveShapeColor);
                        }
                        else if (b.BodyType == BodyType.Static)
                        {
                            DrawShape(f, xf, StaticShapeColor);
                        }
                        else if (b.BodyType == BodyType.Kinematic)
                        {
                            DrawShape(f, xf, KinematicShapeColor);
                        }
                        else if (b.Awake == false)
                        {
                            DrawShape(f, xf, SleepingShapeColor);
                        }
                        else
                        {
                            DrawShape(f, xf, DefaultShapeColor);
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

            if ((Flags & DebugViewFlags.Pair) == DebugViewFlags.Pair)
            {
                Color color = new Color(0.3f, 0.9f, 0.9f);
                for (Contact c = World.ContactManager.ContactList; c != null; c = c.Next)
                {
                    Fixture fixtureA = c.FixtureA;
                    Fixture fixtureB = c.FixtureB;

                    AABB aabbA;
                    fixtureA.GetAABB(out aabbA, 0);
                    AABB aabbB;
                    fixtureB.GetAABB(out aabbB, 0);

                    Vector2 cA = aabbA.Center;
                    Vector2 cB = aabbB.Center;

                    DrawSegment(cA, cB, color);
                }
            }

            if ((Flags & DebugViewFlags.AABB) == DebugViewFlags.AABB)
            {
                Color color = new Color(0.9f, 0.3f, 0.9f);
                BroadPhase bp = World.ContactManager.BroadPhase;

                foreach (Body b in World.BodyList)
                {
                    if (b.Enabled == false)
                    {
                        continue;
                    }

                    foreach (Fixture f in b.FixtureList)
                    {
                        for (int t = 0; t < f.ProxyCount; ++t)
                        {
                            FixtureProxy proxy = f.Proxies[t];
                            AABB aabb;
                            bp.GetFatAABB(proxy.ProxyId, out aabb);

                            DrawAABB(ref aabb, color);
                        }
                    }
                }
            }

            if ((Flags & DebugViewFlags.CenterOfMass) == DebugViewFlags.CenterOfMass)
            {
                foreach (Body b in World.BodyList)
                {
                    Transform xf;
                    b.GetTransform(out xf);
                    xf.Position = b.WorldCenter;
                    DrawTransform(ref xf);
                }
            }
        }

        private void DrawPerformanceGraph()
        {
            _graphValues.Add(World.UpdateTime);

            if (_graphValues.Count > ValuesToGraph + 1)
                _graphValues.RemoveAt(0);

            float x = PerformancePanelBounds.X;
            float deltaX = PerformancePanelBounds.Width / (float)ValuesToGraph;
            float yScale = PerformancePanelBounds.Bottom - (float)PerformancePanelBounds.Top;

            // we must have at least 2 values to start rendering
            if (_graphValues.Count > 2)
            {
                _max = (int)_graphValues.Max();
                _avg = (int)_graphValues.Average();
                _min = (int)_graphValues.Min();

                if (AdaptiveLimits)
                {
                    MaximumValue = _max;
                    MinimumValue = 0;
                }

                // start at last value (newest value added)
                // continue until no values are left
                for (int i = _graphValues.Count - 1; i > 0; i--)
                {
                    float y1 = PerformancePanelBounds.Bottom - ((_graphValues[i] / (MaximumValue - MinimumValue)) * yScale);
                    float y2 = PerformancePanelBounds.Bottom - ((_graphValues[i - 1] / (MaximumValue - MinimumValue)) * yScale);

                    Vector2 x1 = new Vector2(MathHelper.Clamp(x, PerformancePanelBounds.Left, PerformancePanelBounds.Right),
                                             MathHelper.Clamp(y1, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));

                    Vector2 x2 = new Vector2(MathHelper.Clamp(x + deltaX, PerformancePanelBounds.Left, PerformancePanelBounds.Right),
                                             MathHelper.Clamp(y2, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));

                    DrawLocalSegment(x1, x2, Color.LightGreen);

                    x += deltaX;
                }
            }

            DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Top, "Max: " + _max);
            DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Center.Y - 7, "Avg: " + _avg);
            DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Bottom - 15, "Min: " + _min);

            //Draw background.
            _background[0] = new Vector2(PerformancePanelBounds.X, PerformancePanelBounds.Y);
            _background[1] = new Vector2(PerformancePanelBounds.X, PerformancePanelBounds.Y + PerformancePanelBounds.Height);
            _background[2] = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width, PerformancePanelBounds.Y + PerformancePanelBounds.Height);
            _background[3] = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width, PerformancePanelBounds.Y);

            DrawLocalSolidPolygon(_background, 4, Color.DarkGray, true);
        }

        private void DrawDebugPanel()
        {
            int fixtures = 0;
            for (int i = 0; i < World.BodyList.Count; i++)
            {
                fixtures += World.BodyList[i].FixtureList.Count;
            }

            int x = (int)DebugPanelPosition.X;
            int y = (int)DebugPanelPosition.Y;
            const int ySize = 15;

            DrawString(x, y, "Objects: ");
            DrawString(x, y += ySize, "- Bodies: " + World.BodyList.Count);
            DrawString(x, y += ySize, "- Fixtures: " + fixtures);
            DrawString(x, y += ySize, "- Contacts: " + World.ContactCount);
            DrawString(x, y += ySize, "- Joints: " + World.JointList.Count);
            DrawString(x, y += ySize, "- Controllers: " + World.Controllers.Count);
            DrawString(x, y + ySize, "- Proxies: " + World.ProxyCount);

            y = (int)DebugPanelPosition.Y;

            DrawString(x + 110, y, "Update time: ");
            DrawString(x + 110, y += ySize, "- Body: " + World.SolveUpdateTime);
            DrawString(x + 110, y += ySize, "- Contact: " + World.ContactsUpdateTime);
            DrawString(x + 110, y += ySize, "- CCD: " + World.ContinuousPhysicsTime);
            DrawString(x + 110, y += ySize, "- Joint: " + World.Island.JointUpdateTime);
            DrawString(x + 110, y += ySize, "- Controller: " + World.ControllersUpdateTime);
            DrawString(x + 110, y + ySize, "- Total: " + World.UpdateTime);
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

        private void DrawJoint(Joint joint)
        {
            if (!joint.Enabled)
                return;

            Body b1 = joint.BodyA;
            Body b2 = joint.BodyB;
            Transform xf1, xf2;
            b1.GetTransform(out xf1);

            Vector2 x2 = Vector2.Zero;

            // WIP David
            if (!joint.IsFixedType())
            {
                b2.GetTransform(out xf2);
                x2 = xf2.Position;
            }

            Vector2 p1 = joint.WorldAnchorA;
            Vector2 p2 = joint.WorldAnchorB;
            Vector2 x1 = xf1.Position;

            Color color = new Color(0.5f, 0.8f, 0.8f);

            switch (joint.JointType)
            {
                case JointType.Distance:
                    DrawSegment(p1, p2, color);
                    break;
                case JointType.Pulley:
                    PulleyJoint pulley = (PulleyJoint)joint;
                    Vector2 s1 = pulley.GroundAnchorA;
                    Vector2 s2 = pulley.GroundAnchorB;
                    DrawSegment(s1, p1, color);
                    DrawSegment(s2, p2, color);
                    DrawSegment(s1, s2, color);
                    break;
                case JointType.FixedMouse:
                    DrawPoint(p1, 0.5f, new Color(0.0f, 1.0f, 0.0f));
                    DrawSegment(p1, p2, new Color(0.8f, 0.8f, 0.8f));
                    break;
                case JointType.Revolute:
                    //DrawSegment(x2, p1, color);
                    DrawSegment(p2, p1, color);
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
                //case JointType.Weld:
                //    break;
                default:
                    DrawSegment(x1, p1, color);
                    DrawSegment(p1, p2, color);
                    DrawSegment(x2, p2, color);
                    break;
            }
        }

        private void DrawShape(Fixture fixture, Transform xf, Color color)
        {
            switch (fixture.ShapeType)
            {
                case ShapeType.Circle:
                    {
                        CircleShape circle = (CircleShape)fixture.Shape;

                        Vector2 center = MathUtils.Multiply(ref xf, circle.Position);
                        float radius = circle.Radius;
                        Vector2 axis = xf.R.Col1;

                        DrawSolidCircle(center, radius, axis, color);
                    }
                    break;

                case ShapeType.Polygon:
                    {
                        PolygonShape poly = (PolygonShape)fixture.Shape;
                        int vertexCount = poly.Vertices.Count;
                        Debug.Assert(vertexCount <= Settings.MaxPolygonVertices);

                        for (int i = 0; i < vertexCount; ++i)
                        {
                            _tempVertices[i] = MathUtils.Multiply(ref xf, poly.Vertices[i]);
                        }

                        DrawSolidPolygon(_tempVertices, vertexCount, color);
                    }
                    break;


                case ShapeType.Edge:
                    {
                        EdgeShape edge = (EdgeShape)fixture.Shape;
                        Vector2 v1 = MathUtils.Multiply(ref xf, edge.Vertex1);
                        Vector2 v2 = MathUtils.Multiply(ref xf, edge.Vertex2);
                        DrawSegment(v1, v2, color);
                    }
                    break;

                case ShapeType.Loop:
                    {
                        LoopShape loop = (LoopShape)fixture.Shape;
                        int count = loop.Vertices.Count;

                        Vector2 v1 = MathUtils.Multiply(ref xf, loop.Vertices[count - 1]);
                        for (int i = 0; i < count; ++i)
                        {
                            Vector2 v2 = MathUtils.Multiply(ref xf, loop.Vertices[i]);
                            DrawSegment(v1, v2, color);
                            v1 = v2;
                        }
                    }
                    break;
            }
        }

        private void DrawTexturedShapes(ref Matrix projection)
        {
            _texturedEffect.Projection = projection;
            if ((Flags & DebugViewFlags.Shape) == DebugViewFlags.Shape)
            {
                _texturedEffect.Alpha = 0.5f;
            }
            else
            {
                _texturedEffect.Alpha = 1f;
            }

            foreach (KeyValuePair<MaterialType, List<Fixture>> pair in _texturedObjects)
            {
                foreach (Fixture f in pair.Value)
                {
                    if (f.Shape.ShapeType == ShapeType.Circle)
                    {
                        DrawTexturedCircle(f);
                    }
                    if (f.Shape.ShapeType == ShapeType.Polygon)
                    {
                        DrawTexturedPolygon(f);
                    }
                }
                if (_fillCount > 0)
                {
                    if (_materialManager.GetMaterialWrap(pair.Key))
                    {
                        _device.SamplerStates[0] = SamplerState.LinearWrap;
                    }
                    else
                    {
                        _device.SamplerStates[0] = SamplerState.LinearClamp;
                    }
                    _texturedEffect.Texture = _materialManager.GetMaterialTexture(pair.Key);
                    _texturedEffect.Techniques[0].Passes[0].Apply();
                    _device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertsFill, 0, _fillCount);
                }
                _fillCount = 0;
            }
            foreach (Fixture f in _texturedEdges)
            {
                DrawTexturedLineShape(f);
            }
            if (_lineCount > 0)
            {
                _texturedEffect.Texture = _lineTexture;
                _texturedEffect.Techniques[0].Passes[0].Apply();
                _device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertsLines, 0, _lineCount);
            }
            _lineCount = 0;
        }

        private void DrawTexturedLineShape(Fixture fixture)
        {
            Transform xf;
            fixture.Body.GetTransform(out xf);
            if (fixture.ShapeType == ShapeType.Edge)
            {
                EdgeShape edge = (EdgeShape)fixture.Shape;
                DrawTexturedLine(MathUtils.Multiply(ref xf, edge.Vertex1), MathUtils.Multiply(ref xf, edge.Vertex2));
            }
            else if (fixture.ShapeType == ShapeType.Loop)
            {
                Vertices loopVerts = ((LoopShape)fixture.Shape).Vertices;
                for (int i = 0; i < loopVerts.Count - 1; ++i)
                {
                    DrawTexturedLine(MathUtils.Multiply(ref xf, loopVerts[i]), MathUtils.Multiply(ref xf, loopVerts[i + 1]));
                }
                DrawTexturedLine(MathUtils.Multiply(ref xf, loopVerts[loopVerts.Count - 1]), MathUtils.Multiply(ref xf, loopVerts[0]));
            }
        }

        private void DrawLocalSegment(Vector2 start, Vector2 end, Color color)
        {
            _localVertsLines[_localLineCount * 2].Position = new Vector3(start, 0f);
            _localVertsLines[_localLineCount * 2 + 1].Position = new Vector3(end, 0f);
            _localVertsLines[_localLineCount * 2].Color = _localVertsLines[_localLineCount * 2 + 1].Color = color;
            _localLineCount++;
        }

        public void DrawLocalSolidPolygon(Vector2[] vertices, int count, Color color, bool outline)
        {
            if (count == 2)
            {
                DrawLocalPolygon(vertices, count, color);
                return;
            }

            Color colorFill = color * (outline ? 0.5f : 1.0f);

            for (int i = 1; i < count - 1; i++)
            {
                _localVertsFill[_localFillCount * 3].Position = new Vector3(vertices[0], 0f);
                _localVertsFill[_localFillCount * 3].Color = colorFill;

                _localVertsFill[_localFillCount * 3 + 1].Position = new Vector3(vertices[i], 0f);
                _localVertsFill[_localFillCount * 3 + 1].Color = colorFill;

                _localVertsFill[_localFillCount * 3 + 2].Position = new Vector3(vertices[i + 1], 0f);
                _localVertsFill[_localFillCount * 3 + 2].Color = colorFill;

                _localFillCount++;
            }

            if (outline)
            {
                DrawLocalPolygon(vertices, count, color);
            }
        }

        private void DrawLocalPolygon(Vector2[] vertices, int count, Color color)
        {
            for (int i = 0; i < count - 1; i++)
            {
                _localVertsLines[_localLineCount * 2].Position = new Vector3(vertices[i], 0f);
                _localVertsLines[_localLineCount * 2].Color = color;
                _localVertsLines[_localLineCount * 2 + 1].Position = new Vector3(vertices[i + 1], 0f);
                _localVertsLines[_localLineCount * 2 + 1].Color = color;
                _localLineCount++;
            }

            _localVertsLines[_localLineCount * 2].Position = new Vector3(vertices[count - 1], 0f);
            _localVertsLines[_localLineCount * 2].Color = color;
            _localVertsLines[_localLineCount * 2 + 1].Position = new Vector3(vertices[0], 0f);
            _localVertsLines[_localLineCount * 2 + 1].Color = color;
            _localLineCount++;
        }

        public override void DrawPolygon(Vector2[] vertices, int count, float red, float green, float blue)
        {
            DrawPolygon(vertices, count, new Color(red, green, blue));
        }

        public void DrawPolygon(Vector2[] vertices, int count, Color color)
        {
            for (int i = 0; i < count - 1; i++)
            {
                _vertsLines[_lineCount * 2].Position = new Vector3(vertices[i], -0.1f);
                _vertsLines[_lineCount * 2].Color = color;
                _vertsLines[_lineCount * 2 + 1].Position = new Vector3(vertices[i + 1], -0.1f);
                _vertsLines[_lineCount * 2 + 1].Color = color;
                _lineCount++;
            }

            _vertsLines[_lineCount * 2].Position = new Vector3(vertices[count - 1], -0.1f);
            _vertsLines[_lineCount * 2].Color = color;
            _vertsLines[_lineCount * 2 + 1].Position = new Vector3(vertices[0], -0.1f);
            _vertsLines[_lineCount * 2 + 1].Color = color;
            _lineCount++;
        }

        public override void DrawSolidPolygon(Vector2[] vertices, int count, float red, float green, float blue)
        {
            DrawSolidPolygon(vertices, count, new Color(red, green, blue), true);
        }

        public void DrawSolidPolygon(Vector2[] vertices, int count, Color color)
        {
            DrawSolidPolygon(vertices, count, color, true);
        }

        public void DrawSolidPolygon(Vector2[] vertices, int count, Color color, bool outline)
        {
            if (count == 2)
            {
                DrawPolygon(vertices, count, color);
                return;
            }

            Color colorFill = color * (outline ? 0.5f : 1.0f);

            for (int i = 1; i < count - 1; i++)
            {
                _vertsFill[_fillCount * 3].Position = new Vector3(vertices[0], -0.1f);
                _vertsFill[_fillCount * 3].Color = colorFill;

                _vertsFill[_fillCount * 3 + 1].Position = new Vector3(vertices[i], -0.1f);
                _vertsFill[_fillCount * 3 + 1].Color = colorFill;

                _vertsFill[_fillCount * 3 + 2].Position = new Vector3(vertices[i + 1], -0.1f);
                _vertsFill[_fillCount * 3 + 2].Color = colorFill;

                _fillCount++;
            }

            if (outline)
            {
                DrawPolygon(vertices, count, color);
            }
        }

        private void DrawTexturedPolygon(Fixture fixture)
        {
            Transform xf;
            fixture.Body.GetTransform(out xf);
            PolygonShape poly = (PolygonShape)fixture.Shape;
            DebugMaterial material = (DebugMaterial)fixture.UserData;
            int count = poly.Vertices.Count;

            if (count == 2)
            {
                return;
            }

            Color colorFill = material.Color;
            float depth = material.Depth;
            Vector2 texCoordCenter = new Vector2(.5f, .5f);
            if (material.CenterOnBody)
            {
                texCoordCenter += poly.Vertices[0] / material.Scale;
            }

            Vector2 v0 = MathUtils.Multiply(ref xf, poly.Vertices[0]);
            for (int i = 1; i < count - 1; i++)
            {
                Vector2 v1 = MathUtils.Multiply(ref xf, poly.Vertices[i]);
                Vector2 v2 = MathUtils.Multiply(ref xf, poly.Vertices[i + 1]);

                _vertsFill[_fillCount * 3].Position = new Vector3(v0, depth);
                _vertsFill[_fillCount * 3].Color = colorFill;
                _vertsFill[_fillCount * 3].TextureCoordinate = texCoordCenter;
                _vertsFill[_fillCount * 3].TextureCoordinate.Y = 1 - _vertsFill[_fillCount * 3].TextureCoordinate.Y;

                _vertsFill[_fillCount * 3 + 1].Position = new Vector3(v1, depth);
                _vertsFill[_fillCount * 3 + 1].Color = colorFill;
                _vertsFill[_fillCount * 3 + 1].TextureCoordinate = texCoordCenter + (poly.Vertices[i] - poly.Vertices[0]) / material.Scale;
                _vertsFill[_fillCount * 3 + 1].TextureCoordinate.Y = 1 - _vertsFill[_fillCount * 3 + 1].TextureCoordinate.Y;

                _vertsFill[_fillCount * 3 + 2].Position = new Vector3(v2, depth);
                _vertsFill[_fillCount * 3 + 2].Color = colorFill;
                _vertsFill[_fillCount * 3 + 2].TextureCoordinate = texCoordCenter + (poly.Vertices[i + 1] - poly.Vertices[0]) / material.Scale;
                _vertsFill[_fillCount * 3 + 2].TextureCoordinate.Y = 1 - _vertsFill[_fillCount * 3 + 2].TextureCoordinate.Y;

                _fillCount++;

                // outline
                DrawTexturedLine(v1, v2);
            }
            DrawTexturedLine(v0, MathUtils.Multiply(ref xf, poly.Vertices[1]));
            DrawTexturedLine(MathUtils.Multiply(ref xf, poly.Vertices[count - 1]), v0);
        }

        public override void DrawCircle(Vector2 center, float radius, float red, float green, float blue)
        {
            DrawCircle(center, radius, new Color(red, green, blue));
        }

        public void DrawCircle(Vector2 center, float radius, Color color)
        {
            const double increment = Math.PI * 2.0 / CircleSegments;
            double theta = 0.0;

            for (int i = 0; i < CircleSegments; i++)
            {
                Vector2 v1 = center + radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                Vector2 v2 = center +
                             radius *
                             new Vector2((float)Math.Cos(theta + increment), (float)Math.Sin(theta + increment));

                _vertsLines[_lineCount * 2].Position = new Vector3(v1, -0.1f);
                _vertsLines[_lineCount * 2].Color = color;
                _vertsLines[_lineCount * 2 + 1].Position = new Vector3(v2, -0.1f);
                _vertsLines[_lineCount * 2 + 1].Color = color;
                _lineCount++;

                theta += increment;
            }
        }

        public override void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, float red, float green,
                                             float blue)
        {
            DrawSolidCircle(center, radius, axis, new Color(red, green, blue));
        }

        public void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, Color color)
        {
            const double increment = Math.PI * 2.0 / CircleSegments;
            double theta = 0.0;

            Color colorFill = color * 0.5f;

            Vector2 v0 = center + radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
            theta += increment;

            for (int i = 1; i < CircleSegments - 1; i++)
            {
                Vector2 v1 = center + radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                Vector2 v2 = center +
                             radius *
                             new Vector2((float)Math.Cos(theta + increment), (float)Math.Sin(theta + increment));

                _vertsFill[_fillCount * 3].Position = new Vector3(v0, -0.1f);
                _vertsFill[_fillCount * 3].Color = colorFill;

                _vertsFill[_fillCount * 3 + 1].Position = new Vector3(v1, -0.1f);
                _vertsFill[_fillCount * 3 + 1].Color = colorFill;

                _vertsFill[_fillCount * 3 + 2].Position = new Vector3(v2, -0.1f);
                _vertsFill[_fillCount * 3 + 2].Color = colorFill;

                _fillCount++;

                theta += increment;
            }
            DrawCircle(center, radius, color);

            DrawSegment(center, center + axis * radius, color);
        }

        private void DrawTexturedCircle(Fixture fixture)
        {
            Transform xf;
            fixture.Body.GetTransform(out xf);
            CircleShape circle = (CircleShape)fixture.Shape;
            DebugMaterial material = (DebugMaterial)fixture.UserData;
            Vector2 center = circle.Position;
            float radius = circle.Radius;

            const double increment = Math.PI * 2.0 / CircleSegments;
            double theta = increment;

            Color colorFill = material.Color;
            float depth = material.Depth;
            Vector2 v0 = center + radius * Vector2.UnitX;
            Vector2 texCoordCenter = new Vector2(.5f, .5f) + Vector2.UnitX * radius / material.Scale;
            if (material.CenterOnBody)
            {
                texCoordCenter += center / material.Scale;
            }

            for (int i = 1; i < CircleSegments - 1; i++)
            {
                Vector2 v1 = center + radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                Vector2 v2 = center + radius * new Vector2((float)Math.Cos(theta + increment), (float)Math.Sin(theta + increment));

                _vertsFill[_fillCount * 3].Position = new Vector3(MathUtils.Multiply(ref xf, v0), depth);
                _vertsFill[_fillCount * 3].Color = colorFill;
                _vertsFill[_fillCount * 3].TextureCoordinate = texCoordCenter;
                _vertsFill[_fillCount * 3].TextureCoordinate.Y = 1 - _vertsFill[_fillCount * 3].TextureCoordinate.Y;

                _vertsFill[_fillCount * 3 + 1].Position = new Vector3(MathUtils.Multiply(ref xf, v1), depth);
                _vertsFill[_fillCount * 3 + 1].Color = colorFill;
                _vertsFill[_fillCount * 3 + 1].TextureCoordinate = texCoordCenter + (v1 - v0) / material.Scale;
                _vertsFill[_fillCount * 3 + 1].TextureCoordinate.Y = 1 - _vertsFill[_fillCount * 3 + 1].TextureCoordinate.Y;

                _vertsFill[_fillCount * 3 + 2].Position = new Vector3(MathUtils.Multiply(ref xf, v2), depth);
                _vertsFill[_fillCount * 3 + 2].Color = colorFill;
                _vertsFill[_fillCount * 3 + 2].TextureCoordinate = texCoordCenter + (v2 - v0) / material.Scale;
                _vertsFill[_fillCount * 3 + 2].TextureCoordinate.Y = 1 - _vertsFill[_fillCount * 3 + 2].TextureCoordinate.Y;

                _fillCount++;
                theta += increment;

                // outline
                DrawTexturedLine(MathUtils.Multiply(ref xf, v1), MathUtils.Multiply(ref xf, v2));
                if (i == 1)
                {
                    DrawTexturedLine(MathUtils.Multiply(ref xf, v0), MathUtils.Multiply(ref xf, v1));
                }
                if (i == CircleSegments - 2)
                {
                    DrawTexturedLine(MathUtils.Multiply(ref xf, v2), MathUtils.Multiply(ref xf, v0));
                }
            }
        }

        private void DrawTexturedLine(Vector2 a, Vector2 b)
        {
            Vector2 tang = b - a;
            tang.Normalize();
            tang *= DefaultLineWidth / 2f;
            Vector2 norm = new Vector2(-tang.Y, tang.X);

            VertexPositionColorTexture[] vertsLine;
            // define vertices
            if (simpleLines)
            {
                vertsLine = new VertexPositionColorTexture[4];
            }
            else
            {
                vertsLine = new VertexPositionColorTexture[8];
            }
            vertsLine[0].Position = new Vector3(a - tang + norm, -0.2f);
            vertsLine[0].Color = Color.Black;
            vertsLine[1].Position = new Vector3(a - tang - norm, -0.2f);
            vertsLine[1].Color = Color.Black;
            vertsLine[2].Position = new Vector3(b + tang - norm, -0.2f);
            vertsLine[2].Color = Color.Black;
            vertsLine[3].Position = new Vector3(b + tang + norm, -0.2f);
            vertsLine[3].Color = Color.Black;
            if (simpleLines)
            {
                vertsLine[0].TextureCoordinate = new Vector2(.25f, .25f);
                vertsLine[1].TextureCoordinate = new Vector2(.25f, .75f);
                vertsLine[2].TextureCoordinate = new Vector2(.75f, .75f);
                vertsLine[3].TextureCoordinate = new Vector2(.75f, .25f);
            }
            else
            {
                vertsLine[0].TextureCoordinate = new Vector2(0f, .25f);
                vertsLine[1].TextureCoordinate = new Vector2(0f, .75f);
                vertsLine[2].TextureCoordinate = new Vector2(1f, .75f);
                vertsLine[3].TextureCoordinate = new Vector2(1f, .25f);
                vertsLine[4].Position = new Vector3(a + norm, -0.2f);
                vertsLine[4].Color = Color.Black;
                vertsLine[4].TextureCoordinate = new Vector2(.25f, .25f);
                vertsLine[5].Position = new Vector3(a - norm, -0.2f);
                vertsLine[5].Color = Color.Black;
                vertsLine[5].TextureCoordinate = new Vector2(.25f, .75f);
                vertsLine[6].Position = new Vector3(b - norm, -0.2f);
                vertsLine[6].Color = Color.Black;
                vertsLine[6].TextureCoordinate = new Vector2(.75f, .75f);
                vertsLine[7].Position = new Vector3(b + norm, -0.2f);
                vertsLine[7].Color = Color.Black;
                vertsLine[7].TextureCoordinate = new Vector2(.75f, .25f);
            }

            // add triangles
            if (simpleLines)
            {
                _vertsLines[_lineCount * 3] = vertsLine[0];
                _vertsLines[_lineCount * 3 + 1] = vertsLine[1];
                _vertsLines[_lineCount * 3 + 2] = vertsLine[3];
                _lineCount++;
                _vertsLines[_lineCount * 3] = vertsLine[1];
                _vertsLines[_lineCount * 3 + 1] = vertsLine[2];
                _vertsLines[_lineCount * 3 + 2] = vertsLine[3];
                _lineCount++;
            }
            else
            {
                _vertsLines[_lineCount * 3] = vertsLine[0];
                _vertsLines[_lineCount * 3 + 1] = vertsLine[1];
                _vertsLines[_lineCount * 3 + 2] = vertsLine[4];
                _lineCount++;
                _vertsLines[_lineCount * 3] = vertsLine[1];
                _vertsLines[_lineCount * 3 + 1] = vertsLine[5];
                _vertsLines[_lineCount * 3 + 2] = vertsLine[4];
                _lineCount++;
                _vertsLines[_lineCount * 3] = vertsLine[4];
                _vertsLines[_lineCount * 3 + 1] = vertsLine[5];
                _vertsLines[_lineCount * 3 + 2] = vertsLine[7];
                _lineCount++;
                _vertsLines[_lineCount * 3] = vertsLine[5];
                _vertsLines[_lineCount * 3 + 1] = vertsLine[6];
                _vertsLines[_lineCount * 3 + 2] = vertsLine[7];
                _lineCount++;
                _vertsLines[_lineCount * 3] = vertsLine[7];
                _vertsLines[_lineCount * 3 + 1] = vertsLine[6];
                _vertsLines[_lineCount * 3 + 2] = vertsLine[3];
                _lineCount++;
                _vertsLines[_lineCount * 3] = vertsLine[6];
                _vertsLines[_lineCount * 3 + 1] = vertsLine[2];
                _vertsLines[_lineCount * 3 + 2] = vertsLine[3];
                _lineCount++;
            }
        }

        public override void DrawSegment(Vector2 start, Vector2 end, float red, float green, float blue)
        {
            DrawSegment(start, end, new Color(red, green, blue));
        }

        public void DrawSegment(Vector2 start, Vector2 end, Color color)
        {
            _vertsLines[_lineCount * 2].Position = new Vector3(start, -0.1f);
            _vertsLines[_lineCount * 2 + 1].Position = new Vector3(end, -0.1f);
            _vertsLines[_lineCount * 2].Color = _vertsLines[_lineCount * 2 + 1].Color = color;
            _lineCount++;
        }

        public override void DrawTransform(ref Transform transform)
        {
            const float axisScale = 0.4f;
            Vector2 p1 = transform.Position;

            Vector2 p2 = p1 + axisScale * transform.R.Col1;
            DrawSegment(p1, p2, Color.Red);

            p2 = p1 + axisScale * transform.R.Col2;
            DrawSegment(p1, p2, Color.Green);
        }

        public void DrawPoint(Vector2 p, float size, Color color)
        {
            Vector2[] verts = new Vector2[4];
            float hs = size / 2.0f;
            verts[0] = p + new Vector2(-hs, -hs);
            verts[1] = p + new Vector2(hs, -hs);
            verts[2] = p + new Vector2(hs, hs);
            verts[3] = p + new Vector2(-hs, hs);

            DrawSolidPolygon(verts, 4, color, true);
        }

        public void DrawString(int x, int y, string s, params object[] args)
        {
            _stringData.Add(new StringData(x, y, s, args, TextColor));
        }

        public void DrawArrow(Vector2 start, Vector2 end, float length, float width, bool drawStartIndicator,
                              Color color)
        {
            // Draw connection segment between start- and end-point
            DrawSegment(start, end, color);

            // Precalculate halfwidth
            float halfWidth = width / 2;

            // Create directional reference
            Vector2 rotation = (start - end);
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

        public void RenderDebugData(ref Matrix projection)
        {
            //Nothing is enabled - don't draw the debug view.
            if (Flags == 0)
                return;

            _device.RasterizerState = RasterizerState.CullNone;
            _device.DepthStencilState = DepthStencilState.Default;

            if ((Flags & DebugViewFlags.TexturedShape) == DebugViewFlags.TexturedShape)
                DrawTexturedShapes(ref projection);

            DrawDebugData();

            _localEffect.Projection = Matrix.CreateOrthographicOffCenter(0f, _device.Viewport.Width,
                                                                         _device.Viewport.Height, 0f, 0f, 1f);

            //Graph stuff first
            _localEffect.Techniques[0].Passes[0].Apply();

            if (_localFillCount > 0)
                _device.DrawUserPrimitives(PrimitiveType.TriangleList, _localVertsFill, 0, _localFillCount);

            if (_localLineCount > 0)
                _device.DrawUserPrimitives(PrimitiveType.LineList, _localVertsLines, 0, _localLineCount);

            // set the effects projection matrix
            _effect.Projection = projection;

            // we should have only 1 technique and 1 pass
            _effect.Techniques[0].Passes[0].Apply();

            // make sure we have stuff to draw
            if (_fillCount > 0)
                _device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertsFill, 0, _fillCount);

            // make sure we have lines to draw
            if (_lineCount > 0)
                _device.DrawUserPrimitives(PrimitiveType.LineList, _vertsLines, 0, _lineCount);

            // begin the sprite batch effect
            _batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // draw any strings we have
            for (int i = 0; i < _stringData.Count; i++)
            {
                _batch.DrawString(_font, string.Format(_stringData[i].S, _stringData[i].Args),
                                  new Vector2(_stringData[i].X, _stringData[i].Y), _stringData[i].Color);
            }
            // end the sprite batch effect
            _batch.End();

            _stringData.Clear();
            _lineCount = _fillCount = _localLineCount = _localFillCount = 0;
        }

        public void RenderDebugData(ref Matrix projection, ref Matrix view)
        {
            _effect.View = view;
            _texturedEffect.View = view;
            RenderDebugData(ref projection);
        }

        public void LoadContent(GraphicsDevice device, ContentManager content)
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _batch = new SpriteBatch(device);
            _font = content.Load<SpriteFont>("font");
            _device = device;

            _effect = new BasicEffect(device);
            _effect.VertexColorEnabled = true;

            _localEffect = new BasicEffect(device);
            _localEffect.VertexColorEnabled = true;

            _texturedEffect = new BasicEffect(device);
            _texturedEffect.VertexColorEnabled = true;
            _texturedEffect.TextureEnabled = true;

            _stringData = new List<StringData>();

            _maxPrimitiveCount = device.GraphicsProfile == GraphicsProfile.Reach ? 65535 : 1048575;
            _vertsLines = new VertexPositionColorTexture[_maxPrimitiveCount];
            _vertsFill = new VertexPositionColorTexture[_maxPrimitiveCount];

            _materialManager = new MaterialManager();
            _materialManager.LoadContent(content);

            _lineTexture = content.Load<Texture2D>("Materials/line");
            _texturedObjects = new Dictionary<MaterialType, List<Fixture>>();
            _texturedEdges = new List<Fixture>();

            //For now, the local vertices are only used by the graph.
            _localVertsLines = new VertexPositionColor[10000];
            _localVertsFill = new VertexPositionColor[10000];
        }

        public void Update(GameTime gameTime)
        {
            _materialManager.Update(gameTime);
        }

        #region Nested type: ContactPoint

        private struct ContactPoint
        {
            public Vector2 Normal;
            public Vector2 Position;
            public PointState State;
        }

        #endregion

        #region Nested type: StringData

        private struct StringData
        {
            public object[] Args;
            public Color Color;
            public string S;
            public int X, Y;

            public StringData(int x, int y, string s, object[] args, Color color)
            {
                X = x;
                Y = y;
                S = s;
                Args = args;
                Color = color;
            }
        }

        #endregion
    }
}
