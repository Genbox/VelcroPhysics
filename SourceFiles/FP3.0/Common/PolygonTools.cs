using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common;

namespace FarseerPhysics
{
    public static class PolygonTools
    {
        /// <summary>
        /// Build vertices to represent an axis-aligned box.
        /// </summary>
        /// <param name="hx">the half-width.</param>
        /// <param name="hy">the half-height.</param>
        public static Vertices CreateRectangle(float hx, float hy)
        {
            Vertices vertices = new Vertices(4);
            vertices.Add(new Vector2(-hx, -hy));
            vertices.Add(new Vector2(hx, -hy));
            vertices.Add(new Vector2(hx, hy));
            vertices.Add(new Vector2(-hx, hy));

            return vertices;
        }

        /// <summary>
        /// Build vertices to represent an oriented box.
        /// </summary>
        /// <param name="hx">the half-width.</param>
        /// <param name="hy">the half-height.</param>
        /// <param name="center">the center of the box in local coordinates.</param>
        /// <param name="angle">the rotation of the box in local coordinates.</param>
        public static Vertices CreateRectangle(float hx, float hy, Vector2 center, float angle)
        {
            Vertices vertices = CreateRectangle(hx, hy);

            Transform xf = new Transform();
            xf.Position = center;
            xf.R.Set(angle);

            // Transform vertices
            for (int i = 0; i < 4; ++i)
            {
                vertices[i] = MathUtils.Multiply(ref xf, vertices[i]);
            }

            return vertices;
        }

        /// <summary>
        /// Set this as a single edge.
        /// </summary>
        /// <param name="v1">The first point.</param>
        /// <param name="v2">The second point.</param>
        public static Vertices CreateEdge(Vector2 v1, Vector2 v2)
        {
            Vertices vertices = new Vertices(2);
            vertices.Add(v1);
            vertices.Add(v2);

            return vertices;
        }

        /// <summary>
        /// Creates a circle with the specified radius and number of edges.
        /// </summary>
        /// <param name="radius">The radius.</param>
        /// <param name="numberOfEdges">The number of edges. The more edges, the more it resembles a circle</param>
        /// <returns></returns>
        public static Vertices CreateCircle(float radius, int numberOfEdges)
        {
            return CreateEllipse(radius, radius, numberOfEdges);
        }

        /// <summary>
        /// Creates a ellipse with the specified width, height and number of edges.
        /// </summary>
        /// <param name="xRadius">Width of the ellipse.</param>
        /// <param name="yRadius">Height of the ellipse.</param>
        /// <param name="numberOfEdges">The number of edges. The more edges, the more it resembles an ellipse</param>
        /// <returns></returns>
        public static Vertices CreateEllipse(float xRadius, float yRadius, int numberOfEdges)
        {
            Vertices vertices = new Vertices();

            float stepSize = MathHelper.TwoPi / numberOfEdges;

            vertices.Add(new Vector2(xRadius, 0));
            for (int i = 1; i < numberOfEdges; i++)
                vertices.Add(new Vector2(xRadius * (float)Math.Cos(stepSize * i), -yRadius * (float)Math.Sin(stepSize * i)));

            return vertices;
        }

        /// <summary>
        /// Creates a gear shape with the specified radius and number of teeth.
        /// </summary>
        /// <param name="radius">The radius.</param>
        /// <param name="numberOfTeeth">The number of teeth.</param>
        /// <param name="tipPercentage">The tip percentage.</param>
        /// <param name="toothHeight">Height of the tooth.</param>
        /// <returns></returns>
        public static Vertices CreateGear(float radius, int numberOfTeeth, float tipPercentage, float toothHeight)
        {
            Vertices vertices = new Vertices();

            float stepSize = MathHelper.TwoPi / numberOfTeeth;

            float toothTipStepSize = (stepSize / 2f) * tipPercentage;

            float toothAngleStepSize = (stepSize - (toothTipStepSize * 2f)) / 2f;

            for (int i = 0; i < numberOfTeeth; i++)
            {
                vertices.Add(new Vector2((radius) * (float)Math.Cos(stepSize * i),
                    -(radius) * (float)Math.Sin(stepSize * i)));

                vertices.Add(new Vector2((radius + toothHeight) * (float)Math.Cos((stepSize * i) + toothAngleStepSize),
                    -(radius + toothHeight) * (float)Math.Sin((stepSize * i) + toothAngleStepSize)));

                vertices.Add(new Vector2((radius + toothHeight) * (float)Math.Cos((stepSize * i) + toothAngleStepSize + toothTipStepSize),
                    -(radius + toothHeight) * (float)Math.Sin((stepSize * i) + toothAngleStepSize + toothTipStepSize)));

                vertices.Add(new Vector2((radius) * (float)Math.Cos((stepSize * i) + (toothAngleStepSize * 2f) + toothTipStepSize),
                    -(radius) * (float)Math.Sin((stepSize * i) + (toothAngleStepSize * 2f) + toothTipStepSize)));
            }

            return vertices;
        }

        public static Vertices CreatePolygon(uint[] data, int width, int height)
        {
            return TextureConverter.CreatePolygon(data, width, height);
        }

        public static Vertices CreatePolygon(uint[] data, int width, int height, bool holeDetection)
        {
            return TextureConverter.CreatePolygon(data, width, height, holeDetection);
        }

        public static List<Vertices> CreatePolygon(uint[] data, int width, int height, float hullTolerance, byte alphaTolerance, bool multiPartDetection, bool holeDetection)
        {
            return TextureConverter.CreatePolygon(data, width, height, hullTolerance, alphaTolerance, holeDetection, multiPartDetection);
        }
    }
}