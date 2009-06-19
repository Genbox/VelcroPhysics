using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Common;

namespace Box2DX.Collision
{
    public partial class Collision
    {
        public static void CollideEdgeAndCircle(ref Manifold manifold,
            EdgeShape edge, XForm xf1, CircleShape circle, XForm xf2)
        {
            manifold.PointCount = 0;
            Vec2 d;
            Vec2 c = Common.Math.Mul(xf2, circle.GetLocalPosition());
            Vec2 cLocal = Common.Math.MulT(xf1, c);
            Vec2 n = edge.GetNormalVector();
            Vec2 v1 = edge.GetVertex1();
            Vec2 v2 = edge.GetVertex2();
            float radius = circle.GetRadius();
            float separation;

            float dirDist = Vec2.Dot((cLocal - v1), edge.GetDirectionVector());
            if (dirDist <= 0)
            {
                d = cLocal - v1;
                if (Vec2.Dot(d, edge.GetCorner1Vector()) < 0)
                {
                    return;
                }
                d = c - Common.Math.Mul(xf1, v1);
            }
            else if (dirDist >= edge.GetLength())
            {
                d = cLocal - v2;
                if (Vec2.Dot(d, edge.GetCorner2Vector()) > 0)
                {
                    return;
                }
                d = c - Common.Math.Mul(xf1, v2);
            }
            else
            {
                separation = Vec2.Dot(cLocal - v1, n);
                if (separation > radius || separation < -radius)
                {
                    return;
                }
                separation -= radius;
                manifold.Normal = Common.Math.Mul(xf1.R, n);
                manifold.PointCount = 1;
                manifold.Points[0].ID.Key = 0;
                manifold.Points[0].Separation = separation;
                c = c - radius * manifold.Normal;
                manifold.Points[0].LocalPoint1 = Common.Math.MulT(xf1, c);
                manifold.Points[0].LocalPoint2 = Common.Math.MulT(xf2, c);
                return;
            }

            float distSqr = Vec2.Dot(d, d);
            if (distSqr > radius * radius)
            {
                return;
            }

            if (distSqr < Common.Settings.FLT_EPSILON)
            {
                separation = -radius;
                manifold.Normal = Common.Math.Mul(xf1.R, n);
            }
            else
            {
                separation = d.Normalize() - radius;
                manifold.Normal = d;
            }

            manifold.PointCount = 1;
            manifold.Points[0].ID.Key = 0;
            manifold.Points[0].Separation = separation;
            c = c - radius * manifold.Normal;
            manifold.Points[0].LocalPoint1 = Common.Math.MulT(xf1, c);
            manifold.Points[0].LocalPoint2 = Common.Math.MulT(xf2, c);
        }

