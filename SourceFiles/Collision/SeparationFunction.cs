using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using VelcroPhysics.Common;

namespace VelcroPhysics.Collision
{
    public static class SeparationFunction
    {
        [ThreadStatic]
        private static Vector2 _axis;

        [ThreadStatic]
        private static Vector2 _localPoint;

        [ThreadStatic]
        private static DistanceProxy _proxyA;

        [ThreadStatic]
        private static DistanceProxy _proxyB;

        [ThreadStatic]
        private static Sweep _sweepA,
                             _sweepB;

        [ThreadStatic]
        private static SeparationFunctionType _type;

        public static void Set(ref SimplexCache cache, DistanceProxy proxyA, ref Sweep sweepA, DistanceProxy proxyB, ref Sweep sweepB, float t1)
        {
            _localPoint = Vector2.Zero;
            _proxyA = proxyA;
            _proxyB = proxyB;
            int count = cache.Count;
            Debug.Assert(0 < count && count < 3);

            _sweepA = sweepA;
            _sweepB = sweepB;

            Transform xfA, xfB;
            _sweepA.GetTransform(out xfA, t1);
            _sweepB.GetTransform(out xfB, t1);

            if (count == 1)
            {
                _type = SeparationFunctionType.Points;
                Vector2 localPointA = _proxyA.Vertices[cache.IndexA[0]];
                Vector2 localPointB = _proxyB.Vertices[cache.IndexB[0]];
                Vector2 pointA = MathUtils.Mul(ref xfA, localPointA);
                Vector2 pointB = MathUtils.Mul(ref xfB, localPointB);
                _axis = pointB - pointA;
                _axis.Normalize();
            }
            else if (cache.IndexA[0] == cache.IndexA[1])
            {
                // Two points on B and one on A.
                _type = SeparationFunctionType.FaceB;
                Vector2 localPointB1 = proxyB.Vertices[cache.IndexB[0]];
                Vector2 localPointB2 = proxyB.Vertices[cache.IndexB[1]];

                Vector2 a = localPointB2 - localPointB1;
                _axis = new Vector2(a.Y, -a.X);
                _axis.Normalize();
                Vector2 normal = MathUtils.Mul(ref xfB.q, _axis);

                _localPoint = 0.5f * (localPointB1 + localPointB2);
                Vector2 pointB = MathUtils.Mul(ref xfB, _localPoint);

                Vector2 localPointA = proxyA.Vertices[cache.IndexA[0]];
                Vector2 pointA = MathUtils.Mul(ref xfA, localPointA);

                float s = Vector2.Dot(pointA - pointB, normal);
                if (s < 0.0f)
                {
                    _axis = -_axis;
                }
            }
            else
            {
                // Two points on A and one or two points on B.
                _type = SeparationFunctionType.FaceA;
                Vector2 localPointA1 = _proxyA.Vertices[cache.IndexA[0]];
                Vector2 localPointA2 = _proxyA.Vertices[cache.IndexA[1]];

                Vector2 a = localPointA2 - localPointA1;
                _axis = new Vector2(a.Y, -a.X);
                _axis.Normalize();
                Vector2 normal = MathUtils.Mul(ref xfA.q, _axis);

                _localPoint = 0.5f * (localPointA1 + localPointA2);
                Vector2 pointA = MathUtils.Mul(ref xfA, _localPoint);

                Vector2 localPointB = _proxyB.Vertices[cache.IndexB[0]];
                Vector2 pointB = MathUtils.Mul(ref xfB, localPointB);

                float s = Vector2.Dot(pointB - pointA, normal);
                if (s < 0.0f)
                {
                    _axis = -_axis;
                }
            }

            //Velcro note: the returned value that used to be here has been removed, as it was not used.
        }

