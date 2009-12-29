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
    public class OneSidedPlatform : Test
    {
        public enum State
        {
            e_unknown,
            e_above,
            e_below,
        }

        public OneSidedPlatform()
        {
            // Ground
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vector2(-20.0f, 0.0f), new Vector2(20.0f, 0.0f));
                ground.CreateFixture(shape, 0.0f);
            }

            // Platform
            {
                BodyDef bd = new BodyDef();
                bd.position = new Vector2(0.0f, 10.0f);
                Body body = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(3.0f, 0.5f);
                _platform = body.CreateFixture(shape, 0.0f);

                _bottom = 10.0f - 0.5f;
                _top = 10.0f + 0.5f;
            }

            // Actor
            {
                BodyDef bd = new BodyDef();
                bd.type = BodyType.Dynamic;
                bd.position = new Vector2(0.0f, 12.0f);
                Body body = _world.CreateBody(bd);

                _radius = 0.5f;
                CircleShape shape = new CircleShape();
                shape._radius = _radius;
                _character = body.CreateFixture(shape, 1.0f);

                body.SetLinearVelocity(new Vector2(0.0f, -50.0f));

                _state = State.e_unknown;
            }
        }

        public override void PreSolve(Contact contact, ref Manifold oldManifold)
	    {
		    base.PreSolve(contact, ref oldManifold);

		    Fixture fixtureA = contact.GetFixtureA();
		    Fixture fixtureB = contact.GetFixtureB();

		    if (fixtureA != _platform && fixtureA != _character)
		    {
			    return;
		    }

		    if (fixtureB != _character && fixtureB != _character)
		    {
			    return;
		    }

		    Vector2 position = _character.GetBody().GetPosition();

		    if (position.Y < _top)
		    {
			    contact.SetEnabled(false);
		    }
	    }

        public override void Step(Framework.Settings settings)
	    {
		    base.Step(settings);
		    _debugDraw.DrawString(5, _textLine, "Press: (c) create a shape, (d) destroy a shape.");
		    _textLine += 15;
	    }

	    static internal Test Create()
	    {
		    return new OneSidedPlatform();
	    }

        float _radius, _top, _bottom;
        State _state;
        Fixture _platform;
        Fixture _character;
    }
}
