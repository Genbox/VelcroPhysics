﻿using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.PolygonManipulation
{
    public static class CuttingTools
    {
        //Cutting a shape into two is based on the work of Daid and his prototype BoxCutter: http://www.box2d.org/forum/viewtopic.php?f=3&t=1473

        /// <summary>
        /// Split a fixture into 2 vertice collections using the given entry and exit-point.
        /// </summary>
        /// <param name="fixture">The Fixture to split</param>
        /// <param name="entryPoint">The entry point - The start point</param>
        /// <param name="exitPoint">The exit point - The end point</param>
        /// <param name="splitSize">The size of the split. Think of this as the laser-width</param>
        /// <param name="first">The first collection of vertexes</param>
        /// <param name="second">The second collection of vertexes</param>
        public static void SplitShape(Fixture fixture, Vector2 entryPoint, Vector2 exitPoint, float splitSize,
                                      out Vertices first, out Vertices second)
        {
            Vector2 localEntryPoint = fixture.Body.GetLocalPoint(entryPoint);
            Vector2 localExitPoint = fixture.Body.GetLocalPoint(exitPoint);

            PolygonShape shape = fixture.Shape as PolygonShape;

            //TODO: Fix
            //if (shape == null)
            //    return;

            Vertices vertices = new Vertices(ref shape.Vertices);
            Vertices[] newPolygon = new Vertices[2];

            for (int i = 0; i < newPolygon.Length; i++)
            {
                newPolygon[i] = new Vertices(vertices.Count);
            }

            int[] cutAdded = {-1, -1};
            int last = -1;
            for (int i = 0; i < vertices.Count; i++)
            {
                int n;
                //Find out if this vertex is on the old or new shape.
                if (Vector2.Dot(MathUtils.Cross(localExitPoint - localEntryPoint, 1), vertices[i] - localEntryPoint) > 0)
                    n = 0;
                else
                    n = 1;

                if (last != n)
                {
                    //If we switch from one shape to the other add the cut vertices.
                    if (last == 0)
                    {
                        Debug.Assert(cutAdded[0] == -1);
                        cutAdded[0] = newPolygon[last].Count;
                        newPolygon[last].Add(localExitPoint);
                        newPolygon[last].Add(localEntryPoint);
                    }
                    if (last == 1)
                    {
                        Debug.Assert(cutAdded[last] == -1);
                        cutAdded[last] = newPolygon[last].Count;
                        newPolygon[last].Add(localEntryPoint);
                        newPolygon[last].Add(localExitPoint);
                    }
                }

                newPolygon[n].Add(vertices[i]);
                last = n;
            }

            //Add the cut in case it has not been added yet.
            if (cutAdded[0] == -1)
            {
                cutAdded[0] = newPolygon[0].Count;
                newPolygon[0].Add(localExitPoint);
                newPolygon[0].Add(localEntryPoint);
            }
            if (cutAdded[1] == -1)
            {
                cutAdded[1] = newPolygon[1].Count;
                newPolygon[1].Add(localEntryPoint);
                newPolygon[1].Add(localExitPoint);
            }

            for (int n = 0; n < 2; n++)
            {
                Vector2 offset;
                if (cutAdded[n] > 0)
                {
                    offset = (newPolygon[n][cutAdded[n] - 1] - newPolygon[n][cutAdded[n]]);
                }
                else
                {
                    offset = (newPolygon[n][newPolygon[n].Count - 1] - newPolygon[n][0]);
                }
                offset.Normalize();

                newPolygon[n][cutAdded[n]] += splitSize * offset;

                if (cutAdded[n] < newPolygon[n].Count - 2)
                {
                    offset = (newPolygon[n][cutAdded[n] + 2] - newPolygon[n][cutAdded[n] + 1]);
                }
                else
                {
                    offset = (newPolygon[n][0] - newPolygon[n][newPolygon[n].Count - 1]);
                }
                offset.Normalize();

                newPolygon[n][cutAdded[n] + 1] += splitSize * offset;
            }

            first = newPolygon[0];
            second = newPolygon[1];
        }

        /// <summary>
        /// This is a high-level function to cuts fixtures inside the given world, using the start and end points.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="start">The startpoint.</param>
        /// <param name="end">The endpoint.</param>
        /// <param name="thickness">The thickness of the cut</param>
        public static void Cut(World world, Vector2 start, Vector2 end, float thickness)
        {
            List<Fixture> fixtures = new List<Fixture>();
            List<Vector2> entryPoints = new List<Vector2>();
            List<Vector2> exitPoints = new List<Vector2>();

            //Get the entry points
            world.RayCast((f, p, n, fr) =>
                              {
                                  fixtures.Add(f);
                                  entryPoints.Add(p);
                                  return 1;
                              }, start, end);

            //Reverse the ray to get the exitpoints
            world.RayCast((f, p, n, fr) =>
                              {
                                  exitPoints.Add(p);
                                  return 1;
                              }, end, start);

            //We only have a single point. We need at least 2
            if (entryPoints.Count + exitPoints.Count < 2)
                return;

            //There should be as many entry as exit points
            if (entryPoints.Count != exitPoints.Count)
            {
                if (entryPoints.Count > exitPoints.Count)
                {
                    entryPoints.RemoveAt(entryPoints.Count - 1);
                    fixtures.RemoveAt(fixtures.Count - 1);
                }

                if (exitPoints.Count > entryPoints.Count)
                {
                    exitPoints.RemoveAt(exitPoints.Count - 1);
                    fixtures.RemoveAt(fixtures.Count - 1);
                }
            }

            for (int i = 0; i < fixtures.Count; i++)
            {
                // can't cut circles yet !
                if (fixtures[i].Shape.ShapeType != ShapeType.Polygon)
                    continue;

                //Split the shape up into two shapes
                Vertices first;
                Vertices second;
                if (fixtures[i].Body.BodyType != BodyType.Static)
                {
                    SplitShape(fixtures[i], entryPoints[i], exitPoints[i], thickness, out first, out second);

                    //Delete the original shape and create two new. Retain the properties of the body.

                    //TODO: Fix
                    //Body body1 = fixtures[i].Body.Clone(world);
                    //world.Add(body1);

                    //PolygonShape shape1 = new PolygonShape(first, fixtures[i].Shape.Density);
                    //body1.CreateFixture(shape1);

                    //Body body2 = fixtures[i].Body.Clone(world);
                    //world.Add(body2);

                    //PolygonShape shape2 = new PolygonShape(second, fixtures[i].Shape.Density);
                    //body2.CreateFixture(shape2);

                    //world.Remove(fixtures[i].Body);
                }
            }
        }
    }
}