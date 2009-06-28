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
        public static void CollideEdgeAndCircle(ref Manifold manifold,
            EdgeShape edge, XForm transformA, CircleShape circle, XForm transformB)
        {
            manifold.PointCount = 0;
            Vec2 cLocal = Math.MulT(transformA, Math.Mul(transformB, circle.LocalPosition));
            Vec2 normal = edge.Normal;
            Vec2 v1 = edge.V1;
            Vec2 v2 = edge.V2;
            float radius = edge.Radius + circle.Radius;

            // Barycentric coordinates
            float u1 = Vec2.Dot(cLocal - v1, v2 - v1);
            float u2 = Vec2.Dot(cLocal - v2, v1 - v2);

            if (u1 <= 0.0f)
            {
                // Behind v1
                if (Vec2.DistanceSquared(cLocal, v1) > radius * radius)
                {
                    return;
                }

                manifold.PointCount = 1;
                manifold.Type = Manifold.ManifoldType.FaceA;
                manifold.LocalPlaneNormal = cLocal - v1;
                manifold.LocalPlaneNormal.Normalize();
                manifold.LocalPoint = v1;
                manifold.Points[0].LocalPoint = circle.LocalPosition;
                manifold.Points[0].ID.Key = 0;
            }
            else if (u2 <= 0.0f)
            {
                // Ahead of v2
                if (Vec2.DistanceSquared(cLocal, v2) > radius * radius)
                {
                    return;
                }

                manifold.PointCount = 1;
                manifold.Type = Manifold.ManifoldType.FaceA;
                manifold.LocalPlaneNormal = cLocal - v2;
                manifold.LocalPlaneNormal.Normalize();
                manifold.LocalPoint = v2;
                manifold.Points[0].LocalPoint = circle.LocalPosition;
                manifold.Points[0].ID.Key = 0;
            }
            else
            {
                float separation = Vec2.Dot(cLocal - v1, normal);
                if (separation < -radius || radius < separation)
                {
                    return;
                }

                manifold.PointCount = 1;
                manifold.Type = Manifold.ManifoldType.FaceA;
                manifold.LocalPlaneNormal = separation < 0.0f ? -normal : normal;
                manifold.LocalPoint = 0.5f * (v1 + v2);
                manifold.Points[0].LocalPoint = circle.LocalPosition;
                manifold.Points[0].ID.Key = 0;
            }
        }

#if true
        public static void CollidePolyAndEdge(out Manifold manifold,
            PolygonShape polygon, XForm transformA, EdgeShape edge, XForm transformB)
        {
            PolygonShape polygonB = new PolygonShape();
            polygonB.SetAsEdge(edge.V1, edge.V2);

            CollidePolygons(out manifold, polygon, transformA, polygonB, transformB);
        }

