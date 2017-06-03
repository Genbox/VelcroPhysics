using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using VelcroPhysics.Shared;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.ContentPipelines.TextureToVertices
{
    [ContentProcessor(DisplayName = "Texture to Vertices")]
    public class TextureToVerticesProcessor : ContentProcessor<Texture2DContent, Vertices>
    {
        private float _scaleFactor = 1f;

        [DisplayName("Pixel to meter ratio")]
        [Description("The length of one physics simulation unit in pixels.")]
        [DefaultValue(1)]
        public int ScaleFactor
        {
            get { return (int)(1f / _scaleFactor); }
            set { _scaleFactor = 1f / value; }
        }

        [DisplayName("Hole detection")]
        [Description("Detect holes in the traced texture.")]
        [DefaultValue(false)]
        public bool HoleDetection { get; set; }

        public override Vertices Process(Texture2DContent input, ContentProcessorContext context)
        {
            if (ScaleFactor < 1)
                throw new Exception("Pixel to meter ratio must be greater than zero.");

            PixelBitmapContent<Color> bitmapContent = (PixelBitmapContent<Color>)input.Faces[0][0];
            uint[] colorData = new uint[bitmapContent.Width * bitmapContent.Height];
            for (int i = 0; i < bitmapContent.Height; i++)
            {
                for (int j = 0; j < bitmapContent.Width; j++)
                {
                    Color c = bitmapContent.GetPixel(j, i);
                    c.R *= c.A;
                    c.G *= c.A;
                    c.B *= c.A;
                    colorData[i * bitmapContent.Width + j] = c.PackedValue;
                }
            }

            Vertices outline = PolygonUtils.CreatePolygon(colorData, bitmapContent.Width, HoleDetection);

            Vector2 centroid = -outline.GetCentroid();
            outline.Translate(ref centroid);

            Vector2 scale = new Vector2(_scaleFactor);
            outline.Scale(ref scale);

            return outline;
        }
    }
}