using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.Decomposition
{
    public class Polygon : List<Vector2>
    {
        public Polygon()
        {
            
        }

        public Polygon(IEnumerable<Vector2> polygon )
        {
            foreach (Vector2 vector2 in polygon)
            {
                Add(vector2);
            }
        }

        public Vector2 at(int i)
        {
            int s = Count;
            return this[i < 0 ? s - (-i % s) : i % s];
        }

        public Polygon copy(int i, int j)  {
            Polygon p = new Polygon();
            while(j < i) j += Count;
            //p.reserve(j - i + 1);
            for(; i <= j; ++i) {
                p.Add(at(i));
            }
            return p;
        }

        // precondition: ccw; see mnbayazit.com/406/bayazit for details about how this works
        public List<Polygon> convexPartition() 
        { 
            List<Polygon> list = new List<Polygon>();
            float d, dist1, dist2;            Vector2 ip;
            Vector2 ip1 = new Vector2();
            Vector2 ip2= new Vector2(); // intersection points
            int ind1 = 0, ind2 = 0;
            Polygon poly1, poly2;

            for(int i = 0; i < Count; ++i) {
                if(reflex(i)) {
                    dist1 = dist2 = float.MaxValue;// std::numeric_limits<qreal>::max();
                    for(int j = 0; j < Count; ++j) {
                        if(left(at(i - 1), at(i), at(j)) && rightOn(at(i - 1), at(i), at(j - 1))) { // if ray (i-1)->(i) intersects with edge (j, j-1)
                            //QLineF(at(i - 1), at(i)).intersect(QLineF(at(j), at(j - 1)), ip);                            ip = intersection(at(i - 1), at(i), at(j), at(j - 1));
                            if(right(at(i + 1), at(i), ip)) { // intersection point isn't caused by backwards ray
                                d = sqdist(at(i), ip);
                                if(d < dist1) { // take the closest intersection so we know it isn't blocked by another edge
                                    dist1 = d;
                                    ind1 = j;
                                    ip1 = ip;
                                }
                            }
                        }
                        if(left(at(i + 1), at(i), at(j + 1)) && rightOn(at(i + 1), at(i), at(j))) { // if ray (i+1)->(i) intersects with edge (j+1, j)
                            //QLineF(at(i + 1), at(i)).intersect(QLineF(at(j), at(j + 1)), ip);                            ip = intersection(at(i + 1), at(i), at(j), at(j + 1));
                            if(left(at(i - 1), at(i), ip)) {
                                d = sqdist(at(i), ip);
                                if(d < dist2) {
                                    dist2 = d;
                                    ind2 = j;
                                    ip2 = ip;
                                }
                            }
                        }
                    }
                    if(ind1 == (ind2 + 1) % Count) { // no vertices in range
                        Vector2 sp = ((ip1 + ip2) / 2);
                        poly1 = copy(i, ind2);
                        poly1.Add(sp);
                        poly2 = copy(ind1, i);
                        poly2.Add(sp);
                    } else {
                        double highestScore = 0, bestIndex = ind1, score;
                        while(ind2 < ind1) ind2 += Count;
                        for(int j = ind1; j <= ind2; ++j) {
                            if(canSee(i, j)) {
                                score = 1 / (sqdist(at(i), at(j)) + 1);
                                if(reflex(j)) {
                                    if(rightOn(at(j - 1), at(j), at(i)) && leftOn(at(j + 1), at(j), at(i))) {
                                        score += 3;
                                    } else {
                                        score += 2;
                                    }
                                } else {
                                    score += 1;
                                }
                                if(score > highestScore) {
                                    bestIndex = j;
                                    highestScore = score;
                                }
                            }
                        }
                        poly1 = copy(i, (int)bestIndex);
                        poly2 = copy((int)bestIndex, i);
                    }
                    list.AddRange(poly1.convexPartition());
                    list.AddRange(poly2.convexPartition());
                    return list;
                }
            }
            // polygon is already convex
            if(Count > Settings.MaxPolygonVertices) {
                poly1 = copy(0, Count / 2);
                poly2 = copy(Count / 2, 0);
                list.AddRange( poly1.convexPartition());
                list.AddRange(poly2.convexPartition());
            } else list.Add(this);
            return list;
        }


        public bool canSee(int i, int j)  {
            if(reflex(i)) {
                if(leftOn(at(i), at(i - 1), at(j)) && rightOn(at(i), at(i + 1), at(j))) return false;
            } else {
                if(rightOn(at(i), at(i + 1), at(j)) || leftOn(at(i), at(i - 1), at(j))) return false;
            }
            if(reflex(j)) {
                if(leftOn(at(j), at(j - 1), at(i)) && rightOn(at(j), at(j + 1), at(i))) return false;
            } else {
                if(rightOn(at(j), at(j + 1), at(i)) || leftOn(at(j), at(j - 1), at(i))) return false;
            }
            for(int k = 0; k < Count; ++k) {
                if((k + 1) % Count == i || k == i || (k + 1) % Count == j || k == j) {
                    continue; // ignore incident edges
                }
                //if(QLineF(at(i), at(j)).intersect(QLineF(at(k), at(k + 1)), NULL) == QLineF::BoundedIntersection) {
                if (intersection(at(i),at(j),at(k),at(k+1)) != Vector2.Zero)
                {
                    return false;
                }
            }
            return true;
        }

        public Vector2 intersection(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
        {
            Vector2 i = Vector2.Zero;
            float a1, b1, c1, a2, b2, c2, det;
            a1 = p2.Y - p1.Y;
            b1 = p1.X - p2.X;
            c1 = a1 * p1.X + b1 * p1.Y;
            a2 = q2.Y - q1.Y;
            b2 = q1.X - q2.X;
            c2 = a2 * q1.X + b2 * q1.Y;
            det = a1 * b2 - a2 * b1;
            if (!FloatEquals(det, 0))
            { // lines are not parallel
                i.X = (b2 * c1 - b1 * c2) / det;
                i.Y = (a1 * c2 - a2 * c1) / det;
            }
            return i;
        }

        public static bool FloatEquals(float value1, float value2)
        {
            return Math.Abs(value1 - value2) <= 1e-8;
        }

        public bool reflex(int i)  { // precondition: ccw
            return right(i);
        }

        public bool left(int i)  {
            return left(at(i - 1), at(i), at(i + 1));
        }

        bool leftOn(int i)  {
            return leftOn(at(i - 1), at(i), at(i + 1));
        }

        bool right(int i)  {
            return right(at(i - 1), at(i), at(i + 1));
        }

        bool rightOn(int i)  {
            return rightOn(at(i - 1), at(i), at(i + 1));
        }

        bool collinear(int i)  {
            return collinear(at(i - 1), at(i), at(i + 1));
        }

        float area( Vector2 a,  Vector2  b,  Vector2 c) {
            return((b.X - a.X)*(c.Y - a.Y)-((c.X - a.X)*(b.Y - a.Y)));
        }

        bool left( Vector2 a,  Vector2 b,  Vector2 c) {
            return area(a, b, c) > 0;
        }

        bool leftOn(  Vector2 a,   Vector2 b,   Vector2 c) {
            return area(a, b, c) >= 0;
        }

        bool right( Vector2 a,  Vector2 b,  Vector2 c) {
            return area(a, b, c) < 0;
        }

            bool rightOn( Vector2 a,  Vector2 b,  Vector2 c) {
            return area(a, b, c) <= 0;
        }

        bool collinear( Vector2 a,  Vector2 b,  Vector2 c) {
            return area(a, b, c) == 0;
        }

        float sqdist( Vector2 a,  Vector2 b) {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            return dx * dx + dy * dy;
        }

        float area()  {
            float a = 0;
            int j = Count - 1;
            for(int i = 1; i < j; ++i) {
                a += area((this)[0], (this)[i], (this)[i+1]);
            }
            return a;
        }
    }
}