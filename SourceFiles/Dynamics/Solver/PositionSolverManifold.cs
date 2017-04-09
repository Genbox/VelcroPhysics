using System.Diagnostics;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision;
using VelcroPhysics.Common;

namespace VelcroPhysics.Dynamics.Contacts
{
    public static class PositionSolverManifold
    {
        public static void Initialize(ContactPositionConstraint pc, Transform xfA, Transform xfB, int index, out Vector2 normal, out Vector2 point, out float separation)
        {
            Debug.Assert(pc.pointCount > 0);

            switch (pc.type)
            {
                case ManifoldType.Circles:
                    {
                        Vector2 pointA = MathUtils.Mul(ref xfA, pc.localPoint);
                        Vector2 pointB = MathUtils.Mul(ref xfB, pc.localPoints[0]);
                        normal = pointB - pointA;

                        //Velcro: Fix to handle zero normalization
                        if (normal != Vector2.Zero)
                            normal.Normalize();

                        point = 0.5f * (pointA + pointB);
                        separation = Vector2.Dot(pointB - pointA, normal) - pc.radiusA - pc.radiusB;
                    }
                    break;

                case ManifoldType.FaceA:
                    {
                        normal = MathUtils.Mul(xfA.q, pc.localNormal);
                        Vector2 planePoint = MathUtils.Mul(ref xfA, pc.localPoint);

                        Vector2 clipPoint = MathUtils.Mul(ref xfB, pc.localPoints[index]);
                        separation = Vector2.Dot(clipPoint - planePoint, normal) - pc.radiusA - pc.radiusB;
                        point = clipPoint;
                    }
                    break;

                case ManifoldType.FaceB:
                    {
                        normal = MathUtils.Mul(xfB.q, pc.localNormal);
                        Vector2 planePoint = MathUtils.Mul(ref xfB, pc.localPoint);

                        Vector2 clipPoint = MathUtils.Mul(ref xfA, pc.localPoints[index]);
                        separation = Vector2.Dot(clipPoint - planePoint, normal) - pc.radiusA - pc.radiusB;
                        point = clipPoint;

                        // Ensure normal points from A to B
                        normal = -normal;
                    }
                    break;
                default:
                    normal = Vector2.Zero;
                    point = Vector2.Zero;
                    separation = 0;
                    break;
            }
        }
    }
}