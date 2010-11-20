using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.Decomposition
{
    //From the Poly2Tri project http://code.google.com/p/poly2tri/source/browse?repo=archive#hg/scala/src/org/poly2tri/seidel

    /// <summary>
    /// Convex decomposition algorithm created by Mark Bayazit (http://mnbayazit.com/)
    /// For more information about this algorithm, see http://mnbayazit.com/406/bayazit
    /// </summary>
    public static class SeidelDecomposer
    {
        /// <summary>
        /// Decompose the polygon into several smaller non-concave polygon.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static List<Vertices> ConvexPartition(Vertices vertices)
        {
            List<Point> compatList = new List<Point>(vertices.Count);

            foreach (Vector2 vertex in vertices)
            {
                compatList.Add(new Point(vertex.X, vertex.Y));
            }

            Triangulator t = new Triangulator(compatList);

            List<Vertices> list = new List<Vertices>();

            List<List<Point>> triangles = t.polygons;

            foreach (List<Point> triangle in triangles)
            {
                Vertices verts = new Vertices(triangle.Count);

                foreach (Point point in triangle)
                {
                    verts.Add(new Vector2(point.x, point.y));
                }

                list.Add(verts);
            }

            //foreach (Trapezoid trapezoid in t.trapezoids)
            //{
            //    Vertices verts = new Vertices();

            //    verts.Add(new Vector2(trapezoid.top.p.x, trapezoid.top.p.y));
            //    verts.Add(new Vector2(trapezoid.top.q.x, trapezoid.top.q.y));
            //    verts.Add(new Vector2(trapezoid.bottom.q.x, trapezoid.bottom.q.y));
            //    verts.Add(new Vector2(trapezoid.bottom.q.x, trapezoid.bottom.q.y));

            //    list.Add(verts);
            //}

            return list;
        }
    }

    // Doubly linked list
    public class MonotoneMountain
    {
        public Point tail, head;
        public int size;

        public List<Point> convexPoints;

        // Monotone mountain points
        public List<Point> monoPoly;

        // Triangles that constitute the mountain
        public List<List<Point>> triangles;

        // Convex polygons that constitute the mountain
        public List<Point> convexPolies;

        // Used to track which side of the line we are on
        public bool positive;

        // Almost Pi!
        public float PI_SLOP = 3.1f;

        public MonotoneMountain()
        {
            size = 0;
            tail = null;
            head = null;
            positive = false;
            convexPoints = new List<Point>();
            monoPoly = new List<Point>();
            triangles = new List<List<Point>>();
            convexPolies = new List<Point>();
        }

        // Append a point to the list
        public void add(Point point)
        {
            if (size == 0)
            {
                head = point;
                size = 1;
            }
            else if (size == 1)
            {
                // Keep repeat points out of the list
                if (point.neq(head))
                {
                    tail = point;
                    tail.prev = head;
                    head.next = tail;
                    size = 2;
                }
            }
            else
            {
                // Keep repeat points out of the list
                if (point.neq(tail))
                {
                    tail.next = point;
                    point.prev = tail;
                    tail = point;
                    size += 1;
                }
            }
        }

        // Remove a point from the list
        public void remove(Point point)
        {
            Point next = point.next;
            Point prev = point.prev;
            point.prev.next = next;
            point.next.prev = prev;
            size -= 1;
        }

        // Partition a x-monotone mountain into triangles O(n)
        // See "Computational Geometry in C", 2nd edition, by Joseph O'Rourke, page 52
        public void process()
        {
            // Establish the proper sign
            positive = angleSign();
            // create monotone polygon - for dubug purposes
            genMonoPoly();

            // Initialize internal angles at each nonbase vertex
            // Link strictly convex vertices into a list, ignore reflex vertices
            Point p = head.next;
            while (p != tail)
            {
                float a = angle(p);
                // If the point is almost colinear with it's neighbor, remove it!
                if (a >= PI_SLOP || a <= -PI_SLOP || a == 0.0)
                    remove(p);
                else if (convex(p))
                    convexPoints.Add(p);
                p = p.next;
            }

            triangulate();
        }

        private void triangulate()
        {
            while (convexPoints.Count != 0)
            {
                Point ear = convexPoints[0];
                convexPoints.RemoveAt(0);
                Point a = ear.prev;
                Point b = ear;
                Point c = ear.next;
                List<Point> triangle = new List<Point>(3);
                triangle.Add(a);
                triangle.Add(b);
                triangle.Add(c);

                triangles.Add(triangle);

                // Remove ear, update angles and convex list
                remove(ear);
                if (valid(a))
                    convexPoints.Add(a);
                if (valid(c))
                    convexPoints.Add(c);
            }

            Debug.Assert(size <= 3, "Triangulation bug, please report");
        }

        private bool valid(Point p)
        {
            return p != head && p != tail && convex(p);
        }

        // Create the monotone polygon
        private void genMonoPoly()
        {
            Point p = head;
            while (p != null)
            {
                monoPoly.Add(p);
                p = p.next;
            }
        }

        private float angle(Point p)
        {
            Point a = (p.next - p);
            Point b = (p.prev - p);
            return (float)Math.Atan2(a.cross(b), a.dot(b));
        }

        private bool angleSign()
        {
            Point a = (head.next - head);
            Point b = (tail - head);
            return Math.Atan2(a.cross(b), a.dot(b)) >= 0;
        }

        // Determines if the inslide angle is convex or reflex
        private bool convex(Point p)
        {
            if (positive != (angle(p) >= 0))
                return false;
            return true;
        }
    }

    // Node for a Directed Acyclic graph (DAG)
    public abstract class Node
    {
        public List<Node> parentList;
        public Node leftChild;
        public Node rightChild;

        public Node(Node left, Node right)
        {
            parentList = new List<Node>();
            leftChild = left;
            rightChild = right;

            if (left != null)
                left.parentList.Add(this);
            if (right != null)
                right.parentList.Add(this);
        }

        public abstract Sink locate(Edge s);

        // Replace a node in the graph with this node
        // Make sure parent pointers are updated
        public void replace(Node node)
        {
            foreach (Node parent in node.parentList)
            {
                // Select the correct node to replace (left or right child)
                if (parent.leftChild == node)
                    parent.leftChild = this;
                else
                    parent.rightChild = this;

            }
            //NOTE FPE: here is a difference between Scala and Python?
            parentList.AddRange(node.parentList);
        }
    }

    // Directed Acyclic graph (DAG)
    // See "Computational Geometry", 3rd edition, by Mark de Berg et al, Chapter 6.2
    public class QueryGraph
    {
        public Node head;

        public QueryGraph(Node head)
        {
            this.head = head;
        }

        public Trapezoid locate(Edge edge)
        {
            return head.locate(edge).trapezoid;
        }

        public List<Trapezoid> followEdge(Edge edge)
        {
            List<Trapezoid> trapezoids = new List<Trapezoid>();
            trapezoids.Add(locate(edge));
            int j = 0;

            while (edge.q.x > trapezoids[j].rightPoint.x)
            {
                if (edge.isAbove(trapezoids[j].rightPoint))
                {
                    trapezoids.Add(trapezoids[j].upperRight);
                }
                else
                {
                    trapezoids.Add(trapezoids[j].lowerRight);
                }
                j += 1;
            }
            return trapezoids;
        }

        public void replace(Sink sink, Node node)
        {
            //NOTE FPE: Choose Scala here
            if (sink.parentList.Count == 0)
                head = node;
            else
                node.replace(sink);
        }

        public void case1(Sink sink, Edge edge, Trapezoid[] tList)
        {
            YNode yNode = new YNode(edge, Sink.isink(tList[1]), Sink.isink(tList[2]));
            XNode qNode = new XNode(edge.q, yNode, Sink.isink(tList[3]));
            XNode pNode = new XNode(edge.p, Sink.isink(tList[0]), qNode);
            replace(sink, pNode);
        }

        public void case2(Sink sink, Edge edge, Trapezoid[] tList)
        {
            YNode yNode = new YNode(edge, Sink.isink(tList[1]), Sink.isink(tList[2]));
            XNode pNode = new XNode(edge.p, Sink.isink(tList[0]), yNode);
            replace(sink, pNode);
        }

        public void case3(Sink sink, Edge edge, Trapezoid[] tList)
        {
            YNode yNode = new YNode(edge, Sink.isink(tList[0]), Sink.isink(tList[1]));
            replace(sink, yNode);
        }

        public void case4(Sink sink, Edge edge, Trapezoid[] tList)
        {
            YNode yNode = new YNode(edge, Sink.isink(tList[0]), Sink.isink(tList[1]));
            XNode qNode = new XNode(edge.q, yNode, Sink.isink(tList[2]));
            replace(sink, qNode);
        }
    }

    public class Sink : Node
    {
        public Trapezoid trapezoid;

        public Sink(Trapezoid trapezoid)
            : base(null, null)
        {
            this.trapezoid = trapezoid;
            trapezoid.sink = this;
        }

        public static Sink isink(Trapezoid trapezoid)
        {
            //NOTE FPE: Choose Python here
            if (trapezoid.sink == null)
                return new Sink(trapezoid);
            return trapezoid.sink;
        }

        public override Sink locate(Edge edge)
        {
            return this;
        }
    }

    // See "Computational Geometry", 3rd edition, by Mark de Berg et al, Chapter 6.2
    public class TrapezoidalMap
    {
        // Trapezoid container
        public HashSet<Trapezoid> map;

        // AABB margin
        public float margin;

        // Bottom segment that spans multiple trapezoids
        public Edge bCross;

        // Top segment that spans multiple trapezoids
        public Edge tCross;

        public TrapezoidalMap()
        {
            map = new HashSet<Trapezoid>();
            margin = 50.0f;
            bCross = null;
            tCross = null;
        }

        public void clear()
        {
            bCross = null;
            tCross = null;
        }

        // Case 1: segment completely enclosed by trapezoid
        //         break trapezoid into 4 smaller trapezoids
        public Trapezoid[] case1(Trapezoid t, Edge e)
        {
            Trapezoid[] trapezoids = new Trapezoid[4];
            trapezoids[0] = new Trapezoid(t.leftPoint, e.p, t.top, t.bottom);
            trapezoids[1] = new Trapezoid(e.p, e.q, t.top, e);
            trapezoids[2] = new Trapezoid(e.p, e.q, e, t.bottom);
            trapezoids[3] = new Trapezoid(e.q, t.rightPoint, t.top, t.bottom);

            trapezoids[0].updateLeft(t.upperLeft, t.lowerLeft);
            trapezoids[1].updateLeftRight(trapezoids[0], null, trapezoids[3], null);
            trapezoids[2].updateLeftRight(null, trapezoids[0], null, trapezoids[3]);
            trapezoids[3].updateRight(t.upperRight, t.lowerRight);

            return trapezoids;
        }

        // Case 2: Trapezoid contains point p, q lies outside
        //         break trapezoid into 3 smaller trapezoids
        public Trapezoid[] case2(Trapezoid t, Edge e)
        {
            Point rp;
            if (e.q.x == t.rightPoint.x)
                rp = e.q;
            else
                rp = t.rightPoint;

            Trapezoid[] trapezoids = new Trapezoid[3];
            trapezoids[0] = new Trapezoid(t.leftPoint, e.p, t.top, t.bottom);
            trapezoids[1] = new Trapezoid(e.p, rp, t.top, e);
            trapezoids[2] = new Trapezoid(e.p, rp, e, t.bottom);

            trapezoids[0].updateLeft(t.upperLeft, t.lowerLeft);
            trapezoids[1].updateLeftRight(trapezoids[0], null, t.upperRight, null);
            trapezoids[2].updateLeftRight(null, trapezoids[0], null, t.lowerRight);

            bCross = t.bottom;
            tCross = t.top;

            e.above = trapezoids[1];
            e.below = trapezoids[2];

            return trapezoids;
        }

        // Case 3: Trapezoid is bisected
        public Trapezoid[] case3(Trapezoid t, Edge e)
        {
            Point lp;
            if (e.p.x == t.leftPoint.x)
                lp = e.p;
            else
                lp = t.leftPoint;

            Point rp;
            if (e.q.x == t.rightPoint.x)
                rp = e.q;
            else
                rp = t.rightPoint;

            Trapezoid[] trapezoids = new Trapezoid[2];

            if (tCross == t.top)
            {
                trapezoids[0] = t.upperLeft;
                trapezoids[0].updateRight(t.upperRight, null);
                trapezoids[0].rightPoint = rp;
            }
            else
            {
                trapezoids[0] = new Trapezoid(lp, rp, t.top, e);
                trapezoids[0].updateLeftRight(t.upperLeft, e.above, t.upperRight, null);
            }

            if (bCross == t.bottom)
            {
                trapezoids[1] = t.lowerLeft;
                trapezoids[1].updateRight(null, t.lowerRight);
                trapezoids[1].rightPoint = rp;
            }
            else
            {
                trapezoids[1] = new Trapezoid(lp, rp, e, t.bottom);
                trapezoids[1].updateLeftRight(e.below, t.lowerLeft, null, t.lowerRight);
            }

            bCross = t.bottom;
            tCross = t.top;

            e.above = trapezoids[0];
            e.below = trapezoids[1];

            return trapezoids;
        }

        // Case 4: Trapezoid contains point q, p lies outside
        //         break trapezoid into 3 smaller trapezoids
        public Trapezoid[] case4(Trapezoid t, Edge e)
        {
            Point lp;
            if (e.p.x == t.leftPoint.x)
                lp = e.p;
            else
                lp = t.leftPoint;

            Trapezoid[] trapezoids = new Trapezoid[3];

            if (tCross == t.top)
            {
                trapezoids[0] = t.upperLeft;
                trapezoids[0].rightPoint = e.q;
            }
            else
            {
                trapezoids[0] = new Trapezoid(lp, e.q, t.top, e);
                trapezoids[0].updateLeft(t.upperLeft, e.above);
            }

            if (bCross == t.bottom)
            {
                trapezoids[1] = t.lowerLeft;
                trapezoids[1].rightPoint = e.q;
            }
            else
            {
                trapezoids[1] = new Trapezoid(lp, e.q, e, t.bottom);
                trapezoids[1].updateLeft(e.below, t.lowerLeft);
            }

            trapezoids[2] = new Trapezoid(e.q, t.rightPoint, t.top, t.bottom);
            trapezoids[2].updateLeftRight(trapezoids[0], trapezoids[1], t.upperRight, t.lowerRight);

            return trapezoids;
        }

        // Create an AABB around segments
        public Trapezoid boundingBox(List<Edge> edges)
        {
            Point max = edges[0].p + margin;
            Point min = edges[0].q - margin;

            foreach (Edge e in edges)
            {
                if (e.p.x > max.x) max = new Point(e.p.x + margin, max.y);
                if (e.p.y > max.y) max = new Point(max.x, e.p.y + margin);
                if (e.q.x > max.x) max = new Point(e.q.x + margin, max.y);
                if (e.q.y > max.y) max = new Point(max.x, e.q.y + margin);
                if (e.p.x < min.x) min = new Point(e.p.x - margin, min.y);
                if (e.p.y < min.y) min = new Point(min.x, e.p.y - margin);
                if (e.q.x < min.x) min = new Point(e.q.x - margin, min.y);
                if (e.q.y < min.y) min = new Point(min.x, e.q.y - margin);
            }

            Edge top = new Edge(new Point(min.x, max.y), new Point(max.x, max.y));
            Edge bottom = new Edge(new Point(min.x, min.y), new Point(max.x, min.y));
            Point left = bottom.p;
            Point right = top.q;

            //TODO??
            //self.map[trap.key] = trap
            return new Trapezoid(left, right, top, bottom);
        }
    }

    public class Point
    {
        public float x, y;

        // Pointers to next and previous points in Monontone Mountain
        public Point next, prev;
        // The setment this point belongs to
        public Edge Edge;
        // List of edges this point constitutes an upper ending point (CDT)
        public List<Edge> edges = new List<Edge>();

        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
            next = null;
            prev = null;
        }

        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.x - p2.x, p1.y - p2.y);
        }

        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.x + p2.x, p1.y + p2.y);
        }

        public static Point operator -(Point p1, float f)
        {
            return new Point(p1.x - f, p1.y - f);
        }

        public static Point operator +(Point p1, float f)
        {
            return new Point(p1.x + f, p1.y + f);
        }

        public static Point operator *(Point p1, float c1)
        {
            return new Point(p1.x * c1, p1.y * c1);
        }

        public static Point operator /(Point p1, float c1)
        {
            return new Point(p1.x / c1, p1.y / c1);
        }

        public static bool operator <(Point p1, Point p2)
        {
            return p1.x < p2.x;
        }

        public static bool operator >(Point p1, Point p2)
        {
            if (p1.y < p2.y)
                return true;
            else if (p1.y > p2.y)
                return false;
            else
            {
                if (p1.x < p2.x)
                    return true;
                else
                    return false;
            }
        }

        public float cross(Point p)
        {
            return x * p.y - y * p.x;
        }

        public float dot(Point p)
        {
            return x * p.x + y * p.y;
        }

        public float length()
        {
            return (float)Math.Sqrt(x * x + y * y);
        }

        public Point normalize()
        {
            return this / length();
        }

        public bool less(Point p)
        {
            return x < p.x;
        }

        public bool neq(Point p)
        {
            return p.x != x || p.y != y;
        }

        public Point clone()
        {
            return new Point(x, y);
        }

        public static float orient2d(Point pa, Point pb, Point pc)
        {
            float acx = pa.x - pc.x;
            float bcx = pb.x - pc.x;
            float acy = pa.y - pc.y;
            float bcy = pb.y - pc.y;
            return acx * bcy - acy * bcx;
        }
    }

    // Represents a simple polygon's edge
    public class Edge
    {
        public Point p;
        public Point q;

        // Pointers used for building trapezoidal map
        public Trapezoid above, below;

        // Equation of a line: y = m*x + b
        // Slope of the line (m)
        public float slope;

        // Montone mountain points
        public List<Point> mPoints;

        // Y intercept
        public float b;

        public Edge(Point p, Point q)
        {
            this.p = p;
            this.q = q;

            if (q.x - p.x != 0)
                slope = (q.y - p.y) / (q.x - p.x);
            else
                slope = 0;

            b = p.y - (p.x * slope);
            above = null;
            below = null;
            mPoints = new List<Point>();
            mPoints.Add(p);
            mPoints.Add(q);
        }

        // Determines if this segment lies above the given point
        public bool isAbove(Point point)
        {
            return Point.orient2d(p, q, point) < 0;
        }

        // Determines if this segment lies below the given point
        public bool isBelow(Point point)
        {
            return Point.orient2d(p, q, point) > 0;
        }

        //private float signedArea(Point a, Point b, Point c)
        //{
        //    return (a.x - c.x) * (b.y - c.y) - (a.y - c.y) * (b.x - c.x);
        //}

        //public Point intersect(Point c, Point d)
        //{
        //    Point a = p;
        //    Point b = q;

        //    float a1 = signedArea(a, b, d);
        //    float a2 = signedArea(a, b, c);

        //    if (a1 != 0.0f && a2 != 0.0f && a1 * a2 < 0.0f)
        //    {
        //        float a3 = signedArea(c, d, a);
        //        float a4 = a3 + a2 - a1;
        //        if (a3 * a4 < 0.0f)
        //        {
        //            float t = a3 / (a3 - a4);
        //            return a + ((b - a) * t);
        //        }
        //    }

        //    throw new Exception("Error");
        //}
    }

    public class Trapezoid
    {
        public Sink sink;
        public bool inside;

        // Neighbor pointers
        public Trapezoid upperLeft;
        public Trapezoid lowerLeft;
        public Trapezoid upperRight;
        public Trapezoid lowerRight;

        public Point leftPoint;
        public Point rightPoint;

        public Edge top;
        public Edge bottom;

        public Trapezoid(Point leftPoint, Point rightPoint, Edge top, Edge bottom)
        {
            this.leftPoint = leftPoint;
            this.rightPoint = rightPoint;
            this.top = top;
            this.bottom = bottom;
            upperLeft = null;
            upperRight = null;
            lowerLeft = null;
            lowerRight = null;
            inside = true;
            sink = null;

            //TODO??
            //key = hash(self)
        }

        // Update neighbors to the left
        public void updateLeft(Trapezoid ul, Trapezoid ll)
        {
            upperLeft = ul; if (ul != null) ul.upperRight = this;
            lowerLeft = ll; if (ll != null) ll.lowerRight = this;
        }

        // Update neighbors to the right
        public void updateRight(Trapezoid ur, Trapezoid lr)
        {
            upperRight = ur; if (ur != null) ur.upperLeft = this;
            lowerRight = lr; if (lr != null) lr.lowerLeft = this;
        }

        // Update neighbors on both sides
        public void updateLeftRight(Trapezoid ul, Trapezoid ll, Trapezoid ur, Trapezoid lr)
        {
            upperLeft = ul; if (ul != null) ul.upperRight = this;
            lowerLeft = ll; if (ll != null) ll.lowerRight = this;
            upperRight = ur; if (ur != null) ur.upperLeft = this;
            lowerRight = lr; if (lr != null) lr.lowerLeft = this;
        }

        // Recursively trim outside neighbors
        public void trimNeighbors()
        {
            if (inside)
            {
                inside = false;
                if (upperLeft != null) upperLeft.trimNeighbors();
                if (lowerLeft != null) lowerLeft.trimNeighbors();
                if (upperRight != null) upperRight.trimNeighbors();
                if (lowerRight != null) lowerRight.trimNeighbors();
            }
        }

        // Determines if this point lies inside the trapezoid
        public bool contains(Point point)
        {
            return (point.x > leftPoint.x && point.x < rightPoint.x && top.isAbove(point) && bottom.isBelow(point));
        }

        public List<Point> vertices()
        {
            List<Point> verts = new List<Point>(4);
            verts.Add(lineIntersect(top, leftPoint.x));
            verts.Add(lineIntersect(bottom, leftPoint.x));
            verts.Add(lineIntersect(bottom, rightPoint.x));
            verts.Add(lineIntersect(top, rightPoint.x));
            return verts;
        }

        public Point lineIntersect(Edge edge, float x)
        {
            float y = edge.slope * x + edge.b;
            return new Point(x, y);
        }

        // Add points to monotone mountain
        public void addPoints()
        {
            if (leftPoint != bottom.p)
            {
                bottom.mPoints.Add(leftPoint.clone());
            }
            if (rightPoint != bottom.q)
            {
                bottom.mPoints.Add(rightPoint.clone());
            }
            if (leftPoint != top.p)
            {
                top.mPoints.Add(leftPoint.clone());
            }
            if (rightPoint != top.q)
            {
                top.mPoints.Add(rightPoint.clone());
            }
        }
    }

    public class XNode : Node
    {
        private Point point;

        public XNode(Point point, Node lChild, Node rChild)
            : base(lChild, rChild)
        {
            this.point = point;
        }

        public override Sink locate(Edge edge)
        {
            if (edge.p.x >= point.x)
                // Move to the right in the graph
                return rightChild.locate(edge);
            // Move to the left in the graph
            return leftChild.locate(edge);
        }
    }

    public class YNode : Node
    {
        private Edge edge;

        public YNode(Edge edge, Node lChild, Node rChild)
            : base(lChild, rChild)
        {
            this.edge = edge;
        }

        public override Sink locate(Edge edge)
        {
            if (this.edge.isAbove(edge.p))
                // Move down the graph
                return rightChild.locate(edge);

            if (this.edge.isBelow(edge.p))
                // Move up the graph
                return leftChild.locate(edge);

            // s and segment share the same endpoint, p
            if (edge.slope < this.edge.slope)
                // Move down the graph
                return rightChild.locate(edge);

            // Move up the graph
            return leftChild.locate(edge);
        }
    }

    // Based on Raimund Seidel's paper "A simple and fast incremental randomized
    // algorithm for computing trapezoidal decompositions and for triangulating polygons"
    // See also: "Computational Geometry", 3rd edition, by Mark de Berg et al, Chapter 6.2
    //           "Computational Geometry in C", 2nd edition, by Joseph O'Rourke
    public class Triangulator
    {
        public const float SHEER = 0.0001f;

        // Convex polygon list
        public List<List<Point>> polygons;

        // Order and randomize the segments
        public List<Edge> edgeList;

        // Trapezoid decomposition list
        public List<Trapezoid> trapezoids;

        // Initialize trapezoidal map and query structure
        public TrapezoidalMap trapezoidalMap;
        public Trapezoid boundingBox;
        public QueryGraph queryGraph;
        public List<MonotoneMountain> xMonoPoly;

        public Triangulator(List<Point> polyLine)
        {
            polygons = new List<List<Point>>();
            trapezoids = new List<Trapezoid>();
            xMonoPoly = new List<MonotoneMountain>();
            edgeList = initEdges(polyLine);
            trapezoidalMap = new TrapezoidalMap();
            boundingBox = trapezoidalMap.boundingBox(edgeList);
            queryGraph = new QueryGraph(Sink.isink(boundingBox));

            process();
        }

        public List<List<Point>> triangles()
        {
            List<List<Point>> triangles = new List<List<Point>>();
            foreach (List<Point> polygon in polygons)
            {
                List<Point> verts = new List<Point>();
                foreach (Point point in polygon)
                    verts.Add(point);

                triangles.Add(verts);
            }
            return triangles;
        }

        public HashSet<Trapezoid> trapezoidMap()
        {
            return trapezoidalMap.map;
        }

        // Build the trapezoidal map and query graph
        private void process()
        {
            //NOTE FPE: Python here
            foreach (Edge edge in edgeList)
            {
                List<Trapezoid> traps = queryGraph.followEdge(edge);

                // Remove trapezoids from trapezoidal Map
                foreach (Trapezoid t in traps)
                {
                    //TODO??
                    //del self.trapezoidal_map.map[t.key]

                    trapezoidalMap.map.Remove(t);

                    bool cp = t.contains(edge.p);
                    bool cq = t.contains(edge.q);
                    Trapezoid[] tList;

                    if (cp && cq)
                    {
                        tList = trapezoidalMap.case1(t, edge);
                        queryGraph.case1(t.sink, edge, tList);
                    }
                    else if (cp && !cq)
                    {
                        tList = trapezoidalMap.case2(t, edge);
                        queryGraph.case2(t.sink, edge, tList);
                    }
                    else if (!cp && !cq)
                    {
                        tList = trapezoidalMap.case3(t, edge);
                        queryGraph.case3(t.sink, edge, tList);
                    }
                    else
                    {
                        tList = trapezoidalMap.case4(t, edge);
                        queryGraph.case4(t.sink, edge, tList);
                    }
                    // Add new trapezoids to map
                    foreach (Trapezoid y in tList)
                    {
                        trapezoidalMap.map.Add(y);
                    }
                }
                trapezoidalMap.clear();
            }

            // Mark outside trapezoids
            foreach (Trapezoid t in trapezoidalMap.map)
            {
                markOutside(t);
            }

            // Collect interior trapezoids
            foreach (Trapezoid t in trapezoidalMap.map)
            {
                if (t.inside)
                {
                    trapezoids.Add(t);
                    t.addPoints();
                }
            }

            // Generate the triangles
            createMountains();


            //NOTE FPE: Scala here - seems to be a bug in this code
            //int i = 0;
            //while (i < edgeList.Count)
            //{
            //    Edge s = edgeList[i];
            //    List<Trapezoid> traps = queryGraph.followEdge(s);

            //    // Remove trapezoids from trapezoidal Map
            //    int j = 0;
            //    while (j < traps.Count)
            //    {
            //        trapezoidalMap.map.Remove(traps[j]);
            //        j += 1;
            //    }

            //    j = 0;
            //    while (j < traps.Count)
            //    {
            //        Trapezoid t = traps[j];
            //        Trapezoid[] tList;
            //        bool containsP = t.contains(s.p);
            //        bool containsQ = t.contains(s.q);
            //        if (containsP && containsQ)
            //        {
            //            // Case 1
            //            tList = trapezoidalMap.case1(t, s);
            //            queryGraph.case1(t.sink, s, tList);
            //        }
            //        else if (containsP && !containsQ)
            //        {
            //            // Case 2
            //            tList = trapezoidalMap.case2(t, s);
            //            queryGraph.case2(t.sink, s, tList);
            //        }
            //        else if (!containsP && !containsQ)
            //        {
            //            // Case 3
            //            tList = trapezoidalMap.case3(t, s);
            //            queryGraph.case3(t.sink, s, tList);
            //        }
            //        else
            //        {
            //            // Case 4
            //            tList = trapezoidalMap.case4(t, s);
            //            queryGraph.case4(t.sink, s, tList);
            //        }

            //        // Add new trapezoids to map
            //        int k = 0;
            //        while (k < tList.Length)
            //        {
            //            trapezoidalMap.map.Add(tList[k]);
            //            k += 1;
            //        }
            //        j += 1;
            //    }

            //    trapezoidalMap.clear();
            //    i += 1;
            //}

            //// Mark outside trapezoids
            //foreach (Trapezoid t in trapezoidalMap.map)
            //{
            //    markOutside(t);
            //}

            //// Collect interior trapezoids
            //foreach (Trapezoid t in trapezoidalMap.map)
            //{
            //    if (t.inside)
            //    {
            //        trapezoids.Add(t);
            //        t.addPoints();
            //    }

            //    // Generate the triangles
            //    createMountains();

            //    //println("# triangles = " + triangles.size)
            //}
        }

        // Monotone polygons - these are monotone mountains
        List<List<Point>> monoPolies()
        {
            List<List<Point>> polies = new List<List<Point>>();
            foreach (MonotoneMountain x in xMonoPoly)
                polies.Add(x.monoPoly);
            return polies;
        }

        // Build a list of x-monotone mountains
        private void createMountains()
        {
            //NOTE FPE: Python here
            foreach (Edge edge in edgeList)
            {
                if (edge.mPoints.Count > 2)
                {
                    MonotoneMountain mountain = new MonotoneMountain();

                    // Sorting is a perfromance hit. Literature says this can be accomplised in
                    // linear time, although I don't see a way around using traditional methods
                    // when using a randomized incremental algorithm

                    // Insertion sort is one of the fastest algorithms for sorting arrays containing 
                    // fewer than ten elements, or for lists that are already mostly sorted.

                    List<Point> points = new List<Point>(edge.mPoints);
                    points.Sort(new sorter());

                    foreach (Point p in points)
                        mountain.add(p);

                    // Triangulate monotone mountain
                    mountain.process();

                    // Extract the triangles into a single list
                    foreach (List<Point> t in mountain.triangles)
                    {
                        polygons.Add(t);
                    }

                    xMonoPoly.Add(mountain);
                }
            }

            //NOTE FPE: Scala here
            //int i = 0;
            //while (i < edgeList.Count)
            //{
            //    Edge s = edgeList[i];

            //    //NOTE FPE: > 0 in scala and > 2 in python?
            //    if (s.mPoints.Count > 0)
            //    {
            //        MonotoneMountain mountain = new MonotoneMountain();
            //        List<Point> k;

            //        // Sorting is a perfromance hit. Literature says this can be accomplised in
            //        // linear time, although I don't see a way around using traditional methods
            //        // when using a randomized incremental algorithm
            //        //if(s.mPoints.Count < 10) 
            //        // Insertion sort is one of the fastest algorithms for sorting arrays containing 
            //        // fewer than ten elements, or for lists that are already mostly sorted.
            //        //k = Util.insertSort((p1: Point, p2: Point) => p1 < p2)(s.mPoints).toList
            //        //else 
            //        //k = Util.msort((p1: Point, p2: Point) => p1 < p2)(s.mPoints.toList)

            //        k = new List<Point>(s.mPoints);
            //        k.Sort(new sort());

            //        //val points = s.p :: k ::: List(s.q)
            //        List<Point> points = new List<Point>();
            //        points.Add(s.p);
            //        points.AddRange(k);
            //        points.Add(s.q);

            //        int j = 0;
            //        while (j < points.Count)
            //        {
            //            mountain.add(points[j]);
            //            j += 1;
            //        }

            //        // Triangulate monotone mountain
            //        mountain.process();

            //        // Extract the triangles into a single list
            //        j = 0;
            //        while (j < mountain.triangles.Count)
            //        {
            //            polygons.Add(mountain.triangles[j]);
            //            j += 1;
            //        }

            //        xMonoPoly.Add(mountain);
            //    }
            //    i += 1;
            //}
        }

        // Mark the outside trapezoids surrounding the polygon
        private void markOutside(Trapezoid t)
        {
            if (t.top == boundingBox.top || t.bottom == boundingBox.bottom)
                t.trimNeighbors();
        }

        // Create segments and connect end points; update edge event pointer
        private List<Edge> initEdges(List<Point> points)
        {
            List<Edge> edges = new List<Edge>();

            //NOTE FPE: Python
            //for (int i = 0; i < points.Count; i++)
            //{
            //    int j;
            //    if (i < points.Count - 1)
            //        j = i + 1;
            //    else
            //        j = 0;

            //    Point p = new Point(points[i].x, points[i].y);
            //    Point q = new Point(points[j].x, points[j].y);
            //    edges.Add(new Edge(p, q));
            //}
            //return orderSegments(edges);

            //NOTE FPE: Scala
            for (int i = 0; i < points.Count-1; i++)
            {
                edges.Add(new Edge(points[i], points[i + 1]));
            }
            edges.Add(new Edge(points[0], points[points.Count-1]));
            return orderSegments(edges);
        }

        private List<Edge> orderSegments(List<Edge> edgeInput)
        {
            // Ignore vertical segments!
            List<Edge> edges = new List<Edge>();

            foreach (Edge e in edgeInput)
            {
                Point p = shearTransform(e.p);
                Point q = shearTransform(e.q);

                // Point p must be to the left of point q
                if (p.x > q.x)
                {
                    edges.Add(new Edge(q, p));
                }
                else if (p.x < q.x)
                {
                    edges.Add(new Edge(p, q));
                }
            }

            // Randomized triangulation improves performance
            // See Seidel's paper, or O'Rourke's book, p. 57 
            Shuffle(edges);
            return edges;
        }

        private static void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        // Prevents any two distinct endpoints from lying on a common vertical line, and avoiding
        // the degenerate case. See Mark de Berg et al, Chapter 6.3
        private Point shearTransform(Point point)
        {
            return new Point(point.x + SHEER * point.y, point.y);
        }

        private class sorter : IComparer<Point>
        {
            //NOTE FPE: Correct??
            public int Compare(Point p1, Point p2)
            {
                if (p1.x < p2.x)
                    return -1;

                if (p1.x == p2.x)
                    return 0;

                return 1;
            }
        }
    }
}