using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DrawingSystem
{
    public static class DrawingHelper
    {
        public static Texture2D CreateLineTexture(GraphicsDevice graphicsDevice, int lineThickness)
        {
            Texture2D texture2D = new Texture2D(graphicsDevice, 2, lineThickness + 2, 1, TextureUsage.None,
                                                SurfaceFormat.Color);

            //Texture2D texture2D = new Texture2D(graphicsDevice, 2, lineWidth + 2);
            int count = 2 * (lineThickness + 2);
            Color[] colorArray = new Color[count];
            colorArray[0] = Color.TransparentWhite;
            colorArray[1] = Color.TransparentWhite;

            for (int i = 2; i < count - 2; i++) 
            {
                colorArray[i] = Color.White;
            }

            colorArray[count - 2] = Color.TransparentWhite;
            colorArray[count - 1] = Color.TransparentWhite;
            texture2D.SetData(colorArray);
            return texture2D;
        }

        public static Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int radius, Color color)
        {
            return CreateCircleTexture(graphicsDevice, radius, 0, 0, 2, color, color);
        }

        public static Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int radius, Color color,
                                                    Color borderColor)
        {
            return CreateCircleTexture(graphicsDevice, radius, 1, 1, 1, color, borderColor);
        }

        public static Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int radius, int borderWidth,
                                                    Color color, Color borderColor)
        {
            return CreateCircleTexture(graphicsDevice, radius, borderWidth, 1, 2, color, borderColor);
        }

        public static Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int radius, int borderWidth,
                                                    int borderInnerTransitionWidth, int borderOuterTransitionWidth,
                                                    Color color, Color borderColor)
        {
            int y = -1;
            int diameter = (radius + 2) * 2;
            Vector2 center = new Vector2((diameter - 1) / 2f, (diameter - 1) / 2f);

            Texture2D circle = new Texture2D(graphicsDevice, diameter, diameter, 1, TextureUsage.None,
                                             SurfaceFormat.Color);
            Color[] colors = new Color[diameter * diameter];

            for (int i = 0; i < colors.Length; i++)
            {
                int x = i % diameter;

                if (x == 0)
                {
                    y += 1;
                }

                Vector2 diff = new Vector2(x, y) - center;
                float length = diff.Length(); // distance.Length();

                if (length > radius)
                {
                    colors[i] = Color.TransparentBlack;
                }
                else if (length >= radius - borderOuterTransitionWidth)
                {
                    float transitionAmount = (length - (radius - borderOuterTransitionWidth)) / borderOuterTransitionWidth;
                    transitionAmount = 255 * (1 - transitionAmount);
                    colors[i] = new Color(borderColor.R, borderColor.G, borderColor.B, (byte)transitionAmount);
                }
                else if (length > radius - (borderWidth + borderOuterTransitionWidth))
                {
                    colors[i] = borderColor;
                }
                else if (length >= radius - (borderWidth + borderOuterTransitionWidth + borderInnerTransitionWidth))
                {
                    float transitionAmount = (length -
                                              (radius -
                                               (borderWidth + borderOuterTransitionWidth + borderInnerTransitionWidth))) /
                                             (borderInnerTransitionWidth + 1);
                    colors[i] = new Color((byte)MathHelper.Lerp(color.R, borderColor.R, transitionAmount),
                                          (byte)MathHelper.Lerp(color.G, borderColor.G, transitionAmount),
                                          (byte)MathHelper.Lerp(color.B, borderColor.B, transitionAmount));
                }
                else
                {
                    colors[i] = color;
                }
            }
            circle.SetData(colors);
            return circle;
        }

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
            Texture2D texture2D = new Texture2D(graphicsDevice, width, height, 1, TextureUsage.None, SurfaceFormat.Color);

            int y = -1;
            int j;
            int count = width * height;
            Color[] colorArray = new Color[count];
            Color[] shellColor = new Color[borderWidth + borderOuterTransitionWidth + borderInnerTransitionWidth];
            float transitionAmount;

            for (j = 0; j < borderOuterTransitionWidth; j++)
            {
                transitionAmount = (j) / (float)(borderOuterTransitionWidth);
                shellColor[j] = new Color(borderColor.R, borderColor.G, borderColor.B, (byte)(255 * transitionAmount));
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
                                   (j - (borderWidth + borderOuterTransitionWidth) + 1) /
                                   (float)(borderInnerTransitionWidth + 1);
                shellColor[j] = new Color((byte)MathHelper.Lerp(color.R, borderColor.R, transitionAmount),
                                          (byte)MathHelper.Lerp(color.G, borderColor.G, transitionAmount),
                                          (byte)MathHelper.Lerp(color.B, borderColor.B, transitionAmount));
            }


            for (int i = 0; i < count; i++)
            {
                if (i % width == 0)
                {
                    y += 1;
                }
                int x = i % width;

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

        public static Texture2D CreateEllipseTexture(GraphicsDevice graphicsDevice, int xRadius, int yRadius, Color color)
        {
            return CreateEllipseTexture(graphicsDevice, xRadius, yRadius, 0, 0, 2, color, color);
        }

        public static Texture2D CreateEllipseTexture(GraphicsDevice graphicsDevice, int xRadius, int yRadius, Color color,
                                                     Color borderColor)
        {
            return CreateEllipseTexture(graphicsDevice, xRadius, yRadius, 1, 1, 1, color, borderColor);
        }

        public static Texture2D CreateEllipseTexture(GraphicsDevice graphicsDevice, int xRadius, int yRadius, int borderWidth,
                                                     Color color, Color borderColor)
        {
            return CreateEllipseTexture(graphicsDevice, xRadius, yRadius, borderWidth, 1, 2, color, borderColor);
        }


        public static Texture2D CreateEllipseTexture(GraphicsDevice graphicsDevice, int xRadius, int yRadius, int borderWidth,
                                                     int borderInnerTransitionWidth, int borderOuterTransitionWidth,
                                                     Color color, Color borderColor)
        {
            //Is it a circle?
            if (xRadius == yRadius)
                return CreateCircleTexture(graphicsDevice, xRadius, borderWidth,
                                           borderInnerTransitionWidth, borderOuterTransitionWidth, color, borderColor);

            int width = xRadius * 2;
            int height = yRadius * 2;

            //Initialize values
            int diameterX = (width + 8);
            int diameterY = (height + 8);
            Vector2 center = new Vector2((diameterX - 4) / 2f, (diameterY - 4) / 2f);

            Texture2D ellipse = new Texture2D(graphicsDevice, diameterX, diameterY, 1, TextureUsage.None,
                                              SurfaceFormat.Color);

            //Just calculate the upper left quarter of the ellipse
            //(it's axis-symmetric)
            Color[,] ulColors = new Color[diameterX / 2 + 1, diameterY / 2 + 1];

            //Create a description of the ellipse boundary

            //How accurate must it be?
            int accuracy = 360;
            if (Math.Max(width, height) > 1024)
                accuracy = 1440;
            else if (Math.Max(width, height) > 512)
                accuracy = 1080;
            else if (Math.Max(width, height) > 256)
                accuracy = 720;
            Vector2[] ellipseBoundary = CalculateEllipseBoundary(width, height, center, accuracy);

            //Calculate color for every pixel
            for (int y = 0; y < ulColors.GetLength(1); y++)
            {
                for (int x = 0; x < ulColors.GetLength(0); x++)
                {
                    Vector2 curPoint = new Vector2(x, y);

                    // find the closest normal of the ellipse intersecting the current point                    

                    // approximate the point on the ellipse closest to the current point.
                    // this point is where the normal strikes the ellipse
                    Vector2 pointOnEllipse = ApproximateClosestPointOnEllipse(curPoint, ellipseBoundary);

                    // the following lines calculate the shortest distance 
                    // from the current point to the ellipse boundary

                    // calculate this point's distance to the current point.
                    // this is what we need
                    float distanceFromCurToBoundary = (pointOnEllipse - curPoint).Length();


                    //NOTE: hack to find out whether the current point is inside the ellipse

                    // calculate angle of a straight line intersecting the center of the ellipse and the current point
                    Vector2 lineFromCurPointToCenter = curPoint - center;
                    float gradient = lineFromCurPointToCenter.Y / lineFromCurPointToCenter.X;
                    float angle = (float)Math.Atan(gradient * width / height);

                    // find out where the line intersecting the center of the ellipse and the current point 
                    // intersects the ellipse
                    Vector2 intersectionPoint = center + new Vector2(xRadius * (float)Math.Cos(angle),
                                                                     yRadius * (float)Math.Sin(angle));
                    // calculate squared distance from intersection point to center
                    float distanceFromIntersectionPointToCenter = (intersectionPoint - center).LengthSquared();
                    // calculate squared distance from current point to center of ellipse
                    float distanceFromCurPointToCenter = lineFromCurPointToCenter.LengthSquared();

                    // when boundary intersection is further from the center than the current point,
                    // the current point should be inside of the ellipse.
                    // positive values for mean outside of the ellipse, negative values mean inside
                    if (distanceFromIntersectionPointToCenter >= distanceFromCurPointToCenter)
                        distanceFromCurToBoundary = -distanceFromCurToBoundary;


                    // use calculated distanceFromCurToBoundary to 
                    // choose the color for thecurrent pixel                    

                    if (distanceFromCurToBoundary > 0)
                    {
                        // outside of ellipse
                        ulColors[x, y] = Color.TransparentBlack;
                    }
                    else if (distanceFromCurToBoundary > -borderOuterTransitionWidth)
                    {
                        // outside of border, where the border color fades to transparent
                        float transitionAmount = (-distanceFromCurToBoundary) / borderOuterTransitionWidth;
                        transitionAmount = 255 * transitionAmount;
                        ulColors[x, y] = new Color(borderColor.R, borderColor.G, borderColor.B, (byte)transitionAmount);
                    }
                    else if (distanceFromCurToBoundary > -(borderWidth + borderOuterTransitionWidth))
                    {
                        // on border
                        ulColors[x, y] = borderColor;
                    }
                    else if (distanceFromCurToBoundary >= -(borderWidth + borderOuterTransitionWidth + borderInnerTransitionWidth))
                    {
                        // inside of border, where the border color fades to the fill color
                        float transitionAmount = (-distanceFromCurToBoundary - (borderWidth + borderOuterTransitionWidth)) / (borderInnerTransitionWidth + 1);
                        transitionAmount = 1 - transitionAmount;
                        ulColors[x, y] = new Color((byte)MathHelper.Lerp(color.R, borderColor.R, transitionAmount),
                                                   (byte)MathHelper.Lerp(color.G, borderColor.G, transitionAmount),
                                                   (byte)MathHelper.Lerp(color.B, borderColor.B, transitionAmount));
                    }
                    else
                    {
                        // inside of ellipse
                        ulColors[x, y] = color;

                        // because we are just drawing a quarter of the ellipse, 
                        // once we are inside of it we can fill in the rest 
                        // of the current line with the fill color
                        if (x < ulColors.GetUpperBound(0))
                            for (int ix = x + 1; ix < ulColors.GetLength(0); ix++)
                                ulColors[ix, y] = color;

                        // we're done with this line
                        break;
                    }
                }
            }

            // copy the upper left quarter of the ellipse into the final texture
            // and mirror it accordingly
            int ulWidth = ulColors.GetLength(0);
            int ulHeight = ulColors.GetLength(1);

            Color[] finalcolors = new Color[diameterX * diameterY];
            for (int y = 0; y < diameterY; y++)
            {
                for (int x = 0; x < diameterX; x++)
                {
                    int curIndex = y * diameterX + x;

                    if (y < ulHeight)
                    {
                        if (x < ulWidth)
                            // upper left sector of final texture
                            finalcolors[curIndex] = ulColors[x, y];
                        else
                            // upper right sector of final texture - mirror around y-axis
                            finalcolors[curIndex] = ulColors[ulColors.GetUpperBound(0) - x % ulWidth, y];
                    }
                    else
                    {
                        if (x < ulWidth)
                            // lower left sector - mirror around x-axis 
                            finalcolors[curIndex] = ulColors[x, ulColors.GetUpperBound(1) - y % ulHeight];
                        else
                            // lower right sector - mirror around both axes
                            finalcolors[curIndex] = ulColors[ulColors.GetUpperBound(0) - x % ulWidth,
                                                             ulColors.GetUpperBound(1) - y % ulHeight];
                    }
                }
            }

            ellipse.SetData(finalcolors);

            return ellipse;
        }

        /// <summary>
        /// Creates a description of an ellipse by returning a number of points on it's boundary.
        /// </summary>
        /// <param name="width">Width of the ellipse.</param>
        /// <param name="height">Height of the ellipse.</param>
        /// <param name="center">Center of the ellipse.</param>
        /// <param name="count">Number of points to be calculated.</param>
        /// <returns>A number of points that are on the boundary of the ellipse.</returns>
        private static Vector2[] CalculateEllipseBoundary(int width, int height, Vector2 center, int count)
        {
            Vector2[] vectorCache = new Vector2[count];
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;
            float accuracy = MathHelper.TwoPi / count;

            // calculate points
            for (int i = 0; i < count; i++)
                vectorCache[i] = center + new Vector2(halfWidth * (float)Math.Cos(i * accuracy),
                                                      halfHeight * (float)Math.Sin(i * accuracy));

            return vectorCache;
        }

        /// <summary>
        /// Finds the closest point on a given ellipse to a given point.
        /// </summary>
        /// <param name="point">Position to which the point on the ellipse should be closest to.</param>
        /// <param name="ellipseBoundary">Description of the ellipse.</param>
        /// <returns>Point on ellipse boundary that is the closest to point</returns>
        private static Vector2 ApproximateClosestPointOnEllipse(Vector2 point, Vector2[] ellipseBoundary)
        {
            // set the first point to be the one with the shortest distance
            int closestIndex = 0;
            float shortestDistance = (ellipseBoundary[0] - point).Length();

            // check whether any other point is closer to the given point
            for (int i = 1; i < ellipseBoundary.Length; i++)
            {
                Vector2 curPointOnBoundary = ellipseBoundary[i];
                float curDistance = (point - curPointOnBoundary).Length();

                if (curDistance < shortestDistance)
                {
                    closestIndex = i;
                    shortestDistance = curDistance;
                }
            }

            return ellipseBoundary[closestIndex];
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