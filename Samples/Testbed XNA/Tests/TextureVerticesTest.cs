using System.Diagnostics;
using System.IO;
using FarseerPhysics.Common;
using FarseerPhysics.Common.ConvexHull;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class TextureVerticesTest : Test
    {
        private const int PointCount = 64;

        public override void Initialize()
        {
            base.Initialize();

            CreateBody(LoadFromTexture());
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.D1))
                CreateBody(LoadFromTexture());
            else if (keyboardManager.IsNewKeyPress(Keys.D2))
                CreateBody(CreateRandomConvexPolygon());
#if WINDOWS
            else if (keyboardManager.IsNewKeyPress(Keys.D3))
                CreateBody(LoadRandomFromDataFile());
#endif

            base.Keyboard(keyboardManager);
        }

        private void CreateBody(Vertices vertices)
        {
            World.Clear();

            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            //Create a single body with multiple fixtures
            Body compund = BodyFactory.CreateCompoundPolygon(World, EarclipDecomposer.ConvexPartition(vertices), 1);
            compund.BodyType = BodyType.Dynamic;
            compund.Position = new Vector2(0, 20);
        }

        private Vertices LoadFromTexture()
        {
            //Load texture that will represent the physics body
            Texture2D polygonTexture = GameInstance.Content.Load<Texture2D>("Texture");

            //Create an array to hold the data from the texture
            uint[] data = new uint[polygonTexture.Width * polygonTexture.Height];

            //Transfer the texture data to the array
            polygonTexture.GetData(data);

            //Find the vertices that makes up the outline of the shape in the texture
            Vertices verts = PolygonTools.CreatePolygon(data, polygonTexture.Width);

            //For now we need to scale the vertices (result is in pixels, we use meters)
            Vector2 scale = new Vector2(0.07f, -0.07f);
            verts.Scale(ref scale);

            //We also need to move the polygon so that (0,0) is the center of the polygon.
            Vector2 centroid = -verts.GetCentroid();
            verts.Translate(ref centroid);

            return verts;
        }

        private Vertices CreateRandomConvexPolygon()
        {
            Vertices pointCloud1 = new Vertices(PointCount);

            for (int i = 0; i < PointCount; i++)
            {
                float x = Rand.RandomFloat(-10, 10);
                float y = Rand.RandomFloat(-10, 10);

                pointCloud1.Add(new Vector2(x, y));
            }

            return GiftWrap.GetConvexHull(pointCloud1);
        }

#if WINDOWS
        private Vertices LoadRandomFromDataFile()
        {
            string[] files = Directory.GetFiles("Data/", "*.dat");
            string randomFile = files[Rand.Random.Next(0, files.Length)];
            Debug.WriteLine(randomFile);
            string[] lines = File.ReadAllLines(randomFile);

            Vertices vertices = new Vertices(lines.Length);

            foreach (string line in lines)
            {
                string[] split = line.Split(' ');
                vertices.Add(new Vector2(float.Parse(split[0]), float.Parse(split[1])));
            }

            Vector2 scale = new Vector2(0.1f, -0.1f);
            vertices.Scale(ref scale);

            return vertices;
        }
#endif

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DebugView.DrawString(50, TextLine, "Press 1: Convex partition a texture.");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Press 2: Convex partition random convex polygon.");
            TextLine += 15;

#if WINDOWS
            DebugView.DrawString(50, TextLine, "Press 3: Convex partition random preset polygon. (Can be really slow)");
            TextLine += 15;
#endif

            base.Update(settings, gameTime);
        }


        public static Test Create()
        {
            return new TextureVerticesTest();
        }
    }
}