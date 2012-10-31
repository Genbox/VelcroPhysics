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

using System;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Math;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
// If this is an XNA project then we use math from the XNA framework.
#if XNA
#endif

namespace Farseer3TestBed
{
    public class PolygonBrush
    {
        private bool filled;
        private int segments = 24;
        private VertexPositionColor[] vertices;

        public PolygonBrush(Vector2[] verts, int numOfVerts, Color color, bool solid)
        {
            segments = numOfVerts;
            vertices = new VertexPositionColor[segments + 1];

            for (int i = 0; i < segments; i++)
            {
                vertices[i] = new VertexPositionColor(new Vector3(verts[i], 0.0f), color);
            }
            vertices[segments] = new VertexPositionColor(new Vector3(verts[0], 0.0f), color);
            filled = solid;
        }

        public void Draw(ref GraphicsDevice device)
        {
            if (filled)
            {
                device.DrawUserPrimitives(PrimitiveType.TriangleFan, vertices,
                                          0, segments - 1);
                for (int i = 0; i < segments + 1; i++)
                {
                    vertices[i].Color = Color.Black;
                }
                device.DrawUserPrimitives(PrimitiveType.LineStrip, vertices,
                                          0, segments);
            }
            else
                device.DrawUserPrimitives(PrimitiveType.LineStrip, vertices,
                                          0, segments);
        }
    }

    public class CircleBrushDefinition
    {
        private static CircleBrushDefinition _instance;
        public int Segments = 24;
        public Vector2[] Vertices;

        private CircleBrushDefinition()
        {
            // initalize a set of points that define a circle
            Vertices = new Vector2[Segments];
            float k_increment = 2.0f*MathHelper.Pi/Segments;
            float theta = 0.0f;

            for (int i = 0; i < Segments; ++i)
            {
                Vertices[i] = 1.0f*new Vector2((float) Math.Cos(theta), (float) Math.Sin(theta));

                theta += k_increment;
            }
        }

