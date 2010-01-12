using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        const int MaxConnected = 32;
        const float CollapseDistSqr = Settings.Epsilon * Settings.Epsilon;

        /**
         * Find the convex hull of a point cloud using "Gift-wrap" algorithm - start
         * with an extremal point, and walk around the outside edge by testing
         * angles.
         * 
         * Runs in O(N*S) time where S is number of sides of resulting polygon.
         * Worst case: point cloud is all vertices of convex polygon -> O(N^2).
         * 
         * There may be faster algorithms to do this, should you need one -
         * this is just the simplest. You can get O(N log N) expected time if you
         * try, I think, and O(N) if you restrict inputs to simple polygons.
         * 
         * Returns null if number of vertices passed is less than 3.
         * 
         * Results should be passed through convex decomposition afterwards
         * to ensure that each shape has few enough points to be used in Box2d.
         *
         * FIXME?: May be buggy with colinear points on hull. Couldn't find a test
         * case that resulted in wrong behavior. If one turns up, the solution is to
         * supplement angle check with an equality resolver that always picks the
         * longer edge. I think the current solution is working, though it sometimes
         * creates an extra edge along a line.
         */
        //Polygon ConvexHull(Vector2[] v, int nVert)
        //{
        //    float[] cloudX = new float[nVert];
        //    float[] cloudY = new float[nVert];
        //    for (int i = 0; i < nVert; ++i)
        //    {
        //        cloudX[i] = v[i].X;
        //        cloudY[i] = v[i].Y;
        //    }
        //    Polygon result = ConvexHull(cloudX, cloudY, nVert);
        //    return result;
        //}

        //Polygon ConvexHull(float[] cloudX, float[] cloudY, int nVert)
        //{
        //    Debug.Assert(nVert > 2);
        //    int[] edgeList = new int[nVert];
        //    int numEdges = 0;

        //    float minY = float.MaxValue;
        //    int minYIndex = nVert;
        //    for (int i = 0; i < nVert; ++i)
        //    {
        //        if (cloudY[i] < minY)
        //        {
        //            minY = cloudY[i];
        //            minYIndex = i;
        //        }
        //    }

        //    int startIndex = minYIndex;
        //    int winIndex = -1;
        //    float dx = -1.0f;
        //    float dy = 0.0f;
        //    while (winIndex != minYIndex)
        //    {
        //        float maxDot = -2.0f;
        //        float nrm;

        //        for (int i = 0; i < nVert; ++i)
        //        {
        //            if (i == startIndex)
        //                continue;
        //            float newdx = cloudX[i] - cloudX[startIndex];
        //            float newdy = cloudY[i] - cloudY[startIndex];
        //            nrm = (float)Math.Sqrt(newdx * newdx + newdy * newdy);
        //            nrm = (nrm == 0.0f) ? 1.0f : nrm;
        //            newdx /= nrm;
        //            newdy /= nrm;

        //            //Dot products act as proxy for angle
        //            //without requiring inverse trig.
        //            float newDot = newdx * dx + newdy * dy;
        //            if (newDot > maxDot)
        //            {
        //                maxDot = newDot;
        //                winIndex = i;
        //            }
        //        }
        //        edgeList[numEdges++] = winIndex;
        //        dx = cloudX[winIndex] - cloudX[startIndex];
        //        dy = cloudY[winIndex] - cloudY[startIndex];
        //        nrm = (float)Math.Sqrt(dx * dx + dy * dy);
        //        nrm = (nrm == 0.0f) ? 1.0f : nrm;
        //        dx /= nrm;
        //        dy /= nrm;
        //        startIndex = winIndex;
        //    }

        //    float[] xres = new float[numEdges];
        //    float[] yres = new float[numEdges];
        //    for (int i = 0; i < numEdges; i++)
        //    {
        //        xres[i] = cloudX[edgeList[i]];
        //        yres[i] = cloudY[edgeList[i]];
        //        Debug.WriteLine(string.Format("{0}, {1}", xres[i], yres[i]));
        //    }

        //    Polygon returnVal = new Polygon(xres, yres, numEdges);
        //    returnVal.MergeParallelEdges(Settings.AngularSlop);
        //    return returnVal;
        //}

        /*
        Method:
        Start at vertex with minimum y (pick maximum x one if there are two).  
        We aim our "lastDir" vector at (1.0, 0)
        We look at the two rays going off from our start vertex, and follow whichever
        has the smallest angle (in -Pi . Pi) wrt lastDir ("rightest" turn)

        Loop until we hit starting vertex:

        We add our current vertex to the list.
        We check the seg from current vertex to next vertex for intersections
          - if no intersections, follow to next vertex and continue
          - if intersections, pick one with minimum distance
            - if more than one, pick one with "rightest" next point (two possibilities for each)

        */
        //Polygon TraceEdge(Polygon p)
        //{
        //    PolyNode[] nodes = new PolyNode[p.nVertices * p.nVertices];//overkill, but sufficient (order of mag. is right)
        //    int nNodes = 0;

        //    //Add base nodes (raw outline)
        //    for (int i = 0; i < p.nVertices; ++i)
        //    {
        //        Vector2 pos = new Vector2(p.x[i], p.y[i]);
        //        nodes[i].position = pos;
        //        ++nNodes;
        //        int iplus = (i == p.nVertices - 1) ? 0 : i + 1;
        //        int iminus = (i == 0) ? p.nVertices - 1 : i - 1;
        //        nodes[i].AddConnection(nodes[iplus]);
        //        nodes[i].AddConnection(nodes[iminus]);
        //    }

        //    //Process intersection nodes - horribly inefficient
        //    bool dirty = true;
        //    int counter = 0;
        //    while (dirty)
        //    {
        //        dirty = false;
        //        for (int i = 0; i < nNodes; ++i)
        //        {
        //            for (int j = 0; j < nodes[i].nConnected; ++j)
        //            {
        //                for (int k = 0; k < nNodes; ++k)
        //                {
        //                    if (k == i || nodes[k] == nodes[i].connected[j]) continue;
        //                    for (int l = 0; l < nodes[k].nConnected; ++l)
        //                    {

        //                        if (nodes[k].connected[l] == nodes[i].connected[j] ||
        //                             nodes[k].connected[l] == nodes[i]) continue;
        //                        //Check intersection
        //                        Vector2 intersectPt;
        //                        //if (counter > 100) printf("checking intersection: %d, %d, %d, %d\n",i,j,k,l);
        //                        bool crosses = intersect(nodes[i].position, nodes[i].connected[j].position,
        //                                                 nodes[k].position, nodes[k].connected[l].position,
        //                                                 out intersectPt);
        //                        if (crosses)
        //                        {
        //                            /*if (counter > 100) {
        //                                printf("Found crossing at %f, %f\n",intersectPt.x, intersectPt.y);
        //                                printf("Locations: %f,%f - %f,%f | %f,%f - %f,%f\n",
        //                                                nodes[i].position.x, nodes[i].position.y,
        //                                                nodes[i].connected[j].position.x, nodes[i].connected[j].position.y,
        //                                                nodes[k].position.x,nodes[k].position.y,
        //                                                nodes[k].connected[l].position.x,nodes[k].connected[l].position.y);
        //                                printf("Memory addresses: %d, %d, %d, %d\n",(int)&nodes[i],(int)nodes[i].connected[j],(int)&nodes[k],(int)nodes[k].connected[l]);
        //                            }*/
        //                            dirty = true;
        //                            //Destroy and re-hook connections at crossing point
        //                            PolyNode connj = nodes[i].connected[j];
        //                            PolyNode connl = nodes[k].connected[l];
        //                            nodes[i].connected[j].RemoveConnection(nodes[i]);
        //                            nodes[i].RemoveConnection(connj);
        //                            nodes[k].connected[l].RemoveConnection(nodes[k]);
        //                            nodes[k].RemoveConnection(connl);
        //                            nodes[nNodes] = new PolyNode(intersectPt);
        //                            nodes[nNodes].AddConnection(nodes[i]);
        //                            nodes[i].AddConnection(nodes[nNodes]);
        //                            nodes[nNodes].AddConnection(nodes[k]);
        //                            nodes[k].AddConnection(nodes[nNodes]);
        //                            nodes[nNodes].AddConnection(connj);
        //                            connj.AddConnection(nodes[nNodes]);
        //                            nodes[nNodes].AddConnection(connl);
        //                            connl.AddConnection(nodes[nNodes]);
        //                            ++nNodes;
        //                            goto SkipOut;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    SkipOut:
        //        ++counter;
        //        //if (counter > 100) printf("Counter: %d\n",counter);
        //    }

        //    /*
        //    // Debugging: check for connection consistency
        //    for (int i=0; i<nNodes; ++i) {
        //        int nConn = nodes[i].nConnected;
        //        for (int j=0; j<nConn; ++j) {
        //            if (nodes[i].connected[j].nConnected == 0) Assert(false);
        //            PolyNode* connect = nodes[i].connected[j];
        //            bool found = false;
        //            for (int k=0; k<connect.nConnected; ++k) {
        //                if (connect.connected[k] == &nodes[i]) found = true;
        //            }
        //            Assert(found);
        //        }
        //    }*/

        //    //Collapse duplicate points
        //    bool foundDupe = true;
        //    int nActive = nNodes;
        //    while (foundDupe)
        //    {
        //        foundDupe = false;
        //        for (int i = 0; i < nNodes; ++i)
        //        {
        //            if (nodes[i].nConnected == 0) continue;
        //            for (int j = i + 1; j < nNodes; ++j)
        //            {
        //                if (nodes[j].nConnected == 0) continue;
        //                Vector2 diff = nodes[i].position - nodes[j].position;
        //                if (diff.LengthSquared() <= CollapseDistSqr)
        //                {
        //                    if (nActive <= 3) return new Polygon();
        //                    //printf("Found dupe, %d left\n",nActive);
        //                    --nActive;
        //                    foundDupe = true;
        //                    PolyNode inode = nodes[i];
        //                    PolyNode jnode = nodes[j];
        //                    //Move all of j's connections to i, and orphan j
        //                    int njConn = jnode.nConnected;
        //                    for (int k = 0; k < njConn; ++k)
        //                    {
        //                        PolyNode knode = jnode.connected[k];
        //                        Debug.Assert(knode != jnode);
        //                        if (knode != inode)
        //                        {
        //                            inode.AddConnection(knode);
        //                            knode.AddConnection(inode);
        //                        }
        //                        knode.RemoveConnection(jnode);
        //                        //printf("knode %d on node %d now has %d connections\n",k,j,knode.nConnected);
        //                        //printf("Found duplicate point.\n");
        //                    }
        //                    //printf("Orphaning node at address %d\n",(int)jnode);
        //                    //for (int k=0; k<njConn; ++k) {
        //                    //	if (jnode.connected[k].IsConnectedTo(*jnode)) printf("Problem!!!\n");
        //                    //}
        //                    /*
        //                    for (int k=0; k < njConn; ++k){
        //                        jnode.RemoveConnectionByIndex(k);
        //                    }*/
        //                    jnode.nConnected = 0;
        //                }
        //            }
        //        }
        //    }

        //    /*
        //    // Debugging: check for connection consistency
        //    for (int i=0; i<nNodes; ++i) {
        //        int nConn = nodes[i].nConnected;
        //        printf("Node %d has %d connections\n",i,nConn);
        //        for (int j=0; j<nConn; ++j) {
        //            if (nodes[i].connected[j].nConnected == 0) {
        //                printf("Problem with node %d connection at address %d\n",i,(int)(nodes[i].connected[j]));
        //                Assert(false);
        //            }
        //            PolyNode* connect = nodes[i].connected[j];
        //            bool found = false;
        //            for (int k=0; k<connect.nConnected; ++k) {
        //                if (connect.connected[k] == &nodes[i]) found = true;
        //            }
        //            if (!found) printf("Connection %d (of %d) on node %d (of %d) did not have reciprocal connection.\n",j,nConn,i,nNodes);
        //            Assert(found);
        //        }
        //    }//*/

        //    //Now walk the edge of the list

        //    //Find node with minimum y value (max x if equal)
        //    float minY = float.MaxValue;
        //    float maxX = -float.MaxValue;
        //    int minYIndex = -1;
        //    for (int i = 0; i < nNodes; ++i)
        //    {
        //        if (nodes[i].position.Y < minY && nodes[i].nConnected > 1)
        //        {
        //            minY = nodes[i].position.Y;
        //            minYIndex = i;
        //            maxX = nodes[i].position.X;
        //        }
        //        else if (nodes[i].position.Y == minY && nodes[i].position.X > maxX && nodes[i].nConnected > 1)
        //        {
        //            minYIndex = i;
        //            maxX = nodes[i].position.X;
        //        }
        //    }

        //    Vector2 origDir = new Vector2(1.0f, 0.0f);
        //    Vector2[] resultVecs = new Vector2[4 * nNodes];// nodes may be visited more than once, unfortunately - change to growable array!
        //    int nResultVecs = 0;
        //    PolyNode currentNode = nodes[minYIndex];
        //    PolyNode startNode = currentNode;
        //    Debug.Assert(currentNode.nConnected > 0);
        //    PolyNode nextNode = currentNode.GetRightestConnection(origDir);
        //    if (nextNode == null)
        //    {
        //        float[] xres = new float[nResultVecs];
        //        float[] yres = new float[nResultVecs];
        //        for (int i = 0; i < nResultVecs; ++i)
        //        {
        //            xres[i] = resultVecs[i].X;
        //            yres[i] = resultVecs[i].Y;
        //        }
        //        return new Polygon(xres, yres, nResultVecs);
        //    }

        //    // Borked, clean up our mess and return
        //    resultVecs[0] = startNode.position;
        //    ++nResultVecs;
        //    while (nextNode != startNode)
        //    {
        //        if (nResultVecs > 4 * nNodes)
        //        {
        //            /*
        //            printf("%d, %d, %d\n",(int)startNode,(int)currentNode,(int)nextNode);
        //            printf("%f, %f . %f, %f\n",currentNode.position.x,currentNode.position.y, nextNode.position.x, nextNode.position.y);
        //                p.printFormatted();
        //                printf("Dumping connection graph: \n");
        //                for (int i=0; i<nNodes; ++i) {
        //                    printf("nodex[%d] = %f; nodey[%d] = %f;\n",i,nodes[i].position.x,i,nodes[i].position.y);
        //                    printf("//connected to\n");
        //                    for (int j=0; j<nodes[i].nConnected; ++j) {
        //                        printf("connx[%d][%d] = %f; conny[%d][%d] = %f;\n",i,j,nodes[i].connected[j].position.x, i,j,nodes[i].connected[j].position.y);
        //                    }
        //                }
        //                printf("Dumping results thus far: \n");
        //                for (int i=0; i<nResultVecs; ++i) {
        //                    printf("x[%d]=map(%f,-3,3,0,width); y[%d] = map(%f,-3,3,height,0);\n",i,resultVecs[i].x,i,resultVecs[i].y);
        //                }
        //            //*/
        //            Debug.Assert(false); //nodes should never be visited four times apiece (proof?), so we've probably hit a loop...crap
        //        }
        //        resultVecs[nResultVecs++] = nextNode.position;
        //        PolyNode oldNode = currentNode;
        //        currentNode = nextNode;
        //        //printf("Old node connections = %d; address %d\n",oldNode.nConnected, (int)oldNode);
        //        //printf("Current node connections = %d; address %d\n",currentNode.nConnected, (int)currentNode);
        //        nextNode = currentNode.GetRightestConnection(oldNode);
        //        if (nextNode == null)
        //        {
        //            float[] xres1 = new float[nResultVecs];
        //            float[] yres1 = new float[nResultVecs];
        //            for (int i = 0; i < nResultVecs; ++i)
        //            {
        //                xres1[i] = resultVecs[i].X;
        //                yres1[i] = resultVecs[i].Y;
        //            }
        //            Polygon retval = new Polygon(xres1, yres1, nResultVecs);
        //            return retval;

        //        }
        //        // There was a problem, so jump out of the loop and use whatever garbage we've generated so far
        //        //printf("nextNode address: %d\n",(int)nextNode);
        //    }

        //    return new Polygon();
        //}

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
        private bool ResolvePinchPoint(Vertices pin, out Vertices poutA, out Vertices poutB)
        {
            poutA = new Vertices();
            poutB = new Vertices();

            if (pin.Count < 3)
                return false;

            const float tol = .001f;
            bool hasPinchPoint = false;
            int pinchIndexA = -1;
            int pinchIndexB = -1;
            for (int i = 0; i < pin.Count; ++i)
            {
                for (int j = i + 1; j < pin.Count; ++j)
                {
                    //Don't worry about pinch points where the points
                    //are actually just dupe neighbors
                    if (Math.Abs(pin[i].X - pin[j].X) < tol && Math.Abs(pin[i].Y - pin[j].Y) < tol && j != i + 1)
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
                if (sizeA == pin.Count) return false;//has dupe points at wraparound, not a problem here
                for (int i = 0; i < sizeA; ++i)
                {
                    int ind = Remainder(pinchIndexA + i, pin.Count);             // is this right
                    poutA.Add(pin[ind]);
                }

                int sizeB = pin.Count - sizeA;
                for (int i = 0; i < sizeB; ++i)
                {
                    int ind = Remainder(pinchIndexB + i, pin.Count);          // is this right    
                    poutB.Add(pin[ind]);
                }
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
        private int TriangulatePolygon(Vertices vertices, out Triangle[] results)
        {
            results = new Triangle[175];

            if (vertices.Count < 3)
                return 0;

            //Recurse and split on pinch points
            Vertices pA, pB;
            Vertices pin = new Vertices(vertices);
            if (ResolvePinchPoint(pin, out pA, out pB))
            {
                Triangle[] mergeA;
                Triangle[] mergeB;
                int nA = TriangulatePolygon(pA, out mergeA);
                int nB = TriangulatePolygon(pB, out mergeB);
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

            Triangle[] buffer = new Triangle[vertices.Count - 2];
            int bufferSize = 0;
            float[] xrem = new float[vertices.Count];
            float[] yrem = new float[vertices.Count];
            for (int i = 0; i < vertices.Count; ++i)
            {
                xrem[i] = vertices[i].X;
                yrem[i] = vertices[i].Y;
            }

            int vNum = vertices.Count;

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
        private int Remainder(int x, int modulus)
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
        private int PolygonizeTriangles(Triangle[] triangulated, int triangulatedLength, out List<Vertices> polys, int polysLength)
        {
            int polyIndex = 0;
            polys = new List<Vertices>(50);

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
                    Vertices poly = new Vertices(3);

                    for (int i = 0; i < 3; i++)
                    {
                        poly.Add(new Vector2(triangulated[currTri].x[i], triangulated[currTri].y[i]));
                    }

                    covered[currTri] = true;
                    int index = 0;
                    for (int i = 0; i < 2 * triangulatedLength; ++i, ++index)
                    {
                        while (index >= triangulatedLength) index -= triangulatedLength;
                        if (covered[index])
                        {
                            continue;
                        }
                        Vertices newP = poly.AddTriangle(triangulated[index]);
                        if (newP == null)
                        {                                 // is this right
                            continue;
                        }
                        if (newP.Count > Settings.MaxPolygonVertices)
                        {
                            newP = null;
                            continue;
                        }
                        if (newP.IsConvex())
                        {
                            //Or should it be IsUsable?  Maybe re-write IsConvex to apply the angle threshold from Box2d
                            poly = new Vertices(newP);
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
                        if (poly.Count >= 3) 
                            polys.Add(new Vertices(poly));
                        //else printf("Skipping corrupt poly\n");
                    }
                    if (poly.Count >= 3) polyIndex++; //Must be outside (polyIndex < polysLength) test
                }
            }
            return polyIndex;
        }
       
        private const float angularSlop = 1.0f / 180.0f * (float)Math.PI; // 1 degrees

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
        private bool IsEar(int i, float[] xv, float[] yv, int xvLength)
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
        public int DecomposeConvex(Vertices p, out List<Vertices> results, int maxPolys)
        {
            results = new List<Vertices>();

            if (p.Count < 3) return 0;

            Triangle[] triangulated = new Triangle[p.Count - 2];
            int nTri;
            if (p.IsCounterClockWise())
            {
                //printf("It is ccw \n");
                Vertices tempP = new Vertices(p);
                //ReversePolygon(tempP.x, tempP.y, tempP.nVertices);
                tempP.Reverse();
                nTri = TriangulatePolygon(tempP, out triangulated);
                //ReversePolygon(p.x, p.y, p.nVertices); //reset orientation
            }
            else
            {
                //printf("It is not ccw \n");
                nTri = TriangulatePolygon(p, out triangulated);
            }
            if (nTri < 1)
            {
                //Still no luck?  Oh well...
                return -1;
            }
            int nPolys = PolygonizeTriangles(triangulated, nTri, out results, maxPolys);
            return nPolys;
        }

        #region Nested type: PolyNode

        private class PolyNode
        {
            /*
 * Given sines and cosines, tells if A's angle is less than B's on -Pi, Pi
 * (in other words, is A "righter" than B)
 */
            public PolyNode[] connected = new PolyNode[MaxConnected];
            public int nConnected;
            public Vector2 position;
            bool visited;

            public PolyNode()
            {
                nConnected = 0;
                visited = false;
            }
            public PolyNode(Vector2 pos)
            {
                position = pos;
                nConnected = 0;
                visited = false;
            }

            bool IsRighter(float sinA, float cosA, float sinB, float cosB)
            {
                if (sinA < 0)
                {
                    if (sinB > 0 || cosA <= cosB) return true;
                    else return false;
                }
                else
                {
                    if (sinB < 0 || cosA <= cosB) return false;
                    else return true;
                }
            }

            //Fix for obnoxious behavior for the % operator for negative numbers...
            int remainder(int x, int modulus)
            {
                int rem = x % modulus;
                while (rem < 0)
                {
                    rem += modulus;
                }
                return rem;
            }

            public void AddConnection(PolyNode toMe)
            {
                Debug.Assert(nConnected < MaxConnected);

                // Ignore duplicate additions
                for (int i = 0; i < nConnected; ++i)
                {
                    if (connected[i] == toMe) return;
                }
                connected[nConnected] = toMe;
                ++nConnected;
            }

            public void RemoveConnection(PolyNode fromMe)
            {
                bool isFound = false;
                int foundIndex = -1;
                for (int i = 0; i < nConnected; ++i)
                {
                    if (fromMe == connected[i])
                    {//.position == connected[i].position){
                        isFound = true;
                        foundIndex = i;
                        break;
                    }
                }
                Debug.Assert(isFound);
                --nConnected;
                //printf("nConnected: %d\n",nConnected);
                for (int i = foundIndex; i < nConnected; ++i)
                {
                    connected[i] = connected[i + 1];
                }
            }
            void RemoveConnectionByIndex(int index)
            {
                --nConnected;
                //printf("New nConnected = %d\n",nConnected);
                for (int i = index; i < nConnected; ++i)
                {
                    connected[i] = connected[i + 1];
                }
            }
            bool IsConnectedTo(PolyNode me)
            {
                bool isFound = false;
                for (int i = 0; i < nConnected; ++i)
                {
                    if (me == connected[i])
                    {//.position == connected[i].position){
                        isFound = true;
                        break;
                    }
                }
                return isFound;
            }
            public PolyNode GetRightestConnection(PolyNode incoming)
            {
                if (nConnected == 0) Debug.Assert(false); // This means the connection graph is inconsistent
                if (nConnected == 1)
                {
                    //b2Assert(false);
                    // Because of the possibility of collapsing nearby points,
                    // we may end up with "spider legs" dangling off of a region.
                    // The correct behavior here is to turn around.
                    return incoming;
                }
                Vector2 inDir = position - incoming.position;

                float inLength = inDir.Length();
                inDir.Normalize();

                Debug.Assert(inLength > Settings.Epsilon);

                PolyNode result = null;
                for (int i = 0; i < nConnected; ++i)
                {
                    if (connected[i] == incoming) continue;
                    Vector2 testDir = connected[i].position - position;
                    float testLengthSqr = testDir.LengthSquared();
                    testDir.Normalize();
                    /*
                    if (testLengthSqr < COLLAPSE_DIST_SQR) {
                        printf("Problem with connection %d\n",i);
                        printf("This node has %d connections\n",nConnected);
                        printf("That one has %d\n",connected[i].nConnected);
                        if (this == connected[i]) printf("This points at itself.\n");
                    }*/
                    Debug.Assert(testLengthSqr >= CollapseDistSqr);
                    float myCos = Vector2.Dot(inDir, testDir);
                    float mySin = MathUtils.Cross(inDir, testDir);
                    if (result != null)
                    {
                        Vector2 resultDir = result.position - position;
                        resultDir.Normalize();
                        float resCos = Vector2.Dot(inDir, resultDir);
                        float resSin = MathUtils.Cross(inDir, resultDir);
                        if (IsRighter(mySin, myCos, resSin, resCos))
                        {
                            result = connected[i];
                        }
                    }
                    else
                    {
                        result = connected[i];
                    }
                }

                //if (B2_POLYGON_REPORT_ERRORS && result != null)
                //{
                //    printf("nConnected = %d\n", nConnected);
                //    for (int i = 0; i < nConnected; ++i)
                //    {
                //        printf("connected[%d] @ %d\n", i, (int)connected[i]);
                //    }
                //}
                Debug.Assert(result != null);

                return result;
            }

            public PolyNode GetRightestConnection(Vector2 incomingDir)
            {
                Vector2 diff = position - incomingDir;
                PolyNode temp = new PolyNode(diff);
                PolyNode res = GetRightestConnection(temp);
                Debug.Assert(res != null);
                return res;
            }
        }

        #endregion

        #region Nested type: Triangle

        public class Triangle
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

            public Triangle()
            {
                x = new float[3];
                y = new float[3];
            }

            public Triangle(Triangle t)
            {
                x = new float[3];
                y = new float[3];

                x[0] = t.x[0]; x[1] = t.x[1]; x[2] = t.x[2];
                y[0] = t.y[0]; y[1] = t.y[1]; y[2] = t.y[2];
            }

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

        #endregion
    }
}
