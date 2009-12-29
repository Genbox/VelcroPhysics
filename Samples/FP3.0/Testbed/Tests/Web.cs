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
using Box2D.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Box2D.XNA.TestBed.Tests
{
    // This tests distance joints, body destruction, and joint destruction.
    public class Web : Test
    {
        Web()
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
			    shape.SetAsBox(0.5f, 0.5f);

			    BodyDef bd = new BodyDef();
                bd.type = BodyType.Dynamic;

			    bd.position = new Vector2(-5.0f, 5.0f);
			    _bodies[0] = _world.CreateBody(bd);
			    _bodies[0].CreateFixture(shape, 5.0f);

			    bd.position = new Vector2(5.0f, 5.0f);
			    _bodies[1] = _world.CreateBody(bd);
			    _bodies[1].CreateFixture(shape, 5.0f);

			    bd.position = new Vector2(5.0f, 15.0f);
			    _bodies[2] = _world.CreateBody(bd);
			    _bodies[2].CreateFixture(shape, 5.0f);

			    bd.position = new Vector2(-5.0f, 15.0f);
			    _bodies[3] = _world.CreateBody(bd);
			    _bodies[3].CreateFixture(shape, 5.0f);

			    DistanceJointDef jd = new DistanceJointDef();
			    Vector2 p1, p2, d;

			    jd.frequencyHz = 4.0f;
			    jd.dampingRatio = 0.5f;

			    jd.bodyA = ground;
			    jd.bodyB = _bodies[0];
			    jd.localAnchorA = new Vector2(-10.0f, 0.0f);
			    jd.localAnchorB = new Vector2(-0.5f, -0.5f);
			    p1 = jd.bodyA.GetWorldPoint(jd.localAnchorA);
			    p2 = jd.bodyB.GetWorldPoint(jd.localAnchorB);
			    d = p2 - p1;
			    jd.length = d.Length();
			    _joints[0] = _world.CreateJoint(jd);

                jd.bodyA = ground;
                jd.bodyB = _bodies[1];
			    jd.localAnchorA = new Vector2(10.0f, 0.0f);
			    jd.localAnchorB = new Vector2(0.5f, -0.5f);
                p1 = jd.bodyA.GetWorldPoint(jd.localAnchorA);
                p2 = jd.bodyB.GetWorldPoint(jd.localAnchorB);
			    d = p2 - p1;
			    jd.length = d.Length();
			    _joints[1] = _world.CreateJoint(jd);

                jd.bodyA = ground;
                jd.bodyB = _bodies[2];
			    jd.localAnchorA = new Vector2(10.0f, 20.0f);
			    jd.localAnchorB = new Vector2(0.5f, 0.5f);
                p1 = jd.bodyA.GetWorldPoint(jd.localAnchorA);
                p2 = jd.bodyB.GetWorldPoint(jd.localAnchorB);
			    d = p2 - p1;
			    jd.length = d.Length();
			    _joints[2] = _world.CreateJoint(jd);

                jd.bodyA = ground;
                jd.bodyB = _bodies[3];
			    jd.localAnchorA = new Vector2(-10.0f, 20.0f);
			    jd.localAnchorB = new Vector2(-0.5f, 0.5f);
                p1 = jd.bodyA.GetWorldPoint(jd.localAnchorA);
                p2 = jd.bodyB.GetWorldPoint(jd.localAnchorB);
			    d = p2 - p1;
			    jd.length = d.Length();
			    _joints[3] = _world.CreateJoint(jd);

                jd.bodyA = _bodies[0];
                jd.bodyB = _bodies[1];
			    jd.localAnchorA = new Vector2(0.5f, 0.0f);
			    jd.localAnchorB = new Vector2(-0.5f, 0.0f);;
                p1 = jd.bodyA.GetWorldPoint(jd.localAnchorA);
                p2 = jd.bodyB.GetWorldPoint(jd.localAnchorB);
			    d = p2 - p1;
			    jd.length = d.Length();
			    _joints[4] = _world.CreateJoint(jd);

                jd.bodyA = _bodies[1];
                jd.bodyB = _bodies[2];
			    jd.localAnchorA = new Vector2(0.0f, 0.5f);
			    jd.localAnchorB = new Vector2(0.0f, -0.5f);
                p1 = jd.bodyA.GetWorldPoint(jd.localAnchorA);
                p2 = jd.bodyB.GetWorldPoint(jd.localAnchorB);
			    d = p2 - p1;
			    jd.length = d.Length();
			    _joints[5] = _world.CreateJoint(jd);

                jd.bodyA = _bodies[2];
                jd.bodyB = _bodies[3];
			    jd.localAnchorA = new Vector2(-0.5f, 0.0f);
			    jd.localAnchorB = new Vector2(0.5f, 0.0f);
                p1 = jd.bodyA.GetWorldPoint(jd.localAnchorA);
                p2 = jd.bodyB.GetWorldPoint(jd.localAnchorB);
			    d = p2 - p1;
			    jd.length = d.Length();
			    _joints[6] = _world.CreateJoint(jd);

                jd.bodyA = _bodies[3];
                jd.bodyB = _bodies[0];
			    jd.localAnchorA = new Vector2(0.0f, -0.5f);
			    jd.localAnchorB = new Vector2(0.0f, 0.5f);
                p1 = jd.bodyA.GetWorldPoint(jd.localAnchorA);
			    p2 = jd.bodyB.GetWorldPoint(jd.localAnchorB);
			    d = p2 - p1;
			    jd.length = d.Length();
			    _joints[7] = _world.CreateJoint(jd);
		    }
	    }

	    public override void Keyboard(KeyboardState state, KeyboardState oldState)
	    {
            if (state.IsKeyDown(Keys.B) && oldState.IsKeyUp(Keys.B))
            {
                for (int i = 0; i < 4; ++i)
                {
                    if (_bodies[i] != null)
                    {
                        _world.DestroyBody(_bodies[i]);
                        _bodies[i] = null;
                        break;
                    }
                }
            }

            if (state.IsKeyDown(Keys.J) && oldState.IsKeyUp(Keys.J))
            {
                for (int i = 0; i < 8; ++i)
                {
                    if (_joints[i] != null)
                    {
                        _world.DestroyJoint(_joints[i]);
                        _joints[i] = null;
                        break;
                    }
                }
            }
	    }

	    public override void Step(Framework.Settings settings)
	    {
		    base.Step(settings);
		    _debugDraw.DrawString(50, _textLine, "This demonstrates a soft distance joint.");
		    _textLine += 15;
		    _debugDraw.DrawString(50, _textLine, "Press: (b) to delete a body, (j) to delete a joint");
		    _textLine += 15;
	    }

	    public override void JointDestroyed(Joint joint)
	    {
		    for (int i = 0; i < 8; ++i)
		    {
			    if (_joints[i] == joint)
			    {
				    _joints[i] = null;
				    break;
			    }
		    }
	    }

	    static internal Test Create()
	    {
		    return new Web();
	    }

	    Body[] _bodies = new Body[4];
	    Joint[] _joints = new Joint[8];
    }
}