        public static CircleBrushDefinition Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CircleBrushDefinition();
                }
                return _instance;
            }
        }
    }

    /// <summary>
    /// Uses a precreated circle scaled to render circles very fast.
    /// </summary>
    public class CircleBrush
    {
        private CircleBrush()
        {
        }

        public CircleBrush(Vector2 position, float radius, Color color, bool filled)
        {
            Position = position;
            Radius = radius;
            Color = color;
            Filled = filled;
        }

        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public Color Color { get; set; }
        public bool Filled { get; set; }

        public void Draw(ref GraphicsDevice device)
        {
            VertexPositionColor[] verts = new VertexPositionColor[CircleBrushDefinition.Instance.Segments + 1];

            for (int i = 0; i < CircleBrushDefinition.Instance.Segments; i++)
            {
                verts[i] =
                    new VertexPositionColor(
                        new Vector3((CircleBrushDefinition.Instance.Vertices[i]*Radius) + Position, 0.0f), Color);
            }

            verts[CircleBrushDefinition.Instance.Segments] =
                new VertexPositionColor(
                    new Vector3((CircleBrushDefinition.Instance.Vertices[0]*Radius) + Position, 0.0f), Color);

            if (Filled)
            {
                device.DrawUserPrimitives(PrimitiveType.TriangleFan, verts,
                                          0, CircleBrushDefinition.Instance.Segments - 1);
                for (int i = 0; i < CircleBrushDefinition.Instance.Segments + 1; i++)
                {
                    verts[i].Color = Color.Black;
                }
                device.DrawUserPrimitives(PrimitiveType.LineStrip, verts,
                                          0, CircleBrushDefinition.Instance.Segments);
            }
            else
            {
                for (int i = 0; i < CircleBrushDefinition.Instance.Segments + 1; i++)
                {
                    verts[i].Color = Color.Black;
                }
                device.DrawUserPrimitives(PrimitiveType.LineStrip, verts,
                                          0, CircleBrushDefinition.Instance.Segments);
            }
        }
    }

    public class XNADebugRenderer : DebugDraw
    {
        private CircleBrush[] _circleBrush;
        private GraphicsDevice _device;
        private BasicEffect _effect;
        private int _numOfCircles;
        private int _numOfPolygons;
        private PolygonBrush[] _polygons;
        private VertexDeclaration _vertexDeclaration;
        private int triangleCount;

        public XNADebugRenderer(GraphicsDevice device)
        {
            _device = device;
            _effect = new BasicEffect(_device, null);
            _vertexDeclaration = new VertexDeclaration(_device, VertexPositionColor.VertexElements);
            _circleBrush = new CircleBrush[10000];
            _polygons = new PolygonBrush[10000]; // up to 1000 polygons
        }

        public override void DrawPolygon(Vector2[] vertices, int vertexCount, Color color)
        {
            _polygons[_numOfPolygons] = new PolygonBrush(vertices, vertexCount, color, false);
            _numOfPolygons++;
        }

        public override void DrawSolidPolygon(Vector2[] vertices, int vertexCount, Color color)
        {
            _polygons[_numOfPolygons] = new PolygonBrush(vertices, vertexCount, color, true);
            _numOfPolygons++;
        }

        public override void DrawCircle(Vector2 center, float radius, Color color)
        {
            _circleBrush[_numOfCircles] = new CircleBrush(center, radius, color, false);
            _numOfCircles++;
        }

        public override void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, Color color)
        {
            _circleBrush[_numOfCircles] = new CircleBrush(center, radius, color, true);
            _numOfCircles++;
        }

        public override void DrawSegment(Vector2 p1, Vector2 p2, Color color)
        {
        }

        public override void DrawXForm(XForm xf)
        {
        }

        public void Render()
        {
            Matrix cameraMatrix = Matrix.Identity;
            Matrix worldMatrix = Matrix.CreateScale(1.0f);
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(-40, 40, -40, 40, -1.0f, 1.0f);

            _device.VertexDeclaration = _vertexDeclaration;

            // set the effects matrix's
            _effect.World = worldMatrix;
            _effect.View = cameraMatrix;
            _effect.Projection = projectionMatrix;

            _effect.Alpha = 0.9f; // this effect supports a blending mode

            _effect.VertexColorEnabled = true; // we must enable vertex coloring with this effect
            _effect.GraphicsDevice.RenderState.CullMode = CullMode.None;

            _effect.Begin();

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                for (int i = 0; i < _numOfCircles; i++)
                {
                    _circleBrush[i].Draw(ref _device);
                }

                for (int i = 0; i < _numOfPolygons; i++)
                {
                    _polygons[i].Draw(ref _device);
                }

                pass.End();
            }

            _effect.End();
            _numOfCircles = 0;
            _numOfPolygons = 0;
        }

        public Vector2 ConvertScreenToWorld(float x, float y)
        {
            float tw = _device.Viewport.Width;
            float th = _device.Viewport.Height;
            float u = x/tw;
            float v = (th - y)/th;

            float ratio = tw/th;
            Vector2 extents = new Vector2(ratio*50.0f, 50.0f);
            extents *= 1.0f;

            Vector2 lower = new Vector2(0, 0) - extents;
            Vector2 upper = new Vector2(0, 0) + extents;

            Vector2 p = new Vector2();
            p.X = (1.0f - u)*lower.X + u*upper.X;
            p.Y = (1.0f - v)*lower.Y + v*upper.Y;
            return p;
        }

        private List<VertexPositionColor> GetTriangleStrip(Vector3[] points, Color color, float thickness)
        {
            Vector3 lastPoint = Vector3.Zero;
            List<VertexPositionColor> list = new List<VertexPositionColor>();
            triangleCount = -2;

            for (int i = 0; i < points.Length; i++)
            {
                if (i == 0)
                {
                    lastPoint = points[i];
                    continue;
                }
                //the direction of the current line
                Vector3 direction = lastPoint - points[i];
                direction.Normalize();
                //the perpendiculat to the current line
                Vector3 normal = Vector3.Cross(direction, Vector3.UnitZ);
                normal.Normalize();
                Vector3 p1 = lastPoint + normal*thickness;
                triangleCount++;
                Vector3 p2 = lastPoint - normal*thickness;
                triangleCount++;
                Vector3 p3 = points[i] + normal*thickness;
                triangleCount++;
                Vector3 p4 = points[i] - normal*thickness;
                triangleCount++;
                list.Add(new VertexPositionColor(p1, color));
                list.Add(new VertexPositionColor(p2, color));
                list.Add(new VertexPositionColor(p3, color));
                list.Add(new VertexPositionColor(p4, color));
                lastPoint = points[i];
            }
            Vector3 _direction = points[points.Length - 1] - points[0];
            _direction.Normalize();
            //the perpendiculat to the current line
            Vector3 _normal = Vector3.Cross(_direction, Vector3.UnitZ);
            _normal.Normalize();
            Vector3 _p1 = lastPoint + _normal*thickness;
            triangleCount++;
            Vector3 _p2 = lastPoint - _normal*thickness;
            triangleCount++;
            Vector3 _p3 = points[0] + _normal*thickness;
            triangleCount++;
            Vector3 _p4 = points[0] - _normal*thickness;
            triangleCount++;
            list.Add(new VertexPositionColor(_p1, color));
            list.Add(new VertexPositionColor(_p2, color));
            list.Add(new VertexPositionColor(_p3, color));
            list.Add(new VertexPositionColor(_p4, color));
            return list;
        }
    }
}