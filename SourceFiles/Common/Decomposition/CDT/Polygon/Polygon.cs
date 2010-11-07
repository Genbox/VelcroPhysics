/* Poly2Tri
 * Copyright (c) 2009-2010, Poly2Tri Contributors
 * http://code.google.com/p/poly2tri/
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * * Redistributions of source code must retain the above copyright notice,
 *   this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright notice,
 *   this list of conditions and the following disclaimer in the documentation
 *   and/or other materials provided with the distribution.
 * * Neither the name of Poly2Tri nor the names of its contributors may be
 *   used to endorse or promote products derived from this software without specific
 *   prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

// Changes from the Java version
//   Polygon constructors sprused up, checks for 3+ polys
//   Naming of everything
//   getTriangulationMode() -> TriangulationMode { get; }
//   Exceptions replaced
// Future possibilities
//   We have a lot of Add/Clear methods -- we may prefer to just expose the container
//   Some self-explanitory methods may deserve commenting anyways

using System.Collections.Generic;
using FarseerPhysics.Common.Decomposition.CDT.Delaunay;
using FarseerPhysics.Common.Decomposition.CDT.Delaunay.Sweep;

namespace FarseerPhysics.Common.Decomposition.CDT.Polygon
{
    public class Polygon
    {
        private List<Polygon> _holes;
        private List<PolygonPoint> _points = new List<PolygonPoint>();
        private List<DelaunayTriangle> _triangles;

        public IList<Polygon> Holes
        {
            get { return _holes; }
        }

        #region Triangulatable Members

        public IList<PolygonPoint> Points
        {
            get { return _points; }
        }

        public IEnumerable<DelaunayTriangle> Triangles
        {
            get { return _triangles; }
        }

        public void AddTriangle(DelaunayTriangle t)
        {
            _triangles.Add(t);
        }

        public void AddTriangles(IEnumerable<DelaunayTriangle> list)
        {
            _triangles.AddRange(list);
        }

        /// <summary>
        /// Creates constraints and populates the context with points
        /// </summary>
        /// <param name="tcx">The context</param>
        public void Prepare(DTSweepContext tcx)
        {
            if (_triangles == null)
            {
                _triangles = new List<DelaunayTriangle>(_points.Count);
            }
            else
            {
                _triangles.Clear();
            }

            // Outer constraints
            for (int i = 0; i < _points.Count - 1; i++) tcx.NewConstraint(_points[i], _points[i + 1]);
            tcx.NewConstraint(_points[0], _points[_points.Count - 1]);
            tcx.Points.AddRange(_points);

            // Hole constraints
            if (_holes != null)
            {
                foreach (Polygon p in _holes)
                {
                    for (int i = 0; i < p._points.Count - 1; i++) tcx.NewConstraint(p._points[i], p._points[i + 1]);
                    tcx.NewConstraint(p._points[0], p._points[p._points.Count - 1]);
                    tcx.Points.AddRange(p._points);
                }
            }
        }

        #endregion

        /// <summary>
        /// Add a hole to the polygon.
        /// </summary>
        /// <param name="poly">A subtraction polygon fully contained inside this polygon.</param>
        public void AddHole(Polygon poly)
        {
            if (_holes == null) _holes = new List<Polygon>();
            _holes.Add(poly);
            // XXX: tests could be made here to be sure it is fully inside
            //        addSubtraction( poly.getPoints() );
        }
    }
}