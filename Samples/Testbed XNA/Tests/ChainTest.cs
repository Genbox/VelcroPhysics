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
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class ChainTest : Test
    {
        private ChainTest()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            //Chain start / end
            Path path = new Path();
            path.Add(new Vector2(0, 25));
            path.Add(new Vector2(40, 25));

            //A single chainlink
            PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(0.125f, 0.6f), 20);

            //Use PathFactory to create all the chainlinks based on the chainlink created before.
            List<Body> chainLinks = PathManager.EvenlyDistributeShapesAlongPath(World, path, shape, BodyType.Dynamic, 30);

            foreach (Body chainLink in chainLinks)
            {
                foreach (Fixture f in chainLink.FixtureList)
                {
                    f.Friction = 0.2f;
                }
            }

            //TODO
            //Fix the first chainlink to the world
            //FixedRevoluteJoint fixedJoint = new FixedRevoluteJoint(chainLinks[0], Vector2.Zero, chainLinks[0].Position);
            //World.AddJoint(fixedJoint);

            //Attach all the chainlinks together with a revolute joint
            List<RevoluteJoint> joints = PathManager.AttachBodiesWithRevoluteJoint(World, chainLinks, new Vector2(0, -0.6f), new Vector2(0, 0.6f), false, false);

            //The chain is breakable
            for (int i = 0; i < joints.Count; i++)
            {
                RevoluteJoint r = joints[i];
                r.Breakpoint = 10000f;
            }
        }

        internal static Test Create()
        {
            return new ChainTest();
        }
    }
}