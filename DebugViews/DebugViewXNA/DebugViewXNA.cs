using System;
using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Controllers.Buoyancy;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.DebugViewXNA
{
    public class DebugViewXNA : DebugView
    {
        private static VertexPositionColor[] _vertsLines;
        private static VertexPositionColor[] _vertsFill;
        private static int _lineCount;
        private static int _fillCount;
        public static SpriteBatch Batch;
        public static SpriteFont Font;
        public static GraphicsDevice Device;

        private static List<StringData> _stringData;
        private static VertexDeclaration _vertexDeclaration;
        private static BasicEffect _effect;

        public DebugViewXNA(World world)
            : base(world)
        {
            _vertsLines = new VertexPositionColor[1000000];
            _vertsFill = new VertexPositionColor[1000000];
        }

        /// Call this to draw shapes and other debug draw data.
        public void DrawDebugData()
        {
            if ((Flags & DebugViewFlags.Shape) == DebugViewFlags.Shape)
            {
                for (Body b = World._bodyList; b != null; b = b.GetNext())
                {
                    Transform xf;
                    b.GetTransform(out xf);
                    for (Fixture f = b._fixtureList; f != null; f = f._next)
                    {
                        if (b.Enabled == false)
                        {
                            DrawShape(f, xf, new Color(0.5f, 0.5f, 0.3f));
                        }
                        else if (b.BodyType == BodyType.Static)
                        {
                            DrawShape(f, xf, new Color(0.5f, 0.9f, 0.5f));
                        }
                        else if (b.BodyType == BodyType.Kinematic)
                        {
                            DrawShape(f, xf, new Color(0.5f, 0.5f, 0.9f));
                        }
                        else if (b.Awake == false)
                        {
                            DrawShape(f, xf, new Color(0.6f, 0.6f, 0.6f));
                        }
                        else
                        {
                            DrawShape(f, xf, new Color(0.9f, 0.7f, 0.7f));
                        }
                    }
                }
            }

            if ((Flags & DebugViewFlags.Joint) == DebugViewFlags.Joint)
            {
                for (Joint j = World._jointList; j != null; j = j.GetNext())
                {
                    DrawJoint(j);
                }
            }

            if ((Flags & DebugViewFlags.Pair) == DebugViewFlags.Pair)
            {
                Color color = new Color(0.3f, 0.9f, 0.9f);
                for (Contact c = World.ContactManager._contactList; c != null; c = c.GetNext())
                {
                    Fixture fixtureA = c.FixtureA;
                    Fixture fixtureB = c.FixtureB;

                    AABB aabbA;
                    fixtureA.GetAABB(out aabbA, 1);
                    AABB aabbB;
                    fixtureB.GetAABB(out aabbB, 1);

                    Vector2 cA = aabbA.Center;
                    Vector2 cB = aabbB.Center;

                    DrawSegment(cA, cB, color);
                }
            }

            if ((Flags & DebugViewFlags.AABB) == DebugViewFlags.AABB)
            {
                Color color = new Color(0.9f, 0.3f, 0.9f);
                BroadPhase bp = World.ContactManager.BroadPhase;

                for (Body b = World._bodyList; b != null; b = b.GetNext())
                {
                    if (b.Enabled == false)
                    {
                        continue;
                    }

                    for (Fixture f = b._fixtureList; f != null; f = f._next)
                    {

                        for (int t = 0; t < f._proxyCount; ++t)
                        {
                            FixtureProxy proxy = f._proxies[t];
                            AABB aabb;
                            bp.GetFatAABB(proxy.ProxyId, out aabb);
                            Vector2[] vs = new Vector2[4];
                            vs[0] = new Vector2(aabb.LowerBound.X, aabb.LowerBound.Y);
                            vs[1] = new Vector2(aabb.UpperBound.X, aabb.LowerBound.Y);
                            vs[2] = new Vector2(aabb.UpperBound.X, aabb.UpperBound.Y);
                            vs[3] = new Vector2(aabb.LowerBound.X, aabb.UpperBound.Y);

                            DrawPolygon(ref vs, 4, color);
                        }
                    }
                }
            }

            if ((Flags & DebugViewFlags.CenterOfMass) == DebugViewFlags.CenterOfMass)
            {
                for (Body b = World._bodyList; b != null; b = b.GetNext())
                {

                    Transform xf;
                    b.GetTransform(out xf);
                    xf.Position = b.WorldCenter;
                    DrawTransform(ref xf);
                }
            }
        }

        //TODO: Visualize controllers in general
        public void DrawWaveContainer(WaveContainer waveContainer)
        {
            for (int i = 0; i < waveContainer.NodeCount; i++)
            {
                DrawCircle(
                    new Vector2(waveContainer.XPosition[i],
                                waveContainer.Position.Y + waveContainer.Height + waveContainer.YPosition[i]), 0.1f,
                    Color.Red);
            }
        }

        private void DrawJoint(Joint joint)
        {
            Body b1 = joint.BodyA;
            Body b2 = joint.BodyB;
            Transform xf1, xf2;
            b1.GetTransform(out xf1);

            Vector2 x2 = new Vector2();
            Vector2 p2 = new Vector2();

            // WIP David
            if (!joint.IsFixedType())
            {
                b2.GetTransform(out xf2);
                x2 = xf2.Position;
            }
            p2 = joint.WorldAnchorB;

            Vector2 x1 = xf1.Position;

            Vector2 p1 = joint.WorldAnchorA;

            Color color = new Color(0.5f, 0.8f, 0.8f);

            switch (joint.JointType)
            {
                case JointType.Distance:
                    DrawSegment(p1, p2, color);
                    break;

                case JointType.Pulley:
                    {
                        PulleyJoint pulley = (PulleyJoint)joint;
                        Vector2 s1 = pulley.GroundAnchorA;
                        Vector2 s2 = pulley.GroundAnchorB;
                        DrawSegment(s1, p1, color);
                        DrawSegment(s2, p2, color);
                        DrawSegment(s1, s2, color);
                    }
                    break;

                case JointType.FixedMouse:
                    // don't draw this
                    break;
                case JointType.Revolute:
                    //DrawSegment(x2, p1, color);
                    DrawSegment(p2, p1, color);
                    DrawSolidCircle(p2, 0.1f, new Vector2(), Color.Red);
                    DrawSolidCircle(p1, 0.1f, new Vector2(), Color.Blue);
                    break;
                case JointType.FixedRevolute:
                    DrawSegment(x1, p1, color);
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
                    //DrawSegment(x1, p1, color);
                    //DrawSegment(p1, p2, color);
                    break;
                default:
                    DrawSegment(x1, p1, color);
                    DrawSegment(p1, p2, color);
                    DrawSegment(x2, p2, color);
                    break;
            }
        }

        private void DrawShape(Fixture fixture, Transform xf, Color color)
        {
            switch (fixture.Shape.ShapeType)
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
                        Vector2[] vertices = new Vector2[Settings.MaxPolygonVertices];

                        for (int i = 0; i < vertexCount; ++i)
                        {
                            vertices[i] = MathUtils.Multiply(ref xf, poly.Vertices[i]);
                        }

                        DrawSolidPolygon(ref vertices, vertexCount, color);
                    }
                    break;


                case ShapeType.Edge:
                    {
                        EdgeShape edge = (EdgeShape)fixture.Shape;
                        Vector2 v1 = MathUtils.Multiply(ref xf, edge._vertex1);
                        Vector2 v2 = MathUtils.Multiply(ref xf, edge._vertex2);
                        DrawSegment(v1, v2, color);
                    }
                    break;

                case ShapeType.Loop:
                    {
                        LoopShape loop = (LoopShape)fixture.Shape;
                        int count = loop._count;

                        Vector2 v1 = MathUtils.Multiply(ref xf, loop._vertices[count - 1]);
                        for (int i = 0; i < count; ++i)
                        {
                            Vector2 v2 = MathUtils.Multiply(ref xf, loop._vertices[i]);
                            DrawSegment(v1, v2, color);
                            v1 = v2;
                        }
                    }
                    break;
            }
        }

        public override void DrawPolygon(ref Vector2[] vertices, int count, float red, float green, float blue)
        {
            DrawPolygon(ref vertices, count, new Color(red, green, blue));
        }

        public void DrawPolygon(ref Vector2[] vertices, int count, Color color)
        {
            for (int i = 0; i < count - 1; i++)
            {
                _vertsLines[_lineCount * 2].Position = new Vector3(vertices[i], 0.0f);
                _vertsLines[_lineCount * 2].Color = color;
                _vertsLines[_lineCount * 2 + 1].Position = new Vector3(vertices[i + 1], 0.0f);
                _vertsLines[_lineCount * 2 + 1].Color = color;
                _lineCount++;
            }

            _vertsLines[_lineCount * 2].Position = new Vector3(vertices[count - 1], 0.0f);
            _vertsLines[_lineCount * 2].Color = color;
            _vertsLines[_lineCount * 2 + 1].Position = new Vector3(vertices[0], 0.0f);
            _vertsLines[_lineCount * 2 + 1].Color = color;
            _lineCount++;
        }

        public override void DrawSolidPolygon(ref Vector2[] vertices, int count, float red, float green, float blue)
        {
            DrawSolidPolygon(ref vertices, count, new Color(red, green, blue), true);
        }

        public void DrawSolidPolygon(ref Vector2[] vertices, int count, Color color)
        {
            DrawSolidPolygon(ref vertices, count, color, true);
        }

        public void DrawSolidPolygon(ref Vector2[] vertices, int count, Color color, bool outline)
        {
            if (count == 2)
            {
                DrawPolygon(ref vertices, count, color);
                return;
            }

            Color colorFill = new Color(color, outline ? 0.5f : 1.0f);

            for (int i = 1; i < count - 1; i++)
            {
                _vertsFill[_fillCount * 3].Position = new Vector3(vertices[0], 0.0f);
                _vertsFill[_fillCount * 3].Color = colorFill;

                _vertsFill[_fillCount * 3 + 1].Position = new Vector3(vertices[i], 0.0f);
                _vertsFill[_fillCount * 3 + 1].Color = colorFill;

                _vertsFill[_fillCount * 3 + 2].Position = new Vector3(vertices[i + 1], 0.0f);
                _vertsFill[_fillCount * 3 + 2].Color = colorFill;

                _fillCount++;
            }

            if (outline)
            {
                DrawPolygon(ref vertices, count, color);
            }
        }

        public override void DrawCircle(Vector2 center, float radius, float red, float green, float blue)
        {
            DrawCircle(center, radius, new Color(red, green, blue));
        }

        public void DrawCircle(Vector2 center, float radius, Color color)
        {
            const int segments = 32;
            const double increment = Math.PI * 2.0 / segments;
            double theta = 0.0;

            for (int i = 0; i < segments; i++)
            {
                Vector2 v1 = center + radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                Vector2 v2 = center +
                             radius *
                             new Vector2((float)Math.Cos(theta + increment), (float)Math.Sin(theta + increment));

                _vertsLines[_lineCount * 2].Position = new Vector3(v1, 0.0f);
                _vertsLines[_lineCount * 2].Color = color;
                _vertsLines[_lineCount * 2 + 1].Position = new Vector3(v2, 0.0f);
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
            const int segments = 32;
            const double increment = Math.PI * 2.0 / segments;
            double theta = 0.0;

            Color colorFill = new Color(color, 0.5f);

            Vector2 v0 = center + radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
            theta += increment;

            for (int i = 1; i < segments - 1; i++)
            {
                Vector2 v1 = center + radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                Vector2 v2 = center +
                             radius *
                             new Vector2((float)Math.Cos(theta + increment), (float)Math.Sin(theta + increment));

                _vertsFill[_fillCount * 3].Position = new Vector3(v0, 0.0f);
                _vertsFill[_fillCount * 3].Color = colorFill;

                _vertsFill[_fillCount * 3 + 1].Position = new Vector3(v1, 0.0f);
                _vertsFill[_fillCount * 3 + 1].Color = colorFill;

                _vertsFill[_fillCount * 3 + 2].Position = new Vector3(v2, 0.0f);
                _vertsFill[_fillCount * 3 + 2].Color = colorFill;

                _fillCount++;

                theta += increment;
            }
            DrawCircle(center, radius, color);

            DrawSegment(center, center + axis * radius, color);
        }

        public override void DrawSegment(Vector2 p1, Vector2 p2, float red, float green, float blue)
        {
            DrawSegment(p1, p2, new Color(red, green, blue));
        }

        public void DrawSegment(Vector2 p1, Vector2 p2, Color color)
        {
            _vertsLines[_lineCount * 2].Position = new Vector3(p1, 0.0f);
            _vertsLines[_lineCount * 2 + 1].Position = new Vector3(p2, 0.0f);
            _vertsLines[_lineCount * 2].Color = _vertsLines[_lineCount * 2 + 1].Color = color;
            _lineCount++;
        }

        public override void DrawTransform(ref Transform xf)
        {
            const float axisScale = 0.4f;
            Vector2 p1 = xf.Position;

            Vector2 p2 = p1 + axisScale * xf.R.Col1;
            DrawSegment(p1, p2, Color.Red);

            p2 = p1 + axisScale * xf.R.Col2;
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

            DrawSolidPolygon(ref verts, 4, color, true);
        }

        public void DrawString(int x, int y, string s, params object[] args)
        {
            _stringData.Add(new StringData(x, y, s, args));
        }

        public void RenderDebugData(ref Matrix projection)
        {
            // set the cull mode? should be unnecessary
            Device.RenderState.CullMode = CullMode.None;
            // turn alpha blending on
            Device.RenderState.AlphaBlendEnable = true;
            // set the vertex declaration...this ensures if window resizes occur...rendering continues ;)
            Device.VertexDeclaration = _vertexDeclaration;
            // set the effects projection matrix
            _effect.Projection = projection;
            // begin the effect
            _effect.Begin();
            // we should have only 1 technique and 1 pass
            _effect.Techniques[0].Passes[0].Begin();
            // make sure we have stuff to draw
            if (_fillCount > 0)
                Device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertsFill, 0, _fillCount);
            // make sure we have lines to draw
            if (_lineCount > 0)
                Device.DrawUserPrimitives(PrimitiveType.LineList, _vertsLines, 0, _lineCount);

            // end the pass and effect
            _effect.Techniques[0].Passes[0].End();
            _effect.End();

            // begin the sprite batch effect
            Batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None, Matrix.Identity);
            // draw any strings we have
            for (int i = 0; i < _stringData.Count; i++)
            {
                Batch.DrawString(Font, string.Format(_stringData[i].S, _stringData[i].Args),
                                 new Vector2(_stringData[i].X, _stringData[i].Y), _stringData[i].Color);
            }
            // end the sprite batch effect
            Batch.End();

            _stringData.Clear();
            _lineCount = _fillCount = 0;
        }

        public void RenderDebugData(ref Matrix projection, ref Matrix view)
        {
            // set the cull mode? should be unnecessary
            Device.RenderState.CullMode = CullMode.None;
            // turn alpha blending on
            Device.RenderState.AlphaBlendEnable = true;
            // set the vertex declaration...this ensures if window resizes occur...rendering continues ;)
            Device.VertexDeclaration = _vertexDeclaration;
            // set the effects projection matrix
            _effect.Projection = projection;
            // set the effects view matrix
            _effect.View = view;

            // begin the effect
            _effect.Begin();
            // we should have only 1 technique and 1 pass
            _effect.Techniques[0].Passes[0].Begin();
            // make sure we have stuff to draw
            if (_fillCount > 0)
                Device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertsFill, 0, _fillCount);
            // make sure we have lines to draw
            if (_lineCount > 0)
                Device.DrawUserPrimitives(PrimitiveType.LineList, _vertsLines, 0, _lineCount);

            // end the pass and effect
            _effect.Techniques[0].Passes[0].End();
            _effect.End();

            // begin the sprite batch effect
            Batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None, Matrix.Identity);
            // draw any strings we have
            for (int i = 0; i < _stringData.Count; i++)
            {
                Batch.DrawString(Font, string.Format(_stringData[i].S, _stringData[i].Args),
                                 new Vector2(_stringData[i].X, _stringData[i].Y), _stringData[i].Color);
            }
            // end the sprite batch effect
            Batch.End();

            _stringData.Clear();
            _lineCount = _fillCount = 0;
        }

        public void DrawAABB(ref AABB aabb, Color color)
        {
            Vector2[] verts = new Vector2[4];
            verts[0] = new Vector2(aabb.LowerBound.X, aabb.LowerBound.Y);
            verts[1] = new Vector2(aabb.UpperBound.X, aabb.LowerBound.Y);
            verts[2] = new Vector2(aabb.UpperBound.X, aabb.UpperBound.Y);
            verts[3] = new Vector2(aabb.LowerBound.X, aabb.UpperBound.Y);

            DrawPolygon(ref verts, 4, color);
        }

        public static void LoadContent(GraphicsDevice device, ContentManager content)
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            Batch = new SpriteBatch(device);
            Font = content.Load<SpriteFont>("font");
            _vertexDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);
            Device = device;
            _effect = new BasicEffect(device, null);
            _effect.VertexColorEnabled = true;
            _stringData = new List<StringData>();
        }

        #region Nested type: StringData

        private struct StringData
        {
            public object[] Args;
            public Color Color;
            public string S;
            public int X, Y;

            public StringData(int x, int y, string s, object[] args)
            {
                X = x;
                Y = y;
                S = s;
                Args = args;
                Color = new Color(0.9f, 0.6f, 0.6f);
            }

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