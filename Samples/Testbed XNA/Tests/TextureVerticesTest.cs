using System;
using System.Collections.Generic;
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
        private enum TriangulationAlgorithm
        {
            Delauny,
            Earclip,
            Flipcode,
            Seidel,
            Bayazit
        }

        private const int PointCount = 64;
        private int _fileCounter;
        private int _algorithmCounter;
        private string _nextFileName;
        private TriangulationAlgorithm _algorithm = TriangulationAlgorithm.Delauny;
        private Stopwatch _sw = new Stopwatch();

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
                CreateBody(LoadNextDataFile());
#endif
            else if (keyboardManager.IsNewKeyPress(Keys.T))
            {
                if (_algorithmCounter++ >= 4)
                    _algorithmCounter = 0;

                _algorithm = (TriangulationAlgorithm)_algorithmCounter;


            }

            base.Keyboard(keyboardManager);
        }

        private void CreateBody(Vertices vertices)
        {
            World.Clear();

            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            _sw.Reset();
            _sw.Start();
            //Create a single body with multiple fixtures
            Body compund = BodyFactory.CreateCompoundPolygon(World, Triangulate(vertices), 1);
            compund.BodyType = BodyType.Dynamic;
            compund.Position = new Vector2(0, 20);
            _sw.Stop();
        }

        private List<Vertices> Triangulate(Vertices vertices)
        {
            switch (_algorithm)
            {
                case TriangulationAlgorithm.Delauny:
                    return CDTDecomposer.ConvexPartition(vertices);
                case TriangulationAlgorithm.Earclip:
                    return EarclipDecomposer.ConvexPartition(vertices);
                case TriangulationAlgorithm.Flipcode:
                    return FlipcodeDecomposer.ConvexPartition(vertices);
                case TriangulationAlgorithm.Seidel:
                    return SeidelDecomposer.ConvexPartition(vertices);
                case TriangulationAlgorithm.Bayazit:
                    return BayazitDecomposer.ConvexPartition(vertices);
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
        private Vertices LoadNextDataFile()
        {
            string[] files = Directory.GetFiles("Data/", "*.dat");
            _nextFileName = files[_fileCounter];

            if (_fileCounter++ >= files.Length - 1)
                _fileCounter = 0;

            string[] lines = File.ReadAllLines(_nextFileName);

            Vertices vertices = new Vertices(lines.Length);

            foreach (string line in lines)
            {
                string[] split = line.Split(' ');
                vertices.Add(new Vector2(float.Parse(split[0]), float.Parse(split[1])));
            }

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
            DebugView.DrawString(50, TextLine, "Press 3: Convex partition next polygon from datafile. (Can be slow)");
            TextLine += 15;
#endif
            DebugView.DrawString(50, TextLine, "Press T: Change triangulation algorithm. Current: " + _algorithm);
            TextLine += 15;

            if (!string.IsNullOrEmpty(_nextFileName))
            {
                DebugView.DrawString(50, TextLine, "Loaded: " + _nextFileName);
                TextLine += 15;
            }

            DebugView.DrawString(50, TextLine, "Triangulation took " + _sw.ElapsedMilliseconds + " ms");
            TextLine += 15;

            base.Update(settings, gameTime);
        }


        public static Test Create()
        {
            return new TextureVerticesTest();
        }
    }
}