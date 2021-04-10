using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VelcroPhysics.Samples.Demo.MediaSystem
{
    public class Sprite
    {
        private Texture2D _image;

        public Sprite(Texture2D image, Vector2 origin)
        {
            _image = image;
            Origin = origin;
        }

        public Sprite(Texture2D image)
        {
            Image = image;
        }

        public Vector2 Origin { get; set; }

        public Texture2D Image
        {
            get { return _image; }
            set
            {
                _image = value;
                Origin = new Vector2(_image.Width / 2f, _image.Height / 2f);
            }
        }
    }
}