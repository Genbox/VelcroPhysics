using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerGames.FarseerPhysics.Collisions;

namespace FarseerGames.SimpleSamplesXNA.DrawingSystem
{
    public class PolygonBrush
    {
        private Color _borderColor;
        private Color _color;
        private float _layer;
        private float _alpha = 1.0f;
        //private Texture2D _polygonTexture;
        //private Vector2 _polygonOrigin;
        private VertexPositionColorTexture[] _vertices;
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
            _vertices = new VertexPositionColorTexture[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                _vertices[i].Position = new Vector3(vertices[i].X, vertices[i].Y, _layer);
                _vertices[i].Color = Color.White;
                _vertices[i].TextureCoordinate = new Vector2();
            }
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

        public void Load(GraphicsDevice graphicsDevice)
        {
            _effect = new BasicEffect(graphicsDevice, null);        // create a new basic effect for this polygon
            _graphicsDevice = graphicsDevice;
        }

        public void Draw(Vector2 position, float rotation)
        {
            Matrix _cameraMatrix = Matrix.Identity;
            Matrix _projectionMatrix = Matrix.CreateOrthographicOffCenter(0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, -100, 100);
            Matrix _worldMatrix = Matrix.Identity;

            // set the effects matrix's
            _effect.World = _worldMatrix;
            _effect.View = _cameraMatrix;
            _effect.Projection = _projectionMatrix;

            _effect.Alpha = _alpha;    // this effect supports a blending mode

            //_effect.VertexColorEnabled = true;   // we must enable vertex coloring with this effect
            _effect.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;

            _effect.Begin();

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                _effect.DiffuseColor = _color.ToVector3();
                pass.Begin();
                _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleFan, _vertices, 0, _vertices.Length - 2);
                _effect.DiffuseColor = _borderColor.ToVector3();
                _effect.CommitChanges();
                _graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, _vertices, 0, _vertices.Length - 1);
                pass.End();
            }

            _effect.End();
        }
    }
}