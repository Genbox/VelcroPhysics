using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.Samples.DrawingSystem
{
  public class Sprite
  {
    private Vector2 _origin;
    public Vector2 Origin
    {
      get { return _origin; }
      set { _origin = value; }
    }

    private Texture2D _image;
    public Texture2D Image
    {
      get { return _image; }
      set
      {
        _image = value;
        _origin = new Vector2(_image.Width / 2f, _image.Height / 2f);
      }
    }

    public Sprite(Texture2D image, Vector2 origin)
    {
      _image = image;
      _origin = origin;
    }

    public Sprite(Texture2D image)
    {
      Image = image;
    }
  }
}