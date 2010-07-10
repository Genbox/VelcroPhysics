using System;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class CharacterCollisionTest : Test
    {
        private CharacterCollisionTest()
        {
            Fixture ground = FixtureFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f), 0);

            // Collinear edges
            PolygonShape shape = new PolygonShape(0);
            shape.SetAsEdge(new Vector2(-8.0f, 1.0f), new Vector2(-6.0f, 1.0f));
            ground.Body.CreateFixture(shape);
            shape.SetAsEdge(new Vector2(-6.0f, 1.0f), new Vector2(-4.0f, 1.0f));
            ground.Body.CreateFixture(shape);
            shape.SetAsEdge(new Vector2(-4.0f, 1.0f), new Vector2(-2.0f, 1.0f));
            ground.Body.CreateFixture(shape);

            // Square tiles
            PolygonShape tile = new PolygonShape(0);
            tile.SetAsBox(1.0f, 1.0f, new Vector2(4.0f, 3.0f), 0.0f);
            ground.Body.CreateFixture(tile);
            tile.SetAsBox(1.0f, 1.0f, new Vector2(6.0f, 3.0f), 0.0f);
            ground.Body.CreateFixture(tile);
            tile.SetAsBox(1.0f, 1.0f, new Vector2(8.0f, 3.0f), 0.0f);
            ground.Body.CreateFixture(tile);

            // Square made from edges notice how the edges are shrunk to account
            // for the polygon radius. This makes it so the square character does
            // not get snagged. However, ray casts can now go through the cracks.
            PolygonShape square = new PolygonShape(0);
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
            squareCharacter.Body.AllowSleep = false;

#if false
            // Hexagon character
            float angle = 0.0f;
            const float delta = Settings.Pi / 3.0f;
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

        public static Test Create()
        {
            return new CharacterCollisionTest();
        }
    }
}