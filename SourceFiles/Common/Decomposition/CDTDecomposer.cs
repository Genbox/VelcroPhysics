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

namespace Poly2Tri
{
    public static class Poly2Tri
    {
        public static void Triangulate(PolygonSet ps)
        {
            TriangulationContext tcx = CreateContext();
            foreach (Polygon p in ps.Polygons)
            {
                tcx.PrepareTriangulation(p);
                Triangulate(tcx);
                tcx.Clear();
            }
        }

        public static void Triangulate(Polygon p)
        {
            Triangulate(p);
        }

        public static void Triangulate(ConstrainedPointSet cps)
        {
            Triangulate(cps);
        }

        public static void Triangulate(PointSet ps)
        {
            Triangulate(ps);
        }

        public static TriangulationContext CreateContext()
        {
            return new DTSweepContext();

        }

        public static void Triangulate(ITriangulatable t)
        {
            TriangulationContext tcx;

            //        long time = System.nanoTime();
            tcx = CreateContext();
            tcx.PrepareTriangulation(t);
            Triangulate(tcx);
            //        logger.info( "Triangulation of {} points [{}ms]", tcx.getPoints().size(), ( System.nanoTime() - time ) / 1e6 );
        }

        public static void Triangulate(TriangulationContext tcx)
        {
            DTSweep.Triangulate((DTSweepContext)tcx);
        }


        /// <summary>
        /// Will do a warmup run to let the JVM optimize the triangulation code -- or would if this were Java --MM
        /// </summary>
        public static void Warmup()
        {
#if false
    /*
			 * After a method is run 10000 times, the Hotspot compiler will compile
			 * it into native code. Periodically, the Hotspot compiler may recompile
			 * the method. After an unspecified amount of time, then the compilation
			 * system should become quiet.
			 */
			Polygon poly = PolygonGenerator.RandomCircleSweep2(50, 50000);
			TriangulationProcess process = new TriangulationProcess();
			process.triangulate(poly);
#endif
        }
    }
}