using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Collision;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
    // Ported by Matthew Bettcher

    /// <summary>
    /// Given a position this delegate should return true if the given position is inside a solid area.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public delegate float Evaluate(float x, float y);

    public static class MarchingSquares
    {
        internal class GeomPolyVal
        {
            /** Associated polygon at coordinate **/
            /** Key of original sub-polygon **/
            public int Key;
            public Vertices P;

            public GeomPolyVal(Vertices p, int key)
            {
                P = p;
                Key = key;
            }
        }

        private static int[] _lookMarch = new[]
        {
            0x00, 0xE0, 0x38, 0xD8, 0x0E, 0xEE, 0x36, 0xD6, 0x83, 0x63, 0xBB, 0x5B,
            0x8D, 0x6D, 0xB5, 0x55
        };

        /// <summary>
        /// Linearly interpolate between (x0 to x1) given a value at these coordinates (v0 and v1) 
        /// such as to approximate value(return) = 0
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="x1"></param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        private static float Lerp(float x0, float x1, float v0, float v1)
        {
            float dv = v0 - v1;
            float g;
            bool t = dv * dv < float.Epsilon;
            if (t) g = 0.5f;
            else g = v0 / dv;
            return x0 + g * (x1 - x0);
        }

        // TODO make sure f is supposed to be a 2 dimensional array of floats
        /** Recursive linear interpolation for use in marching squares **/

        private static float Xlerp(float x0, float x1, float y, float v0, float v1, Evaluate f, int c)
        {
            float xm = Lerp(x0, x1, v0, v1);
            if (c == 0) return xm;
            else
            {
                float vm = f(xm, y);
                if (v0 * vm < 0) return Xlerp(x0, xm, y, v0, vm, f, c - 1);
                else return Xlerp(xm, x1, y, vm, v1, f, c - 1);
            }
        }

        // TODO make sure f is supposed to be a 2 dimensional array of floats
        /** Recursive linear interpolation for use in marching squares **/

        private static float Ylerp(float y0, float y1, float x, float v0, float v1, Evaluate f, int c)
        {
            float ym = Lerp(y0, y1, v0, v1);
            if (c == 0) return ym;
            else
            {
                float vm = f(x, ym);
                if (v0 * vm < 0) return Ylerp(y0, ym, x, v0, vm, f, c - 1);
                else return Ylerp(ym, y1, x, vm, v1, f, c - 1);
            }
        }

        public static List<Vertices> DetectSquares(AABB domain, float cellWidth, float cellHeight, Evaluate f,
                                                           int recursionLevels, bool combine)
        {
            // this is a list of all our scanline polygons for this row inside a list of of scanlines
            List<Vertices> polys = new List<Vertices>();

            float domainWidth = domain.UpperBound.X - domain.LowerBound.X;
            float domainHeight = domain.UpperBound.Y - domain.LowerBound.Y;

            int xn = (int)(domainWidth / cellWidth);
            bool xp = xn == (domainWidth / cellWidth);
            int yn = (int)(domainHeight / cellHeight);
            bool yp = yn == (domainHeight / cellHeight);
            if (!xp) xn++;
            if (!yp) yn++;

            float[,] fs = new float[xn + 1, yn + 1];

            // ps is only needed for combining slices together and is not needed yet!
            //ps = new GeomPolyVal[xn + 1, yn + 1];

            //populate shared function lookups.
            for (int x = 0; x < (xn + 1); x++)
            {
                float x0;
                if (x == xn)
                    x0 = domain.UpperBound.X;
                else
                    x0 = x * cellWidth + domain.LowerBound.X;

                for (int y = 0; y < (yn + 1); y++)
                {
                    float y0;
                    if (y == yn)
                        y0 = domain.UpperBound.Y;
                    else
                        y0 = y * cellHeight + domain.LowerBound.Y;
                    fs[x, y] = f(x0, y0);
                }
            }

            //generate sub-polys and combine to scan lines
            for (int y = 0; y < yn; y++)
            {
                float y0 = y * cellHeight + domain.LowerBound.Y;
                float y1;
                if (y == yn - 1)
                    y1 = domain.UpperBound.Y;
                else
                    y1 = y0 + cellHeight;

                // our big scanline poly
                LinkedList<Vector2> scanlinePoly = new LinkedList<Vector2>();
                LinkedListNode<Vector2> rightTopMost = null;
                LinkedListNode<Vector2> rightBottomMost = null;

                for (int x = 0; x < xn; x++)
                {
                    float x0 = x * cellWidth + domain.LowerBound.X;
                    float x1;
                    if (x == xn - 1)
                        x1 = domain.UpperBound.X;
                    else
                        x1 = x0 + cellWidth;

                    Vertices p = new Vertices();
                    var key = MarchSquare(f, fs, ref p, x, y, x0, y0, x1, y1, recursionLevels);

                    if (combine)
                    {
                        // In the new method we create scanline (read horizontal) polygons on the fly!
                        // These polygons must be as simple as possible.

                        // first get to the code to add the current key to the scanline polygon or
                        // end this scanline polygon.
                        switch (key)
                        {
                            case 1:
                                // scanline polygon finisher
                                rightBottomMost.Value = p[0];
                                polys.Add(new Vertices(scanlinePoly.ToList()));
                                break;
                            case 2:
                                // this is a starter key
                                scanlinePoly.Clear();
                                rightTopMost = scanlinePoly.AddLast(p[0]);
                                rightBottomMost = scanlinePoly.AddBefore(rightTopMost, p[2]);
                                rightBottomMost = scanlinePoly.AddBefore(rightBottomMost, p[1]);
                                break;
                            case 3:
                                // this square should add a top vertex and move the bottom
                                // first check if the previous point has the same y
                                if (rightTopMost.Previous.Value.Y == rightTopMost.Value.Y)
                                    rightTopMost.Value = p[0];
                                else
                                    rightTopMost = scanlinePoly.AddAfter(rightTopMost, p[0]);
                                rightBottomMost.Value = p[1];
                                break;
                            case 4:
                                // this is a starter key
                                scanlinePoly.Clear();
                                scanlinePoly.AddLast(p[0]);
                                rightTopMost = scanlinePoly.AddLast(p[1]);
                                rightBottomMost = scanlinePoly.AddLast(p[2]);
                                break;
                            case 5:
                                // this is the ambiguous case...
                                rightTopMost = scanlinePoly.AddAfter(rightTopMost, p[0]);
                                rightTopMost = scanlinePoly.AddAfter(rightTopMost, p[1]);
                                rightBottomMost = scanlinePoly.AddBefore(rightBottomMost, p[3]);
                                rightBottomMost = scanlinePoly.AddBefore(rightBottomMost, p[2]);
                                break;
                            case 6:
                                // this is a starter key
                                scanlinePoly.Clear();
                                scanlinePoly.AddLast(p[0]);
                                rightTopMost = scanlinePoly.AddLast(p[1]);
                                rightBottomMost = scanlinePoly.AddLast(p[2]);
                                scanlinePoly.AddLast(p[3]);
                                break;
                            case 7:
                                rightTopMost = scanlinePoly.AddAfter(rightTopMost, p[0]);
                                rightTopMost = scanlinePoly.AddAfter(rightTopMost, p[1]);
                                rightBottomMost.Value = p[2];
                                break;
                            case 8:
                                // scanline polygon finisher
                                rightTopMost.Value = p[1];
                                polys.Add(new Vertices(scanlinePoly.ToList()));
                                break;
                            case 9:
                                // scanline polygon finisher
                                rightTopMost.Value = p[1];
                                rightBottomMost.Value = p[2];
                                polys.Add(new Vertices(scanlinePoly.ToList()));
                                break;
                            case 10:
                                // this is the ambiguous case...
                                rightTopMost = scanlinePoly.AddAfter(rightTopMost, p[0]);
                                rightTopMost = scanlinePoly.AddAfter(rightTopMost, p[1]);
                                rightBottomMost = scanlinePoly.AddBefore(rightBottomMost, p[3]);
                                rightBottomMost = scanlinePoly.AddBefore(rightBottomMost, p[2]);
                                break;
                            case 11:
                                rightTopMost.Value = p[1];
                                rightTopMost = scanlinePoly.AddAfter(rightTopMost, p[2]);
                                rightBottomMost.Value = p[3];
                                break;
                            case 12:
                                rightTopMost.Value = p[1];
                                rightBottomMost = scanlinePoly.AddBefore(rightBottomMost, p[2]);
                                break;
                            case 13:
                                //
                                rightTopMost.Value = p[1];
                                rightBottomMost.Value = p[3];
                                rightBottomMost = scanlinePoly.AddBefore(rightBottomMost, p[2]);
                                break;
                            case 14:
                                rightTopMost.Value = p[1];
                                rightBottomMost = scanlinePoly.AddBefore(rightBottomMost, p[3]);
                                rightBottomMost = scanlinePoly.AddBefore(rightBottomMost, p[2]);
                                break;
                            case 15:
                                // this square just moves the top and bottom vertices over
                                rightTopMost.Value = p[1];
                                rightBottomMost.Value = p[2];
                                break;
                        }
                    }
                    else
                    {
                        if (p.Count > 0)
                        {
                            polys.Add(p);
                        }
                    }
                    // again ps will only be used when the combine code is finished
                    //ps[x, y] = new GeomPolyVal(p, key);
                }
            }

            return polys;
        }

        /// <summary>
        /// Perform a single celled marching square for for the given cell defined by (x0,y0) (x1,y1)
        /// using the function f for recursive interpolation, given the look-up table 'fs' of
        /// the values of 'f' at cell vertices with the result to be stored in 'poly' given the actual
        /// coordinates of 'ax' 'ay' in the marching squares mesh.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="fs"></param>
        /// <param name="poly"></param>
        /// <param name="ax"></param>
        /// <param name="ay"></param>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="bin"></param>
        /// <returns></returns>
        private static int MarchSquare(Evaluate f, float[,] fs, ref Vertices poly, int ax, int ay, float x0, float y0,
                                      float x1, float y1, int bin)
        {
            //key lookup
            int key = 0;
            float v0 = fs[ax, ay];
            if (v0 < 0) key |= 8;
            float v1 = fs[ax + 1, ay];
            if (v1 < 0) key |= 4;
            float v2 = fs[ax + 1, ay + 1];
            if (v2 < 0) key |= 2;
            float v3 = fs[ax, ay + 1];
            if (v3 < 0) key |= 1;

            int val = _lookMarch[key];
            if (val != 0)
            {
                int pi = 0; // null?
                for (int i = 0; i < 8; i++)
                {
                    Vector2 p;
                    if ((val & (1 << i)) != 0)
                    {
                        if (i == 7 && (val & 1) == 0)
                        {
                            p = new Vector2(x0, Ylerp(y0, y1, x0, v0, v3, f, bin));
                            poly.Add(p);
                        }
                        else
                        {
                            if (i == 0) p = new Vector2(x0, y0);
                            else if (i == 2) p = new Vector2(x1, y0);
                            else if (i == 4) p = new Vector2(x1, y1);
                            else if (i == 6) p = new Vector2(x0, y1);

                            else if (i == 1) p = new Vector2(Xlerp(x0, x1, y0, v0, v1, f, bin), y0);
                            else if (i == 5) p = new Vector2(Xlerp(x0, x1, y1, v3, v2, f, bin), y1);

                            else if (i == 3) p = new Vector2(x1, Ylerp(y0, y1, x1, v1, v2, f, bin));
                            else p = new Vector2(x0, Ylerp(y0, y1, x0, v0, v3, f, bin));

                            poly.Add(p);
                            pi = poly.IndexOf(p);
                        }
                    }
                }
                // what could possibly need simplified here?
                //poly.simplify(Const.EPSILON,Const.EPSILON);
            }
            return key;
        }
    }
}