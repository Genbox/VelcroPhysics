/*
* C# Version Ported by Matt Bettcher and Ian Qvist 2009-2010
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

using System.Collections.Generic;
using System.Diagnostics;

namespace FarseerPhysics.Common.PolygonManipulation
{
    public static class SimpleCombiner
    {
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
        ///<param name="maxPolys">The maximun number of polygons</param>
        ///<param name="tolerance">The tolerance</param>
        ///<returns></returns>
        public static List<Vertices> PolygonizeTriangles(List<Vertices> triangulated, int maxPolys = int.MaxValue, float tolerance = 0)
        {
            List<Vertices> polys = new List<Vertices>();

            int polyIndex = 0;

            if (triangulated.Count <= 0)
            {
                //return empty polygon list
                return polys;
            }

            bool[] covered = new bool[triangulated.Count];
            for (int i = 0; i < triangulated.Count; ++i)
            {
                covered[i] = false;

                //Check here for degenerate triangles
                if (((triangulated[i][0].X == triangulated[i][1].X) && (triangulated[i][0].Y == triangulated[i][1].Y))
                    ||
                    ((triangulated[i][1].X == triangulated[i][2].X) && (triangulated[i][1].Y == triangulated[i][2].Y))
                    ||
                    ((triangulated[i][0].X == triangulated[i][2].X) && (triangulated[i][0].Y == triangulated[i][2].Y)))
                {
                    covered[i] = true;
                }
            }

            bool notDone = true;
            while (notDone)
            {
                int currTri = -1;
                for (int i = 0; i < triangulated.Count; ++i)
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
                    Vertices poly = new Vertices(3);

                    for (int i = 0; i < 3; i++)
                    {
                        poly.Add(triangulated[currTri][i]);
                    }

                    covered[currTri] = true;
                    int index = 0;
                    for (int i = 0; i < 2 * triangulated.Count; ++i, ++index)
                    {
                        while (index >= triangulated.Count) index -= triangulated.Count;
                        if (covered[index])
                        {
                            continue;
                        }
                        Vertices newP = AddTriangle(triangulated[index], poly);
                        if (newP == null)
                            continue; // is this right

                        if (newP.Count > Settings.MaxPolygonVertices)
                            continue;

                        if (newP.IsConvex())
                        {
                            //Or should it be IsUsable?  Maybe re-write IsConvex to apply the angle threshold from Box2d
                            poly = new Vertices(newP);
                            covered[index] = true;
                        }
                    }

                    //We have a maximum of polygons that we need to keep under.
                    if (polyIndex < maxPolys)
                    {
                        SimplifyTools.MergeParallelEdges(poly, tolerance);

                        //If identical points are present, a triangle gets
                        //borked by the MergeParallelEdges function, hence
                        //the vertex number check
                        if (poly.Count >= 3)
                            polys.Add(new Vertices(poly));
                        else
                            Debug.WriteLine("Skipping corrupt poly.");
                    }

                    if (poly.Count >= 3)
                        polyIndex++; //Must be outside (polyIndex < polysLength) test
                }
            }

            //TODO: Add sanity check
            //Remove empty vertice collections
            for (int i = polys.Count - 1; i >= 0; i--)
            {
                if (polys[i].Count == 0)
                    polys.RemoveAt(i);
            }

            return polys;
        }

        private static Vertices AddTriangle(Vertices t, Vertices vertices)
        {
            // First, find vertices that connect
            int firstP = -1;
            int firstT = -1;
            int secondP = -1;
            int secondT = -1;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (t[0].X == vertices[i].X && t[0].Y == vertices[i].Y)
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
                else if (t[1].X == vertices[i].X && t[1].Y == vertices[i].Y)
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
                else if (t[2].X == vertices[i].X && t[2].Y == vertices[i].Y)
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
            if (firstP == 0 && secondP == vertices.Count - 1)
            {
                firstP = vertices.Count - 1;
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

            Vertices result = new Vertices(vertices.Count + 1);
            for (int i = 0; i < vertices.Count; i++)
            {
                result.Add(vertices[i]);

                if (i == firstP)
                    result.Add(t[tipT]);
            }

            return result;
        }
    }
}
