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
    private VertexPositionTexture[] _vertices = null;
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
        // Clean up effects here

        _isDisposed = true;
      }
    }

    public void LoadContent()
    {
      _vertices = new VertexPositionTexture[] { 
        new VertexPositionTexture(new Vector3(0,0,1), new Vector2(1,1)),
        new VertexPositionTexture(new Vector3(0,0,1),new Vector2(0,1)),
        new VertexPositionTexture(new Vector3(0,0,1),new Vector2(0,0)),
        new VertexPositionTexture(new Vector3(0,0,1),new Vector2(1,0)) 
      };

    }

    public void Render(Vector2 v1, Vector2 v2)
    {
      _vertices[0].Position.X = v2.X;
      _vertices[0].Position.Y = v1.Y;

      _vertices[1].Position.X = v1.X;
      _vertices[1].Position.Y = v1.Y;

      _vertices[2].Position.X = v1.X;
      _vertices[2].Position.Y = v2.Y;

      _vertices[3].Position.X = v2.X;
      _vertices[3].Position.Y = v2.Y;

      _device.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, _vertices, 0, 4, _indexBuffer, 0, 2);
    }
  }
}