        public static float FindMinSeparation(out int indexA, out int indexB, float t)
        {
            Transform xfA, xfB;
            _sweepA.GetTransform(out xfA, t);
            _sweepB.GetTransform(out xfB, t);

            switch (_type)
            {
                case SeparationFunctionType.Points:
                {
                    Vector2 axisA = MathUtils.MulT(ref xfA.q, _axis);
                    Vector2 axisB = MathUtils.MulT(ref xfB.q, -_axis);

                    indexA = _proxyA.GetSupport(axisA);
                    indexB = _proxyB.GetSupport(axisB);

                    Vector2 localPointA = _proxyA.Vertices[indexA];
                    Vector2 localPointB = _proxyB.Vertices[indexB];

                    Vector2 pointA = MathUtils.Mul(ref xfA, localPointA);
                    Vector2 pointB = MathUtils.Mul(ref xfB, localPointB);

                    float separation = Vector2.Dot(pointB - pointA, _axis);
                    return separation;
                }

                case SeparationFunctionType.FaceA:
                {
                    Vector2 normal = MathUtils.Mul(ref xfA.q, _axis);
                    Vector2 pointA = MathUtils.Mul(ref xfA, _localPoint);

                    Vector2 axisB = MathUtils.MulT(ref xfB.q, -normal);

                    indexA = -1;
                    indexB = _proxyB.GetSupport(axisB);

                    Vector2 localPointB = _proxyB.Vertices[indexB];
                    Vector2 pointB = MathUtils.Mul(ref xfB, localPointB);

                    float separation = Vector2.Dot(pointB - pointA, normal);
                    return separation;
                }

                case SeparationFunctionType.FaceB:
                {
                    Vector2 normal = MathUtils.Mul(ref xfB.q, _axis);
                    Vector2 pointB = MathUtils.Mul(ref xfB, _localPoint);

                    Vector2 axisA = MathUtils.MulT(ref xfA.q, -normal);

                    indexB = -1;
                    indexA = _proxyA.GetSupport(axisA);

                    Vector2 localPointA = _proxyA.Vertices[indexA];
                    Vector2 pointA = MathUtils.Mul(ref xfA, localPointA);

                    float separation = Vector2.Dot(pointA - pointB, normal);
                    return separation;
                }

                default:
                    Debug.Assert(false);
                    indexA = -1;
                    indexB = -1;
                    return 0.0f;
            }
        }

        public static float Evaluate(int indexA, int indexB, float t)
        {
            Transform xfA, xfB;
            _sweepA.GetTransform(out xfA, t);
            _sweepB.GetTransform(out xfB, t);

            switch (_type)
            {
                case SeparationFunctionType.Points:
                {
                    Vector2 localPointA = _proxyA.Vertices[indexA];
                    Vector2 localPointB = _proxyB.Vertices[indexB];

                    Vector2 pointA = MathUtils.Mul(ref xfA, localPointA);
                    Vector2 pointB = MathUtils.Mul(ref xfB, localPointB);
                    float separation = Vector2.Dot(pointB - pointA, _axis);

                    return separation;
                }
                case SeparationFunctionType.FaceA:
                {
                    Vector2 normal = MathUtils.Mul(ref xfA.q, _axis);
                    Vector2 pointA = MathUtils.Mul(ref xfA, _localPoint);

                    Vector2 localPointB = _proxyB.Vertices[indexB];
                    Vector2 pointB = MathUtils.Mul(ref xfB, localPointB);

                    float separation = Vector2.Dot(pointB - pointA, normal);
                    return separation;
                }
                case SeparationFunctionType.FaceB:
                {
                    Vector2 normal = MathUtils.Mul(ref xfB.q, _axis);
                    Vector2 pointB = MathUtils.Mul(ref xfB, _localPoint);

                    Vector2 localPointA = _proxyA.Vertices[indexA];
                    Vector2 pointA = MathUtils.Mul(ref xfA, localPointA);

                    float separation = Vector2.Dot(pointA - pointB, normal);
                    return separation;
                }
                default:
                    Debug.Assert(false);
                    return 0.0f;
            }
        }
    }
}