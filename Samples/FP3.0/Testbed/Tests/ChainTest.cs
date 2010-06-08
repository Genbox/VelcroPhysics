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
            FixtureFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f), 0);

            //Chain start / end
            Path path = new Path();
            path.Add(new Vector2(0, 20));
            path.Add(new Vector2(30, 20));

            //A single chainlink
            Fixture chainlink = FixtureFactory.CreateRectangle(World, 0.125f, 0.6f, 20);
            chainlink.Friction = 0.2f;
            chainlink.Body.BodyType = BodyType.Dynamic;

            //Use PathFactory to create all the chainlinks based on the chainlink created before.
            List<Body> chainLinks = PathFactory.EvenlyDistibuteShapesAlongPath(World, path, chainlink.Body, 50);

            //Fix the first chainlink to the world
            FixedRevoluteJoint fixedJoint = new FixedRevoluteJoint(chainLinks[0], chainLinks[0].Position);
            World.Add(fixedJoint);

            //Attach all the chainlinks together with a revolute joint
            PathFactory.AttachBodiesWithRevoluteJoint(World, chainLinks, new Vector2(0, -0.25f), new Vector2(0, 0.25f), false, false);
        }

        internal static Test Create()
        {
            return new ChainTest();
        }
    }
}