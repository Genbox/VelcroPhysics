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
    public class Chain : Test
    {
        public Chain()
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
			    shape.SetAsBox(0.6f, 0.125f);

			    FixtureDef fd = new FixtureDef();
			    fd.shape = shape;
			    fd.density = 20.0f;
			    fd.friction = 0.2f;

                RevoluteJointDef jd = new RevoluteJointDef();
			    jd.collideConnected = false;

			    float y = 25.0f;
			    Body prevBody = ground;
			    for (int i = 0; i < 30; ++i)
			    {
				    BodyDef bd = new BodyDef();
                    bd.type = BodyType.Dynamic;
				    bd.position = new Vector2(0.5f + i, y);
				    Body body = _world.CreateBody(bd);
				    body.CreateFixture(fd);

				    Vector2 anchor = new Vector2((float)i, y);
				    jd.Initialize(prevBody, body, anchor);
				    _world.CreateJoint(jd);

				    prevBody = body;
			    }
		    }
	    }

        internal static Test Create()
	    {
		    return new Chain();
	    }
    }
}
