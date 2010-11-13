/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
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

using System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class LockTest : Test
    {
        private Fixture _rectangle;

        private LockTest()
        {
            FixtureFactory.CreateEdge(World, new Vector2(-20, 0), new Vector2(20, 0));

            _rectangle = FixtureFactory.CreateRectangle(World, 2, 2, 1);
            _rectangle.Body.BodyType = BodyType.Dynamic;
            _rectangle.Body.Position = new Vector2(0, 10);
            _rectangle.OnCollision += OnCollision;

            //Properties and methods that were checking for lock before
            //Body.Active
            //Body.LocalCenter
            //Body.Mass
            //Body.Inertia
            //Fixture.DestroyFixture()
            //Body.SetTransformIgnoreContacts()
            //Fixture()
        }

        private bool OnCollision(Fixture fixturea, Fixture fixtureb, Contact manifold)
        {
            _rectangle.Body.CreateFixture(_rectangle.Shape); //Calls the constructor in Fixture
            _rectangle.Body.DestroyFixture(_rectangle);
            //_rectangle.Body.Inertia = 40;
            //_rectangle.Body.LocalCenter = new Vector2(-1,-1);
            //_rectangle.Body.Mass = 10;
            //_rectangle.Body.Active = false;);)
            return true;
        }

        internal static Test Create()
        {
            return new LockTest();
        }
    }
}