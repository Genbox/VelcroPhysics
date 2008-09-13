using System;
using System.Timers;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.FarseerPhysics.Factories;
using System.Diagnostics;

namespace FarseerPerformance
{
    public class Program
    {
        private static PhysicsSimulator _simulator;
        private static Stopwatch _watch;

        public static void Main()
        {
            _watch = new Stopwatch();

            const int pyramidBaseBodyCount = 40;
            const int baseWidth = 1600;
            const int baseHeight = 100;

            _simulator = new PhysicsSimulator(new Vector2(0, 150));
            Body body1 = BodyFactory.Instance.CreateRectangleBody(_simulator, baseWidth, baseHeight, 1);
            body1.Position = new Vector2(1600 / 2f, 768);
            GeomFactory.Instance.CreateRectangleGeom(_simulator, body1, baseWidth, baseHeight, 0);

            Body rectangleBody = BodyFactory.Instance.CreateRectangleBody(32, 32, 1f); //template              
            Geom rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(rectangleBody, 32, 32); //template
            rectangleGeom.FrictionCoefficient = .4f;
            rectangleGeom.RestitutionCoefficient = 0f;

            Pyramid pyramid = new Pyramid(rectangleBody, rectangleGeom, 32f / 3f, 32f / 3f, 32, 32, pyramidBaseBodyCount,
                                   new Vector2(baseWidth / 2f - pyramidBaseBodyCount * .5f * (32 + 32 / 3), 600));
            pyramid.Load(_simulator);

            long sec = 0;
            const int iterations = 500;

            for (int i = 0; i < iterations; i++)
            {
                _watch.Start();
                _simulator.Update(16.6f);
                _watch.Stop();
                sec += _watch.ElapsedMilliseconds;
                _watch.Reset();
            }

            Console.WriteLine("It took on average {0} ms with {1} iterations", sec / iterations, iterations);
        }
    }
}
