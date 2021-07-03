using System.Diagnostics;
using System.Globalization;
using System.IO;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Tools.Triangulation.TriangulationBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests.Velcro
{
    public class TriangulationTest : Test
    {
        private readonly Body[] _bodies = new Body[6];
        private readonly string[] _names = { "Seidel", "Seidel (trapezoids)", "Delauny", "Earclip", "Flipcode", "Bayazit" };
        private readonly Stopwatch _sw = new Stopwatch();
        private readonly float[] _timings = new float[6];
        private int _fileCounter;
        private string _nextFileName;

        public override void Initialize()
        {
            base.Initialize();

            CreateBody(LoadNextDataFile());
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.T))
                CreateBody(LoadNextDataFile());

            base.Keyboard(keyboard);
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
                vertices.Add(new Vector2(float.Parse(split[0], NumberFormatInfo.InvariantInfo), float.Parse(split[1], NumberFormatInfo.InvariantInfo)));
            }

            return vertices;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Loaded: " + _nextFileName + " - Press T for next");

            Vector2 offset = new Vector2(-6, 12);

            for (int i = 0; i < _names.Length; i++)
            {
                string title = $"{_names[i]}: {_timings[i]} ms - {_bodies[i].FixtureList.Count} triangles";

                Vector2 screenPosition = GameInstance.ConvertWorldToScreen(_bodies[i].Position + offset);
                DebugView.DrawString((int)screenPosition.X, (int)screenPosition.Y, title);
            }

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new TriangulationTest();
        }
    }
}