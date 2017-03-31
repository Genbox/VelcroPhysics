/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
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

using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public class CollisionProcessingTest : Test
    {
        private List<Body> _removeBodies = new List<Body>();

        private CollisionProcessingTest()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            const float xLo = -5.0f;
            const float xHi = 5.0f;
            const float yLo = 2.0f;
            const float yHi = 35.0f;

            // Small triangle
            Vertices vertices = new Vertices(3);
            vertices.Add(new Vector2(-1.0f, 0.0f));
            vertices.Add(new Vector2(1.0f, 0.0f));
            vertices.Add(new Vector2(0.0f, 2.0f));

            PolygonShape polygon = new PolygonShape(vertices, 1);

            Body body1 = BodyFactory.CreateBody(World);
            body1.BodyType = BodyType.Dynamic;
            body1.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));

            Fixture fixture = body1.CreateFixture(polygon);
            fixture.OnCollision += OnCollision;

            // Large triangle (recycle definitions)
            vertices[0] *= 2.0f;
            vertices[1] *= 2.0f;
            vertices[2] *= 2.0f;
            polygon.Vertices = vertices;

            Body body2 = BodyFactory.CreateBody(World);
            body2.BodyType = BodyType.Dynamic;
            body2.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));
            fixture = body2.CreateFixture(polygon);
            fixture.OnCollision += OnCollision;

            // Small box
            Vertices smallBox = PolygonTools.CreateRectangle(1.0f, 0.5f);
            polygon.Vertices = smallBox;

            Body body3 = BodyFactory.CreateBody(World);
            body3.BodyType = BodyType.Dynamic;
            body3.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));
            fixture = body3.CreateFixture(polygon);
            fixture.OnCollision += OnCollision;

            // Large box (recycle definitions)
            Vertices largeBox = PolygonTools.CreateRectangle(2.0f, 1.0f);
            polygon.Vertices = largeBox;

            Body body4 = BodyFactory.CreateBody(World);
            body4.BodyType = BodyType.Dynamic;
            body4.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));
            fixture = body4.CreateFixture(polygon);
            fixture.OnCollision += OnCollision;

            // Small circle
            CircleShape circle = new CircleShape(1.0f, 1);

            Body body5 = BodyFactory.CreateBody(World);
            body5.BodyType = BodyType.Dynamic;
            body5.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));
            fixture = body5.CreateFixture(circle);
            fixture.OnCollision += OnCollision;

            // Large circle
            circle.Radius *= 2.0f;

            Body body6 = BodyFactory.CreateBody(World);
            body6.BodyType = BodyType.Dynamic;
            body6.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));
            fixture = body6.CreateFixture(circle);
            fixture.OnCollision += OnCollision;
        }

        private bool OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            Body body1 = fixtureA.Body;
            Body body2 = fixtureB.Body;
            float mass1 = body1.Mass;
            float mass2 = body2.Mass;

            if (mass1 > 0.0f && mass2 > 0.0f)
            {
                if (mass2 > mass1)
                {
                    if (!_removeBodies.Contains(body1))
                        _removeBodies.Add(body1);
                    return false;
                }
            }

            return true;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            for (int i = 0; i < _removeBodies.Count; i++)
            {
                World.RemoveBody(_removeBodies[i]);
            }

            _removeBodies.Clear();
        }

        internal static Test Create()
        {
            return new CollisionProcessingTest();
        }
    }
}