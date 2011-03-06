using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Collision;
using Microsoft.Xna.Framework;
using System;

namespace FarseerPhysics.Common
{
    // Ported by Matthew Bettcher - Feb 2011

    /*
    Copyright (c) 2010, Luca Deltodesco
    All rights reserved.

    Redistribution and use in source and binary forms, with or without modification, are permitted
    provided that the following conditions are met:

        * Redistributions of source code must retain the above copyright notice, this list of conditions
	      and the following disclaimer.
        * Redistributions in binary form must reproduce the above copyright notice, this list of
	      conditions and the following disclaimer in the documentation and/or other materials provided
	      with the distribution.
        * Neither the name of the nape project nor the names of its contributors may be used to endorse
	     or promote products derived from this software without specific prior written permission.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
    IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
    FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
    CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
    DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
    DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
    IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
    OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
    */

    public static class MarchingSquares
    {
        


        /// <summary>
        /// Marching squares over the given domain using the mesh defined via the dimensions
        ///    (wid,hei) to build a set of polygons such that f(x,y) < 0, using the given number
        ///    'bin' for recursive linear inteprolation along cell boundaries.
        ///
        ///    if 'comb' is true, then the polygons will also be composited into larger possible concave
        ///    polygons.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="cell_width"></param>
        /// <param name="cell_height"></param>
        /// <param name="f"></param>
        /// <param name="lerp_count"></param>
        /// <param name="combine"></param>
        /// <returns></returns>
        public static List<Vertices> DetectSquares(AABB domain, float cell_width, float cell_height, sbyte[,] f, int lerp_count, bool combine)
        {
            var ret = new CxFastList<GeomPoly>();

            List<Vertices> verticesList = new List<Vertices>();

            List<GeomPoly> polyList = ret.GetListOfElements();

            GeomPoly gp = new GeomPoly();

            var xn = (int)(domain.Extents.X * 2 / cell_width); var xp = xn == (domain.Extents.X * 2 / cell_width);
            var yn = (int)(domain.Extents.Y * 2 / cell_height); var yp = yn == (domain.Extents.Y * 2 / cell_height);
            if (!xp) xn++;
            if (!yp) yn++;

            var fs = new sbyte[xn + 1, yn + 1];
            var ps = new GeomPolyVal[xn + 1, yn + 1];
            //populate shared function lookups.
            for (int x = 0; x < xn + 1; x++)
            {
                int x0;
                if (x == xn) x0 = (int)domain.UpperBound.X; else x0 = (int)(x * cell_width + domain.LowerBound.X);
                for (int y = 0; y < yn + 1; y++)
                {
                    int y0;
                    if (y == yn) y0 = (int)domain.UpperBound.Y; else y0 = (int)(y * cell_height + domain.LowerBound.Y);
                    fs[x, y] = f[x0, y0];
                }
            }

            //generate sub-polys and combine to scan lines
            for (int y = 0; y < yn; y++)
            {
                var y0 = y * cell_height + domain.LowerBound.Y; float y1; if (y == yn - 1) y1 = domain.UpperBound.Y; else y1 = y0 + cell_height;
                GeomPoly pre = null;
                for (int x = 0; x < xn; x++)
                {
                    var x0 = x * cell_width + domain.LowerBound.X; float x1; if (x == xn - 1) x1 = domain.UpperBound.X; else x1 = x0 + cell_width;
                    
                    gp = new GeomPoly();

                    var key = marchSquare(f, fs, ref gp, x, y, x0, y0, x1, y1, lerp_count);
                    if (gp.length != 0)
                    {
                        if (combine && pre != null && (key & 9) != 0)
                        {
                            combLeft(ref pre, ref gp);
                            gp = pre;
                        }
                        else
                            ret.add(gp);
                        ps[x, y] = new GeomPolyVal(gp, key);
                    }
                    else
                        gp = null;
                    pre = gp;
                }
            }
            if (!combine)
            {
                polyList = ret.GetListOfElements();

                foreach (var poly in polyList)
                {
                    verticesList.Add(new Vertices(poly.points.GetListOfElements()));
                }

                return verticesList;
            }

            //combine scan lines together
            for (int y = 1; y < yn; y++)
            {
                var x = 0;
                while (x < xn)
                {
                    var p = ps[x, y];

                    //skip along scan line if no polygon exists at this point
                    if (p == null) { x++; continue; }

                    //skip along if current polygon cannot be combined above.
                    if ((p.key & 12) == 0) { x++; continue; }

                    //skip along if no polygon exists above.
                    var u = ps[x, y - 1];
                    if (u == null) { x++; continue; }

                    //skip along if polygon above cannot be combined with.
                    if ((u.key & 3) == 0) { x++; continue; }

                    var ax = x * cell_width + domain.LowerBound.X;
                    var ay = y * cell_height + domain.LowerBound.Y;

                    var bp = p.p.points;
                    var ap = u.p.points;

                    //skip if it's already been combined with above polygon
                    if (u.p == p.p) { x++; continue; }

                    //combine above (but disallow the hole thingies
                    var bi = bp.begin();
                    while (square(bi.elem().Y - ay) > float.Epsilon || bi.elem().X < ax) bi = bi.next();

                    Vector2 b0 = bi.elem();
                    var b1 = bi.next().elem();
                    if (square(b1.Y - ay) > float.Epsilon) { x++; continue; }

                    var brk = true;
                    var ai = ap.begin();
                    while (ai != ap.end())
                    {
                        if (vec_dsq(ai.elem(), b1) < float.Epsilon)
                        {
                            brk = false;
                            break;
                        }
                        ai = ai.next();
                    }
                    if (brk) { x++; continue; }

                    var bj = bi.next().next(); if (bj == bp.end()) bj = bp.begin();
                    while (bj != bi)
                    {
                        ai = ap.insert(ai, bj.elem());  // .clone()
                        bj = bj.next(); if (bj == bp.end()) bj = bp.begin();
                        u.p.length++;
                    }
                    //u.p.simplify(float.Epsilon,float.Epsilon);
                    //
                    ax = x + 1;
                    while (ax < xn)
                    {
                        var p2 = ps[(int)ax, y];
                        if (p2 == null || p2.p != p.p) { ax++; continue; }
                        p2.p = u.p;
                        ax++;
                    }
                    ax = x - 1;
                    while (ax >= 0)
                    {
                        var p2 = ps[(int)ax, y];
                        if (p2 == null || p2.p != p.p) { ax--; continue; }
                        p2.p = u.p;
                        ax--;
                    }
                    ret.remove(p.p);
                    p.p = u.p;

                    x = (int)((bi.next().elem().X - domain.LowerBound.X) / cell_width) + 1;
                    //x++; this was already commented out!
                }
            }

            polyList = ret.GetListOfElements();

            foreach (var poly in polyList)
            {
               verticesList.Add(new Vertices(poly.points.GetListOfElements()));
            }

            return verticesList;
        }

