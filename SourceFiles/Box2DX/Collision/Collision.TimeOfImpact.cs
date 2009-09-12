/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2007 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

using Box2DX.Common;

namespace Box2DX.Collision
{
    public partial class Collision
    {
        public static int ToiCalls, ToiIters, ToiMaxIters;
        public static int ToiRootIters, ToiMaxRootIters;

        /// Inpute parameters for b2TimeOfImpact
        public class TOIInput
        {
            public DistanceProxy ProxyA = new DistanceProxy();
            public DistanceProxy ProxyB = new DistanceProxy();
            public Sweep SweepA;
            public Sweep SweepB;
            public float Tolerance;
        };

        public class SeparationFunction
        {
            public enum Type
            {
                Points,
                FaceA,
                FaceB
            };

            public void Initialize(SimplexCache cache,
                DistanceProxy proxyA, Transform transformA,
                DistanceProxy proxyB, Transform transformB)
            {
                _proxyA = proxyA;
                _proxyB = proxyB;
                int count = cache.Count;
                Box2DXDebug.Assert(0 < count && count < 3);

                if (count == 1)
                {
                    _type = Type.Points;
                    Vec2 localPointA = _proxyA.GetVertex(cache.IndexA[0]);
                    Vec2 localPointB = _proxyB.GetVertex(cache.IndexB[0]);
                    Vec2 pointA = Math.Mul(transformA, localPointA);
                    Vec2 pointB = Math.Mul(transformB, localPointB);
                    _axis = pointB - pointA;
                    _axis.Normalize();
                }
                else if (cache.IndexB[0] == cache.IndexB[1])
                {
                    // Two points on A and one on B
                    _type = Type.FaceA;
                    Vec2 localPointA1 = _proxyA.GetVertex(cache.IndexA[0]);
                    Vec2 localPointA2 = _proxyA.GetVertex(cache.IndexA[1]);
                    Vec2 localPointB = _proxyB.GetVertex(cache.IndexB[0]);
                    _localPoint = 0.5f*(localPointA1 + localPointA2);
                    _axis = Vec2.Cross(localPointA2 - localPointA1, 1.0f);
                    _axis.Normalize();

                    Vec2 normal = Math.Mul(transformA.R, _axis);
                    Vec2 pointA = Math.Mul(transformA, _localPoint);
                    Vec2 pointB = Math.Mul(transformB, localPointB);

                    float s = Vec2.Dot(pointB - pointA, normal);
                    if (s < 0.0f)
                    {
                        _axis = -_axis;
                    }
                }
                else if (cache.IndexA[0] == cache.IndexA[1])
                {
                    // Two points on B and one on A.
                    _type = Type.FaceB;
                    Vec2 localPointA = proxyA.GetVertex(cache.IndexA[0]);
                    Vec2 localPointB1 = proxyB.GetVertex(cache.IndexB[0]);
                    Vec2 localPointB2 = proxyB.GetVertex(cache.IndexB[1]);
                    _localPoint = 0.5f*(localPointB1 + localPointB2);
                    _axis = Vec2.Cross(localPointB2 - localPointB1, 1.0f);
                    _axis.Normalize();

                    Vec2 normal = Math.Mul(transformB.R, _axis);
                    Vec2 pointB = Math.Mul(transformB, _localPoint);
                    Vec2 pointA = Math.Mul(transformA, localPointA);

                    float s = Vec2.Dot(pointA - pointB, normal);
                    if (s < 0.0f)
                    {
                        _axis = -_axis;
                    }
                }
                else
                {
                    // Two points on B and two points on A.
                    // The faces are parallel.
                    Vec2 localPointA1 = _proxyA.GetVertex(cache.IndexA[0]);
                    Vec2 localPointA2 = _proxyA.GetVertex(cache.IndexA[1]);
                    Vec2 localPointB1 = _proxyB.GetVertex(cache.IndexB[0]);
                    Vec2 localPointB2 = _proxyB.GetVertex(cache.IndexB[1]);

                    Vec2 pA = Math.Mul(transformA, localPointA1);
                    Vec2 dA = Math.Mul(transformA.R, localPointA2 - localPointA1);
                    Vec2 pB = Math.Mul(transformB, localPointB1);
                    Vec2 dB = Math.Mul(transformB.R, localPointB2 - localPointB1);

                    float a = Vec2.Dot(dA, dA);
                    float e = Vec2.Dot(dB, dB);
                    Vec2 r = pA - pB;
                    float c = Vec2.Dot(dA, r);
                    float f = Vec2.Dot(dB, r);

                    float b = Vec2.Dot(dA, dB);
                    float denom = a*e - b*b;

                    float s = 0.0f;
                    if (denom != 0.0f)
                    {
                        s = Math.Clamp((b*f - c*e)/denom, 0.0f, 1.0f);
                    }

                    float t = (b*s + f)/e;

                    if (t < 0.0f)
                    {
                        t = 0.0f;
                        s = Math.Clamp(-c/a, 0.0f, 1.0f);
                    }
                    else if (t > 1.0f)
                    {
                        t = 1.0f;
                        s = Math.Clamp((b - c)/a, 0.0f, 1.0f);
                    }

                    Vec2 localPointA = localPointA1 + s*(localPointA2 - localPointA1);
                    Vec2 localPointB = localPointB1 + t*(localPointB2 - localPointB1);

                    if (s == 0.0f || s == 1.0f)
                    {
                        _type = Type.FaceB;
                        _axis = Vec2.Cross(localPointB2 - localPointB1, 1.0f);
                        _axis.Normalize();

                        _localPoint = localPointB;

                        Vec2 normal = Math.Mul(transformB.R, _axis);
                        Vec2 pointA = Math.Mul(transformA, localPointA);
                        Vec2 pointB = Math.Mul(transformB, localPointB);

                        float sgn = Vec2.Dot(pointA - pointB, normal);
                        if (sgn < 0.0f)
                        {
                            _axis = -_axis;
                        }
                    }
                    else
                    {
                        _type = Type.FaceA;
                        _axis = Vec2.Cross(localPointA2 - localPointA1, 1.0f);
                        _axis.Normalize();

                        _localPoint = localPointA;

                        Vec2 normal = Math.Mul(transformA.R, _axis);
                        Vec2 pointA = Math.Mul(transformA, localPointA);
                        Vec2 pointB = Math.Mul(transformB, localPointB);

                        float sgn = Vec2.Dot(pointB - pointA, normal);
                        if (sgn < 0.0f)
                        {
                            _axis = -_axis;
                        }
                    }
                }
            }

