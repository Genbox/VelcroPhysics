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

using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public class ConveyorBeltTest : Test
    {
        private Fixture _platform;

        ConveyorBeltTest()
        {
            // Ground
            {
                Body ground = BodyFactory.CreateBody(World);
                FixtureFactory.AttachEdge(new Vector2(-20.0f, 0.0f), new Vector2(20.0f, 0.0f), ground);
            }

            // Platform
            {
                Body body = BodyFactory.CreateBody(World, new Vector2(-5, 5));
                _platform = FixtureFactory.AttachRectangle(20, 1f, 1, Vector2.Zero, body);
                _platform.Friction = 0.8f;
            }

            // Boxes
            for (int i = 0; i < 5; ++i)
            {
                Body body = BodyFactory.CreateRectangle(World, 1f, 1f, 20, new Vector2(-10.0f + 2.0f * i, 7.0f));
                body.BodyType = BodyType.Dynamic;
            }
        }

        protected override void PreSolve(Contact contact, ref Manifold oldManifold)
        {
            base.PreSolve(contact, ref oldManifold);

            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;

            if (fixtureA == _platform)
            {
                contact.TangentSpeed = 5.0f;
            }

            if (fixtureB == _platform)
            {
                contact.TangentSpeed = -5.0f;
            }
        }

        internal static Test Create()
        {
            return new ConveyorBeltTest();
        }
    }
}