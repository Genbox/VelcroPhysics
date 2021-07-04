using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem.Graphics
{
    public class Sprite
    {
        private Texture2D _image;
        private Vector2 _origin;

        public Sprite(Texture2D image, Vector2 origin)
        {
            _image = image;
            _origin = origin;
        }

        public Sprite(Texture2D image)
        {
            Image = image;
        }

        public Vector2 Origin
        {
            get => _origin;
            set => _origin = value;
        }

        public Texture2D Image
        {
            get => _image;
            set
            {
                _image = value;
                _origin = new Vector2(_image.Width / 2f, _image.Height / 2f);
            }
        }
    }
}