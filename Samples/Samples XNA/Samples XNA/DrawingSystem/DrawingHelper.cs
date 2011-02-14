using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.DemoBaseXNA.DrawingSystem
{
    public static class DrawingHelper
    {
        public static Texture2D CreateRectangleTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            return CreateRectangleTexture(graphicsDevice, width, height, 0, 0, 2, color, color);
        }

        public static Texture2D CreateRectangleTexture(GraphicsDevice graphicsDevice, int width, int height, Color color,
                                                       Color borderColor)
        {
            return CreateRectangleTexture(graphicsDevice, width, height, 1, 1, 2, color, borderColor);
        }

        public static Texture2D CreateRectangleTexture(GraphicsDevice graphicsDevice, int width, int height,
                                                       int borderWidth, Color color, Color borderColor)
        {
            return CreateRectangleTexture(graphicsDevice, width, height, borderWidth, 1, 2, color, borderColor);
        }

        public static Texture2D CreateRectangleTexture(GraphicsDevice graphicsDevice, int width, int height,
                                                       int borderWidth, int borderInnerTransitionWidth,
                                                       int borderOuterTransitionWidth, Color color, Color borderColor)
        {
            Texture2D texture2D = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);

            int y = -1;
            int j;
            int count = width*height;
            Color[] colorArray = new Color[count];
            Color[] shellColor = new Color[borderWidth + borderOuterTransitionWidth + borderInnerTransitionWidth];
            float transitionAmount;

            for (j = 0; j < borderOuterTransitionWidth; j++)
            {
                transitionAmount = (j)/(float) (borderOuterTransitionWidth);
                shellColor[j] = new Color(borderColor.R, borderColor.G, borderColor.B, (byte) (255*transitionAmount));
            }
            for (j = borderOuterTransitionWidth; j < borderWidth + borderOuterTransitionWidth; j++)
            {
                shellColor[j] = borderColor;
            }
            for (j = borderWidth + borderOuterTransitionWidth;
                 j < borderWidth + borderOuterTransitionWidth + borderInnerTransitionWidth;
                 j++)
            {
                transitionAmount = 1 -
                                   (j - (borderWidth + borderOuterTransitionWidth) + 1)/
                                   (float) (borderInnerTransitionWidth + 1);
                shellColor[j] = new Color((byte) MathHelper.Lerp(color.R, borderColor.R, transitionAmount),
                                          (byte) MathHelper.Lerp(color.G, borderColor.G, transitionAmount),
                                          (byte) MathHelper.Lerp(color.B, borderColor.B, transitionAmount));
            }


            for (int i = 0; i < count; i++)
            {
                if (i%width == 0)
                {
                    y += 1;
                }
                int x = i%width;

                //check if pixel is in one of the rectangular border shells
                bool isInShell = false;
                for (int k = 0; k < shellColor.Length; k++)
                {
                    if (InShell(x, y, width, height, k))
                    {
                        colorArray[i] = shellColor[k];
                        isInShell = true;
                        break;
                    }
                }
                //pixel is not in shell so it is in the center
                if (!isInShell)
                {
                    colorArray[i] = color;
                }
            }

            texture2D.SetData(colorArray);
            return texture2D;
        }

        private static bool InShell(int x, int y, int width, int height, int shell)
        {
            //check each line of rectangle.
            if ((x == shell && IsBetween(y, shell, height - 1 - shell)) ||
                (x == width - 1 - shell && IsBetween(y, shell, height - 1 - shell)))
            {
                return true;
            }
            if ((y == shell && IsBetween(x, shell, width - 1 - shell)) ||
                (y == height - 1 - shell && IsBetween(x, shell, width - 1 - shell)))
            {
                return true;
            }
            return false;
        }

        private static bool IsBetween(float value, float min, float max)
        {
            if (value >= min && value <= max)
            {
                return true;
            }
            return false;
        }
    }
}