            public float Evaluate(Transform transformA, Transform transformB)
            {
                switch (_type)
                {
                    case Type.Points:
                        {
                            Vec2 axisA = Math.MulT(transformA.R, _axis);
                            Vec2 axisB = Math.MulT(transformB.R, -_axis);
                            Vec2 localPointA = _proxyA.GetSupportVertex(axisA);
                            Vec2 localPointB = _proxyB.GetSupportVertex(axisB);
                            Vec2 pointA = Math.Mul(transformA, localPointA);
                            Vec2 pointB = Math.Mul(transformB, localPointB);
                            float separation = Vec2.Dot(pointB - pointA, _axis);
                            return separation;
                        }

                    case Type.FaceA:
                        {
                            Vec2 normal = Math.Mul(transformA.R, _axis);
                            Vec2 pointA = Math.Mul(transformA, _localPoint);

                            Vec2 axisB = Math.MulT(transformB.R, -normal);

                            Vec2 localPointB = _proxyB.GetSupportVertex(axisB);
                            Vec2 pointB = Math.Mul(transformB, localPointB);

                            float separation = Vec2.Dot(pointB - pointA, normal);
                            return separation;
                        }

                    case Type.FaceB:
                        {
                            Vec2 normal = Math.Mul(transformB.R, _axis);
                            Vec2 pointB = Math.Mul(transformB, _localPoint);

                            Vec2 axisA = Math.MulT(transformA.R, -normal);

                            Vec2 localPointA = _proxyA.GetSupportVertex(axisA);
                            Vec2 pointA = Math.Mul(transformA, localPointA);

                            float separation = Vec2.Dot(pointA - pointB, normal);
                            return separation;
                        }

                    default:
                        Box2DXDebug.Assert(false);
                        return 0.0f;
                }
            }

            private DistanceProxy _proxyA;
            private DistanceProxy _proxyB;
            private Type _type;
            private Vec2 _localPoint;
            private Vec2 _axis;
        };

