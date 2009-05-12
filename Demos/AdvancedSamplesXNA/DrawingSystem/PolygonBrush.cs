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
        private BasicEffect _effect;
        private GraphicsDevice _graphicsDevice;
        private Vector2[] _triangulatedVertices;
        private VertexPositionColor[] _shaderVertices;
        private int[] _indices;
            

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
                _shaderVertices = new VertexPositionColor[value.Count];

                Vertices.Triangulate(_vertices.GetVerticesArray(), Vertices.WindingOrder.CounterClockwise, out _triangulatedVertices, out _indices);

                for (int i = 0; i < _vertices.Count; i++)
                {
                    _shaderVertices[i].Position = new Vector3(_triangulatedVertices[i].X, _triangulatedVertices[i].Y, _layer);
                    _shaderVertices[i].Color = _color;
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
            Matrix cameraMatrix = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, -100, 100);
            Matrix worldMatrix = Matrix.CreateRotationZ(rotation);
            worldMatrix *= Matrix.CreateTranslation(new Vector3(position, 0));


            // set the effects matrix's
            _effect.World = worldMatrix;
            _effect.View = cameraMatrix;
            _effect.Projection = projectionMatrix;

            _effect.Alpha = _alpha;    // this effect supports a blending mode

            _effect.VertexColorEnabled = true;   // we must enable vertex coloring with this effect
            _effect.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            _effect.Begin();

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                _effect.DiffuseColor = _color.ToVector3();
                pass.Begin();
                //_graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleFan, _coloredVertices, 0, _coloredVertices.Length - 2);
                _graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, _shaderVertices, 0, _triangulatedVertices.Length, _indices, 0, _indices.Length / 3);
                //_effect.DiffuseColor = _borderColor.ToVector3();
                //_effect.CommitChanges();
                //_graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, _coloredVertices, 0, _coloredVertices.Length - 1);
                pass.End();
            }

            _effect.End();
        }
    }
}