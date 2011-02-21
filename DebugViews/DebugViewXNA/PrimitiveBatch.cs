using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.DebugViews
{
    public class PrimitiveBatch : IDisposable
    {
        private const int DefaultBufferSize = 500;
        private VertexPositionColor[] triangleVertices;
        private VertexPositionColor[] lineVertices;
        private int triangleVertsCount = 0;
        private int lineVertsCount = 0;

        // a basic effect, which contains the shaders that we will use to draw our
        // primitives.
        private BasicEffect basicEffect;

        // the device that we will issue draw calls to.
        private GraphicsDevice device;

        // hasBegun is flipped to true once Begin is called, and is used to make
        // sure users don't call End before Begin is called.
        private bool hasBegun = false;

        private bool isDisposed = false;

        // the constructor creates a new PrimitiveBatch and sets up all of the internals
        // that PrimitiveBatch will need.
        public PrimitiveBatch(GraphicsDevice graphicsDevice) : this(graphicsDevice, DefaultBufferSize) { }

        public PrimitiveBatch(GraphicsDevice graphicsDevice, int bufferSize)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            device = graphicsDevice;

            triangleVertices = new VertexPositionColor[bufferSize - bufferSize % 3];
            lineVertices = new VertexPositionColor[bufferSize - bufferSize % 2];

            // set up a new basic effect, and enable vertex colors.
            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.VertexColorEnabled = true;
        }

        public void SetProjection(ref Matrix projection)
        {
            basicEffect.Projection = projection;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                if (basicEffect != null)
                    basicEffect.Dispose();

                isDisposed = true;
            }
        }

        // Begin is called to tell the PrimitiveBatch what kind of primitives will be
        // drawn, and to prepare the graphics card to render those primitives.
        public void Begin(ref Matrix projection, ref Matrix view)
        {
            if (hasBegun)
            {
                throw new InvalidOperationException("End must be called before Begin can be called again.");
            }

            //tell our basic effect to begin.
            basicEffect.Projection = projection;
            basicEffect.View = view;
            basicEffect.CurrentTechnique.Passes[0].Apply();

            // flip the error checking boolean. It's now ok to call AddVertex, Flush,
            // and End.
            hasBegun = true;
        }

        public bool IsReady()
        {
            return hasBegun;
        }

        public void AddVertex(Vector2 vertex, Color color, PrimitiveType primitiveType)
        {
            if (!hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before AddVertex can be called.");
            }
            if (primitiveType == PrimitiveType.LineStrip ||
                primitiveType == PrimitiveType.TriangleStrip)
            {
                throw new NotSupportedException("The specified primitiveType is not supported by PrimitiveBatch.");
            }

            if (primitiveType == PrimitiveType.TriangleList)
            {
                if (triangleVertsCount >= triangleVertices.Length)
                {
                    FlushTriangles();
                }
                triangleVertices[triangleVertsCount].Position = new Vector3(vertex, -0.1f);
                triangleVertices[triangleVertsCount].Color = color;
                triangleVertsCount++;
            }
            if (primitiveType == PrimitiveType.LineList)
            {
                if (lineVertsCount >= lineVertices.Length)
                {
                    FlushLines();
                }
                lineVertices[lineVertsCount].Position = new Vector3(vertex, 0f);
                lineVertices[lineVertsCount].Color = color;
                lineVertsCount++;
            }
        }

        // End is called once all the primitives have been drawn using AddVertex.
        // it will call Flush to actually submit the draw call to the graphics card, and
        // then tell the basic effect to end.
        public void End()
        {
            if (!hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before End can be called.");
            }

            // Draw whatever the user wanted us to draw
            FlushTriangles();
            FlushLines();

            hasBegun = false;
        }

        private void FlushTriangles()
        {
            if (!hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before Flush can be called.");
            }
            if (triangleVertsCount >= 3)
            {
                int primitiveCount = triangleVertsCount / 3;
                // submit the draw call to the graphics card
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, triangleVertices, 0, primitiveCount);
                triangleVertsCount -= primitiveCount * 3;
            }
        }

        private void FlushLines()
        {
            if (!hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before Flush can be called.");
            }
            if (lineVertsCount >= 2)
            {
                int primitiveCount = lineVertsCount / 2;
                // submit the draw call to the graphics card
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, lineVertices, 0, primitiveCount);
                lineVertsCount -= primitiveCount * 2;
            }
        }
    }
}