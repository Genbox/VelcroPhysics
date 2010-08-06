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

using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class CollisionProcessingTest : Test
    {
        private CollisionProcessingTest()
        {
            //Ground
            FixtureFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f), 0);

            const float xLo = -5.0f;
            const float xHi = 5.0f;
            const float yLo = 2.0f;
            const float yHi = 35.0f;

            // Small triangle
            Vertices vertices = new Vertices(3);
            vertices.Add(new Vector2(-1.0f, 0.0f));
            vertices.Add(new Vector2(1.0f, 0.0f));
            vertices.Add(new Vector2(0.0f, 2.0f));

            PolygonShape polygon = new PolygonShape(vertices);

            Body body1 = BodyFactory.CreateBody(World);
            body1.BodyType = BodyType.Dynamic;
            body1.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));

            body1.CreateFixture(polygon, 1);

            // Large triangle (recycle definitions)
            vertices[0] *= 2.0f;
            vertices[1] *= 2.0f;
            vertices[2] *= 2.0f;
            polygon.Set(vertices);

            Body body2 = BodyFactory.CreateBody(World);
            body2.BodyType = BodyType.Dynamic;
            body2.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));
            body2.CreateFixture(polygon, 1);

            // Small box
            Vertices smallBox = PolygonTools.CreateRectangle(1.0f, 0.5f);
            polygon.Set(smallBox);

            Body body3 = BodyFactory.CreateBody(World);
            body3.BodyType = BodyType.Dynamic;
            body3.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));
            body3.CreateFixture(polygon, 1);

            // Large box (recycle definitions)
            Vertices largeBox = PolygonTools.CreateRectangle(2.0f, 1.0f);
            polygon.Set(largeBox);

            Body body4 = BodyFactory.CreateBody(World);
            body4.BodyType = BodyType.Dynamic;
            body4.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));
            body4.CreateFixture(polygon, 1);

            // Small circle
            CircleShape circle = new CircleShape(1.0f);

            Body body5 = BodyFactory.CreateBody(World);
            body5.BodyType = BodyType.Dynamic;
            body5.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));
            body5.CreateFixture(circle, 1);

            // Large circle
            circle.Radius *= 2.0f;

            Body body6 = BodyFactory.CreateBody(World);
            body6.BodyType = BodyType.Dynamic;
            body6.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));
            body6.CreateFixture(circle, 1);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            // We are going to destroy some bodies according to contact
            // points. We must buffer the bodies that should be destroyed
            // because they may belong to multiple contact points.
            const int k_maxNuke = 6;
            Body[] nuke = new Body[k_maxNuke];
            int nukeCount = 0;

            // Traverse the contact results. Destroy bodies that
            // are touching heavier bodies.
            for (int i = 0; i < PointCount; ++i)
            {
                ContactPoint point = Points[i];

                Body body1 = point.FixtureA.Body;
                Body body2 = point.FixtureB.Body;
                float mass1 = body1.Mass;
                float mass2 = body2.Mass;

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
                    World.RemoveBody(b);
                    dupes.Add(b);
                }
            }
        }

        internal static Test Create()
        {
            return new CollisionProcessingTest();
        }
    }
}