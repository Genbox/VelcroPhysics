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

            const int screenWidth = 768;
            const int screenHight = 1024;
            const int pyramidBaseBodyCount = 18;
            const int runs = 5;
            long totalTicks = 0;

            for (int k = 0; k < 3; k++)
            {
                long ticks = 0;

                for (int j = 1; j <= runs; j++)
                {
                    PhysicsSimulator simulator = new PhysicsSimulator(new Vector2(0, 0));

                    Border border = new Border(screenWidth, screenHight, 10, new Vector2(screenWidth / 2f, screenHight / 2f));
                    border.Load(simulator);

                    Body rectangleBody = BodyFactory.Instance.CreateRectangleBody(32, 32, 1f); //template              
                    Geom rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(rectangleBody, 32, 32); //template
                    rectangleGeom.FrictionCoefficient = .4f;
                    rectangleGeom.RestitutionCoefficient = 0f;

                    Pyramid pyramid = new Pyramid(rectangleBody, rectangleGeom, 32f / 3f, 32f / 3f, 32, 32, pyramidBaseBodyCount,
                                           new Vector2(screenWidth / 2f - pyramidBaseBodyCount * .5f * (32 + 32 / 3),
                                                       screenHight - 60)); pyramid.Load(simulator);

                    watch.Start();

                    for (int i = 0; i < 1000; i++)
                    {
                        simulator.Update(0.01f);
                    }

                    watch.Stop();
                    ticks += watch.ElapsedTicks;
                    watch.Reset();
                    Console.WriteLine("1000 iterations took: {0} ticks", ticks);
                    totalTicks += ticks;
                }
                Console.WriteLine();
                Console.WriteLine("Took {0} ticks on average.", totalTicks / runs);
                Console.WriteLine();
                totalTicks = 0;
            }
            Console.ReadLine();
        }
    }
}
