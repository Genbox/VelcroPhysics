using System;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem.Graphics
{
    public class LineBatch : IDisposable
    {
        private const int DefaultBufferSize = 500;

        // a basic effect, which contains the shaders that we will use to draw our
        // primitives.
        private readonly BasicEffect _basicEffect;

        // the device that we will issue draw calls to.
        private readonly GraphicsDevice _device;

        private readonly VertexPositionColor[] _lineVertices;

        // hasBegun is flipped to true once Begin is called, and is used to make
        // sure users don't call End before Begin is called.
        private bool _hasBegun;

        private bool _isDisposed;
        private int _lineVertsCount;

        public LineBatch(GraphicsDevice graphicsDevice, int bufferSize = DefaultBufferSize)
        {
            _device = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));

            _lineVertices = new VertexPositionColor[bufferSize - bufferSize % 2];

            // set up a new basic effect, and enable vertex colors.
            _basicEffect = new BasicEffect(graphicsDevice);
            _basicEffect.VertexColorEnabled = true;

            _isDisposed = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                _basicEffect?.Dispose();
                _isDisposed = true;
            }
        }

        public void Begin(ref Matrix projection, ref Matrix view)
        {
#if DEBUG
            if (_hasBegun)
                throw new InvalidOperationException("End must be called before Begin can be called again.");
#endif

            _device.SamplerStates[0] = SamplerState.AnisotropicClamp;

            // Tell our basic effect to begin.
            _basicEffect.Projection = projection;
            _basicEffect.View = view;
            _basicEffect.CurrentTechnique.Passes[0].Apply();

            // Flip the error checking boolean. It's now ok to call DrawLineShape, Flush, and End.
            _hasBegun = true;
        }

        public void DrawLineShape(Shape shape)
        {
            DrawLineShape(shape, Color.Black);
        }

        public void DrawLineShape(Shape shape, Color color)
        {
#if DEBUG
            if (!_hasBegun)
                throw new InvalidOperationException("Begin must be called before DrawLineShape can be called.");

            if (shape.ShapeType != ShapeType.Edge && shape.ShapeType != ShapeType.Chain)
                throw new NotSupportedException("The specified shapeType is not supported by LineBatch.");
#endif

            if (shape.ShapeType == ShapeType.Edge)
            {
                EdgeShape edge = (EdgeShape)shape;
                DrawLine(edge.Vertex1, edge.Vertex2, color);
            }
            else if (shape.ShapeType == ShapeType.Chain)
            {
                ChainShape chain = (ChainShape)shape;
                DrawVertices(chain.Vertices, color);
            }
        }

        public void DrawVertices(Vertices vertices)
        {
            DrawVertices(vertices, Color.Black);
        }

        public void DrawVertices(Vertices vertices, Color color)
        {
#if DEBUG
            if (!_hasBegun)
                throw new InvalidOperationException("Begin must be called before DrawVertices can be called.");
#endif

            for (int i = 0; i < vertices.Count; ++i)
            {
                if (_lineVertsCount >= _lineVertices.Length)
                    Flush();

                _lineVertices[_lineVertsCount].Position = new Vector3(vertices[i], 0f);
                _lineVertices[_lineVertsCount + 1].Position = new Vector3(vertices.NextVertex(i), 0f);
                _lineVertices[_lineVertsCount].Color = _lineVertices[_lineVertsCount + 1].Color = color;
                _lineVertsCount += 2;
            }
        }

        public void DrawLine(Vector2 start, Vector2 end)
        {
            DrawLine(start, end, Color.Black);
        }

        public void DrawLine(Vector2 start, Vector2 end, Color color)
        {
#if DEBUG
            if (!_hasBegun)
                throw new InvalidOperationException("Begin must be called before DrawLineShape can be called.");
#endif

            if (_lineVertsCount >= _lineVertices.Length)
                Flush();

            _lineVertices[_lineVertsCount].Position = new Vector3(start, 0f);
            _lineVertices[_lineVertsCount + 1].Position = new Vector3(end, 0f);
            _lineVertices[_lineVertsCount].Color = _lineVertices[_lineVertsCount + 1].Color = color;
            _lineVertsCount += 2;
        }

        /// <summary>End is called once all the primitives have been drawn using AddVertex. it will call Flush to actually submit
        /// the draw call to the graphics card, and then tell the basic effect to end.</summary>
        public void End()
        {
#if DEBUG
            if (!_hasBegun)
                throw new InvalidOperationException("Begin must be called before End can be called.");
#endif

            // Draw whatever the user wanted us to draw
            Flush();

            _hasBegun = false;
        }

        private void Flush()
        {
#if DEBUG
            if (!_hasBegun)
                throw new InvalidOperationException("Begin must be called before Flush can be called.");
#endif

            if (_lineVertsCount >= 2)
            {
                int primitiveCount = _lineVertsCount / 2;

                // Submit the draw call to the graphics card
                _device.DrawUserPrimitives(PrimitiveType.LineList, _lineVertices, 0, primitiveCount);
                _lineVertsCount -= primitiveCount * 2;
            }
        }
    }
}