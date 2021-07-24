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
using Genbox.VelcroPhysics.Definitions.Joints;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class ChainTest : Test
    {
        private ChainTest()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.AddFixture(shape);
            }

            {
                PolygonShape shape = new PolygonShape(20.0f);
                shape.SetAsBox(0.6f, 0.125f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Friction = 0.2f;

                RevoluteJointDef jd = new RevoluteJointDef();
                jd.CollideConnected = false;

                const float y = 25.0f;
                Body prevBody = ground;
                for (int i = 0; i < 30; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(0.5f + i, y);
                    Body body = BodyFactory.CreateFromDef(World, bd);

                    body.AddFixture(fd);

                    Vector2 anchor = new Vector2(i, y);
                    jd.Initialize(prevBody, body, anchor);
                    JointFactory.CreateFromDef(World, jd);

                    prevBody = body;
                }
            }
        }

        internal static Test Create()
        {
            return new ChainTest();
        }
    }
}