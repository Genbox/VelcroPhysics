using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class ConvexDecompositionTest : Test
    {
        public ConvexDecompositionTest()
        {
            {
                Body ground = World.CreateBody();

                Vertices edge = PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                PolygonShape shape = new PolygonShape(edge, 0);
                ground.CreateFixture(shape);
            }
        }

        public static Test Create()
        {
            return new ConvexDecompositionTest();
        }
    }
}
