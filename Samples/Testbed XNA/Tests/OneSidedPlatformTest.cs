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
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public class OneSidedPlatformTest : Test
    {
        private Fixture _character;
        private Fixture _platform;
        private float _radius, _top;

        private OneSidedPlatformTest()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            // Platform
            {
                Body body = BodyFactory.CreateBody(World);
                body.Position = new Vector2(0.0f, 10.0f);

                PolygonShape shape = new PolygonShape(1);
                shape.Vertices = PolygonTools.CreateRectangle(3.0f, 0.5f);
                _platform = body.CreateFixture(shape);

                _top = 10.0f + 0.5f;
            }

            // Actor
            {
                Body body = BodyFactory.CreateBody(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(0.0f, 12.0f);

                _radius = 0.5f;
                CircleShape shape = new CircleShape(_radius, 20);
                _character = body.CreateFixture(shape);

                body.LinearVelocity = new Vector2(0.0f, -50.0f);
            }
        }

        protected override void PreSolve(Contact contact, ref Manifold oldManifold)
        {
            base.PreSolve(contact, ref oldManifold);

            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;

            if (fixtureA != _platform && fixtureA != _character)
            {
                return;
            }

            if (fixtureB != _platform && fixtureB != _character)
            {
                return;
            }

            Vector2 position = _character.Body.Position;

            if (position.Y < _top + _radius - 3.0f * Settings.LinearSlop)
            {
                contact.Enabled = false;
            }
        }

        internal static Test Create()
        {
            return new OneSidedPlatformTest();
        }
    }
}