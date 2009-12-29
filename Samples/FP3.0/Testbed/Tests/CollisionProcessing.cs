/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
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
using Box2D.XNA.TestBed.Framework;
using Microsoft.Xna.Framework;
using Box2D.XNA;
using System.Collections.Generic;

namespace Box2D.XNA.TestBed.Tests
{
    public class CollisionProcessing : Test
    {
        public CollisionProcessing()
	    {
		    // Ground body
		    {
			    PolygonShape shape = new PolygonShape();
			    shape.SetAsEdge(new Vector2(-50.0f, 0.0f), new Vector2(50.0f, 0.0f));

                FixtureDef sd = new FixtureDef();
			    sd.shape = shape;

			    BodyDef bd = new BodyDef();
			    Body ground = _world.CreateBody(bd);
			    ground.CreateFixture(sd);
		    }

		    float xLo = -5.0f, xHi = 5.0f;
		    float yLo = 2.0f, yHi = 35.0f;

		    // Small triangle
		    Vector2[] vertices = new Vector2[3];
		    vertices[0] = new Vector2(-1.0f, 0.0f);
		    vertices[1] = new Vector2(1.0f, 0.0f);
		    vertices[2] = new Vector2(0.0f, 2.0f);

            PolygonShape polygon = new PolygonShape();
		    polygon.Set(vertices, 3);

            FixtureDef triangleShapeDef = new FixtureDef();
		    triangleShapeDef.shape = polygon;
		    triangleShapeDef.density = 1.0f;

            BodyDef triangleBodyDef = new BodyDef();
            triangleBodyDef.type = BodyType.Dynamic;
		    triangleBodyDef.position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));

		    Body body1 = _world.CreateBody(triangleBodyDef);
		    body1.CreateFixture(triangleShapeDef);

		    // Large triangle (recycle definitions)
		    vertices[0] *= 2.0f;
		    vertices[1] *= 2.0f;
		    vertices[2] *= 2.0f;
		    polygon.Set(vertices, 3);

		    triangleBodyDef.position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));

		    Body body2 = _world.CreateBody(triangleBodyDef);
		    body2.CreateFixture(triangleShapeDef);
    		
		    // Small box
		    polygon.SetAsBox(1.0f, 0.5f);

            FixtureDef boxShapeDef = new FixtureDef();
		    boxShapeDef.shape = polygon;
		    boxShapeDef.density = 1.0f;

            BodyDef boxBodyDef = new BodyDef();
            boxBodyDef.type = BodyType.Dynamic;
		    boxBodyDef.position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));

		    Body body3 = _world.CreateBody(boxBodyDef);
		    body3.CreateFixture(boxShapeDef);

		    // Large box (recycle definitions)
		    polygon.SetAsBox(2.0f, 1.0f);
		    boxBodyDef.position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));
    		
		    Body body4 = _world.CreateBody(boxBodyDef);
		    body4.CreateFixture(boxShapeDef);

		    // Small circle
            CircleShape circle = new CircleShape();
		    circle._radius = 1.0f;

            FixtureDef circleShapeDef = new FixtureDef();
		    circleShapeDef.shape = circle;
		    circleShapeDef.density = 1.0f;

            BodyDef circleBodyDef = new BodyDef();
            circleBodyDef.type = BodyType.Dynamic;
		    circleBodyDef.position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));

		    Body body5 = _world.CreateBody(circleBodyDef);
		    body5.CreateFixture(circleShapeDef);

		    // Large circle
		    circle._radius *= 2.0f;
		    circleBodyDef.position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));

		    Body body6 = _world.CreateBody(circleBodyDef);
		    body6.CreateFixture(circleShapeDef);
	    }

        static int stepCount = 0;
        public override void Step(Framework.Settings settings)
	    {
            stepCount++;
		    base.Step(settings);

		    // We are going to destroy some bodies according to contact
		    // points. We must buffer the bodies that should be destroyed
		    // because they may belong to multiple contact points.
		    int k_maxNuke = 6;
		    Body[] nuke = new Body[k_maxNuke];
		    int nukeCount = 0;

		    // Traverse the contact results. Destroy bodies that
		    // are touching heavier bodies.
		    for (int i = 0; i < _pointCount; ++i)
		    {
			    ContactPoint point = _points[i];

			    Body body1 = point.fixtureA.GetBody();
			    Body body2 = point.fixtureB.GetBody();
			    float mass1 = body1.GetMass();
			    float mass2 = body2.GetMass();

			    if (mass1 > 0.0f && mass2 > 0.0f)
			    {
				    if (mass2 > mass1)
				    {
					    nuke[nukeCount++] = body1;
				    }
				    else
				    {
					    nuke[nukeCount++] = body2;
				    }

				    if (nukeCount == k_maxNuke)
				    {
					    break;
				    }
			    }
		    }

            List<Body> dupes = new List<Body>();

		    // Destroy the bodies, skipping duplicates.
		    int j = 0;
		    while (j < nukeCount)
		    {
			    Body b = nuke[j++];
			    while (j < nukeCount && nuke[j] == b)
			    {
				    ++j;
			    }

                if (b != null && !dupes.Contains(b))
                {
                    _world.DestroyBody(b);
                    dupes.Add(b);
                }
		    }
	    }

        internal static Test Create()
	    {
		    return new CollisionProcessing();
	    }
    }
}
