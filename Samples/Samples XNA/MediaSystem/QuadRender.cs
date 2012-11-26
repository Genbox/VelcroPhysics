#region Using System
using System;
#endregion
#region Using XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FarseerPhysics.Samples.MediaSystem
{
  public class QuadRenderer : IDisposable
  {
    private BasicEffect _basicEffect;

    private VertexPositionColorTexture[] _verticesTexture;
    private short[] _indexBuffer = new short[] { 0, 1, 2, 2, 3, 0 };

    private GraphicsDevice _device;
    private bool _isDisposed;

    public QuadRenderer(GraphicsDevice graphicsDevice)
    {
      if (graphicsDevice == null)
      {
        throw new ArgumentNullException("graphicsDevice");
      }
      _device = graphicsDevice;
      _isDisposed = false;

      _verticesTexture = new VertexPositionColorTexture[] { 
        new VertexPositionColorTexture(new Vector3(0f, 0f, 0f), Color.White, new Vector2(1f, 1f)),
        new VertexPositionColorTexture(new Vector3(0f, 0f, 0f), Color.White, new Vector2(0f, 1f)),
        new VertexPositionColorTexture(new Vector3(0f, 0f, 0f), Color.White, new Vector2(0f, 0f)),
        new VertexPositionColorTexture(new Vector3(0f, 0f, 0f), Color.White, new Vector2(1f, 0f)) 
      };

      _basicEffect = new BasicEffect(graphicsDevice);
      _basicEffect.VertexColorEnabled = true;
      _basicEffect.View = Matrix.Identity;
      _basicEffect.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0.0f) * Matrix.CreateOrthographicOffCenter(0f, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0f, 0f, 1f);
    }

    #region IDisposable Members

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion

    protected virtual void Dispose(bool disposing)
    {
      if (disposing && !_isDisposed)
      {
        if (_basicEffect != null)
        {
          _basicEffect.Dispose();
        }

        _isDisposed = true;
      }
    }

    public void Render(Vector2 v1, Vector2 v2, Texture2D texture, params Color[] color)
    {
      _device.SamplerStates[0] = SamplerState.AnisotropicClamp;
      _device.RasterizerState = RasterizerState.CullNone;
      if (texture == null)
      {
        _basicEffect.TextureEnabled = false;
      }
      else
      {
        _basicEffect.Texture = texture;
        _basicEffect.TextureEnabled = true;
      }

      _verticesTexture[0].Position.X = v2.X;
      _verticesTexture[0].Position.Y = v1.Y;

      _verticesTexture[1].Position.X = v1.X;
      _verticesTexture[1].Position.Y = v1.Y;

      _verticesTexture[2].Position.X = v1.X;
      _verticesTexture[2].Position.Y = v2.Y;

      _verticesTexture[3].Position.X = v2.X;
      _verticesTexture[3].Position.Y = v2.Y;

      for (int i = 0; i < 4; i++)
      {
        if (color.Length > 0)
        {
          _verticesTexture[i].Color = color[i % color.Length];
        }
        else
        {
          _verticesTexture[i].Color = Color.White;
        }
      }

      _basicEffect.CurrentTechnique.Passes[0].Apply();
      _basicEffect.CurrentTechnique.Passes[0].Apply();
      _device.DrawUserIndexedPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, _verticesTexture, 0, 4, _indexBuffer, 0, 2);
    }
  }
}