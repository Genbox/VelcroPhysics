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

using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    //TODO: Upgrade
    public class CharacterCollisionTest : Test
    {
        private bool _collision;

        private CharacterCollisionTest()
        {
            Fixture ground = FixtureFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f), 0);

            // Collinear edges
            PolygonShape shape = new PolygonShape();
            shape.SetAsEdge(new Vector2(-8.0f, 1.0f), new Vector2(-6.0f, 1.0f));
            ground.Body.CreateFixture(shape);
            shape.SetAsEdge(new Vector2(-6.0f, 1.0f), new Vector2(-4.0f, 1.0f));
            ground.Body.CreateFixture(shape);
            shape.SetAsEdge(new Vector2(-4.0f, 1.0f), new Vector2(-2.0f, 1.0f));
            ground.Body.CreateFixture(shape);

            // Square tiles
            PolygonShape tile = new PolygonShape();
            tile.SetAsBox(1.0f, 1.0f, new Vector2(4.0f, 3.0f), 0.0f);
            ground.Body.CreateFixture(tile);
            tile.SetAsBox(1.0f, 1.0f, new Vector2(6.0f, 3.0f), 0.0f);
            ground.Body.CreateFixture(tile);
            tile.SetAsBox(1.0f, 1.0f, new Vector2(8.0f, 3.0f), 0.0f);
            ground.Body.CreateFixture(tile);

            // Square made from edge loop
            PolygonShape square = new PolygonShape();
            const float d = 2.0f * Settings.PolygonRadius;
            square.SetAsEdge(new Vector2(-1.0f + d, 3.0f), new Vector2(1.0f - d, 3.0f));
            ground.Body.CreateFixture(square);
            square.SetAsEdge(new Vector2(1.0f, 3.0f + d), new Vector2(1.0f, 5.0f - d));
            ground.Body.CreateFixture(square);
            square.SetAsEdge(new Vector2(1.0f - d, 5.0f), new Vector2(-1.0f + d, 5.0f));
            ground.Body.CreateFixture(square);
            square.SetAsEdge(new Vector2(-1.0f, 5.0f - d), new Vector2(-1.0f, 3.0f + d));
            ground.Body.CreateFixture(square);

            // Square character
            Fixture squareCharacter = FixtureFactory.CreateRectangle(World, 1, 1, 20);
            squareCharacter.Body.Position = new Vector2(-3.0f, 5.0f);
            squareCharacter.Body.BodyType = BodyType.Dynamic;
            squareCharacter.Body.FixedRotation = true;
            squareCharacter.Body.SleepingAllowed = false;

            squareCharacter.OnCollision += CharacterOnCollision;
            squareCharacter.OnSeparation += CharacterOnSeparation;

#if false
    // Hexagon character
            float angle = 0.0f;
            const float delta = Settings.b2_pi / 3.0f;
            Vertices vertices = new Vertices();

            for (int i = 0; i < 6; ++i)
            {
                vertices.Add(new Vector2(0.5f * (float)Math.Cos(angle), 0.5f * (float)Math.Sin(angle)));
                angle += delta;
            }

            Fixture hexCharacter = FixtureFactory.CreatePolygon(World, vertices, 20);
            hexCharacter.Body.Position = new Vector2(-5.0f, 5.0f);
            hexCharacter.Body.BodyType = BodyType.Dynamic;
            hexCharacter.Body.FixedRotation = true;
            hexCharacter.Body.AllowSleep = false;

            // Circle character
            Fixture circleCharacter = FixtureFactory.CreateCircle(World, 0.5f, 20);
            circleCharacter.Body.Position = new Vector2(3.0f, 5.0f);
            circleCharacter.Body.BodyType = BodyType.Dynamic;
            circleCharacter.Body.FixedRotation = true;
            circleCharacter.Body.AllowSleep = false;
#endif
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
            if (_collision)
            {
                DebugView.DrawString(50, TextLine, "OnCollision fired");
            }
            else
            {
                DebugView.DrawString(50, TextLine, "OnSeparation fired");
            }

            base.Update(settings, gameTime);
        }

        public static Test Create()
        {
            return new CharacterCollisionTest();
        }
    }
}