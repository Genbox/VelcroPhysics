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

using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class ContinuousTest : Test
    {
        private float _angularVelocity;
        private Body _body;

        public ContinuousTest()
        {
            {
                Body body = World.Add();

                PolygonShape shape = new PolygonShape(1);

                shape.SetAsEdge(new Vector2(-10.0f, 0.0f), new Vector2(10.0f, 0.0f));
                body.CreateFixture(shape);

                shape.SetAsBox(0.2f, 1.0f, new Vector2(0.5f, 1.0f), 0.0f);
                body.CreateFixture(shape);
            }

#if true
            {
                PolygonShape shape = new PolygonShape(1.0f);
                shape.SetAsBox(2.0f, 0.1f);

                _body = World.Add();
                _body.BodyType = BodyType.Dynamic;
                _body.Position = new Vector2(0.0f, 20.0f);

                _body.CreateFixture(shape);

                _angularVelocity = Rand.RandomFloat(-50.0f, 50.0f);
                _angularVelocity = 33.468121f;
                _body.LinearVelocity = new Vector2(0.0f, -100.0f);
                _body.AngularVelocity = _angularVelocity;
            }
#else
		{
			BodyDef bd = new BodyDef();
			bd.type = BodyType.Dynamic;
			bd.position = new Vector2(0.0f, 0.5f);
			Body body = _world.CreateBody(bd);

			CircleShape shape = new CircleShape();
			shape._p = new Vector2Zero();
			shape._radius = 0.5f;
			body.CreateFixture(shape, 1.0f);

			//bd.bullet = true;
			bd.position = new Vector2(0.0f, 10.0f);
			body = _world.CreateBody(bd);
			body.CreateFixture(shape, 1.0f);
			body.SetLinearVelocity(new Vector2(0.0f, -100.0f));
		}
#endif
        }

        private void Launch()
        {
            _body.SetTransform(new Vector2(0.0f, 20.0f), 0.0f);
            _angularVelocity = Rand.RandomFloat(-50.0f, 50.0f);
            _body.LinearVelocity = new Vector2(0.0f, -100.0f);
            _body.AngularVelocity = _angularVelocity;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            if (StepCount == 12)
            {
                StepCount += 0;
            }

            base.Update(settings, gameTime);

            if (StepCount % 60 == 0)
            {
                Launch();
            }
        }

        internal static Test Create()
        {
            return new ContinuousTest();
        }
    }
}