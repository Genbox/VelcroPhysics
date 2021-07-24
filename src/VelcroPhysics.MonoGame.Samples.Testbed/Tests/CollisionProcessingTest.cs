/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
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

using System;
using System.Collections.Generic;
using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Solver;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class CollisionProcessingTest : Test
    {
        private readonly List<Body[]> _bodies = new List<Body[]>();

        private CollisionProcessingTest()
        {
            // Ground body
            {
                EdgeShape shape = new EdgeShape(new Vector2(-50.0f, 0.0f), new Vector2(50.0f, 0.0f));

                FixtureDef sd = new FixtureDef();
                sd.Shape = shape;

                BodyDef bd = new BodyDef();
                Body ground = BodyFactory.CreateFromDef(World, bd);
                ground.AddFixture(sd);
            }

            float xLo = -5.0f, xHi = 5.0f;
            float yLo = 2.0f, yHi = 35.0f;

            // Small triangle
            Vertices vertices = new Vertices(3);
            vertices.Add(new Vector2(-1.0f, 0.0f));
            vertices.Add(new Vector2(1.0f, 0.0f));
            vertices.Add(new Vector2(0.0f, 2.0f));

            PolygonShape polygon = new PolygonShape(vertices, 1.0f);

            FixtureDef triangleShapeDef = new FixtureDef();
            triangleShapeDef.Shape = polygon;

            BodyDef triangleBodyDef = new BodyDef();
            triangleBodyDef.Type = BodyType.Dynamic;
            triangleBodyDef.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));

            Body body1 = BodyFactory.CreateFromDef(World, triangleBodyDef);
            body1.AddFixture(triangleShapeDef);

            // Large triangle (recycle definitions)
            vertices[0] *= 2.0f;
            vertices[1] *= 2.0f;
            vertices[2] *= 2.0f;
            polygon.Vertices = vertices;

            triangleBodyDef.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));

            Body body2 = BodyFactory.CreateFromDef(World, triangleBodyDef);
            body2.AddFixture(triangleShapeDef);

            // Small box
            polygon.SetAsBox(1.0f, 0.5f);

            FixtureDef boxShapeDef = new FixtureDef();
            boxShapeDef.Shape = polygon;

            BodyDef boxBodyDef = new BodyDef();
            boxBodyDef.Type = BodyType.Dynamic;
            boxBodyDef.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));

            Body body3 = BodyFactory.CreateFromDef(World, boxBodyDef);
            body3.AddFixture(boxShapeDef);

            // Large box (recycle definitions)
            polygon.SetAsBox(2.0f, 1.0f);
            boxBodyDef.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));

            Body body4 =BodyFactory.CreateFromDef(World, boxBodyDef);
            body4.AddFixture(boxShapeDef);

            // Small circle
            CircleShape circle = new CircleShape(1.0f, 1.0f);

            FixtureDef circleShapeDef = new FixtureDef();
            circleShapeDef.Shape = circle;

            BodyDef circleBodyDef = new BodyDef();
            circleBodyDef.Type = BodyType.Dynamic;
            circleBodyDef.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));

            Body body5 = BodyFactory.CreateFromDef(World, circleBodyDef);
            body5.AddFixture(circleShapeDef);

            // Large circle
            circle.Radius *= 2.0f;
            circleBodyDef.Position = new Vector2(Rand.RandomFloat(xLo, xHi), Rand.RandomFloat(yLo, yHi));

            Body body6 = BodyFactory.CreateFromDef(World, circleBodyDef);
            body6.AddFixture(circleShapeDef);
        }

        protected override void PostSolve(Contact contact, ContactVelocityConstraint contactConstraint)
        {
            _bodies.Add(new[] { contact.FixtureA.Body, contact.FixtureB.Body });

            base.PostSolve(contact, contactConstraint);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            // We are going to destroy some bodies according to contact
            // points. We must buffer the bodies that should be destroyed
            // because they may belong to multiple contact points.
            int k_maxNuke = 6;
            Body[] nuke = new Body[k_maxNuke];
            int nukeCount = 0;

            // Traverse the contact results. Destroy bodies that
            // are touching heavier bodies.
            for (int i = 0; i < _bodies.Count; ++i)
            {
                Body[] pair = _bodies[i];

                Body body1 = pair[0];
                Body body2 = pair[1];
                float mass1 = body1.Mass;
                float mass2 = body2.Mass;

                if (mass1 > 0.0f && mass2 > 0.0f)
                {
                    if (mass2 > mass1)
                        nuke[nukeCount++] = body1;
                    else
                        nuke[nukeCount++] = body2;

                    if (nukeCount == k_maxNuke)
                        break;
                }
            }

            // Sort the nuke array to group duplicates.
            Array.Sort(nuke, new ReferenceComparer());

            // Destroy the bodies, skipping duplicates.
            int j = 0;
            while (j < nukeCount)
            {
                Body b = nuke[j++];
                while (j < nukeCount && nuke[j] == b)
                    ++j;

                World.RemoveBody(b);
            }

            _bodies.Clear();
        }

        internal static Test Create()
        {
            return new CollisionProcessingTest();
        }

        private class ReferenceComparer : IComparer<Body>
        {
            public int Compare(Body x, Body y)
            {
                if (ReferenceEquals(x, y))
                    return 0;

                if (x == null)
                    return 1;

                return -1;
            }
        }
    }
}