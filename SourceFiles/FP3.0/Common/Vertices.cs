using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
#if !(XBOX360)
    [DebuggerDisplay("Count = {Count} Vertices = {ToString()}")]
#endif
    public class Vertices : List<Vector2>
    {
        public Vertices()
        {
        }

        public Vertices(int capacity)
        {
            Capacity = capacity;
        }

        public Vertices(ref Vector2[] vector2)
        {
            for (int i = 0; i < vector2.Length; i++)
            {
                Add(vector2[i]);
            }
        }

        public Vertices(IList<Vector2> vertices)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                Add(vertices[i]);
            }
        }

        public int NextIndex(int index)
        {
            if (index == Count - 1)
            {
                return 0;
            }
            return index + 1;
        }

        /// <summary>
        /// Gets the previous index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public int PreviousIndex(int index)
        {
            if (index == 0)
            {
                return Count - 1;
            }
            return index - 1;
        }

        /// <summary>
        /// Gets the signed area.
        /// </summary>
        /// <returns></returns>
        public float GetSignedArea()
        {
            int i;
            float area = 0;

            for (i = 0; i < Count; i++)
            {
                int j = (i + 1) % Count;
                area += this[i].X * this[j].Y;
                area -= this[i].Y * this[j].X;
            }
            area /= 2.0f;
            return area;
        }

        /// <summary>
        /// Gets the area.
        /// </summary>
        /// <returns></returns>
        public float GetArea()
        {
            int i;
            float area = 0;

            for (i = 0; i < Count; i++)
            {
                int j = (i + 1) % Count;
                area += this[i].X * this[j].Y;
                area -= this[i].Y * this[j].X;
            }
            area /= 2.0f;
            return (area < 0 ? -area : area);
        }

        /// <summary>
        /// Gets the centroid.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCentroid()
        {
            // Same algorithm is used by Box2D

            Vector2 c = Vector2.Zero;
            float area = 0.0f;

            const float inv3 = 1.0f / 3.0f;
            Vector2 pRef = new Vector2(0.0f, 0.0f);
            for (int i = 0; i < Count; ++i)
            {
                // Triangle vertices.
                Vector2 p1 = pRef;
                Vector2 p2 = this[i];
                Vector2 p3 = i + 1 < Count ? this[i + 1] : this[0];

                Vector2 e1 = p2 - p1;
                Vector2 e2 = p3 - p1;

                float D = MathUtils.Cross(e1, e2);

                float triangleArea = 0.5f * D;
                area += triangleArea;

                // Area weighted centroid
                c += triangleArea * inv3 * (p1 + p2 + p3);
            }

            // Centroid
            c *= 1.0f / area;
            return c;
        }

        /// <summary>
        /// Translates the vertices with the specified vector.
        /// </summary>
        /// <param name="vector">The vector.</param>
        public void Translate(ref Vector2 vector)
        {
            for (int i = 0; i < Count; i++)
                this[i] = Vector2.Add(this[i], vector);
        }

        /// <summary>
        /// Scales the vertices with the specified vector.
        /// </summary>
        /// <param name="value">The Value.</param>
        public void Scale(ref Vector2 value)
        {
            for (int i = 0; i < Count; i++)
                this[i] = Vector2.Multiply(this[i], value);
        }

        /// <summary>
        /// Rotate the vertices with the defined value in radians.
        /// </summary>
        /// <param name="value">The amount to rotate by in radians.</param>
        public void Rotate(float value)
        {
            Matrix rotationMatrix;
            Matrix.CreateRotationZ(value, out rotationMatrix);

            for (int i = 0; i < Count; i++)
                this[i] = Vector2.Transform(this[i], rotationMatrix);
        }

        /// <summary>
        /// Assuming the polygon is simple; determines whether the polygon is convex.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if it is convex; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvex()
        {
            bool isPositive = false;

            for (int i = 0; i < Count; ++i)
            {
                int lower = (i == 0) ? (Count - 1) : (i - 1);
                int middle = i;
                int upper = (i == Count - 1) ? (0) : (i + 1);

                float dx0 = this[middle].X - this[lower].X;
                float dy0 = this[middle].Y - this[lower].Y;
                float dx1 = this[upper].X - this[middle].X;
                float dy1 = this[upper].Y - this[middle].Y;

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

        public bool IsCounterClockWise()
        {
            return (GetSignedArea() > 0.0f);
        }

        /// <summary>
        /// Forces counter clock wise order.
        /// </summary>
        public void ForceCounterClockWise()
        {
            // the sign of the 'area' of the polygon is all
            // we are interested in.
            float area = GetSignedArea();
            if (area > 0)
            {
                Reverse();
            }
        }

        /// <summary>
        /// Check for edge crossings
        /// </summary>
        /// <returns></returns>
        public bool IsSimple()
        {
            for (int i = 0; i < Count; ++i)
            {
                int iplus = (i + 1 > Count - 1) ? 0 : i + 1;
                Vector2 a1 = new Vector2(this[i].X, this[i].Y);
                Vector2 a2 = new Vector2(this[iplus].X, this[iplus].Y);
                for (int j = i + 1; j < Count; ++j)
                {
                    int jplus = (j + 1 > Count - 1) ? 0 : j + 1;
                    Vector2 b1 = new Vector2(this[j].X, this[j].Y);
                    Vector2 b2 = new Vector2(this[jplus].X, this[jplus].Y);

                    Vector2 temp;

                    if (LineTools.LineIntersect2(a1, a2, b1, b2, out temp))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // From Eric Jordan's convex decomposition library
        /// <summary>
        /// Checks if polygon is valid for use in Box2d engine.
        ///Last ditch effort to ensure no invalid polygons are
        ///added to world geometry.
        ///
        ///Performs a full check, for simplicity, convexity,
        ///orientation, minimum angle, and volume.  This won't
        ///be very efficient, and a lot of it is redundant when
        ///other tools in this section are used.
        /// </summary>
        /// <param name="printErrors"></param>
        /// <returns></returns>
        public bool CheckPolygon()
        {
            int error = -1;
            if (Count < 3 || Count > Settings.MaxPolygonVertices)
            {
                error = 0;
            }
            if (!IsConvex())
            {
                error = 1;
            }
            if (!IsSimple())
            {
                error = 2;
            }
            if (GetArea() < Settings.Epsilon)
            {
                error = 3;
            }

            //Compute normals
            Vector2[] normals = new Vector2[Count];
            Vertices vertices = new Vertices(Count);
            for (int i = 0; i < Count; ++i)
            {
                vertices.Add(new Vector2(this[i].X, this[i].Y));
                int i1 = i;
                int i2 = i + 1 < Count ? i + 1 : 0;
                Vector2 edge = new Vector2(this[i2].X - this[i1].X, this[i2].Y - this[i1].Y);
                normals[i] = MathUtils.Cross(edge, 1.0f);
                normals[i].Normalize();
            }

            //Required side checks
            for (int i = 0; i < Count; ++i)
            {
                int iminus = (i == 0) ? Count - 1 : i - 1;

                //Parallel sides check
                float cross = MathUtils.Cross(normals[iminus], normals[i]);
                cross = MathUtils.Clamp(cross, -1.0f, 1.0f);
                float angle = (float) Math.Asin(cross);
                if (angle <= Settings.AngularSlop)
                {
                    error = 4;
                    break;
                }

                //Too skinny check
                for (int j = 0; j < Count; ++j)
                {
                    if (j == i || j == (i + 1) % Count)
                    {
                        continue;
                    }
                    float s = Vector2.Dot(normals[i], vertices[j] - vertices[i]);
                    if (s >= -Settings.LinearSlop)
                    {
                        error = 5;
                    }
                }


                Vector2 centroid = vertices.GetCentroid();
                Vector2 n1 = normals[iminus];
                Vector2 n2 = normals[i];
                Vector2 v = vertices[i] - centroid;
                ;

                Vector2 d = new Vector2();
                d.X = Vector2.Dot(n1, v); // - toiSlop;
                d.Y = Vector2.Dot(n2, v); // - toiSlop;

                // Shifting the edge inward by toiSlop should
                // not cause the plane to pass the centroid.
                if ((d.X < 0.0f) || (d.Y < 0.0f))
                {
                    error = 6;
                }
            }

            if (error != -1)
            {
                Debug.WriteLine("Found invalid polygon, ");
                switch (error)
                {
                    case 0:
                        Debug.WriteLine(string.Format("must have between 3 and {0} vertices.\n",
                                                      Settings.MaxPolygonVertices));
                        break;
                    case 1:
                        Debug.WriteLine("must be convex.\n");
                        break;
                    case 2:
                        Debug.WriteLine("must be simple (cannot intersect itself).\n");
                        break;
                    case 3:
                        Debug.WriteLine("area is too small.\n");
                        break;
                    case 4:
                        Debug.WriteLine("sides are too close to parallel.\n");
                        break;
                    case 5:
                        Debug.WriteLine("polygon is too thin.\n");
                        break;
                    case 6:
                        Debug.WriteLine("core shape generation would move edge past centroid (too thin).\n");
                        break;
                    default:
                        Debug.WriteLine("don't know why.\n");
                        break;
                }
            }
            return error != -1;
        }

        // From Eric Jordan's convex decomposition library
        /// <summary>
        /// Trace the edge of a non-simple polygon and return a simple polygon.
        /// 
        ///Method:
        ///Start at vertex with minimum y (pick maximum x one if there are two).  
        ///We aim our "lastDir" vector at (1.0, 0)
        ///We look at the two rays going off from our start vertex, and follow whichever
        ///has the smallest angle (in -Pi . Pi) wrt lastDir ("rightest" turn)
        ///
        ///Loop until we hit starting vertex:
        ///
        ///We add our current vertex to the list.
        ///We check the seg from current vertex to next vertex for intersections
        ///  - if no intersections, follow to next vertex and continue
        ///  - if intersections, pick one with minimum distance
        ///    - if more than one, pick one with "rightest" next point (two possibilities for each)
        /// </summary>
        /// <param name="verts"></param>
        /// <returns></returns>
        public Vertices TraceEdge(Vertices verts)
        {
            PolyNode[] nodes = new PolyNode[verts.Count * verts.Count];
            //overkill, but sufficient (order of mag. is right)
            int nNodes = 0;

            //Add base nodes (raw outline)
            for (int i = 0; i < verts.Count; ++i)
            {
                Vector2 pos = new Vector2(verts[i].X, verts[i].Y);
                nodes[i].position = pos;
                ++nNodes;
                int iplus = (i == verts.Count - 1) ? 0 : i + 1;
                int iminus = (i == 0) ? verts.Count - 1 : i - 1;
                nodes[i].AddConnection(nodes[iplus]);
                nodes[i].AddConnection(nodes[iminus]);
            }

            //Process intersection nodes - horribly inefficient
            bool dirty = true;
            int counter = 0;
            while (dirty)
            {
                dirty = false;
                for (int i = 0; i < nNodes; ++i)
                {
                    for (int j = 0; j < nodes[i].nConnected; ++j)
                    {
                        for (int k = 0; k < nNodes; ++k)
                        {
                            if (k == i || nodes[k] == nodes[i].connected[j]) continue;
                            for (int l = 0; l < nodes[k].nConnected; ++l)
                            {
                                if (nodes[k].connected[l] == nodes[i].connected[j] ||
                                    nodes[k].connected[l] == nodes[i]) continue;
                                //Check intersection
                                Vector2 intersectPt;
                                //if (counter > 100) printf("checking intersection: %d, %d, %d, %d\n",i,j,k,l);
                                bool crosses = LineTools.LineIntersect(nodes[i].position, nodes[i].connected[j].position,
                                                                       nodes[k].position, nodes[k].connected[l].position,
                                                                       out intersectPt);
                                if (crosses)
                                {
                                    /*if (counter > 100) {
                                        printf("Found crossing at %f, %f\n",intersectPt.x, intersectPt.y);
                                        printf("Locations: %f,%f - %f,%f | %f,%f - %f,%f\n",
                                                        nodes[i].position.x, nodes[i].position.y,
                                                        nodes[i].connected[j].position.x, nodes[i].connected[j].position.y,
                                                        nodes[k].position.x,nodes[k].position.y,
                                                        nodes[k].connected[l].position.x,nodes[k].connected[l].position.y);
                                        printf("Memory addresses: %d, %d, %d, %d\n",(int)&nodes[i],(int)nodes[i].connected[j],(int)&nodes[k],(int)nodes[k].connected[l]);
                                    }*/
                                    dirty = true;
                                    //Destroy and re-hook connections at crossing point
                                    PolyNode connj = nodes[i].connected[j];
                                    PolyNode connl = nodes[k].connected[l];
                                    nodes[i].connected[j].RemoveConnection(nodes[i]);
                                    nodes[i].RemoveConnection(connj);
                                    nodes[k].connected[l].RemoveConnection(nodes[k]);
                                    nodes[k].RemoveConnection(connl);
                                    nodes[nNodes] = new PolyNode(intersectPt);
                                    nodes[nNodes].AddConnection(nodes[i]);
                                    nodes[i].AddConnection(nodes[nNodes]);
                                    nodes[nNodes].AddConnection(nodes[k]);
                                    nodes[k].AddConnection(nodes[nNodes]);
                                    nodes[nNodes].AddConnection(connj);
                                    connj.AddConnection(nodes[nNodes]);
                                    nodes[nNodes].AddConnection(connl);
                                    connl.AddConnection(nodes[nNodes]);
                                    ++nNodes;
                                    goto SkipOut;
                                }
                            }
                        }
                    }
                }
                SkipOut:
                ++counter;
                //if (counter > 100) printf("Counter: %d\n",counter);
            }

            /*
            // Debugging: check for connection consistency
            for (int i=0; i<nNodes; ++i) {
                int nConn = nodes[i].nConnected;
                for (int j=0; j<nConn; ++j) {
                    if (nodes[i].connected[j].nConnected == 0) Assert(false);
                    PolyNode* connect = nodes[i].connected[j];
                    bool found = false;
                    for (int k=0; k<connect.nConnected; ++k) {
                        if (connect.connected[k] == &nodes[i]) found = true;
                    }
                    Assert(found);
                }
            }*/

            //Collapse duplicate points
            bool foundDupe = true;
            int nActive = nNodes;
            while (foundDupe)
            {
                foundDupe = false;
                for (int i = 0; i < nNodes; ++i)
                {
                    if (nodes[i].nConnected == 0) continue;
                    for (int j = i + 1; j < nNodes; ++j)
                    {
                        if (nodes[j].nConnected == 0) continue;
                        Vector2 diff = nodes[i].position - nodes[j].position;
                        if (diff.LengthSquared() <= Settings.Epsilon * Settings.Epsilon)
                        {
                            if (nActive <= 3)
                                return new Vertices();

                            //printf("Found dupe, %d left\n",nActive);
                            --nActive;
                            foundDupe = true;
                            PolyNode inode = nodes[i];
                            PolyNode jnode = nodes[j];
                            //Move all of j's connections to i, and orphan j
                            int njConn = jnode.nConnected;
                            for (int k = 0; k < njConn; ++k)
                            {
                                PolyNode knode = jnode.connected[k];
                                Debug.Assert(knode != jnode);
                                if (knode != inode)
                                {
                                    inode.AddConnection(knode);
                                    knode.AddConnection(inode);
                                }
                                knode.RemoveConnection(jnode);
                                //printf("knode %d on node %d now has %d connections\n",k,j,knode.nConnected);
                                //printf("Found duplicate point.\n");
                            }
                            //printf("Orphaning node at address %d\n",(int)jnode);
                            //for (int k=0; k<njConn; ++k) {
                            //	if (jnode.connected[k].IsConnectedTo(*jnode)) printf("Problem!!!\n");
                            //}
                            /*
                            for (int k=0; k < njConn; ++k){
                                jnode.RemoveConnectionByIndex(k);
                            }*/
                            jnode.nConnected = 0;
                        }
                    }
                }
            }

            /*
            // Debugging: check for connection consistency
            for (int i=0; i<nNodes; ++i) {
                int nConn = nodes[i].nConnected;
                printf("Node %d has %d connections\n",i,nConn);
                for (int j=0; j<nConn; ++j) {
                    if (nodes[i].connected[j].nConnected == 0) {
                        printf("Problem with node %d connection at address %d\n",i,(int)(nodes[i].connected[j]));
                        Assert(false);
                    }
                    PolyNode* connect = nodes[i].connected[j];
                    bool found = false;
                    for (int k=0; k<connect.nConnected; ++k) {
                        if (connect.connected[k] == &nodes[i]) found = true;
                    }
                    if (!found) printf("Connection %d (of %d) on node %d (of %d) did not have reciprocal connection.\n",j,nConn,i,nNodes);
                    Assert(found);
                }
            }//*/

            //Now walk the edge of the list

            //Find node with minimum y value (max x if equal)
            float minY = float.MaxValue;
            float maxX = -float.MaxValue;
            int minYIndex = -1;
            for (int i = 0; i < nNodes; ++i)
            {
                if (nodes[i].position.Y < minY && nodes[i].nConnected > 1)
                {
                    minY = nodes[i].position.Y;
                    minYIndex = i;
                    maxX = nodes[i].position.X;
                }
                else if (nodes[i].position.Y == minY && nodes[i].position.X > maxX && nodes[i].nConnected > 1)
                {
                    minYIndex = i;
                    maxX = nodes[i].position.X;
                }
            }

            Vector2 origDir = new Vector2(1.0f, 0.0f);
            Vector2[] resultVecs = new Vector2[4 * nNodes];
            // nodes may be visited more than once, unfortunately - change to growable array!
            int nResultVecs = 0;
            PolyNode currentNode = nodes[minYIndex];
            PolyNode startNode = currentNode;
            Debug.Assert(currentNode.nConnected > 0);
            PolyNode nextNode = currentNode.GetRightestConnection(origDir);
            if (nextNode == null)
            {
                Vertices vertices = new Vertices(nResultVecs);

                for (int i = 0; i < nResultVecs; ++i)
                {
                    vertices.Add(resultVecs[i]);
                }

                return vertices;
            }

            // Borked, clean up our mess and return
            resultVecs[0] = startNode.position;
            ++nResultVecs;
            while (nextNode != startNode)
            {
                if (nResultVecs > 4 * nNodes)
                {
                    /*
                    printf("%d, %d, %d\n",(int)startNode,(int)currentNode,(int)nextNode);
                    printf("%f, %f . %f, %f\n",currentNode.position.x,currentNode.position.y, nextNode.position.x, nextNode.position.y);
                        verts.printFormatted();
                        printf("Dumping connection graph: \n");
                        for (int i=0; i<nNodes; ++i) {
                            printf("nodex[%d] = %f; nodey[%d] = %f;\n",i,nodes[i].position.x,i,nodes[i].position.y);
                            printf("//connected to\n");
                            for (int j=0; j<nodes[i].nConnected; ++j) {
                                printf("connx[%d][%d] = %f; conny[%d][%d] = %f;\n",i,j,nodes[i].connected[j].position.x, i,j,nodes[i].connected[j].position.y);
                            }
                        }
                        printf("Dumping results thus far: \n");
                        for (int i=0; i<nResultVecs; ++i) {
                            printf("x[%d]=map(%f,-3,3,0,width); y[%d] = map(%f,-3,3,height,0);\n",i,resultVecs[i].x,i,resultVecs[i].y);
                        }
                    //*/
                    Debug.Assert(false);
                    //nodes should never be visited four times apiece (proof?), so we've probably hit a loop...crap
                }
                resultVecs[nResultVecs++] = nextNode.position;
                PolyNode oldNode = currentNode;
                currentNode = nextNode;
                //printf("Old node connections = %d; address %d\n",oldNode.nConnected, (int)oldNode);
                //printf("Current node connections = %d; address %d\n",currentNode.nConnected, (int)currentNode);
                nextNode = currentNode.GetRightestConnection(oldNode);
                if (nextNode == null)
                {
                    Vertices vertices = new Vertices(nResultVecs);
                    for (int i = 0; i < nResultVecs; ++i)
                    {
                        vertices.Add(resultVecs[i]);
                    }
                    return vertices;
                }
                // There was a problem, so jump out of the loop and use whatever garbage we've generated so far
                //printf("nextNode address: %d\n",(int)nextNode);
            }

            return new Vertices();
        }

        private class PolyNode
        {
            private const int MaxConnected = 32;

            /*
             * Given sines and cosines, tells if A's angle is less than B's on -Pi, Pi
             * (in other words, is A "righter" than B)
             */
            public PolyNode[] connected = new PolyNode[MaxConnected];
            public int nConnected;
            public Vector2 position;
            private bool visited;

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

            private bool IsRighter(float sinA, float cosA, float sinB, float cosB)
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
            private int remainder(int x, int modulus)
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
                    {
//.position == connected[i].position){
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

            private void RemoveConnectionByIndex(int index)
            {
                --nConnected;
                //printf("New nConnected = %d\n",nConnected);
                for (int i = index; i < nConnected; ++i)
                {
                    connected[i] = connected[i + 1];
                }
            }

            private bool IsConnectedTo(PolyNode me)
            {
                bool isFound = false;
                for (int i = 0; i < nConnected; ++i)
                {
                    if (me == connected[i])
                    {
//.position == connected[i].position){
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
                    Debug.Assert(testLengthSqr >= Settings.Epsilon * Settings.Epsilon);
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

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                builder.Append(this[i].ToString());
                if (i < Count - 1)
                {
                    builder.Append(" ");
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Projects to axis.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        public void ProjectToAxis(ref Vector2 axis, out float min, out float max)
        {
            // To project a point on an axis use the dot product
            float dotProduct = Vector2.Dot(axis, this[0]);
            min = dotProduct;
            max = dotProduct;

            for (int i = 0; i < Count; i++)
            {
                dotProduct = Vector2.Dot(this[i], axis);
                if (dotProduct < min)
                {
                    min = dotProduct;
                }
                else
                {
                    if (dotProduct > max)
                    {
                        max = dotProduct;
                    }
                }
            }
        }
    }
}