using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem.Graphics
{
    public class QuadRenderer : IDisposable
    {
        private readonly BasicEffect _basicEffect;

        private readonly GraphicsDevice _device;
        private readonly short[] _lineBuffer = { 0, 1, 3, 2, 0 };

        private readonly VertexPositionColorTexture[] _verticesQuad;
        private bool _hasBegun;
        private bool _isDisposed;

        public QuadRenderer(GraphicsDevice graphicsDevice)
        {
            _device = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _isDisposed = false;

            _verticesQuad = new[]
            {
                new VertexPositionColorTexture(new Vector3(0f, 0f, 0f), Color.White, new Vector2(0f, 1f)),
                new VertexPositionColorTexture(new Vector3(0f, 0f, 0f), Color.White, new Vector2(1f, 1f)),
                new VertexPositionColorTexture(new Vector3(0f, 0f, 0f), Color.White, new Vector2(0f, 0f)),
                new VertexPositionColorTexture(new Vector3(0f, 0f, 0f), Color.White, new Vector2(1f, 0f))
            };

            _basicEffect = new BasicEffect(graphicsDevice);
            _basicEffect.View = Matrix.Identity;
            _basicEffect.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0.0f) * Matrix.CreateOrthographicOffCenter(0f, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0f, 0f, 1f);
            _basicEffect.VertexColorEnabled = true;
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
                if (_basicEffect != null)
                    _basicEffect.Dispose();

                _isDisposed = true;
            }
        }

        public void Begin()
        {
#if DEBUG
            if (_hasBegun)
                throw new InvalidOperationException("End must be called before Begin can be called again.");
#endif

            _device.SamplerStates[0] = SamplerState.AnisotropicClamp;
            _device.RasterizerState = RasterizerState.CullNone;

            _hasBegun = true;
        }

        public void End()
        {
#if DEBUG
            if (!_hasBegun)
                throw new InvalidOperationException("Begin must be called before End can be called.");
#endif

            _hasBegun = false;
        }

        public void Render(Vector2 v1, Vector2 v2, Texture2D texture, params Color[] color)
        {
            Render(v1, v2, texture, false, Color.White, color);
        }

        public void Render(Vector2 v1, Vector2 v2, Texture2D texture, bool outline, Color outlineColor, params Color[] color)
        {
#if DEBUG
            if (!_hasBegun)
                throw new InvalidOperationException("Begin must be called before DrawLineShape can be called.");
#endif

            if (texture == null)
                _basicEffect.TextureEnabled = false;
            else
            {
                _basicEffect.Texture = texture;
                _basicEffect.TextureEnabled = true;
            }

            _verticesQuad[0].Position.X = v1.X;
            _verticesQuad[0].Position.Y = v1.Y;

            _verticesQuad[1].Position.X = v2.X;
            _verticesQuad[1].Position.Y = v1.Y;

            _verticesQuad[2].Position.X = v1.X;
            _verticesQuad[2].Position.Y = v2.Y;

            _verticesQuad[3].Position.X = v2.X;
            _verticesQuad[3].Position.Y = v2.Y;

            for (int i = 0; i < 4; i++)
            {
                if (color.Length > 0)
                    _verticesQuad[i].Color = color[i % color.Length];
                else
                    _verticesQuad[i].Color = Color.White;
            }

            _basicEffect.CurrentTechnique.Passes[0].Apply();
            _device.DrawUserPrimitives(PrimitiveType.TriangleStrip, _verticesQuad, 0, 2);

            if (outline)
            {
                for (int i = 0; i < 4; i++)
                {
                    _verticesQuad[i].Color = outlineColor;
                }

                _basicEffect.CurrentTechnique.Passes[0].Apply();
                _device.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, _verticesQuad, 0, 4, _lineBuffer, 0, 4);
            }
        }
    }
}