        #region Private Methods

        //- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

        /** Linearly interpolate between (x0 to x1) given a value at these coordinates (v0 and v1)
            such as to approximate value(return) = 0
        **/
        private static float lerp(float x0, float x1, float v0, float v1)
        {
            var dv = v0 - v1;
            float t;
            if (dv * dv < float.Epsilon)
                t = 0.5f;
            else t = v0 / dv;
            return x0 + t * (x1 - x0);
        }

        //- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

        /** Recursive linear interpolation for use in marching squares **/
        private static float xlerp(float x0, float x1, float y, float v0, float v1, sbyte[,] f, int c)
        {
            var xm = lerp(x0, x1, v0, v1);
            if (c == 0) return xm;
            else
            {
                var vm = f[(int)xm, (int)y];
                if (v0 * vm < 0) return xlerp(x0, xm, y, v0, vm, f, c - 1);
                else return xlerp(xm, x1, y, vm, v1, f, c - 1);
            }
        }

        /** Recursive linear interpolation for use in marching squares **/
        private static float ylerp(float y0, float y1, float x, float v0, float v1, sbyte[,] f, int c)
        {
            var ym = lerp(y0, y1, v0, v1);
            if (c == 0) return ym;
            else
            {
                var vm = f[(int)x, (int)ym];
                if (v0 * vm < 0) return ylerp(y0, ym, x, v0, vm, f, c - 1);
                else return ylerp(ym, y1, x, vm, v1, f, c - 1);
            }
        }

