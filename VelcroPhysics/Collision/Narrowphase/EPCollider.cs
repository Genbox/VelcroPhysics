using System;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Shared;
using VelcroPhysics.Shared.Optimization;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Collision.Narrowphase
{
    public class EPCollider
    {
        private Vector2 _centroidB;
        private bool _front;
        private Vector2 _lowerLimit, _upperLimit;
        private Vector2 _normal;
        private Vector2 _normal0, _normal1, _normal2;
        private TempPolygon _polygonB = new TempPolygon();
        private float _radius;
        private Vector2 _v0, _v1, _v2, _v3;
        private Transform _xf;

        public void Collide(ref Manifold manifold, EdgeShape edgeA, ref Transform xfA, PolygonShape polygonB, ref Transform xfB)
        {
            // Algorithm:
            // 1. Classify v1 and v2
            // 2. Classify polygon centroid as front or back
            // 3. Flip normal if necessary
            // 4. Initialize normal range to [-pi, pi] about face normal
            // 5. Adjust normal range according to adjacent edges
            // 6. Visit each separating axes, only accept axes within the range
            // 7. Return if _any_ axis indicates separation
            // 8. Clip

            _xf = MathUtils.MulT(xfA, xfB);

            _centroidB = MathUtils.Mul(ref _xf, polygonB.MassData.Centroid);

            _v0 = edgeA.Vertex0;
            _v1 = edgeA._vertex1;
            _v2 = edgeA._vertex2;
            _v3 = edgeA.Vertex3;

            bool hasVertex0 = edgeA.HasVertex0;
            bool hasVertex3 = edgeA.HasVertex3;

            Vector2 edge1 = _v2 - _v1;
            edge1.Normalize();
            _normal1 = new Vector2(edge1.Y, -edge1.X);
            float offset1 = Vector2.Dot(_normal1, _centroidB - _v1);
            float offset0 = 0.0f, offset2 = 0.0f;
            bool convex1 = false, convex2 = false;

            // Is there a preceding edge?
            if (hasVertex0)
            {
                Vector2 edge0 = _v1 - _v0;
                edge0.Normalize();
                _normal0 = new Vector2(edge0.Y, -edge0.X);
                convex1 = MathUtils.Cross(edge0, edge1) >= 0.0f;
                offset0 = Vector2.Dot(_normal0, _centroidB - _v0);
            }

            // Is there a following edge?
            if (hasVertex3)
            {
                Vector2 edge2 = _v3 - _v2;
                edge2.Normalize();
                _normal2 = new Vector2(edge2.Y, -edge2.X);
                convex2 = MathUtils.Cross(edge1, edge2) > 0.0f;
                offset2 = Vector2.Dot(_normal2, _centroidB - _v2);
            }

            // Determine front or back collision. Determine collision normal limits.
            if (hasVertex0 && hasVertex3)
            {
                if (convex1 && convex2)
                {
                    _front = offset0 >= 0.0f || offset1 >= 0.0f || offset2 >= 0.0f;
                    if (_front)
                    {
                        _normal = _normal1;
                        _lowerLimit = _normal0;
                        _upperLimit = _normal2;
                    }
                    else
                    {
                        _normal = -_normal1;
                        _lowerLimit = -_normal1;
                        _upperLimit = -_normal1;
                    }
                }
                else if (convex1)
                {
                    _front = offset0 >= 0.0f || (offset1 >= 0.0f && offset2 >= 0.0f);
                    if (_front)
                    {
                        _normal = _normal1;
                        _lowerLimit = _normal0;
                        _upperLimit = _normal1;
                    }
                    else
                    {
                        _normal = -_normal1;
                        _lowerLimit = -_normal2;
                        _upperLimit = -_normal1;
                    }
                }
                else if (convex2)
                {
                    _front = offset2 >= 0.0f || (offset0 >= 0.0f && offset1 >= 0.0f);
                    if (_front)
                    {
                        _normal = _normal1;
                        _lowerLimit = _normal1;
                        _upperLimit = _normal2;
                    }
                    else
                    {
                        _normal = -_normal1;
                        _lowerLimit = -_normal1;
                        _upperLimit = -_normal0;
                    }
                }
                else
                {
                    _front = offset0 >= 0.0f && offset1 >= 0.0f && offset2 >= 0.0f;
                    if (_front)
                    {
                        _normal = _normal1;
                        _lowerLimit = _normal1;
                        _upperLimit = _normal1;
                    }
                    else
                    {
                        _normal = -_normal1;
                        _lowerLimit = -_normal2;
                        _upperLimit = -_normal0;
                    }
                }
            }
            else if (hasVertex0)
            {
                if (convex1)
                {
                    _front = offset0 >= 0.0f || offset1 >= 0.0f;
                    if (_front)
                    {
                        _normal = _normal1;
                        _lowerLimit = _normal0;
                        _upperLimit = -_normal1;
                    }
                    else
                    {
                        _normal = -_normal1;
                        _lowerLimit = _normal1;
                        _upperLimit = -_normal1;
                    }
                }
                else
                {
                    _front = offset0 >= 0.0f && offset1 >= 0.0f;
                    if (_front)
                    {
                        _normal = _normal1;
                        _lowerLimit = _normal1;
                        _upperLimit = -_normal1;
                    }
                    else
                    {
                        _normal = -_normal1;
                        _lowerLimit = _normal1;
                        _upperLimit = -_normal0;
                    }
                }
            }
            else if (hasVertex3)
            {
                if (convex2)
                {
                    _front = offset1 >= 0.0f || offset2 >= 0.0f;
                    if (_front)
                    {
                        _normal = _normal1;
                        _lowerLimit = -_normal1;
                        _upperLimit = _normal2;
                    }
                    else
                    {
                        _normal = -_normal1;
                        _lowerLimit = -_normal1;
                        _upperLimit = _normal1;
                    }
                }
                else
                {
                    _front = offset1 >= 0.0f && offset2 >= 0.0f;
                    if (_front)
                    {
                        _normal = _normal1;
                        _lowerLimit = -_normal1;
                        _upperLimit = _normal1;
                    }
                    else
                    {
                        _normal = -_normal1;
                        _lowerLimit = -_normal2;
                        _upperLimit = _normal1;
                    }
                }
            }
            else
            {
                _front = offset1 >= 0.0f;
                if (_front)
                {
                    _normal = _normal1;
                    _lowerLimit = -_normal1;
                    _upperLimit = -_normal1;
                }
                else
                {
                    _normal = -_normal1;
                    _lowerLimit = _normal1;
                    _upperLimit = _normal1;
                }
            }

            // Get polygonB in frameA
            _polygonB.Count = polygonB.Vertices.Count;
            for (int i = 0; i < polygonB.Vertices.Count; ++i)
            {
                _polygonB.Vertices[i] = MathUtils.Mul(ref _xf, polygonB.Vertices[i]);
                _polygonB.Normals[i] = MathUtils.Mul(_xf.q, polygonB.Normals[i]);
            }

            _radius = polygonB.Radius + edgeA.Radius;

            manifold.PointCount = 0;

            EPAxis edgeAxis = ComputeEdgeSeparation();

            // If no valid normal can be found than this edge should not collide.
            if (edgeAxis.Type == EPAxisType.Unknown)
            {
                return;
            }

            if (edgeAxis.Separation > _radius)
            {
                return;
            }

            EPAxis polygonAxis = ComputePolygonSeparation();
            if (polygonAxis.Type != EPAxisType.Unknown && polygonAxis.Separation > _radius)
            {
                return;
            }

            // Use hysteresis for jitter reduction.
            const float k_relativeTol = 0.98f;
            const float k_absoluteTol = 0.001f;

            EPAxis primaryAxis;
            if (polygonAxis.Type == EPAxisType.Unknown)
            {
                primaryAxis = edgeAxis;
            }
            else if (polygonAxis.Separation > k_relativeTol * edgeAxis.Separation + k_absoluteTol)
            {
                primaryAxis = polygonAxis;
            }
            else
            {
                primaryAxis = edgeAxis;
            }

            FixedArray2<ClipVertex> ie = new FixedArray2<ClipVertex>();
            ReferenceFace rf;
            if (primaryAxis.Type == EPAxisType.EdgeA)
            {
                manifold.Type = ManifoldType.FaceA;

                // Search for the polygon normal that is most anti-parallel to the edge normal.
                int bestIndex = 0;
                float bestValue = Vector2.Dot(_normal, _polygonB.Normals[0]);
                for (int i = 1; i < _polygonB.Count; ++i)
                {
                    float value = Vector2.Dot(_normal, _polygonB.Normals[i]);
                    if (value < bestValue)
                    {
                        bestValue = value;
                        bestIndex = i;
                    }
                }

                int i1 = bestIndex;
                int i2 = i1 + 1 < _polygonB.Count ? i1 + 1 : 0;

                ie.Value0.V = _polygonB.Vertices[i1];
                ie.Value0.ID.ContactFeature.IndexA = 0;
                ie.Value0.ID.ContactFeature.IndexB = (byte)i1;
                ie.Value0.ID.ContactFeature.TypeA = ContactFeatureType.Face;
                ie.Value0.ID.ContactFeature.TypeB = ContactFeatureType.Vertex;

                ie.Value1.V = _polygonB.Vertices[i2];
                ie.Value1.ID.ContactFeature.IndexA = 0;
                ie.Value1.ID.ContactFeature.IndexB = (byte)i2;
                ie.Value1.ID.ContactFeature.TypeA = ContactFeatureType.Face;
                ie.Value1.ID.ContactFeature.TypeB = ContactFeatureType.Vertex;

                if (_front)
                {
                    rf.i1 = 0;
                    rf.i2 = 1;
                    rf.v1 = _v1;
                    rf.v2 = _v2;
                    rf.Normal = _normal1;
                }
                else
                {
                    rf.i1 = 1;
                    rf.i2 = 0;
                    rf.v1 = _v2;
                    rf.v2 = _v1;
                    rf.Normal = -_normal1;
                }
            }
            else
            {
                manifold.Type = ManifoldType.FaceB;

                ie.Value0.V = _v1;
                ie.Value0.ID.ContactFeature.IndexA = 0;
                ie.Value0.ID.ContactFeature.IndexB = (byte)primaryAxis.Index;
                ie.Value0.ID.ContactFeature.TypeA = ContactFeatureType.Vertex;
                ie.Value0.ID.ContactFeature.TypeB = ContactFeatureType.Face;

                ie.Value1.V = _v2;
                ie.Value1.ID.ContactFeature.IndexA = 0;
                ie.Value1.ID.ContactFeature.IndexB = (byte)primaryAxis.Index;
                ie.Value1.ID.ContactFeature.TypeA = ContactFeatureType.Vertex;
                ie.Value1.ID.ContactFeature.TypeB = ContactFeatureType.Face;

                rf.i1 = primaryAxis.Index;
                rf.i2 = rf.i1 + 1 < _polygonB.Count ? rf.i1 + 1 : 0;
                rf.v1 = _polygonB.Vertices[rf.i1];
                rf.v2 = _polygonB.Vertices[rf.i2];
                rf.Normal = _polygonB.Normals[rf.i1];
            }

            rf.SideNormal1 = new Vector2(rf.Normal.Y, -rf.Normal.X);
            rf.SideNormal2 = -rf.SideNormal1;
            rf.SideOffset1 = Vector2.Dot(rf.SideNormal1, rf.v1);
            rf.SideOffset2 = Vector2.Dot(rf.SideNormal2, rf.v2);

            // Clip incident edge against extruded edge1 side edges.
            FixedArray2<ClipVertex> clipPoints1;
            FixedArray2<ClipVertex> clipPoints2;
            int np;

            // Clip to box side 1
            np = Collision.ClipSegmentToLine(out clipPoints1, ref ie, rf.SideNormal1, rf.SideOffset1, rf.i1);

            if (np < Settings.MaxManifoldPoints)
            {
                return;
            }

            // Clip to negative box side 1
            np = Collision.ClipSegmentToLine(out clipPoints2, ref clipPoints1, rf.SideNormal2, rf.SideOffset2, rf.i2);

            if (np < Settings.MaxManifoldPoints)
            {
                return;
            }

            // Now clipPoints2 contains the clipped points.
            if (primaryAxis.Type == EPAxisType.EdgeA)
            {
                manifold.LocalNormal = rf.Normal;
                manifold.LocalPoint = rf.v1;
            }
            else
            {
                manifold.LocalNormal = polygonB.Normals[rf.i1];
                manifold.LocalPoint = polygonB.Vertices[rf.i1];
            }

            int pointCount = 0;
            for (int i = 0; i < Settings.MaxManifoldPoints; ++i)
            {
                float separation = Vector2.Dot(rf.Normal, clipPoints2[i].V - rf.v1);

                if (separation <= _radius)
                {
                    ManifoldPoint cp = manifold.Points[pointCount];

                    if (primaryAxis.Type == EPAxisType.EdgeA)
                    {
                        cp.LocalPoint = MathUtils.MulT(ref _xf, clipPoints2[i].V);
                        cp.Id = clipPoints2[i].ID;
                    }
                    else
                    {
                        cp.LocalPoint = clipPoints2[i].V;
                        cp.Id.ContactFeature.TypeA = clipPoints2[i].ID.ContactFeature.TypeB;
                        cp.Id.ContactFeature.TypeB = clipPoints2[i].ID.ContactFeature.TypeA;
                        cp.Id.ContactFeature.IndexA = clipPoints2[i].ID.ContactFeature.IndexB;
                        cp.Id.ContactFeature.IndexB = clipPoints2[i].ID.ContactFeature.IndexA;
                    }

                    manifold.Points[pointCount] = cp;
                    ++pointCount;
                }
            }

            manifold.PointCount = pointCount;
        }

        private EPAxis ComputeEdgeSeparation()
        {
            EPAxis axis;
            axis.Type = EPAxisType.EdgeA;
            axis.Index = _front ? 0 : 1;
            axis.Separation = Settings.MaxFloat;

            for (int i = 0; i < _polygonB.Count; ++i)
            {
                float s = Vector2.Dot(_normal, _polygonB.Vertices[i] - _v1);
                if (s < axis.Separation)
                {
                    axis.Separation = s;
                }
            }

            return axis;
        }

        private EPAxis ComputePolygonSeparation()
        {
            EPAxis axis;
            axis.Type = EPAxisType.Unknown;
            axis.Index = -1;
            axis.Separation = -Settings.MaxFloat;

            Vector2 perp = new Vector2(-_normal.Y, _normal.X);

            for (int i = 0; i < _polygonB.Count; ++i)
            {
                Vector2 n = -_polygonB.Normals[i];

                float s1 = Vector2.Dot(n, _polygonB.Vertices[i] - _v1);
                float s2 = Vector2.Dot(n, _polygonB.Vertices[i] - _v2);
                float s = Math.Min(s1, s2);

                if (s > _radius)
                {
                    // No collision
                    axis.Type = EPAxisType.EdgeB;
                    axis.Index = i;
                    axis.Separation = s;
                    return axis;
                }

                // Adjacency
                if (Vector2.Dot(n, perp) >= 0.0f)
                {
                    if (Vector2.Dot(n - _upperLimit, _normal) < -Settings.AngularSlop)
                    {
                        continue;
                    }
                }
                else
                {
                    if (Vector2.Dot(n - _lowerLimit, _normal) < -Settings.AngularSlop)
                    {
                        continue;
                    }
                }

                if (s > axis.Separation)
                {
                    axis.Type = EPAxisType.EdgeB;
                    axis.Index = i;
                    axis.Separation = s;
                }
            }

            return axis;
        }
    }
}