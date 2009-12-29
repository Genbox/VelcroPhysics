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
using Microsoft.Xna.Framework.Input;

namespace Box2D.XNA.TestBed.Tests
{
    public class Cantilever : Test
    {
        static int e_count = 8;

        public Cantilever()
        {
            Body ground = null;
		    {
                BodyDef bd = new BodyDef();
			    ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
			    shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
			    ground.CreateFixture(shape, 0.0f);
		    }

		    {
                PolygonShape shape = new PolygonShape();
			    shape.SetAsBox(0.5f, 0.125f);

                FixtureDef fd = new FixtureDef();
			    fd.shape = shape;
			    fd.density = 20.0f;

                WeldJointDef jd = new WeldJointDef();

			    Body prevBody = ground;
			    for (int i = 0; i < e_count; ++i)
			    {
                    BodyDef bd = new BodyDef();
				    bd.type = BodyType.Dynamic;
				    bd.position = new Vector2(-14.5f + 1.0f * i, 5.0f);
				    Body body = _world.CreateBody(bd);
				    body.CreateFixture(fd);

				    Vector2 anchor = new Vector2(-15.0f + 1.0f * i, 5.0f);
				    jd.Initialize(prevBody, body, anchor);
				    _world.CreateJoint(jd);

				    prevBody = body;
			    }
		    }

		   	{
                PolygonShape shape = new PolygonShape();
			    shape.SetAsBox(0.5f, 0.125f);

                FixtureDef fd = new FixtureDef();
			    fd.shape = shape;
			    fd.density = 20.0f;

                WeldJointDef jd = new WeldJointDef();

			    Body prevBody = ground;
			    for (int i = 0; i < e_count; ++i)
			    {
                    BodyDef bd = new BodyDef();
				    bd.type = BodyType.Dynamic;
				    bd.position = new Vector2(-14.5f + 1.0f * i, 15.0f);
				    bd.inertiaScale = 10.0f;
				    Body body = _world.CreateBody(bd);
				    body.CreateFixture(fd);

				    Vector2 anchor = new Vector2(-15.0f + 1.0f * i, 15.0f);
				    jd.Initialize(prevBody, body, anchor);
				    _world.CreateJoint(jd);

				    prevBody = body;
			    }
		    }

		    {
                PolygonShape shape = new PolygonShape();
			    shape.SetAsBox(0.5f, 0.125f);

                FixtureDef fd = new FixtureDef();
			    fd.shape = shape;
			    fd.density = 20.0f;

                WeldJointDef jd = new WeldJointDef();

			    Body prevBody = ground;
			    for (int i = 0; i < e_count; ++i)
			    {
                    BodyDef bd = new BodyDef();
				    bd.type = BodyType.Dynamic;
				    bd.position = new Vector2(-4.5f + 1.0f * i, 5.0f);
				    Body body = _world.CreateBody(bd);
				    body.CreateFixture(fd);

				    if (i > 0)
				    {
					    Vector2 anchor = new Vector2(-5.0f + 1.0f * i, 5.0f);
					    jd.Initialize(prevBody, body, anchor);
					    _world.CreateJoint(jd);
				    }

				    prevBody = body;
			    }
		    }
            {
                PolygonShape shape = new PolygonShape();
			    shape.SetAsBox(0.5f, 0.125f);

                FixtureDef fd = new FixtureDef();
			    fd.shape = shape;
			    fd.density = 20.0f;

                WeldJointDef jd = new WeldJointDef();

			    Body prevBody = ground;
			    for (int i = 0; i < e_count; ++i)
			    {
                    BodyDef bd = new BodyDef();
				    bd.type = BodyType.Dynamic;
				    bd.position = new Vector2(5.5f + 1.0f * i, 10.0f);
				    bd.inertiaScale = 10.0f;
				    Body body = _world.CreateBody(bd);
				    body.CreateFixture(fd);

				    if (i > 0)
				    {
					    Vector2 anchor = new Vector2(5.0f + 1.0f * i, 10.0f);
					    jd.Initialize(prevBody, body, anchor);
					    _world.CreateJoint(jd);
				    }

				    prevBody = body;
			    }
		    }

		    for (int i = 0; i < 2; ++i)
		    {
			    Vector2[] vertices = new Vector2[3];
			    vertices[0] = new Vector2(-0.5f, 0.0f);
			    vertices[1] = new Vector2(0.5f, 0.0f);
			    vertices[2] = new Vector2(0.0f, 1.5f);

                PolygonShape shape = new PolygonShape();
			    shape.Set(vertices, 3);

                FixtureDef fd = new FixtureDef();
			    fd.shape = shape;
			    fd.density = 1.0f;

                BodyDef bd = new BodyDef();
			    bd.type = BodyType.Dynamic;
			    bd.position = new Vector2(-8.0f + 8.0f * i, 12.0f);
			    Body body = _world.CreateBody(bd);
			    body.CreateFixture(fd);
		    }

            for (int i = 0; i < 2; ++i)
		    {
                CircleShape shape = new CircleShape();
			    shape._radius = 0.5f;

                FixtureDef fd = new FixtureDef();
			    fd.shape = shape;
			    fd.density = 1.0f;

                BodyDef bd = new BodyDef();
			    bd.type = BodyType.Dynamic;
			    bd.position = new Vector2(-6.0f + 6.0f * i, 10.0f);
			    Body body = _world.CreateBody(bd);
			    body.CreateFixture(fd);
		    }
        }

	    static internal Test Create()
	    {
            return new Cantilever();
	    }

	    Body _middle;
    }
}
