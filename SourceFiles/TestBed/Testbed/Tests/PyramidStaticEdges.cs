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

namespace TestBed
{
    class PyramidStaticEdges : Test
    {

        public PyramidStaticEdges()
	    {
		    {
			    float[] coords = 
			    {
				    50.0f,0.0f,
				    -50.0f,0.0f
			    };
    			
			    Vec2[] verts = new Vec2[2];
    			
			    for (int i = 0; i < 2; i++)
			    {
				    verts[i].Set(coords[i*2], coords[i*2 + 1]);
			    }
    			
			    BodyDef bd = new BodyDef();
			    bd.Position.Set( 0.0f, 0.0f );
			    Body body = _world.CreateBody(bd);
			    EdgeDef edgeDef = new EdgeDef();
			    edgeDef.Vertex1 = verts[0];
			    edgeDef.Vertex2 = verts[1];
			    body.CreateFixture(edgeDef);
    			
			    //body->SetMassFromShapes();
		    }

		    {
			    PolygonDef sd = new PolygonDef();
			    float a = 0.5f;
			    sd.SetAsBox(a, a);
			    sd.Density = 5.0f;

			    Vec2 x = new Vec2(-10.0f, 1.0f);
			    Vec2 y;
			    Vec2 deltaX = new Vec2(0.5625f, 2.0f);
			    Vec2 deltaY = new Vec2(1.125f, 0.0f);

			    const int N = 2;

			    for (int i = 0; i < N; ++i)
			    {
				    y = x;

				    for (int j = i; j < N; ++j)
				    {
					    BodyDef bd = new BodyDef();
					    bd.Position = y;
					    Body body = _world.CreateBody(bd);
					    body.CreateFixture(sd);
					    body.SetMassFromShapes();

					    y += deltaY;
				    }

				    x += deltaX;
			    }
		    }
	    }

        public static Test Create()
	    {
            return new PyramidStaticEdges();
	    }
    }
}