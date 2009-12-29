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
    public class Confined : Test
    {
        static int e_columnCount = 0;
        static int e_rowCount = 0;

        public Confined()
        {
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();

                // Floor
                shape.SetAsEdge(new Vector2(-10.0f, 0.0f), new Vector2(10.0f, 0.0f));
                ground.CreateFixture(shape, 0.0f);

                // Left wall
                shape.SetAsEdge(new Vector2(-10.0f, 0.0f), new Vector2(-10.0f, 20.0f));
                ground.CreateFixture(shape, 0.0f);

                // Right wall
                shape.SetAsEdge(new Vector2(10.0f, 0.0f), new Vector2(10.0f, 20.0f));
                ground.CreateFixture(shape, 0.0f);

                // Roof
                shape.SetAsEdge(new Vector2(-10.0f, 20.0f), new Vector2(10.0f, 20.0f));
                ground.CreateFixture(shape, 0.0f);
            }

            float radius = 0.5f;
            CircleShape shape2 = new CircleShape();
            shape2._p = Vector2.Zero;
            shape2._radius = radius;

            FixtureDef fd = new FixtureDef();
            fd.shape = shape2;
            fd.density = 1.0f;
            fd.friction = 0.1f;

            for (int j = 0; j < e_columnCount; ++j)
            {
                for (int i = 0; i < e_rowCount; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.type = BodyType.Dynamic;
                    bd.position = new Vector2(-10.0f + (2.1f * j + 1.0f + 0.01f * i) * radius, (2.0f * i + 1.0f) * radius);
                    Body body = _world.CreateBody(bd);

                    body.CreateFixture(fd);
                }
            }

            _world.Gravity = new Vector2(0.0f, 0.0f);
        }

        void CreateCircle()
        {
            float radius = 0.5f;
            CircleShape shape = new CircleShape();
            shape._p = Vector2.Zero;
            shape._radius = radius;

            FixtureDef fd = new FixtureDef();
            fd.shape = shape;
            fd.density = 1.0f;
            fd.friction = 0.0f;

            BodyDef bd = new BodyDef();
            bd.type = BodyType.Dynamic;
            bd.position = new Vector2(Rand.RandomFloat(), (2.0f + Rand.RandomFloat()) * radius);
            Body body = _world.CreateBody(bd);

            body.CreateFixture(fd);
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.C))
            {
                CreateCircle();
            }
	    }

        public override void Step(Framework.Settings settings)
	    {
            uint oldFlag = settings.enableContinuous;

            settings.enableContinuous = 0;
		    base.Step(settings);
		    _debugDraw.DrawString(5, _textLine, "Press 'c' to create a circle.");
		    _textLine += 15;

            settings.enableContinuous = oldFlag;
	    }

	    static internal Test Create()
	    {
            return new Confined();
	    }
    }
}
