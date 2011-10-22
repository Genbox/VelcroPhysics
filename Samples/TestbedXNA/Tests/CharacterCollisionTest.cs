/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.box2d.org 
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
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class CharacterCollisionTest : Test
    {
        private bool _collision;

        private CharacterCollisionTest()
        {
            //Ground body
            Body ground = BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            // Collinear edges
            EdgeShape shape = new EdgeShape(new Vector2(-8.0f, 1.0f), new Vector2(-6.0f, 1.0f));
            ground.CreateFixture(shape);
            shape = new EdgeShape(new Vector2(-6.0f, 1.0f), new Vector2(-4.0f, 1.0f));
            ground.CreateFixture(shape);
            shape = new EdgeShape(new Vector2(-4.0f, 1.0f), new Vector2(-2.0f, 1.0f));
            ground.CreateFixture(shape);

            // Square tiles
            PolygonShape tile = new PolygonShape(1);
            tile.SetAsBox(1.0f, 1.0f, new Vector2(4.0f, 3.0f), 0.0f);
            ground.CreateFixture(tile);
            tile.SetAsBox(1.0f, 1.0f, new Vector2(6.0f, 3.0f), 0.0f);
            ground.CreateFixture(tile);
            tile.SetAsBox(1.0f, 1.0f, new Vector2(8.0f, 3.0f), 0.0f);
            ground.CreateFixture(tile);

            // Square made from an edge chain.
            Vertices vertices = new Vertices(4);
            vertices.Add(new Vector2(-1.0f, 3.0f));
            vertices.Add(new Vector2(1.0f, 3.0f));
            vertices.Add(new Vector2(1.0f, 5.0f));
            vertices.Add(new Vector2(-1.0f, 5.0f));
            ChainShape chainShape = new ChainShape(vertices);
            ground.CreateFixture(chainShape);

            // Edge chain.
            vertices = new Vertices(10);
            vertices.Add(new Vector2(0.0f, 0.0f));
            vertices.Add(new Vector2(6.0f, 0.0f));
            vertices.Add(new Vector2(6.0f, 2.0f));
            vertices.Add(new Vector2(4.0f, 1.0f));
            vertices.Add(new Vector2(2.0f, 2.0f));
            vertices.Add(new Vector2(-2.0f, 2.0f));
            vertices.Add(new Vector2(-4.0f, 3.0f));
            vertices.Add(new Vector2(-6.0f, 2.0f));
            vertices.Add(new Vector2(-6.0f, 0.0f));

            BodyFactory.CreateChainShape(World, vertices, new Vector2(-10, 4));

            // Square character
            Body squareCharacter = BodyFactory.CreateRectangle(World, 1, 1, 20);
            squareCharacter.Position = new Vector2(-3.0f, 5.0f);
            squareCharacter.BodyType = BodyType.Dynamic;
            squareCharacter.FixedRotation = true;
            squareCharacter.SleepingAllowed = false;

            squareCharacter.OnCollision += CharacterOnCollision;
            squareCharacter.OnSeparation += CharacterOnSeparation;

            // Square character 2
            Body squareCharacter2 = BodyFactory.CreateRectangle(World, 0.5f, 0.5f, 20);
            squareCharacter2.Position = new Vector2(-5.0f, 5.0f);
            squareCharacter2.BodyType = BodyType.Dynamic;
            squareCharacter2.FixedRotation = true;
            squareCharacter2.SleepingAllowed = false;

            // Hexagon character
            float angle = 0.0f;
            const float delta = Settings.Pi / 3.0f;
            vertices = new Vertices(6);

            for (int i = 0; i < 6; ++i)
            {
                vertices.Add(new Vector2(0.5f * (float)Math.Cos(angle), 0.5f * (float)Math.Sin(angle)));
                angle += delta;
            }

            Body hexCharacter = BodyFactory.CreatePolygon(World, vertices, 20);
            hexCharacter.Position = new Vector2(-5.0f, 8.0f);
            hexCharacter.BodyType = BodyType.Dynamic;
            hexCharacter.FixedRotation = true;
            hexCharacter.SleepingAllowed = false;

            // Circle character
            Body circleCharacter = BodyFactory.CreateCircle(World, 0.5f, 20);
            circleCharacter.Position = new Vector2(3.0f, 5.0f);
            circleCharacter.BodyType = BodyType.Dynamic;
            circleCharacter.FixedRotation = true;
            circleCharacter.SleepingAllowed = false;
        }

        private bool CharacterOnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            _collision = true;
            return true;
        }

        private void CharacterOnSeparation(Fixture fixtureA, Fixture fixtureB)
        {
            _collision = false;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DebugView.DrawString(50, TextLine, _collision ? "OnCollision fired" : "OnSeparation fired");

            base.Update(settings, gameTime);
        }

        public static Test Create()
        {
            return new CharacterCollisionTest();
        }
    }
}