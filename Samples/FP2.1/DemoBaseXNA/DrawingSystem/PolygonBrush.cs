using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerGames.FarseerPhysics.Collisions;

namespace DemoBaseXNA.DrawingSystem
{
    public class PolygonBrush
    {
        private Color _borderColor;
        private Color _color;
        private float _layer;
        private float _alpha = 0.5f;
        private float _boarderThickness;
        private Vertices _vertices;
        private BasicEffect _effect;
        private GraphicsDevice _graphicsDevice;
        private Vector2[] _triangulatedVertices;
        private VertexPositionColor[] _shaderVertices;
        private short[] _indices;
        private List<VertexPositionColor> _lineStrip;
        private int _triangleCount;
        private VertexDeclaration _vertexDeclaration;

        public PolygonBrush()
        {
            _color = Color.White;
            _borderColor = Color.Black;
        }

        public PolygonBrush(Vertices vertices, Color color, Color borderColor, float borderThickness, float layer)
        {
            _color = color;
            _borderColor = borderColor;
            _layer = layer;
            _boarderThickness = borderThickness;
            Vertices = vertices;
        }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; }
        }

        public float BorderThickness
        {
            get { return _boarderThickness; }
            set { _boarderThickness = value; }
        }

        public float Layer
        {
            get { return _layer; }
            set { _layer = value; }
        }

        public float Alpha
        {
            get { return _alpha; }
            set { _alpha = value; }
        }

        public Vertices Vertices
        {
            get { return _vertices; }
            set
            {
                _vertices = value;
                _shaderVertices = new VertexPositionColor[value.Count];
                Vector3[] tempVerts = new Vector3[value.Count];

                Vertices.Triangulate(_vertices.GetVerticesArray(), Vertices.WindingOrder.Clockwise, out _triangulatedVertices, out _indices);

                for (int i = 0; i < _vertices.Count; i++)
                {
                    _shaderVertices[i].Position = new Vector3(_triangulatedVertices[i].X, _triangulatedVertices[i].Y, _layer);
                    _shaderVertices[i].Color = _color;

                    tempVerts[i] = new Vector3(_triangulatedVertices[i], 0.0f);
                }

                _lineStrip = GetTriangleStrip(tempVerts, _boarderThickness);

            }
        }

        public void Load(GraphicsDevice graphicsDevice)
        {
            _effect = new BasicEffect(graphicsDevice, null);        // create a new basic effect for this polygon
            _graphicsDevice = graphicsDevice;
            _vertexDeclaration = new VertexDeclaration(_graphicsDevice, VertexPositionColor.VertexElements);
        }

        public void Draw(Vector2 position, float rotation)
        {
            Matrix cameraMatrix = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, -100, 100);
            Matrix worldMatrix = Matrix.CreateRotationZ(rotation);
            worldMatrix *= Matrix.CreateTranslation(new Vector3(position, 0));

            _graphicsDevice.VertexDeclaration = _vertexDeclaration;

            // set the effects matrix's
            _effect.World = worldMatrix;
            _effect.View = cameraMatrix;
            _effect.Projection = projectionMatrix;

            _effect.Alpha = _alpha;    // this effect supports a blending mode

            _effect.VertexColorEnabled = true;   // we must enable vertex coloring with this effect
            _effect.GraphicsDevice.RenderState.CullMode = CullMode.None;

            _effect.Begin();

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                _graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _shaderVertices, 0, _triangulatedVertices.Length, _indices, 0, _indices.Length / 3);
                _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _lineStrip.ToArray(), 0, _triangleCount);
                pass.End();
            }

            _effect.End();
        }

        public void Draw(Matrix matrix)
        {
            Matrix cameraMatrix = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, -100, 100);
            Matrix worldMatrix = matrix;

            _graphicsDevice.VertexDeclaration = _vertexDeclaration;

            // set the effects matrix's
            _effect.World = worldMatrix;
            _effect.View = cameraMatrix;
            _effect.Projection = projectionMatrix;

            _effect.Alpha = _alpha;    // this effect supports a blending mode

            _effect.VertexColorEnabled = true;   // we must enable vertex coloring with this effect
            _effect.GraphicsDevice.RenderState.CullMode = CullMode.None;

            _effect.Begin();

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                _graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _shaderVertices, 0, _triangulatedVertices.Length, _indices, 0, _indices.Length / 3);
                _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _lineStrip.ToArray(), 0, _triangleCount);
                pass.End();
            }

            _effect.End();
        }

        private List<VertexPositionColor> GetTriangleStrip(Vector3[] points, float thickness)
        {
            Vector3 lastPoint = Vector3.Zero;
            List<VertexPositionColor> list = new List<VertexPositionColor>();
            _triangleCount = -2;

            for (int i = 0; i < points.Length; i++)
            {
                if (i == 0) { lastPoint = points[0]; continue; }
                //the direction of the current line
                Vector3 direction = lastPoint - points[i];
                direction.Normalize();
                //the perpendiculat to the current line
                Vector3 normal = Vector3.Cross(direction, Vector3.UnitZ);
                normal.Normalize();
                Vector3 p1 = lastPoint + normal * thickness; _triangleCount++;
                Vector3 p2 = lastPoint - normal * thickness; _triangleCount++;
                Vector3 p3 = points[i] + normal * thickness; _triangleCount++;
                Vector3 p4 = points[i] - normal * thickness; _triangleCount++;
                list.Add(new VertexPositionColor(p1, _borderColor));
                list.Add(new VertexPositionColor(p2, _borderColor));
                list.Add(new VertexPositionColor(p3, _borderColor));
                list.Add(new VertexPositionColor(p4, _borderColor));
                lastPoint = points[i];
            }

            Vector3 _direction = points[points.Length - 1] - points[0];
            _direction.Normalize();
            //the perpendiculat to the current line
            Vector3 _normal = Vector3.Cross(_direction, Vector3.UnitZ);
            _normal.Normalize();
            Vector3 _p1 = lastPoint + _normal * thickness; _triangleCount++;
            Vector3 _p2 = lastPoint - _normal * thickness; _triangleCount++;
            Vector3 _p3 = points[0] + _normal * thickness; _triangleCount++;
            Vector3 _p4 = points[0] - _normal * thickness; _triangleCount++;
            list.Add(new VertexPositionColor(_p1, _borderColor));
            list.Add(new VertexPositionColor(_p2, _borderColor));
            list.Add(new VertexPositionColor(_p3, _borderColor));
            list.Add(new VertexPositionColor(_p4, _borderColor));

            return list;
        }
    }
}