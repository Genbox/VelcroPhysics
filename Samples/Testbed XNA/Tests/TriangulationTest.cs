using FarseerPhysics.Testbed.Framework;
#if WINDOWS
using System.Diagnostics;
using System.IO;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
{
    public class TriangulationTest : Test
    {
        private int _fileCounter;
        private string _nextFileName;
        private Stopwatch _sw = new Stopwatch();
        private float[] _timings = new float[6];
        private Body[] _bodies = new Body[6];
        private string[] _names = new[] { "Seidel", "Seidel (trapezoids)", "Delauny", "Earclip", "Flipcode", "Bayazit" };

        public override void Initialize()
        {
            base.Initialize();

            CreateBody(LoadNextDataFile());
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.T))
                CreateBody(LoadNextDataFile());

            base.Keyboard(keyboardManager);
        }

        private void CreateBody(Vertices vertices)
        {
            World.Clear();

            _sw.Start();
            _bodies[0] = BodyFactory.CreateCompoundPolygon(World, Triangulate.ConvexPartition(vertices, TriangulationAlgorithm.Seidel), 1);
            _bodies[0].Position = new Vector2(-30, 28);
            _timings[0] = _sw.ElapsedMilliseconds;

            _sw.Restart();
            _bodies[1] = BodyFactory.CreateCompoundPolygon(World, Triangulate.ConvexPartition(vertices, TriangulationAlgorithm.SeidelTrapezoids), 1);
            _bodies[1].Position = new Vector2(0, 28);
            _timings[1] = _sw.ElapsedMilliseconds;

            _sw.Restart();
            _bodies[2] = BodyFactory.CreateCompoundPolygon(World, Triangulate.ConvexPartition(vertices, TriangulationAlgorithm.Delauny), 1);
            _bodies[2].Position = new Vector2(30, 28);
            _timings[2] = _sw.ElapsedMilliseconds;

            _sw.Restart();
            _bodies[3] = BodyFactory.CreateCompoundPolygon(World, Triangulate.ConvexPartition(vertices, TriangulationAlgorithm.Earclip), 1);
            _bodies[3].Position = new Vector2(-30, 5);
            _timings[3] = _sw.ElapsedMilliseconds;

            _sw.Restart();
            _bodies[4] = BodyFactory.CreateCompoundPolygon(World, Triangulate.ConvexPartition(vertices, TriangulationAlgorithm.Flipcode), 1);
            _bodies[4].Position = new Vector2(0, 5);
            _timings[4] = _sw.ElapsedMilliseconds;

            _sw.Restart();
            _bodies[5] = BodyFactory.CreateCompoundPolygon(World, Triangulate.ConvexPartition(vertices, TriangulationAlgorithm.Bayazit), 1);
            _bodies[5].Position = new Vector2(30, 5);
            _timings[5] = _sw.ElapsedMilliseconds;

            _sw.Stop();
        }

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

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Loaded: " + _nextFileName + " - Press T for next");

            Vector2 offset = new Vector2(-6, 12);

            for (int i = 0; i < _names.Length; i++)
            {
                string title = string.Format("{0}: {1} ms - {2} triangles", _names[i], _timings[i], _bodies[i].FixtureList.Count);

                Vector2 screenPosition = GameInstance.ConvertWorldToScreen(_bodies[i].Position + offset);
                DebugView.DrawString((int)screenPosition.X, (int)screenPosition.Y, title);
            }

            base.Update(settings, gameTime);
        }

        public static Test Create()
        {
            return new TriangulationTest();
        }
    }
}
#endif