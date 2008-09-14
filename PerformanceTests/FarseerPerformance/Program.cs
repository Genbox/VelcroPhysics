using System;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Diagnostics;

namespace FarseerPerformance
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Please wait for the results to come in...");
            Stopwatch watch = new Stopwatch();

            for (int k = 0; k < 3; k++)
            {
                const int pyramidBaseBodyCount = 40;
                const int baseWidth = 1600;
                const int baseHeight = 100;

                const int startIterations = 50;
                const int runs = 5;
                int totalIterations = 0;
                long totalTime = 0;

                for (int j = 1; j <= runs; j++)
                {
                    PhysicsSimulator simulator = new PhysicsSimulator(new Vector2(0, 150));
                    Body body1 = BodyFactory.Instance.CreateRectangleBody(simulator, baseWidth, baseHeight, 1);
                    body1.Position = new Vector2(1600 / 2f, 768);
                    GeomFactory.Instance.CreateRectangleGeom(simulator, body1, baseWidth, baseHeight, 0);

                    Body rectangleBody = BodyFactory.Instance.CreateRectangleBody(32, 32, 1f); //template              
                    Geom rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(rectangleBody, 32, 32); //template
                    rectangleGeom.FrictionCoefficient = .4f;
                    rectangleGeom.RestitutionCoefficient = 0f;

                    Pyramid pyramid = new Pyramid(rectangleBody, rectangleGeom, 32f / 3f, 32f / 3f, 32, 32, pyramidBaseBodyCount,
                                           new Vector2(baseWidth / 2f - pyramidBaseBodyCount * .5f * (32 + 32 / 3), 600));
                    pyramid.Load(simulator);

                    int currentIterations = startIterations * j;
                    totalIterations += currentIterations;
                    long runTime = 0;

                    for (int i = 0; i < currentIterations; i++)
                    {
                        watch.Start();
                        simulator.Update(16.6f);
                        watch.Stop();
                        runTime += watch.ElapsedMilliseconds;
                        watch.Reset();
                    }
                    totalTime += runTime;

                    Console.WriteLine("Run {0} took {1} ms with {2} simulation iterations", j, runTime, currentIterations);
                }
                Console.WriteLine();
                Console.WriteLine("It took on average {0} ms for each iteration for {1} simulation iterations in {2} runs. Average: {3} ms", totalTime / totalIterations, totalIterations, runs, totalTime / runs);
            }
            Console.ReadLine();
        }
    }
}
