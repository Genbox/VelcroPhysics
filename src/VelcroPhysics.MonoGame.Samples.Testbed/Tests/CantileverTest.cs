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
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class CantileverTest : Test
    {
        private const int _count = 8;

        private CantileverTest()
        {
            Body ground = BodyFactory.CreateEdge(World, new Vector2(-40, 0), new Vector2(40, 0));

            {
                PolygonShape shape = new PolygonShape(20);
                shape.SetAsBox(0.5f, 0.125f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;

                WeldJointDef jd = new WeldJointDef();

                Body prevBody = ground;
                for (int i = 0; i < _count; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-14.5f + 1.0f * i, 5.0f);
                    Body body = BodyFactory.CreateFromDef(World, bd);
                    body.AddFixture(fd);

                    Vector2 anchor = new Vector2(-15.0f + 1.0f * i, 5.0f);
                    jd.Initialize(prevBody, body, anchor);
                    JointFactory.CreateFromDef(World, jd);

                    prevBody = body;
                }
            }

            {
                PolygonShape shape = new PolygonShape(20.0f);
                shape.SetAsBox(1.0f, 0.125f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;

                WeldJointDef jd = new WeldJointDef();

                float frequencyHz = 5.0f;
                float dampingRatio = 0.7f;

                Body prevBody = ground;
                for (int i = 0; i < 3; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-14.0f + 2.0f * i, 15.0f);
                    Body body = BodyFactory.CreateFromDef(World, bd);
                    body.AddFixture(fd);

                    Vector2 anchor = new Vector2(-15.0f + 2.0f * i, 15.0f);
                    jd.Initialize(prevBody, body, anchor);
                    JointHelper.AngularStiffness(frequencyHz, dampingRatio, jd.BodyA, jd.BodyB, out float stiffness, out float damping);
                    jd.Stiffness = stiffness;
                    jd.Damping = damping;

                    JointFactory.CreateFromDef(World, jd);

                    prevBody = body;
                }
            }

            {
                PolygonShape shape = new PolygonShape(20.0f);
                shape.SetAsBox(0.5f, 0.125f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;

                WeldJointDef jd = new WeldJointDef();

                Body prevBody = ground;
                for (int i = 0; i < _count; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-4.5f + 1.0f * i, 5.0f);
                    Body body = BodyFactory.CreateFromDef(World, bd);
                    body.AddFixture(fd);

                    if (i > 0)
                    {
                        Vector2 anchor = new Vector2(-5.0f + 1.0f * i, 5.0f);
                        jd.Initialize(prevBody, body, anchor);
                        JointFactory.CreateFromDef(World, jd);
                    }

                    prevBody = body;
                }
            }

            {
                PolygonShape shape = new PolygonShape(20.0f);
                shape.SetAsBox(0.5f, 0.125f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;

                WeldJointDef jd = new WeldJointDef();
                float frequencyHz = 8.0f;
                float dampingRatio = 0.7f;

                Body prevBody = ground;
                for (int i = 0; i < _count; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(5.5f + 1.0f * i, 10.0f);
                    Body body = BodyFactory.CreateFromDef(World, bd);
                    body.AddFixture(fd);

                    if (i > 0)
                    {
                        Vector2 anchor = new Vector2(5.0f + 1.0f * i, 10.0f);
                        jd.Initialize(prevBody, body, anchor);

                        JointHelper.AngularStiffness(frequencyHz, dampingRatio, jd.BodyA, jd.BodyB, out float stiffness, out float damping);
                        jd.Stiffness = stiffness;
                        jd.Damping = damping;

                        JointFactory.CreateFromDef(World, jd);
                    }

                    prevBody = body;
                }
            }

            for (int i = 0; i < 2; ++i)
            {
                Vertices vertices = new Vertices(3);
                vertices.Add(new Vector2(-0.5f, 0.0f));
                vertices.Add(new Vector2(0.5f, 0.0f));
                vertices.Add(new Vector2(0.0f, 1.5f));

                PolygonShape shape = new PolygonShape(vertices, 1.0f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-8.0f + 8.0f * i, 12.0f);
                Body body = BodyFactory.CreateFromDef(World, bd);
                body.AddFixture(fd);
            }

            for (int i = 0; i < 2; ++i)
            {
                CircleShape shape = new CircleShape(0.5f, 1.0f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-6.0f + 6.0f * i, 10.0f);
                Body body = BodyFactory.CreateFromDef(World, bd);
                body.AddFixture(fd);
            }
        }

        internal static Test Create()
        {
            return new CantileverTest();
        }
    }
}