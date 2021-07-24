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

using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class ChainProblemTest : Test
    {
        private ChainProblemTest()
        {
            {
                Vector2 g = new Vector2(0.0f, -10.0f);
                World.Gravity = g;
                Body[] bodies = new Body[2];
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Static;
                    bodies[0] = BodyFactory.CreateFromDef(World, bd);

                    {
                        Vector2 v1 = new Vector2(0.0f, 1.0f);
                        Vector2 v2 = new Vector2(0.0f, 0.0f);
                        Vector2 v3 = new Vector2(4.0f, 0.0f);

                        EdgeShape shape = new EdgeShape(v1, v2);
                        bodies[0].AddFixture(shape);

                        shape.SetTwoSided(v2, v3);
                        bodies[0].AddFixture(shape);
                    }
                }
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(1.0f, 3.0f);
                    bodies[1] = BodyFactory.CreateFromDef(World, bd);

                    {
                        FixtureDef fd = new FixtureDef();
                        fd.Friction = 0.2f;

                        PolygonShape shape = new PolygonShape(10.0f);
                        Vertices vs = new Vertices(4);
                        vs.Add(new Vector2(0.5f, -3.0f));
                        vs.Add(new Vector2(0.5f, 3.0f));
                        vs.Add(new Vector2(-0.5f, 3.0f));
                        vs.Add(new Vector2(-0.5f, -3.0f));
                        shape.Vertices = vs;

                        fd.Shape = shape;

                        bodies[1].AddFixture(fd);
                    }
                }
            }
        }

        internal static Test Create()
        {
            return new ChainProblemTest();
        }
    }
}