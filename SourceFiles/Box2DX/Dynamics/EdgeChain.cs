/*
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com
*
* This software is provided 'as-is', without any express or implied
* warranty.  In no event will the authors be held liable for any damages
* arising from the use of this software.
* Permission is granted to anyone to use this software for any purpose,
* including commercial applications, and to alter it and redistribute it
* freely, subject to the following restrictions:
* 1. The origin of this software must not be misrepresented; you must not
* claim that you wrote the original software. If you use this software
* in a product, an acknowledgment in the product documentation would be
* appreciated but is not required.
* 2. Altered source versions must be plainly marked as such, and must not be
* misrepresented as being the original software.
* 3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;

/// This structure is used to build edge chain shapes.
public class EdgeChainDef
{
	public EdgeChainDef()
	{
		userData = null;
		friction = 0.2f;
		restitution = 0.0f;
		isSensor = false;
        filter = new FilterData();
		filter.CategoryBits = 0x0001;
		filter.MaskBits = 0xFFFF;
		filter.GroupIndex = 0;
		vertices = null;
		vertexCount = 0;
		isLoop = true;
	}

	/// Use this to store application specific fixture data. This is assigned
	/// to each fixture in the chain.
	public object userData;

	/// The friction coefficient, usually in the range [0,1].
    public float friction;

	/// The restitution (elasticity) usually in the range [0,1].
    public float restitution;

	/// A sensor shape collects contact information but never generates a collision
	/// response.
    public bool isSensor;

	/// Contact filtering data.
    public FilterData filter;

	/// The vertices in local coordinates. You must manage the memory
	/// of this array on your own, outside of Box2D. 
    public Vec2[] vertices;

	/// The number of vertices in the chain. 
    public int vertexCount;

	/// Whether to create an extra edge between the first and last vertices:
    public bool isLoop;

    /// Create a chain of edges on the provided body. The edge chain does not alter the mass
    /// of the body, this must be done manually through b2Body::SetMassData.
    /// @return the first fixture of the chain.
    public Fixture CreateEdgeChain(Body body,  EdgeChainDef def)
    {
        Vec2 v1, v2;
	    int i;

	    if (def.isLoop)
	    {
		    v1 = def.vertices[def.vertexCount-1];
		    i = 0;
	    }
	    else
	    {
		    v1 = def.vertices[0];
		    i = 1;
	    }

	    EdgeDef edgeDef = new EdgeDef();
	    edgeDef.UserData = def.userData;
	    edgeDef.Friction = def.friction;
	    edgeDef.Restitution = def.restitution;
	    edgeDef.Density = 0.0f;
	    edgeDef.Filter = def.filter;
        edgeDef.IsSensor = def.isSensor;

	    Fixture fixture0 = null;
	    Fixture fixture1 = null;
	    Fixture fixture2 = null;

	    for (; i < def.vertexCount; ++i)
	    {
		    v2 = def.vertices[i];

		    edgeDef.Vertex1 = v1;
		    edgeDef.Vertex2 = v2;

		    fixture2 = body.CreateFixture(edgeDef);

		    if (fixture1 == null)
		    {
			    fixture0 = fixture2;
		    }
		    else
		    {
			    EdgeShape edge1 = (EdgeShape)fixture1.GetShape();
			    EdgeShape edge2 = (EdgeShape)fixture2.GetShape();
			    ConnectEdges(edge1, edge2);
		    }

		    fixture1 = fixture2;
		    v1 = v2;
	    }

	    if (def.isLoop)
	    {
		    EdgeShape edge1 = (EdgeShape)fixture1.GetShape();
		    EdgeShape edge0 = (EdgeShape)fixture0.GetShape();
		    ConnectEdges(edge1, edge0);
	    }
    	
	    return fixture0;
    }

    /// Destroy an edge chain provided the first edge fixture.
    public void DestroyEdgeChain(Body body, Fixture firstEdge)
    {
    }

    public static void ConnectEdges(EdgeShape edgeA, EdgeShape edgeB)
    {
        Vec2 cornerDir = edgeA.GetDirectionVector() + edgeB.GetDirectionVector();
        cornerDir.Normalize();
        bool convex = Box2DX.Common.Vec2.Dot(edgeA.GetDirectionVector(), edgeB.GetNormalVector()) > 0.0f;
        edgeA.SetNextEdge(edgeB, ref cornerDir, convex);
        edgeB.SetPrevEdge(edgeA, ref cornerDir, convex);
    }
}