#else
        public static void b2CollidePolyAndEdge(Manifold manifold,
                                            PolygonShape polygon,
                                            XForm transformA,
                                            EdgeShape edge,
                                            XForm transformB)
        {
	        manifold.PointCount = 0;
	        Vec2 v1 = Math.Mul(transformB, edge.GetVertex1());
	        Vec2 v2 = Math.Mul(transformB, edge.GetVertex2());
	        Vec2 n = Math.Mul(transformB.R, edge.GetNormalVector());
	        Vec2 v1Local = Math.MulT(transformA, v1);
	        Vec2 v2Local = Math.MulT(transformA, v2);
	        Vec2 nLocal = Math.MulT(transformA.R, n);

	        float totalRadius = polygon.Radius + edge.Radius;

	        float separation1;
	        int separationIndex1 = -1;			// which normal on the polygon found the shallowest depth?
	        float separationMax1 = -Settings.FLT_MAX;	// the shallowest depth of edge in polygon
	        float separation2;
	        int separationIndex2 = -1;			// which normal on the polygon found the shallowest depth?
	        float separationMax2 = -Settings.FLT_MAX;	// the shallowest depth of edge in polygon
	        float separationMax = -Settings.FLT_MAX;	// the shallowest depth of edge in polygon
	        bool separationV1 = false;				// is the shallowest depth from edge's v1 or v2 vertex?
	        int separationIndex = -1;				 // which normal on the polygon found the shallowest depth?

	        int vertexCount = polygon.VertexCount;
	        Vec2[] vertices = polygon.Vertices;
	        Vec2[] normals = polygon.Normals;

	        int enterStartIndex = -1; // the last polygon vertex above the edge
	        int enterEndIndex = -1;	// the first polygon vertex below the edge
	        int exitStartIndex = -1;	// the last polygon vertex below the edge
	        int exitEndIndex = -1;	// the first polygon vertex above the edge
	        //int deepestIndex;

	        // the "N" in the following variables refers to the edge's normal. 
	        // these are projections of polygon vertices along the edge's normal, 
	        // a.k.a. they are the separation of the polygon from the edge. 
	        float prevSepN = totalRadius;
	        float nextSepN = totalRadius;
	        float enterSepN = totalRadius;	// the depth of enterEndIndex under the edge (stored as a separation, so it's negative)
	        float exitSepN = totalRadius;	// the depth of exitStartIndex under the edge (stored as a separation, so it's negative)
	        float deepestSepN = Settings.FLT_MAX; // the depth of the deepest polygon vertex under the end (stored as a separation, so it's negative)

	        // for each polygon normal, get the edge's depth into the polygon. 
	        // for each polygon vertex, get the vertex's depth into the edge. 
	        // use these calculations to define the remaining variables declared above.
	        prevSepN = Vec2.Dot(vertices[vertexCount-1] - v1Local, nLocal);
	        for (int i = 0; i < vertexCount; i++)
	        {
		        // Polygon normal separation.
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

		        // Edge normal separation
		        nextSepN = Vec2.Dot(vertices[i] - v1Local, nLocal);
		        if (nextSepN >= totalRadius && prevSepN < totalRadius)
		        {
			        exitStartIndex = (i == 0) ? vertexCount-1 : i-1;
			        exitEndIndex = i;
			        exitSepN = prevSepN;
		        }
		        else if (nextSepN < totalRadius && prevSepN >= totalRadius)
		        {
			        enterStartIndex = (i == 0) ? vertexCount-1 : i-1;
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
		        // Edge normal separation
		        // polygon is entirely below or entirely above edge, return with no contact:
		        return;
	        }

	        if (separationMax > totalRadius)
	        {
		        // Face normal separation
		        // polygon is laterally disjoint with edge, return with no contact:
		        return;
	        }

	        // if the polygon is near a convex corner on the edge
	        if ((separationV1 && edge.Corner1IsConvex()) || (!separationV1 && edge.Corner2IsConvex()))
	        {
		        // if shallowest depth was from a polygon normal (meaning the polygon face is longer than the edge shape),
		        // use the edge's vertex as the contact point:
		        if (separationMax > deepestSepN + Settings.LinearSlop)
		        {
			        // if -normal angle is closer to adjacent edge than this edge, 
			        // let the adjacent edge handle it and return with no contact:
			        if (separationV1)
			        {
				        if (Vec2.Dot(normals[separationIndex1], Math.MulT(transformA.R, Math.Mul(transformB.R, edge.GetCorner1Vector()))) >= 0.0f)
				        {
					        return;
				        }
			        }
			        else
			        {
				        if (Vec2.Dot(normals[separationIndex2], Math.MulT(transformA.R, Math.Mul(transformB.R, edge.GetCorner2Vector()))) <= 0.0f)
				        {
					        return;
				        }
			        }

			        manifold.PointCount = 1;
		            manifold.Type = Manifold.ManifoldType.FaceA;
			        manifold.LocalPlaneNormal = normals[separationIndex];
			        manifold.Points[0].ID.Key = 0;
			        manifold.Points[0].ID.Features.IncidentEdge = (byte)separationIndex;
			        manifold.Points[0].ID.Features.IncidentVertex = NullFeature;
			        manifold.Points[0].ID.Features.ReferenceEdge = 0;
			        manifold.Points[0].ID.Features.Flip = 0;
			        if (separationV1)
			        {
				        manifold.Points[0].LocalPoint = edge.GetVertex1();
			        }
			        else
			        {
				        manifold.Points[0].LocalPoint = edge.GetVertex2();
			        }
			        return;
		        }
	        }

	        // We're going to use the edge's normal now.
	        manifold.LocalPlaneNormal = edge.GetNormalVector();
	        manifold.LocalPoint = 0.5f * (edge.V1 + edge.V2);

	        // Check whether we only need one contact point.
	        if (enterEndIndex == exitStartIndex)
	        {
		        manifold.PointCount = 1;
		        manifold.Points[0].ID.Key = 0;
                manifold.Points[0].ID.Features.IncidentEdge = (byte)enterEndIndex;
                manifold.Points[0].ID.Features.IncidentVertex = NullFeature;
                manifold.Points[0].ID.Features.ReferenceEdge = 0;
                manifold.Points[0].ID.Features.Flip = 0;
                manifold.Points[0].LocalPoint = vertices[enterEndIndex];
		        return;
	        }

	        manifold.PointCount = 2;

	        // dirLocal should be the edge's direction vector, but in the frame of the polygon.
	        Vec2 dirLocal = Vec2.Cross(nLocal, -1.0f); // TODO: figure out why this optimization didn't work
	        //Vec2 dirLocal = b2MulT(transformA.R, b2Mul(transformB.R, edge->GetDirectionVector()));

	        float dirProj1 = Vec2.Dot(dirLocal, vertices[enterEndIndex] - v1Local);
	        float dirProj2;

	        // The contact resolution is more robust if the two manifold points are 
	        // adjacent to each other on the polygon. So pick the first two polygon
	        // vertices that are under the edge:
	        exitEndIndex = (enterEndIndex == vertexCount - 1) ? 0 : enterEndIndex + 1;
	        if (exitEndIndex != exitStartIndex)
	        {
		        exitStartIndex = exitEndIndex;
		        exitSepN = Vec2.Dot(nLocal, vertices[exitStartIndex] - v1Local);
	        }
	        dirProj2 = Vec2.Dot(dirLocal, vertices[exitStartIndex] - v1Local);

	        manifold.Points[0].ID.Key = 0;
            manifold.Points[0].ID.Features.IncidentEdge = (byte)enterEndIndex;
            manifold.Points[0].ID.Features.IncidentVertex = NullFeature;
            manifold.Points[0].ID.Features.ReferenceEdge = 0;
            manifold.Points[0].ID.Features.Flip = 0;

	        if (dirProj1 > edge.GetLength())
	        {
                //manifold.Points[0].LocalPointA = v2Local;
                //manifold.Points[0].LocalPointB = edge.GetVertex2();
	        }
	        else
	        {
                //manifold.Points[0].LocalPointA = vertices[enterEndIndex];
                //manifold.Points[0].LocalPointB = Math.MulT(transformB, Math.Mul(transformA, vertices[enterEndIndex]));
	        }

            manifold.Points[1].ID.Key = 0;
            manifold.Points[1].ID.Features.IncidentEdge = (byte)exitStartIndex;
            manifold.Points[1].ID.Features.IncidentVertex = NullFeature;
            manifold.Points[1].ID.Features.ReferenceEdge = 0;
            manifold.Points[1].ID.Features.Flip = 0;

	        if (dirProj2 < 0.0f)
	        {
                //manifold.Points[1].LocalPointA = v1Local;
                //manifold.Points[1].LocalPointB = edge.GetVertex1();
	        }
	        else
	        {
                //manifold.Points[1].LocalPointA = vertices[exitStartIndex];
                //manifold.Points[1].LocalPointB = Math.MulT(transformB, Math.Mul(transformA, vertices[exitStartIndex]));
                //manifold.Points[1].Separation = exitSepN - totalRadius;
	        }
        }
#endif
    }
}
