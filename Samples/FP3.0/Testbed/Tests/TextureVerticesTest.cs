using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.TestBed.Tests
{
    public class TextureVerticesTest : Test
    {
        private Texture2D _polygonTexture;
        private List<Vertices> list;
        private Vertices verts;

        private TextureVerticesTest()
        {
            //Ground
            FixtureFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f), 0);
        }

        public override void Initialize()
        {
            //load texture that will represent the physics body
            _polygonTexture = GameInstance.Content.Load<Texture2D>("Texture");

            //Create an array to hold the data from the texture
            uint[] data = new uint[_polygonTexture.Width * _polygonTexture.Height];

            //Transfer the texture data to the array
            _polygonTexture.GetData(data);

            //Find the vertices that makes up the outline of the shape in the texture
            verts = PolygonTools.CreatePolygon(data, _polygonTexture.Width, _polygonTexture.Height, true);

            //For now we need to scale the vertices (result is in pixels, we use meters)
            Vector2 scale = new Vector2(0.07f, 0.07f);
            verts.Scale(ref scale);

            //Since it is a concave polygon, we need to partition it into several smaller convex polygons
            list = BayazitDecomposer.ConvexPartition(verts);

            //Create a single body with multiple fixtures
            List<Fixture> compund = FixtureFactory.CreateCompundPolygon(World, list, 1);
            compund[0].Body.BodyType = BodyType.Dynamic;

            List<Fixture> fixtures = FixtureFactory.CreateCapsule(World, 3, 1, 1);
            fixtures[0].Body.Position = new Vector2(-10, 15);
            fixtures[0].Body.BodyType = BodyType.Dynamic;

            FixtureFactory.CreateRoundedRectangle(World, 3, 3, 0.25F, 0.25F, 2, 1, new Vector2(-10, 10));

            base.Initialize();
        }

        public static Test Create()
        {
            return new TextureVerticesTest();
        }
    }
}