        /// <summary>
        /// CCD via the secant method.
        /// Compute the time when two shapes begin to touch or touch at a closer distance.
        /// TOI considers the shape radii. It attempts to have the radii overlap by the tolerance.
        /// Iterations terminate with the overlap is within 0.5 * tolerance. The tolerance should be
        /// smaller than sum of the shape radii.
        /// @warning the sweeps must have the same time interval.
        /// fraction=0 means the shapes begin touching/overlapped, and fraction=1 means the shapes don't touch.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="shapeA">The shape A.</param>
        /// <param name="shapeB">The shape B.</param>
        /// <returns>
        /// fraction between [0,1] in which the shapes first touch.
        /// </returns>
        public static float TimeOfImpact(TOIInput input)
        {
            ++ToiCalls;

            DistanceProxy proxyA = input.ProxyA;
            DistanceProxy proxyB = input.ProxyB;

            Sweep sweepA = input.SweepA;
            Sweep sweepB = input.SweepB;

            Box2DXDebug.Assert(sweepA.T0 == sweepB.T0);
            Box2DXDebug.Assert(1.0f - sweepA.T0 > Settings.FLT_EPSILON);

            float radius = proxyA._radius + proxyB._radius;
            float tolerance = input.Tolerance;

            float alpha = 0.0f;

            const int k_maxIterations = 1000; // TODO_ERIN b2Settings
            int iter = 0;
            float target = 0.0f;

            // Prepare input for distance query.
            SimplexCache cache = new SimplexCache();
            cache.Count = 0;
            DistanceInput distanceInput = new DistanceInput();
            distanceInput.proxyA = input.ProxyA;
            distanceInput.proxyB = input.ProxyB;
            distanceInput.UseRadii = false;

            for (;;)
            {
                Transform xfA, xfB;
                sweepA.GetTransform(out xfA, alpha);
                sweepB.GetTransform(out xfB, alpha);

                // Get the distance between shapes.
                distanceInput.TransformA = xfA;
                distanceInput.TransformB = xfB;
                DistanceOutput distanceOutput;
                Distance(out distanceOutput, cache, distanceInput);

                if (distanceOutput.Distance <= 0.0f)
                {
                    alpha = 1.0f;
                    break;
                }

                SeparationFunction fcn = new SeparationFunction();
                fcn.Initialize(cache, proxyA, xfA, proxyB, xfB);

                float separation = fcn.Evaluate(xfA, xfB);
                if (separation <= 0.0f)
                {
                    alpha = 1.0f;
                    break;
                }

                if (iter == 0)
                {
                    // Compute a reasonable target distance to give some breathing room
                    // for conservative advancement. We take advantage of the shape radii
                    // to create additional clearance.
                    if (separation > radius)
                    {
                        target = Math.Max(radius - tolerance, 0.75f*radius);
                    }
                    else
                    {
                        target = Math.Max(separation - tolerance, 0.02f*radius);
                    }
                }

                if (separation - target < 0.5f*tolerance)
                {
                    if (iter == 0)
                    {
                        alpha = 1.0f;
                        break;
                    }

                    break;
                }

#if false
		// Dump the curve seen by the root finder
		{
			const int N = 100;
			float dx = 1.0f / N;
			float xs[N+1];
			float fs[N+1];

			float x = 0.0f;

			for (int i = 0; i <= N; ++i)
			{
				sweepA.GetTransform(&xfA, x);
				sweepB.GetTransform(&xfB, x);
				float f = fcn.Evaluate(xfA, xfB) - target;

				printf("%g %g\n", x, f);

				xs[i] = x;
				fs[i] = f;

				x += dx;
			}
		}
#endif

                // Compute 1D root of: f(x) - target = 0
                float newAlpha = alpha;
                {
                    float x1 = alpha, x2 = 1.0f;

                    float f1 = separation;

                    sweepA.GetTransform(out xfA, x2);
                    sweepB.GetTransform(out xfB, x2);
                    float f2 = fcn.Evaluate(xfA, xfB);

                    // If intervals don't overlap at t2, then we are done.
                    if (f2 >= target)
                    {
                        alpha = 1.0f;
                        break;
                    }

                    // Determine when intervals intersect.
                    int rootIterCount = 0;
                    for (;;)
                    {
                        // Use a mix of the secant rule and bisection.
                        float x;
#warning "flag check is correct right?"
                        if ((rootIterCount & 1) != 0)
                        {
                            // Secant rule to improve convergence.
                            x = x1 + (target - f1)*(x2 - x1)/(f2 - f1);
                        }
                        else
                        {
                            // Bisection to guarantee progress.
                            x = 0.5f*(x1 + x2);
                        }

                        sweepA.GetTransform(out xfA, x);
                        sweepB.GetTransform(out xfB, x);

                        float f = fcn.Evaluate(xfA, xfB);

                        if (Math.Abs(f - target) < 0.025f*tolerance)
                        {
                            newAlpha = x;
                            break;
                        }

                        // Ensure we continue to bracket the root.
                        if (f > target)
                        {
                            x1 = x;
                            f1 = f;
                        }
                        else
                        {
                            x2 = x;
                            f2 = f;
                        }

                        ++rootIterCount;
                        ++ToiRootIters;

                        if (rootIterCount == 50)
                        {
                            break;
                        }
                    }

                    ToiMaxRootIters = Math.Max(ToiMaxRootIters, rootIterCount);
                }

                // Ensure significant advancement.
                if (newAlpha < (1.0f + 100.0f*Settings.FLT_EPSILON)*alpha)
                {
                    break;
                }

                alpha = newAlpha;

                ++iter;
                ++ToiIters;

                if (iter == k_maxIterations)
                {
                    break;
                }
            }

            ToiMaxIters = Math.Max(ToiMaxIters, iter);

            return alpha;
        }
    }
}