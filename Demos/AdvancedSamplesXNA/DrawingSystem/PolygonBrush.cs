using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerGames.FarseerPhysics.Collisions;

namespace FarseerGames.AdvancedSamplesXNA.DrawingSystem
{
    public class PolygonBrush
    {
        private Color _borderColor;
        private Color _color;
        private float _layer;
        private float _alpha = 1.0f;
        private Vertices _vertices;
        private Vector3[] _coloredVertices;
        private BasicEffect _effect;
        private GraphicsDevice _graphicsDevice;

        public PolygonBrush()
        {
            _color = Color.White;
            _borderColor = Color.Black;
        }

        public PolygonBrush(Vertices vertices, Color color, Color borderColor, float layer)
        {
            _color = color;
            _borderColor = borderColor;
            _layer = layer;
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
                _coloredVertices = new Vector3[_vertices.Count];

                for (int i = 0; i < _vertices.Count; i++)
                {
                    _coloredVertices[i] = new Vector3(_vertices[i].X, _vertices[i].Y, _layer);
                }
            }
        }

        public void Load(GraphicsDevice graphicsDevice)
        {
            _effect = new BasicEffect(graphicsDevice, null);        // create a new basic effect for this polygon
            _graphicsDevice = graphicsDevice;
        }

        public void Draw(Vector2 position, float rotation)
        {
            Vector2[] triangulatedVertices;
            VertexPositionColor[] shaderVertices = new VertexPositionColor[_vertices.Count];
            int[] indices;
            
            Matrix cameraMatrix = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, -100, 100);
            Matrix worldMatrix = Matrix.CreateRotationZ(rotation);
            worldMatrix *= Matrix.CreateTranslation(new Vector3(position, 0));

            Vertices.Triangulate(_vertices.GetVerticesArray(), Vertices.WindingOrder.CounterClockwise, out triangulatedVertices, out indices);

            for (int i = 0; i < triangulatedVertices.Length; i++)
            {
                shaderVertices[i].Position = new Vector3(triangulatedVertices[i].X, triangulatedVertices[i].Y, 0.5f);
                shaderVertices[i].Color = _color;
            }

            // set the effects matrix's
            _effect.World = worldMatrix;
            _effect.View = cameraMatrix;
            _effect.Projection = projectionMatrix;

            //_effect.Alpha = _alpha;    // this effect supports a blending mode

            //_effect.VertexColorEnabled = true;   // we must enable vertex coloring with this effect
            _effect.GraphicsDevice.RenderState.CullMode = CullMode.None;

            _effect.Begin();

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                _effect.DiffuseColor = _color.ToVector3();
                pass.Begin();
                //_graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleFan, _coloredVertices, 0, _coloredVertices.Length - 2);
                _graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, shaderVertices, 0, triangulatedVertices.Length, indices, 0, indices.Length / 3);
                //_effect.DiffuseColor = _borderColor.ToVector3();
                //_effect.CommitChanges();
                //_graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, _coloredVertices, 0, _coloredVertices.Length - 1);
                pass.End();
            }

            _effect.End();
        }
    }
}