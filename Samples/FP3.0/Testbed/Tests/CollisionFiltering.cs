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

namespace Box2D.XNA.TestBed.Tests
{
    

    public class CollisionFiltering : Test
    {
        // This is a test of collision filtering.
        // There is a triangle, a box, and a circle.
        // There are 6 shapes. 3 large and 3 small.
        // The 3 small ones always collide.
        // The 3 large ones never collide.
        // The boxes don't collide with triangles (except if both are small).
        static Int16 k_smallGroup = 1;
        static Int16 k_largeGroup = -1;

        static UInt16 k_defaultCategory = 0x0001;
        static UInt16 k_triangleCategory = 0x0002;
        static UInt16 k_boxCategory = 0x0004;
        static UInt16 k_circleCategory = 0x0008;

        static UInt16 k_triangleMask = 0xFFFF;
        static UInt16 k_boxMask = (ushort)(0xFFFF ^ k_triangleCategory);
        static UInt16 k_circleMask = 0xFFFF;

        public CollisionFiltering()
	    {
		    // Ground body
		    {
			    PolygonShape shape = new PolygonShape();
			    shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

                FixtureDef sd = new FixtureDef();
			    sd.shape = shape;
			    sd.friction = 0.3f;

			    BodyDef bd = new BodyDef();
			    Body ground = _world.CreateBody(bd);
			    ground.CreateFixture(sd);
		    }

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

		    triangleShapeDef.filter.groupIndex = k_smallGroup;
		    triangleShapeDef.filter.categoryBits = k_triangleCategory;
		    triangleShapeDef.filter.maskBits = k_triangleMask;

		    BodyDef triangleBodyDef = new BodyDef();
            triangleBodyDef.type = BodyType.Dynamic;
		    triangleBodyDef.position = new Vector2(-5.0f, 2.0f);

		    Body body1 = _world.CreateBody(triangleBodyDef);
		    body1.CreateFixture(triangleShapeDef);

		    // Large triangle (recycle definitions)
		    vertices[0] *= 2.0f;
		    vertices[1] *= 2.0f;
		    vertices[2] *= 2.0f;
		    polygon.Set(vertices, 3);
		    triangleShapeDef.filter.groupIndex = k_largeGroup;
		    triangleBodyDef.position = new Vector2(-5.0f, 6.0f);
		    triangleBodyDef.fixedRotation = true; // look at me!

		    Body body2 = _world.CreateBody(triangleBodyDef);
		    body2.CreateFixture(triangleShapeDef);

            {
                BodyDef bd = new BodyDef();
                bd.type = BodyType.Dynamic;
                bd.position = new Vector2(-5.0f, 10.0f);
                Body body = _world.CreateBody(bd);

                PolygonShape p = new PolygonShape();
                p.SetAsBox(0.5f, 1.0f);
                body.CreateFixture(p, 1.0f);

                PrismaticJointDef jd = new PrismaticJointDef();
                jd.bodyA = body2;
                jd.bodyB = body;
                jd.enableLimit = true;
                jd.localAnchorA = new Vector2(0.0f, 4.0f);
                jd.localAnchorB = Vector2.Zero;
                jd.localAxis1 = new Vector2(0.0f, 1.0f);
                jd.lowerTranslation = -1.0f;
                jd.upperTranslation = 1.0f;

                _world.CreateJoint(jd);
            }


		    // Small box
		    polygon.SetAsBox(1.0f, 0.5f);
            FixtureDef boxShapeDef = new FixtureDef();
		    boxShapeDef.shape = polygon;
		    boxShapeDef.density = 1.0f;
		    boxShapeDef.restitution = 0.1f;

		    boxShapeDef.filter.groupIndex = k_smallGroup;
		    boxShapeDef.filter.categoryBits = k_boxCategory;
		    boxShapeDef.filter.maskBits = k_boxMask;

            BodyDef boxBodyDef = new BodyDef();
            boxBodyDef.type = BodyType.Dynamic;
		    boxBodyDef.position = new Vector2(0.0f, 2.0f);

		    Body body3 = _world.CreateBody(boxBodyDef);
		    body3.CreateFixture(boxShapeDef);

		    // Large box (recycle definitions)
		    polygon.SetAsBox(2.0f, 1.0f);
		    boxShapeDef.filter.groupIndex = k_largeGroup;
		    boxBodyDef.position = new Vector2(0.0f, 6.0f);

		    Body body4 = _world.CreateBody(boxBodyDef);
		    body4.CreateFixture(boxShapeDef);

		    // Small circle
		    CircleShape circle = new CircleShape();
		    circle._radius = 1.0f;

		    FixtureDef circleShapeDef = new FixtureDef();
		    circleShapeDef.shape = circle;
		    circleShapeDef.density = 1.0f;

		    circleShapeDef.filter.groupIndex = k_smallGroup;
		    circleShapeDef.filter.categoryBits = k_circleCategory;
		    circleShapeDef.filter.maskBits = k_circleMask;

		    BodyDef circleBodyDef = new BodyDef();
            circleBodyDef.type = BodyType.Dynamic;
		    circleBodyDef.position = new Vector2(5.0f, 2.0f);
    		
		    Body body5 = _world.CreateBody(circleBodyDef);
		    body5.CreateFixture(circleShapeDef);

		    // Large circle
		    circle._radius *= 2.0f;
		    circleShapeDef.filter.groupIndex = k_largeGroup;
		    circleBodyDef.position = new Vector2(5.0f, 6.0f);

		    Body body6 = _world.CreateBody(circleBodyDef);
		    body6.CreateFixture(circleShapeDef);
	    }
	    static internal Test Create()
	    {
		    return new CollisionFiltering();
	    }
    }
}