        public static void CollidePolyAndEdge(ref Manifold manifold,
            PolygonShape polygon, XForm xf1, EdgeShape edge, XForm xf2)
        {
            manifold.PointCount = 0;
            Vec2 v1 = Common.Math.Mul(xf2, edge.GetVertex1());
            Vec2 v2 = Common.Math.Mul(xf2, edge.GetVertex2());
            Vec2 n = Common.Math.Mul(xf2.R, edge.GetNormalVector());
            Vec2 v1Local = Common.Math.MulT(xf1, v1);
            Vec2 v2Local = Common.Math.MulT(xf1, v2);
            Vec2 nLocal = Common.Math.MulT(xf1.R, n);

            float separation1;
            int separationIndex1 = -1; // which normal on the poly found the shallowest depth?
            float separationMax1 = -Settings.FLT_MAX; // the shallowest depth of edge in poly
            float separation2;
            int separationIndex2 = -1; // which normal on the poly found the shallowest depth?
            float separationMax2 = -Settings.FLT_MAX; // the shallowest depth of edge in poly
            float separationMax = -Settings.FLT_MAX; // the shallowest depth of edge in poly
            bool separationV1 = false; // is the shallowest depth from edge's v1 or v2 vertex?
            int separationIndex = -1; // which normal on the poly found the shallowest depth?

            int vertexCount = polygon.VertexCount;
            Vec2[] vertices = polygon.GetVertices();
            Vec2[] normals = polygon.Normals;

            int enterStartIndex = -1; // the last poly vertex above the edge
            int enterEndIndex = -1; // the first poly vertex below the edge
            int exitStartIndex = -1; // the last poly vertex below the edge
            int exitEndIndex = -1; // the first poly vertex above the edge
            //int deepestIndex;

            // the "N" in the following variables refers to the edge's normal. 
            // these are projections of poly vertices along the edge's normal, 
            // a.k.a. they are the separation of the poly from the edge. 
            float prevSepN = 0.0f;
            float nextSepN = 0.0f;
            float enterSepN = 0.0f; // the depth of enterEndIndex under the edge (stored as a separation, so it's negative)
            float exitSepN = 0.0f; // the depth of exitStartIndex under the edge (stored as a separation, so it's negative)
            float deepestSepN = Settings.FLT_MAX; // the depth of the deepest poly vertex under the end (stored as a separation, so it's negative)


            // for each poly normal, get the edge's depth into the poly. 
            // for each poly vertex, get the vertex's depth into the edge. 
            // use these calculations to define the remaining variables declared above.
            prevSepN = Vec2.Dot(vertices[vertexCount - 1] - v1Local, nLocal);
            for (int i = 0; i < vertexCount; i++)
            {
                separation1 = Vec2.Dot(v1Local - vertices[i], normals[i]);
                separation2 = Vec2.Dot(v2Local - vertices[i], normals[i]);
                if (separation2 < separation1)
                {
                    if (separation2 > separationMax)
                    {
                        separationMax = separation2;
                        separationV1 = false;
                        separationIndex = i;
                    }
                }
                else
                {
                    if (separation1 > separationMax)
                    {
                        separationMax = separation1;
                        separationV1 = true;
                        separationIndex = i;
                    }
                }
                if (separation1 > separationMax1)
                {
                    separationMax1 = separation1;
                    separationIndex1 = i;
                }
                if (separation2 > separationMax2)
                {
                    separationMax2 = separation2;
                    separationIndex2 = i;
                }

                nextSepN = Vec2.Dot(vertices[i] - v1Local, nLocal);
                if (nextSepN >= 0.0f && prevSepN < 0.0f)
                {
                    exitStartIndex = (i == 0) ? vertexCount - 1 : i - 1;
                    exitEndIndex = i;
                    exitSepN = prevSepN;
                }
                else if (nextSepN < 0.0f && prevSepN >= 0.0f)
                {
                    enterStartIndex = (i == 0) ? vertexCount - 1 : i - 1;
                    enterEndIndex = i;
                    enterSepN = nextSepN;
                }
                if (nextSepN < deepestSepN)
                {
                    deepestSepN = nextSepN;
                    //deepestIndex = i;
                }
                prevSepN = nextSepN;
            }

            if (enterStartIndex == -1)
            {
                // poly is entirely below or entirely above edge, return with no contact:
                return;
            }
            if (separationMax > 0.0f)
            {
                // poly is laterally disjoint with edge, return with no contact:
                return;
            }

            // if the poly is near a convex corner on the edge
            if ((separationV1 && edge.Corner1IsConvex()) || (!separationV1 && edge.Corner2IsConvex()))
            {
                // if shallowest depth was from edge into poly, 
                // use the edge's vertex as the contact point:
                if (separationMax > deepestSepN + Settings.LinearSlop)
                {
                    // if -normal angle is closer to adjacent edge than this edge, 
                    // let the adjacent edge handle it and return with no contact:
                    if (separationV1)
                    {
                        if (Vec2.Dot(normals[separationIndex1], Common.Math.MulT(xf1.R, Common.Math.Mul(xf2.R, edge.GetCorner1Vector()))) >= 0.0f)
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (Vec2.Dot(normals[separationIndex2], Common.Math.MulT(xf1.R, Common.Math.Mul(xf2.R, edge.GetCorner2Vector()))) <= 0.0f)
                        {
                            return;
                        }
                    }

                    manifold.PointCount = 1;
                    manifold.Normal = Common.Math.Mul(xf1.R, normals[separationIndex]);
                    manifold.Points[0].Separation = separationMax;
                    manifold.Points[0].ID.Features.IncidentEdge = (byte)separationIndex;
                    manifold.Points[0].ID.Features.IncidentVertex = 0;
                    manifold.Points[0].ID.Features.ReferenceEdge = 0;
                    manifold.Points[0].ID.Features.Flip = 0;
                    if (separationV1)
                    {
                        manifold.Points[0].LocalPoint1 = v1Local;
                        manifold.Points[0].LocalPoint2 = edge.GetVertex1();
                    }
                    else
                    {
                        manifold.Points[0].LocalPoint1 = v2Local;
                        manifold.Points[0].LocalPoint2 = edge.GetVertex2();
                    }
                    return;
                }
            }

            // We're going to use the edge's normal now.
            manifold.Normal = (-1.0f) * n;

            // Check whether we only need one contact point.
            if (enterEndIndex == exitStartIndex)
            {
                manifold.PointCount = 1;
                manifold.Points[0].ID.Features.IncidentEdge = (byte)enterEndIndex;
                manifold.Points[0].ID.Features.IncidentVertex = 0;
                manifold.Points[0].ID.Features.ReferenceEdge = 0;
                manifold.Points[0].ID.Features.Flip = 0;
                manifold.Points[0].LocalPoint1 = vertices[enterEndIndex];
                manifold.Points[0].LocalPoint2 = Common.Math.MulT(xf2, Common.Math.Mul(xf1, vertices[enterEndIndex]));
                manifold.Points[0].Separation = enterSepN;
                return;
            }

            manifold.PointCount = 2;

            // dirLocal should be the edge's direction vector, but in the frame of the polygon.
            Vec2 dirLocal = Vec2.Cross(nLocal, -1.0f); // TODO: figure out why this optimization didn't work
            //Vec2 dirLocal = Common.Math.MulT(xf1.R, Common.Math.Mul(xf2.R, edge.GetDirectionVector()));

            float dirProj1 = Vec2.Dot(dirLocal, vertices[enterEndIndex] - v1Local);
            float dirProj2;

            // The contact resolution is more robust if the two manifold points are 
            // adjacent to each other on the polygon. So pick the first two poly
            // vertices that are under the edge:
            exitEndIndex = (enterEndIndex == vertexCount - 1) ? 0 : enterEndIndex + 1;
            if (exitEndIndex != exitStartIndex)
            {
                exitStartIndex = exitEndIndex;
                exitSepN = Vec2.Dot(nLocal, vertices[exitStartIndex] - v1Local);
            }
            dirProj2 = Vec2.Dot(dirLocal, vertices[exitStartIndex] - v1Local);

            manifold.Points[0].ID.Features.IncidentEdge = (byte)enterEndIndex;
            manifold.Points[0].ID.Features.IncidentVertex = 0;
            manifold.Points[0].ID.Features.ReferenceEdge = 0;
            manifold.Points[0].ID.Features.Flip = 0;

            if (dirProj1 > edge.GetLength())
            {
                manifold.Points[0].LocalPoint1 = v2Local;
                manifold.Points[0].LocalPoint2 = edge.GetVertex2();
                float ratio = (edge.GetLength() - dirProj2) / (dirProj1 - dirProj2);
                if (ratio > 100.0f * Settings.FLT_EPSILON && ratio < 1.0f)
                {
                    manifold.Points[0].Separation = exitSepN * (1.0f - ratio) + enterSepN * ratio;
                }
                else
                {
                    manifold.Points[0].Separation = enterSepN;
                }
            }
            else
            {
                manifold.Points[0].LocalPoint1 = vertices[enterEndIndex];
                manifold.Points[0].LocalPoint2 = Common.Math.MulT(xf2, Common.Math.Mul(xf1, vertices[enterEndIndex]));
                manifold.Points[0].Separation = enterSepN;
            }

            manifold.Points[1].ID.Features.IncidentEdge = (byte)exitStartIndex;
            manifold.Points[1].ID.Features.IncidentVertex = 0;
            manifold.Points[1].ID.Features.ReferenceEdge = 0;
            manifold.Points[1].ID.Features.Flip = 0;

            if (dirProj2 < 0.0f)
            {
                manifold.Points[1].LocalPoint1 = v1Local;
                manifold.Points[1].LocalPoint2 = edge.GetVertex1();
                float ratio = (-dirProj1) / (dirProj2 - dirProj1);
                if (ratio > 100.0f * Settings.FLT_EPSILON && ratio < 1.0f)
                {
                    manifold.Points[1].Separation = enterSepN * (1.0f - ratio) + exitSepN * ratio;
                }
                else
                {
                    manifold.Points[1].Separation = exitSepN;
                }
            }
            else
            {
                manifold.Points[1].LocalPoint1 = vertices[exitStartIndex];
                manifold.Points[1].LocalPoint2 = Common.Math.MulT(xf2, Common.Math.Mul(xf1, vertices[exitStartIndex]));
                manifold.Points[1].Separation = exitSepN;
            }
        }
    }
}
