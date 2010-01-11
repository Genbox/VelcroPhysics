using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.Decomposition
{
    public class EarclipDecomposer
    {
        /*
         * C# Version Ported by Matt Bettcher 2009
         * 
         * Original C++ Version Copyright (c) 2007 Eric Jordan
         *
         * This software is provided 'as-is', without any express or implied
         * warranty.  In no event will the authors be held liable for any damages
         * arising from the use of this software.
         * Permission is granted to anyone to use this software for any purpose,
         * including commercial applications, and to alter it and redistribute it
         * freely, subject to the following restrictions:
         * 1. The origin of this software must not be misrepresented; you must not
         * claim that you wrote the original software. If you use this software
         * in a product, an acknowledgment in the product documentation would be
         * appreciated but is not required.
         * 2. Altered source versions must be plainly marked as such, and must not be
         * misrepresented as being the original software.
         * 3. This notice may not be removed or altered from any source distribution.
         */

        private class Triangle
        {
            public float[] x;
            public float[] y;

            //Constructor automatically fixes orientation to ccw
            public Triangle(float x1, float y1, float x2, float y2, float x3, float y3)
            {
                x = new float[3];
                y = new float[3];
                float dx1 = x2 - x1;
                float dx2 = x3 - x1;
                float dy1 = y2 - y1;
                float dy2 = y3 - y1;
                float cross = dx1 * dy2 - dx2 * dy1;
                bool ccw = (cross > 0);
                if (ccw)
                {
                    x[0] = x1; x[1] = x2; x[2] = x3;
                    y[0] = y1; y[1] = y2; y[2] = y3;
                }
                else
                {
                    x[0] = x1; x[1] = x3; x[2] = x2;
                    y[0] = y1; y[1] = y3; y[2] = y2;
                }
            }

            //public Triangle()
            //{
            //    x = new float[3];
            //    y = new float[3];
            //}

            public Triangle(Triangle t)
            {
                x = new float[3];
                y = new float[3];

                x[0] = t.x[0]; x[1] = t.x[1]; x[2] = t.x[2];
                y[0] = t.y[0]; y[1] = t.y[1]; y[2] = t.y[2];
            }

            //public void Set(ref Triangle toMe)
            //{
            //    for (int i = 0; i < 3; ++i)
            //    {
            //        x[i] = toMe.x[i];
            //        y[i] = toMe.y[i];
            //    }
            //}

            public bool IsInside(float _x, float _y)
            {
                if (_x < x[0] && _x < x[1] && _x < x[2]) return false;
                if (_x > x[0] && _x > x[1] && _x > x[2]) return false;
                if (_y < y[0] && _y < y[1] && _y < y[2]) return false;
                if (_y > y[0] && _y > y[1] && _y > y[2]) return false;

                float vx2 = _x - x[0]; float vy2 = _y - y[0];
                float vx1 = x[1] - x[0]; float vy1 = y[1] - y[0];
                float vx0 = x[2] - x[0]; float vy0 = y[2] - y[0];

                float dot00 = vx0 * vx0 + vy0 * vy0;
                float dot01 = vx0 * vx1 + vy0 * vy1;
                float dot02 = vx0 * vx2 + vy0 * vy2;
                float dot11 = vx1 * vx1 + vy1 * vy1;
                float dot12 = vx1 * vx2 + vy1 * vy2;
                float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
                float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
                float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

                return ((u > 0) && (v > 0) && (u + v < 1));
            }
        }

        internal class Polygon
        {
            private const int maxVerticesPerPolygon = 32;
            private const float angularSlop = 1.0f / 180.0f * (float)Math.PI; // 1 degrees

            private float[] x; //vertex arrays
            private float[] y;
            private int nVertices;
            private float area;

            private Polygon(float[] _x, float[] _y, int nVert)
            {
                nVertices = nVert;
                x = new float[nVertices];
                y = new float[nVertices];
                for (int i = 0; i < nVertices; ++i)
                {
                    x[i] = _x[i];
                    y[i] = _y[i];
                }
            }

            private Polygon(Vector2[] v, int nVert)
            {
                nVertices = nVert;
                x = new float[nVertices];
                y = new float[nVertices];
                for (int i = 0; i < nVertices; ++i)
                {
                    x[i] = v[i].X;
                    y[i] = v[i].Y;

                }
            }

            private Polygon()
            {
                x = null;
                y = null;
                nVertices = 0;
            }

            private float GetArea()
            {
                area = 0.0f;

                //First do wraparound
                area += x[nVertices - 1] * y[0] - x[0] * y[nVertices - 1];
                for (int i = 0; i < nVertices - 1; ++i)
                {
                    area += x[i] * y[i + 1] - x[i + 1] * y[i];
                }
                area *= .5f;
                return area;
            }

            private bool IsCCW()
            {
                return (GetArea() > 0.0f);
            }

            private void MergeParallelEdges(float tolerance)
            {
                if (nVertices <= 3) return;             //Can't do anything useful here to a triangle
                bool[] mergeMe = new bool[nVertices];
                int newNVertices = nVertices;
                for (int i = 0; i < nVertices; ++i)
                {
                    int lower = (i == 0) ? (nVertices - 1) : (i - 1);
                    int middle = i;
                    int upper = (i == nVertices - 1) ? (0) : (i + 1);
                    float dx0 = x[middle] - x[lower];
                    float dy0 = y[middle] - y[lower];
                    float dx1 = x[upper] - x[middle];
                    float dy1 = y[upper] - y[middle];
                    float norm0 = (float)Math.Sqrt(dx0 * dx0 + dy0 * dy0);
                    float norm1 = (float)Math.Sqrt(dx1 * dx1 + dy1 * dy1);
                    if (!(norm0 > 0.0f && norm1 > 0.0f) && newNVertices > 3)
                    {
                        //Merge identical points
                        mergeMe[i] = true;
                        --newNVertices;
                    }
                    dx0 /= norm0; dy0 /= norm0;
                    dx1 /= norm1; dy1 /= norm1;
                    float cross = dx0 * dy1 - dx1 * dy0;
                    float dot = dx0 * dx1 + dy0 * dy1;
                    if (Math.Abs(cross) < tolerance && dot > 0 && newNVertices > 3)
                    {
                        mergeMe[i] = true;
                        --newNVertices;
                    }
                    else
                    {
                        mergeMe[i] = false;
                    }
                }
                if (newNVertices == nVertices || newNVertices == 0)
                {
                    return;
                }
                float[] newx = new float[newNVertices];
                float[] newy = new float[newNVertices];
                int currIndex = 0;
                for (int i = 0; i < nVertices; ++i)
                {
                    if (mergeMe[i] || newNVertices == 0 || currIndex == newNVertices) continue;

                    //b2Assert(currIndex < newNVertices);
                    newx[currIndex] = x[i];
                    newy[currIndex] = y[i];
                    ++currIndex;
                }

                x = newx;
                y = newy;
                nVertices = newNVertices;
                //	printf("%d \n", newNVertices);
            }

            private Polygon(Triangle t)
            {
                nVertices = 3;
                x = new float[nVertices];
                y = new float[nVertices];
                for (int i = 0; i < nVertices; ++i)
                {
                    x[i] = t.x[i];
                    y[i] = t.y[i];
                }
            }

            private Polygon(Polygon p)
            {
                nVertices = p.nVertices;
                x = new float[nVertices];
                y = new float[nVertices];
                for (int i = 0; i < nVertices; ++i)
                {
                    x[i] = p.x[i];
                    y[i] = p.y[i];
                }
            }

            private void Set(Polygon p)
            {
                if (nVertices != p.nVertices)
                {
                    nVertices = p.nVertices;

                    x = new float[nVertices];
                    y = new float[nVertices];
                }

                for (int i = 0; i < nVertices; ++i)
                {
                    x[i] = p.x[i];
                    y[i] = p.y[i];
                }
            }

            /// <summary>
            /// Assuming the polygon is simple, checks if it is convex.
            /// </summary>
            /// <returns>
            /// 	<c>true</c> if this instance is convex; otherwise, <c>false</c>.
            /// </returns>
            private bool IsConvex()
            {
                bool isPositive = false;
                for (int i = 0; i < nVertices; ++i)
                {
                    int lower = (i == 0) ? (nVertices - 1) : (i - 1);
                    int middle = i;
                    int upper = (i == nVertices - 1) ? (0) : (i + 1);
                    float dx0 = x[middle] - x[lower];
                    float dy0 = y[middle] - y[lower];
                    float dx1 = x[upper] - x[middle];
                    float dy1 = y[upper] - y[middle];
                    float cross = dx0 * dy1 - dx1 * dy0;
                    // Cross product should have same sign
                    // for each vertex if poly is convex.
                    bool newIsP = (cross >= 0) ? true : false;
                    if (i == 0)
                    {
                        isPositive = newIsP;
                    }
                    else if (isPositive != newIsP)
                    {
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// Tries to add a triangle to the polygon. Returns null if it can't connect
            /// properly, otherwise returns a pointer to the new Polygon. Assumes bitwise
            /// equality of joined vertex positions.
            ///
            /// For internal use.
            /// </summary>
            /// <param name="t">The triangle to add.</param>
            /// <returns></returns>
            private Polygon Add(Triangle t)
            {
                //		float32 equalTol = .001f;
                // First, find vertices that connect
                int firstP = -1;
                int firstT = -1;
                int secondP = -1;
                int secondT = -1;
                for (int i = 0; i < nVertices; i++)
                {
                    if (t.x[0] == x[i] && t.y[0] == y[i])
                    {
                        if (firstP == -1)
                        {
                            firstP = i;
                            firstT = 0;
                        }
                        else
                        {
                            secondP = i;
                            secondT = 0;
                        }
                    }
                    else if (t.x[1] == x[i] && t.y[1] == y[i])
                    {
                        if (firstP == -1)
                        {
                            firstP = i;
                            firstT = 1;
                        }
                        else
                        {
                            secondP = i;
                            secondT = 1;
                        }
                    }
                    else if (t.x[2] == x[i] && t.y[2] == y[i])
                    {
                        if (firstP == -1)
                        {
                            firstP = i;
                            firstT = 2;
                        }
                        else
                        {
                            secondP = i;
                            secondT = 2;
                        }
                    }
                }
                // Fix ordering if first should be last vertex of poly
                if (firstP == 0 && secondP == nVertices - 1)
                {
                    firstP = nVertices - 1;
                    secondP = 0;
                }

                // Didn't find it
                if (secondP == -1)
                {
                    return null;
                }

                // Find tip index on triangle
                int tipT = 0;
                if (tipT == firstT || tipT == secondT)
                    tipT = 1;
                if (tipT == firstT || tipT == secondT)
                    tipT = 2;

                float[] newx = new float[nVertices + 1];
                float[] newy = new float[nVertices + 1];
                int currOut = 0;
                for (int i = 0; i < nVertices; i++)
                {
                    newx[currOut] = x[i];
                    newy[currOut] = y[i];
                    if (i == firstP)
                    {
                        ++currOut;
                        newx[currOut] = t.x[tipT];
                        newy[currOut] = t.y[tipT];
                    }
                    ++currOut;
                }
                Polygon result = new Polygon(newx, newy, nVertices + 1);

                return result;
            }

            /// <summary>
            /// Finds and fixes "pinch points," points where two polygon
            /// vertices are at the same point.
            /// If a pinch point is found, pin is broken up into poutA and poutB
            /// and true is returned; otherwise, returns false.
            /// Mostly for internal use.
            /// </summary>
            /// <param name="pin">The pin.</param>
            /// <param name="poutA">The pout A.</param>
            /// <param name="poutB">The pout B.</param>
            /// <returns></returns>
            private static bool ResolvePinchPoint(Polygon pin, out Polygon poutA, out Polygon poutB)
            {
                poutA = new Polygon();
                poutB = new Polygon();

                if (pin.nVertices < 3) return false;
                const float tol = .001f;
                bool hasPinchPoint = false;
                int pinchIndexA = -1;
                int pinchIndexB = -1;
                for (int i = 0; i < pin.nVertices; ++i)
                {
                    for (int j = i + 1; j < pin.nVertices; ++j)
                    {
                        //Don't worry about pinch points where the points
                        //are actually just dupe neighbors
                        if (Math.Abs(pin.x[i] - pin.x[j]) < tol && Math.Abs(pin.y[i] - pin.y[j]) < tol && j != i + 1)
                        {
                            pinchIndexA = i;
                            pinchIndexB = j;
                            //printf("pinch: %f, %f == %f, %f\n",pin.x[i],pin.y[i],pin.x[j],pin.y[j]);
                            //printf("at indexes %d, %d\n",i,j);
                            hasPinchPoint = true;
                            break;
                        }
                    }
                    if (hasPinchPoint) break;
                }
                if (hasPinchPoint)
                {
                    //printf("Found pinch point\n");
                    int sizeA = pinchIndexB - pinchIndexA;
                    if (sizeA == pin.nVertices) return false;//has dupe points at wraparound, not a problem here
                    float[] xA = new float[sizeA];
                    float[] yA = new float[sizeA];
                    for (int i = 0; i < sizeA; ++i)
                    {
                        int ind = Remainder(pinchIndexA + i, pin.nVertices);             // is this right
                        xA[i] = pin.x[ind];
                        yA[i] = pin.y[ind];
                    }
                    Polygon tempA = new Polygon(xA, yA, sizeA);
                    poutA.Set(tempA);

                    int sizeB = pin.nVertices - sizeA;
                    float[] xB = new float[sizeB];
                    float[] yB = new float[sizeB];
                    for (int i = 0; i < sizeB; ++i)
                    {
                        int ind = Remainder(pinchIndexB + i, pin.nVertices);          // is this right    
                        xB[i] = pin.x[ind];
                        yB[i] = pin.y[ind];
                    }
                    Polygon tempB = new Polygon(xB, yB, sizeB);
                    poutB.Set(tempB);
                    //printf("Size of a: %d, size of b: %d\n",sizeA,sizeB);
                }
                return hasPinchPoint;
            }

            /// <summary>
            /// Triangulates a polygon using simple ear-clipping algorithm. Returns
            /// size of Triangle array unless the polygon can't be triangulated.
            /// This should only happen if the polygon self-intersects,
            /// though it will not _always_ return null for a bad polygon - it is the
            /// caller's responsibility to check for self-intersection, and if it
            /// doesn't, it should at least check that the return value is non-null
            /// before using. You're warned!
            ///
            /// Triangles may be degenerate, especially if you have identical points
            /// in the input to the algorithm.  Check this before you use them.
            ///
            /// This is totally unoptimized, so for large polygons it should not be part
            /// of the simulation loop.
            ///
            /// Returns:
            /// -1 if algorithm fails (self-intersection most likely)
            /// 0 if there are not enough vertices to triangulate anything.
            /// Number of triangles if triangulation was successful.
            ///
            /// results will be filled with results - ear clipping always creates vNum - 2
            /// or fewer (due to pinch point polygon snipping), so allocate an array of
            /// this size.
            /// </summary>
            /// <param name="xv">The xv.</param>
            /// <param name="yv">The yv.</param>
            /// <param name="vNum">The v num.</param>
            /// <param name="results">The results.</param>
            /// <returns></returns>
            private static int TriangulatePolygon(float[] xv, float[] yv, int vNum, out Triangle[] results)
            {
                results = new Triangle[175];

                if (vNum < 3)
                    return 0;

                //Recurse and split on pinch points
                Polygon pA, pB;
                Polygon pin = new Polygon(xv, yv, vNum);
                if (ResolvePinchPoint(pin, out pA, out pB))
                {
                    Triangle[] mergeA = new Triangle[pA.nVertices];
                    Triangle[] mergeB = new Triangle[pB.nVertices];
                    int nA = TriangulatePolygon(pA.x, pA.y, pA.nVertices, out mergeA);
                    int nB = TriangulatePolygon(pB.x, pB.y, pB.nVertices, out mergeB);
                    if (nA == -1 || nB == -1)
                    {
                        return -1;
                    }
                    for (int i = 0; i < nA; ++i)
                    {
                        results[i] = new Triangle(mergeA[i]);
                    }
                    for (int i = 0; i < nB; ++i)
                    {
                        results[nA + i] = new Triangle(mergeB[i]);
                    }
                    return (nA + nB);
                }

                Triangle[] buffer = new Triangle[vNum - 2];
                int bufferSize = 0;
                float[] xrem = new float[vNum];
                float[] yrem = new float[vNum];
                for (int i = 0; i < vNum; ++i)
                {
                    xrem[i] = xv[i];
                    yrem[i] = yv[i];
                }

                while (vNum > 3)
                {
                    // Find an ear
                    int earIndex = -1;
                    //float32 earVolume = -1.0f;
                    float earMaxMinCross = -10.0f;
                    for (int i = 0; i < vNum; ++i)
                    {
                        if (IsEar(i, xrem, yrem, vNum))
                        {
                            int lower = Remainder(i - 1, vNum);
                            int upper = Remainder(i + 1, vNum);
                            Vector2 d1 = new Vector2(xrem[upper] - xrem[i], yrem[upper] - yrem[i]);
                            Vector2 d2 = new Vector2(xrem[i] - xrem[lower], yrem[i] - yrem[lower]);
                            Vector2 d3 = new Vector2(xrem[lower] - xrem[upper], yrem[lower] - yrem[upper]);

                            d1.Normalize();
                            d2.Normalize();
                            d3.Normalize();
                            float cross12;
                            MathUtils.Cross(ref d1, ref d2, out cross12);
                            cross12 = Math.Abs(cross12);

                            float cross23;
                            MathUtils.Cross(ref d2, ref d3, out cross23);
                            cross23 = Math.Abs(cross23);

                            float cross31;
                            MathUtils.Cross(ref d3, ref d1, out cross31);
                            cross31 = Math.Abs(cross31);

                            //Find the maximum minimum angle
                            float minCross = Math.Min(cross12, Math.Min(cross23, cross31));
                            if (minCross > earMaxMinCross)
                            {
                                earIndex = i;
                                earMaxMinCross = minCross;
                            }

                            /*//This bit chooses the ear with greatest volume first
                            float32 testVol = b2Abs( d1.x*d2.y-d2.x*d1.y );
                            if (testVol > earVolume){
                                earIndex = i;
                                earVolume = testVol;
                            }*/
                        }
                    }

                    // If we still haven't found an ear, we're screwed.
                    // Note: sometimes this is happening because the
                    // remaining points are collinear.  Really these
                    // should just be thrown out without halting triangulation.
                    if (earIndex == -1)
                    {
                        for (int i = 0; i < bufferSize; i++)
                        {
                            results[i] = new Triangle(buffer[i]);
                        }

                        if (bufferSize > 0)
                            return bufferSize;

                        return -1;
                    }

                    // Clip off the ear:
                    // - remove the ear tip from the list

                    --vNum;
                    float[] newx = new float[vNum];
                    float[] newy = new float[vNum];
                    int currDest = 0;
                    for (int i = 0; i < vNum; ++i)
                    {
                        if (currDest == earIndex) ++currDest;
                        newx[i] = xrem[currDest];
                        newy[i] = yrem[currDest];
                        ++currDest;
                    }

                    // - add the clipped triangle to the triangle list
                    int under = (earIndex == 0) ? (vNum) : (earIndex - 1);
                    int over = (earIndex == vNum) ? 0 : (earIndex + 1);
                    Triangle toAdd = new Triangle(xrem[earIndex], yrem[earIndex], xrem[over], yrem[over], xrem[under], yrem[under]);
                    buffer[bufferSize] = new Triangle(toAdd);
                    ++bufferSize;

                    // - replace the old list with the new one
                    xrem = newx;
                    yrem = newy;
                }

                Triangle tooAdd = new Triangle(xrem[1], yrem[1], xrem[2], yrem[2],
                                          xrem[0], yrem[0]);
                buffer[bufferSize] = new Triangle(tooAdd);
                ++bufferSize;

                //b2Assert(bufferSize == xremLength-2);

                for (int i = 0; i < bufferSize; i++)
                {
                    results[i] = new Triangle(buffer[i]);
                }

                return bufferSize;
            }

            /// <summary>
            /// Fix for obnoxious behavior for the % operator for negative numbers...
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="modulus">The modulus.</param>
            /// <returns></returns>
            private static int Remainder(int x, int modulus)
            {
                int rem = x % modulus;
                while (rem < 0)
                {
                    rem += modulus;
                }
                return rem;
            }

            /// <summary>
            /// Turns a list of triangles into a list of convex polygons. Very simple
            /// method - start with a seed triangle, keep adding triangles to it until
            /// you can't add any more without making the polygon non-convex.
            ///
            /// Returns an integer telling how many polygons were created.  Will fill
            /// polys array up to polysLength entries, which may be smaller or larger
            /// than the return value.
            /// 
            /// Takes O(N///P) where P is the number of resultant polygons, N is triangle
            /// count.
            /// 
            /// The final polygon list will not necessarily be minimal, though in
            /// practice it works fairly well.
            /// </summary>
            /// <param name="triangulated">The triangulated.</param>
            /// <param name="triangulatedLength">Length of the triangulated.</param>
            /// <param name="polys">The polys.</param>
            /// <param name="polysLength">Length of the polys.</param>
            /// <returns></returns>
            private static int PolygonizeTriangles(Triangle[] triangulated, int triangulatedLength, out Polygon[] polys, int polysLength)
            {
                int polyIndex = 0;
                polys = new Polygon[50];

                if (triangulatedLength <= 0)
                {
                    return 0;
                }
                bool[] covered = new bool[triangulatedLength];
                for (int i = 0; i < triangulatedLength; ++i)
                {
                    covered[i] = false;
                    //Check here for degenerate triangles
                    if (((triangulated[i].x[0] == triangulated[i].x[1]) && (triangulated[i].y[0] == triangulated[i].y[1]))
                         || ((triangulated[i].x[1] == triangulated[i].x[2]) && (triangulated[i].y[1] == triangulated[i].y[2]))
                         || ((triangulated[i].x[0] == triangulated[i].x[2]) && (triangulated[i].y[0] == triangulated[i].y[2])))
                    {
                        covered[i] = true;
                    }
                }

                bool notDone = true;
                while (notDone)
                {
                    int currTri = -1;
                    for (int i = 0; i < triangulatedLength; ++i)
                    {
                        if (covered[i])
                            continue;
                        currTri = i;
                        break;
                    }
                    if (currTri == -1)
                    {
                        notDone = false;
                    }
                    else
                    {
                        Polygon poly = new Polygon(triangulated[currTri]);
                        covered[currTri] = true;
                        int index = 0;
                        for (int i = 0; i < 2 * triangulatedLength; ++i, ++index)
                        {
                            while (index >= triangulatedLength) index -= triangulatedLength;
                            if (covered[index])
                            {
                                continue;
                            }
                            Polygon newP = poly.Add(triangulated[index]);
                            if (newP == null)
                            {                                 // is this right
                                continue;
                            }
                            if (newP.nVertices > maxVerticesPerPolygon)
                            {
                                newP = null;
                                continue;
                            }
                            if (newP.IsConvex())
                            { //Or should it be IsUsable?  Maybe re-write IsConvex to apply the angle threshold from Box2d
                                poly = new Polygon(newP);
                                newP = null;
                                covered[index] = true;
                            }
                            else
                            {
                                newP = null;
                            }
                        }
                        if (polyIndex < polysLength)
                        {
                            poly.MergeParallelEdges(angularSlop);
                            //If identical points are present, a triangle gets
                            //borked by the MergeParallelEdges function, hence
                            //the vertex number check
                            if (poly.nVertices >= 3) polys[polyIndex] = new Polygon(poly);
                            //else printf("Skipping corrupt poly\n");
                        }
                        if (poly.nVertices >= 3) polyIndex++; //Must be outside (polyIndex < polysLength) test
                    }
                }
                return polyIndex;
            }

            /// <summary>
            /// Checks if vertex i is the tip of an ear in polygon defined by xv[] and
            /// yv[].
            ///
            /// Assumes clockwise orientation of polygon...ick
            /// </summary>
            /// <param name="i">The i.</param>
            /// <param name="xv">The xv.</param>
            /// <param name="yv">The yv.</param>
            /// <param name="xvLength">Length of the xv.</param>
            /// <returns>
            /// 	<c>true</c> if the specified i is ear; otherwise, <c>false</c>.
            /// </returns>
            private static bool IsEar(int i, float[] xv, float[] yv, int xvLength)
            {
                float dx0, dy0, dx1, dy1;
                if (i >= xvLength || i < 0 || xvLength < 3)
                {
                    return false;
                }
                int upper = i + 1;
                int lower = i - 1;
                if (i == 0)
                {
                    dx0 = xv[0] - xv[xvLength - 1];
                    dy0 = yv[0] - yv[xvLength - 1];
                    dx1 = xv[1] - xv[0];
                    dy1 = yv[1] - yv[0];
                    lower = xvLength - 1;
                }
                else if (i == xvLength - 1)
                {
                    dx0 = xv[i] - xv[i - 1];
                    dy0 = yv[i] - yv[i - 1];
                    dx1 = xv[0] - xv[i];
                    dy1 = yv[0] - yv[i];
                    upper = 0;
                }
                else
                {
                    dx0 = xv[i] - xv[i - 1];
                    dy0 = yv[i] - yv[i - 1];
                    dx1 = xv[i + 1] - xv[i];
                    dy1 = yv[i + 1] - yv[i];
                }
                float cross = dx0 * dy1 - dx1 * dy0;
                if (cross > 0)
                    return false;
                Triangle myTri = new Triangle(xv[i], yv[i], xv[upper], yv[upper],
                                          xv[lower], yv[lower]);
                for (int j = 0; j < xvLength; ++j)
                {
                    if (j == i || j == lower || j == upper)
                        continue;
                    if (myTri.IsInside(xv[j], yv[j]))
                        return false;
                }
                return true;
            }

            private static void ReversePolygon(float[] x, float[] y, int n)
            {
                if (n == 1)
                    return;
                int low = 0;
                int high = n - 1;
                while (low < high)
                {
                    float buffer = x[low];
                    x[low] = x[high];
                    x[high] = buffer;
                    buffer = y[low];
                    y[low] = y[high];
                    y[high] = buffer;
                    ++low;
                    --high;
                }
            }

            /// <summary>
            /// Decomposes a non-convex polygon into a number of convex polygons, up
            /// to maxPolys (remaining pieces are thrown out, but the total number
            /// is returned, so the return value can be greater than maxPolys).
            ///
            /// Each resulting polygon will have no more than maxVerticesPerPolygon
            /// vertices (set to b2MaxPolyVertices by default, though you can change
            /// this).
            /// 
            /// Returns -1 if operation fails (usually due to self-intersection of
            /// polygon).
            /// </summary>
            /// <param name="p">The p.</param>
            /// <param name="results">The results.</param>
            /// <param name="maxPolys">The max polys.</param>
            /// <returns></returns>
            private static int DecomposeConvex(Polygon p, out Polygon[] results, int maxPolys)
            {
                results = new Polygon[1];

                if (p.nVertices < 3) return 0;

                Triangle[] triangulated = new Triangle[p.nVertices - 2];
                int nTri;
                if (p.IsCCW())
                {
                    //printf("It is ccw \n");
                    Polygon tempP = new Polygon(p);
                    ReversePolygon(tempP.x, tempP.y, tempP.nVertices);
                    nTri = TriangulatePolygon(tempP.x, tempP.y, tempP.nVertices, out triangulated);
                    //			ReversePolygon(p->x, p->y, p->nVertices); //reset orientation
                }
                else
                {
                    //printf("It is not ccw \n");
                    nTri = TriangulatePolygon(p.x, p.y, p.nVertices, out triangulated);
                }
                if (nTri < 1)
                {
                    //Still no luck?  Oh well...
                    return -1;
                }
                int nPolys = PolygonizeTriangles(triangulated, nTri, out results, maxPolys);
                return nPolys;
            }

            public static Vertices[] DecomposeVertices(Vertices v, int max)
            {
                Polygon p = new Polygon(v.ToArray(), v.Count);      // convert the vertices to a polygon

                Polygon[] output;

                DecomposeConvex(p, out output, max);

                Vertices[] verticesOut = new Vertices[output.Length];

                int i;

                for (i = 0; i < output.Length; i++)
                {
                    if (output[i] != null)
                    {
                        verticesOut[i] = new Vertices();

                        for (int j = 0; j < output[i].nVertices; j++)
                            verticesOut[i].Add(new Vector2(output[i].x[j], output[i].y[j]));
                    }
                    else
                        break;
                }

                Vertices[] verts = new Vertices[i];
                for (int k = 0; k < i; k++)
                {
                    verts[k] = new Vertices(verticesOut[k]);
                }

                return verts;
            }
        }

    }
}