        /** Square value for use in marching squares **/
        private static float square(float x) { return x * x; }

        private static float vec_dsq(Vector2 a, Vector2 b)
        {
            Vector2 d = a - b;
            return d.X * d.X + d.Y * d.Y;
        }

        private static float vec_cross(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        /** Look-up table to relate polygon key with the vertices that should be used for
            the sub polygon in marching squares
        **/
        private static int[] look_march = { 0x00, 0xE0, 0x38, 0xD8, 0x0E, 0xEE, 0x36, 0xD6, 0x83, 0x63, 0xBB, 0x5B, 0x8D, 0x6D, 0xB5, 0x55 };
        /** Perform a single celled marching square for for the given cell defined by (x0,y0) (x1,y1)
            using the function f for recursive interpolation, given the look-up table 'fs' of
            the values of 'f' at cell vertices with the result to be stored in 'poly' given the actual
            coordinates of 'ax' 'ay' in the marching squares mesh.
        **/
        private static int marchSquare(sbyte[,] f, sbyte[,] fs, ref GeomPoly poly, int ax, int ay, float x0, float y0, float x1, float y1, int bin)
        {
            //key lookup
            var key = 0;
            var v0 = fs[ax, ay]; if (v0 < 0) key |= 8;
            var v1 = fs[ax + 1, ay]; if (v1 < 0) key |= 4;
            var v2 = fs[ax + 1, ay + 1]; if (v2 < 0) key |= 2;
            var v3 = fs[ax, ay + 1]; if (v3 < 0) key |= 1;

            var val = look_march[key];
            if (val != 0)
            {
                CxFastListNode<Vector2> pi = null;
                for (int i = 0; i < 8; i++)
                {
                    Vector2 p;
                    if ((val & (1 << i)) != 0)
                    {
                        if (i == 7 && (val & 1) == 0)
                            poly.points.add(p = new Vector2(x0, ylerp(y0, y1, x0, v0, v3, f, bin)));
                        else
                        {
                            if (i == 0) p = new Vector2(x0, y0);
                            else if (i == 2) p = new Vector2(x1, y0);
                            else if (i == 4) p = new Vector2(x1, y1);
                            else if (i == 6) p = new Vector2(x0, y1);

                            else if (i == 1) p = new Vector2(xlerp(x0, x1, y0, v0, v1, f, bin), y0);
                            else if (i == 5) p = new Vector2(xlerp(x0, x1, y1, v3, v2, f, bin), y1);

                            else if (i == 3) p = new Vector2(x1, ylerp(y0, y1, x1, v1, v2, f, bin));
                            else p = new Vector2(x0, ylerp(y0, y1, x0, v0, v3, f, bin));

                            pi = poly.points.insert(pi, p);
                        }
                        poly.length++;
                    }
                }
                //poly.simplify(float.Epsilon,float.Epsilon);
            }
            return key;
        }

        /** Used in polygon composition to composit polygons into scan lines
            Combining polya and polyb into one super-polygon stored in polya.
        **/
        private static void combLeft(ref GeomPoly polya, ref GeomPoly polyb)
        {
            var ap = polya.points;
            var bp = polyb.points;
            var ai = ap.begin();
            var bi = bp.begin();

            var b = bi.elem();
            CxFastListNode<Vector2> prea = null;
            while (ai != ap.end())
            {
                var a = ai.elem();
                if (vec_dsq(a, b) < float.Epsilon)
                {
                    //ignore shared vertex if parallel
                    if (prea != null)
                    {
                        var a0 = prea.elem();
                        b = bi.next().elem();

                        Vector2 u = a - a0;
                        //vec_new(u); vec_sub(a.p.p, a0.p.p, u);
                        Vector2 v = b - a;
                        //vec_new(v); vec_sub(b.p.p, a.p.p, v);
                        var dot = vec_cross(u, v);
                        if (dot * dot < float.Epsilon)
                        {
                            ap.erase(prea, ai);
                            polya.length--;
                            ai = prea;
                        }
                    }

                    //insert polyb into polya
                    var fst = true;
                    CxFastListNode<Vector2> preb = null;
                    while (!bp.empty())
                    {
                        var bb = bp.front();
                        bp.pop();
                        if (!fst && !bp.empty())
                        {
                            ai = ap.insert(ai, bb);
                            polya.length++;
                            preb = ai;
                        }
                        fst = false;
                    }

                    //ignore shared vertex if parallel
                    ai = ai.next();
                    var a1 = ai.elem();
                    ai = ai.next(); if (ai == ap.end()) ai = ap.begin();
                    var a2 = ai.elem();
                    var a00 = preb.elem();
                    Vector2 uu = a1 - a00;
                    //vec_new(u); vec_sub(a1.p, a0.p, u);
                    Vector2 vv = a2 - a1;
                    //vec_new(v); vec_sub(a2.p, a1.p, v);
                    var dot1 = vec_cross(uu, vv);
                    if (dot1 * dot1 < float.Epsilon)
                    {
                        ap.erase(preb, preb.next());
                        polya.length--;
                    }

                    return;
                }
                prea = ai;
                ai = ai.next();
            }
        }

        #endregion

        #region CxFastList from nape physics

        internal class CxFastListNode<T>
        {
            internal T _elt;
            internal CxFastListNode<T> _next;

            public T elem()
            {
                return _elt;
            }

            public CxFastListNode<T> next()
            {
                return _next;
            }

            public CxFastListNode(T obj)
            {
                _elt = obj;
            }
        }


        /// <summary>
        /// Designed as a complete port of CxFastList from CxStd.
        /// </summary>
        internal class CxFastList<T>
        {
            // first node in the list
            private CxFastListNode<T> _head = null;
            private int count;

            /// <summary>
            /// Iterator to start of list (O(1))
            /// </summary>
            public CxFastListNode<T> begin()
            {
                return _head;
            }

            /// <summary>
            /// Iterator to end of list (O(1))
            /// </summary>
            public CxFastListNode<T> end()
            {
                return null;
            }

            /// <summary>
            /// Returns first element of list (O(1))
            /// </summary>
            public T front()
            {
                return _head.elem();
            }

            /// <summary>
            /// Returns last element of list (O(n))
            /// </summary>
            public T back()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Returns element at index 'ind' (O(ind))
            /// </summary>
            public T at(int i)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Returns iterator to element at 'ind' (O(ind))
            /// </summary>
            //public CxFastListNode<T> at(int i)
            //{
            //    throw new NotImplementedException();
            //}

            /// <summary>
            /// Reverses the list (O(n))
            /// </summary>
            public void reverse()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Element pointed to by iterator
            /// </summary>
            public T elem()
            {
                return _head.elem();
            }

            /// <summary>
            /// add object to list (O(1))
            /// </summary>
            public CxFastListNode<T> add(T value)
            {
                CxFastListNode<T> newNode = new CxFastListNode<T>(value);
                if (this._head == null)
                {
                    newNode._next = null;
                    this._head = newNode;
                    this.count++;
                    return newNode;
                }
                newNode._next = this._head;
                this._head = newNode;

                this.count++;

                return newNode;
            }

            /// <summary>
            /// add all elements of the list of same type. (O(n)) with n length of the argument list.
            /// </summary>
            public void addAll(CxFastList<T> list)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// remove object from list, returns true if an element was removed (O(n))
            /// </summary>
            public bool remove(T value)
            {
                CxFastListNode<T> head = _head;
                CxFastListNode<T> prev = _head;

                EqualityComparer<T> comparer = EqualityComparer<T>.Default;

                if (head != null)
                {
                    if (value != null)
                    {
                        do
                        {
                            // if we are on the value to be removed
                            if (comparer.Equals(head._elt, value))
                            {
                                // then we need to patch the list
                                // check to see if we are removing the _head
                                if (head == _head)
                                {
                                    _head = head._next;
                                    this.count--;
                                    return true;
                                }
                                else
                                {
                                    // were not at the head
                                    prev._next = head._next;
                                    this.count--;
                                    return true;
                                }
                            }
                            // cache the current as the previous for the next go around
                            prev = head;
                            head = head._next;
                        }
                        while (head != null);
                    }
                }
                return false;
            }

            /// <summary>
            /// pop element from head of list (O(1)) Note: this does not return the object popped! 
            /// There is good reason to this, and it regards the Alloc list variants which guarantee 
            /// objects are released to the object pool. You do not want to retrieve an element 
            /// through pop or else that object may suddenly be used by another piece of code which 
            /// retrieves it from the object pool.
            /// </summary>
            public CxFastListNode<T> pop()
            {
                return erase(null, _head);
            }

            /// <summary>
            /// insert object after 'node' returning an iterator to the inserted object.
            /// </summary>
            public CxFastListNode<T> insert(CxFastListNode<T> node, T value)
            {
                if (node == null)
                {
                    return add(value);
                }
                CxFastListNode<T> newNode = new CxFastListNode<T>(value);
                CxFastListNode<T> nextNode = node._next;
                newNode._next = nextNode;
                node._next = newNode;

                this.count++;

                return newNode;
            }

            /// <summary>
            /// removes the element pointed to by 'node' with 'prev' being the previous iterator, 
            /// returning an iterator to the element following that of 'node' (O(1))
            /// </summary>
            public CxFastListNode<T> erase(CxFastListNode<T> prev, CxFastListNode<T> node)
            {
                // cache the node after the node to be removed
                CxFastListNode<T> nextNode = node._next;
                if (prev != null)
                    prev._next = nextNode;
                else if (_head != null)
                    _head = _head._next;
                else
                    return null;

                this.count--;
                return nextNode;
            }

            /// <summary>
            /// removes 'cnt' elements starting at 'cur' with 'pre' being the previous iterator, 
            /// returning an iterator to the element following those deleted. (O(cnt)).
            /// </summary>
            public CxFastListNode<T> splice(CxFastListNode<T> pre, CxFastListNode<T> curr, int cnt)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// whether the list is empty (O(1))
            /// </summary>
            public bool empty()
            {
                if (_head == null)
                    return true;
                return false;
            }

            /// <summary>
            /// computes size of list (O(n))
            /// </summary>
            public int size()
            {
                var i = begin();
                var count = 0;

                do
                {
                    count++;
                } while (i.next() != null);

                return count;
            }

            /// <summary>
            /// empty the list (O(1) if CxMixList, O(n) otherwise)
            /// </summary>
            public void clear()
            {
                CxFastListNode<T> head = this._head;
                while (head != null)
                {
                    CxFastListNode<T> node2 = head;
                    head = head._next;
                    node2._next = null;
                }
                this._head = null;
                this.count = 0;
            }

            /// <summary>
            /// returns true if 'value' is an element of the list (O(n))
            /// </summary>
            public bool has(T value)
            {
                return (this.Find(value) != null);
            }

            // Non CxFastList Methods 
            public CxFastListNode<T> Find(T value)
            {
                // start at head
                CxFastListNode<T> head = this._head;
                EqualityComparer<T> comparer = EqualityComparer<T>.Default;
                if (head != null)
                {
                    if (value != null)
                    {
                        do
                        {
                            if (comparer.Equals(head._elt, value))
                            {
                                return head;
                            }
                            head = head._next;
                        }
                        while (head != this._head);
                    }
                    else
                    {
                        do
                        {
                            if (head._elt == null)
                            {
                                return head;
                            }
                            head = head._next;
                        }
                        while (head != this._head);
                    }
                }
                return null;
            }

            public List<T> GetListOfElements()
            {
                List<T> list = new List<T>();

                var iter = begin();

                if (iter != null)
                {
                    do
                    {
                        list.Add(iter._elt);
                        iter = iter._next;
                    } while (iter != null);

                }
                return list;
            }
        }

        #endregion

        #region Internal Stuff

        internal class GeomPolyVal
        {
            /** Associated polygon at coordinate **/
            public GeomPoly p;
            /** Key of original sub-polygon **/
            public int key;

            public GeomPolyVal(GeomPoly P, int K)
            {
                p = P;
                key = K;
            }
        }

        internal class GeomPoly
        {
            public CxFastList<Vector2> points;

            public int length;

            public GeomPoly()
            {
                points = new CxFastList<Vector2>();
                length = 0;
            }
        }

        #endregion
    }
}