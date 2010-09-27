/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
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

        //Contributed by Jonathan Smars - jsmars@gmail.com
      
        /// <summary>
        /// Creates a rounded rectangle with the specified width and height.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="xRadius">The rounding X radius.</param>
        /// <param name="yRadius">The rounding Y radius.</param>
        /// <param name="segments">The number of segments to subdivide the edges.</param>
        /// <returns></returns>
        public static Vertices CreateRoundedRectangle(float width, float height, float xRadius, float yRadius,
                                                      int segments)
        {
            if (yRadius > height / 2 || xRadius > width / 2)
                throw new Exception("Rounding amount can't be more than half the height and width respectively.");
            if (segments < 0)
                throw new Exception("Segments must be zero or more.");

            //We need at least 8 vertices to create a rounded rectangle
            Debug.Assert(Settings.MaxPolygonVertices >= 8);

            Vertices vertices = new Vertices();
            if (segments == 0)
            {
                vertices.Add(new Vector2(width * .5f - xRadius, -height * .5f));
                vertices.Add(new Vector2(width * .5f, -height * .5f + yRadius));

                vertices.Add(new Vector2(width * .5f, height * .5f - yRadius));
                vertices.Add(new Vector2(width * .5f - xRadius, height * .5f));

                vertices.Add(new Vector2(-width * .5f + xRadius, height * .5f));
                vertices.Add(new Vector2(-width * .5f, height * .5f - yRadius));

                vertices.Add(new Vector2(-width * .5f, -height * .5f + yRadius));
                vertices.Add(new Vector2(-width * .5f + xRadius, -height * .5f));
            }
            else
            {
                int numberOfEdges = (segments * 4 + 8);

                float stepSize = MathHelper.TwoPi / (numberOfEdges - 4);
                int perPhase = numberOfEdges / 4;

                Vector2 posOffset = new Vector2(width / 2 - xRadius, height / 2 - yRadius);
                vertices.Add(posOffset + new Vector2(xRadius, -yRadius + yRadius));
                short phase = 0;
                for (int i = 1; i < numberOfEdges; i++)
                {
                    if (i - perPhase == 0 || i - perPhase * 3 == 0)
                    {
                        posOffset.X *= -1;
                        phase--;
                    }
                    else if (i - perPhase * 2 == 0)
                    {
                        posOffset.Y *= -1;
                        phase--;
                    }

                    vertices.Add(posOffset + new Vector2(xRadius * (float) Math.Cos(stepSize * -(i + phase)),
                                                         -yRadius * (float) Math.Sin(stepSize * -(i + phase))));
                }
            }

            return vertices;
        }

        /// <summary>
        /// Set this as a single edge.
        /// </summary>
        /// <param name="start">The first point.</param>
        /// <param name="end">The second point.</param>
        public static Vertices CreateEdge(Vector2 start, Vector2 end)
        {
            Vertices vertices = new Vertices(2);
            vertices.Add(start);
            vertices.Add(end);

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
            for (int i = numberOfEdges - 1; i > 0; --i)
                vertices.Add(new Vector2(xRadius * (float) Math.Cos(stepSize * i),
                                         -yRadius * (float) Math.Sin(stepSize * i)));

            return vertices;
        }

        //Contributed by Yobiv
      
        /// <summary>
        /// Creates an capsule with the specified height, radius and number of edges.
        /// A capsule has the same form as a pill capsule.
        /// </summary>
        /// <param name="height">Height (inner height + 2 * radius) of the capsule.</param>
        /// <param name="endRadius">Radius of the capsule ends.</param>
        /// <param name="edges">The number of edges of the capsule ends. The more edges, the more it resembles an capsule</param>
        /// <returns></returns>
        public static Vertices CreateCapsule(float height, float endRadius, int edges)
        {
            if (endRadius >= height / 2)
                throw new ArgumentException(
                    "The radius must be lower than height / 2. Higher values of radius would create a circle, and not a half circle.",
                    "endRadius");

            return CreateCapsule(height, endRadius, edges, endRadius, edges);
        }

        /// <summary>
        /// Creates an capsule with the specified  height, radius and number of edges.
        /// A capsule has the same form as a pill capsule.
        /// </summary>
        /// <param name="height">Height (inner height + radii) of the capsule.</param>
        /// <param name="topRadius">Radius of the top.</param>
        /// <param name="topEdges">The number of edges of the top. The more edges, the more it resembles an capsule</param>
        /// <param name="bottomRadius">Radius of bottom.</param>
        /// <param name="bottomEdges">The number of edges of the bottom. The more edges, the more it resembles an capsule</param>
        /// <returns></returns>
        public static Vertices CreateCapsule(float height, float topRadius, int topEdges, float bottomRadius,
                                             int bottomEdges)
        {
            if (height <= 0)
                throw new ArgumentException("Height must be longer than 0", "height");

            if (topRadius <= 0)
                throw new ArgumentException("The top radius must be more than 0", "topRadius");

            if (topEdges <= 0)
                throw new ArgumentException("Top edges must be more than 0", "topEdges");

            if (bottomRadius <= 0)
                throw new ArgumentException("The bottom radius must be more than 0", "bottomRadius");

            if (bottomEdges <= 0)
                throw new ArgumentException("Bottom edges must be more than 0", "bottomEdges");

            if (topRadius >= height / 2)
                throw new ArgumentException(
                    "The top radius must be lower than height / 2. Higher values of top radius would create a circle, and not a half circle.",
                    "topRadius");

            if (bottomRadius >= height / 2)
                throw new ArgumentException(
                    "The bottom radius must be lower than height / 2. Higher values of bottom radius would create a circle, and not a half circle.",
                    "bottomRadius");

            Vertices vertices = new Vertices();

            float newHeight = (height - topRadius - bottomRadius) * 0.5f;

            // top
            vertices.Add(new Vector2(topRadius, newHeight));

            float stepSize = MathHelper.Pi / topEdges;
            for (int i = 1; i < topEdges; i++)
            {
                vertices.Add(new Vector2(topRadius * (float) Math.Cos(stepSize * i),
                                         topRadius * (float) Math.Sin(stepSize * i) + newHeight));
            }

            vertices.Add(new Vector2(-topRadius, newHeight));

            // bottom
            vertices.Add(new Vector2(-bottomRadius, -newHeight));

            stepSize = MathHelper.Pi / bottomEdges;
            for (int i = 1; i < bottomEdges; i++)
            {
                vertices.Add(new Vector2(-bottomRadius * (float) Math.Cos(stepSize * i),
                                         -bottomRadius * (float) Math.Sin(stepSize * i) - newHeight));
            }

            vertices.Add(new Vector2(bottomRadius, -newHeight));

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
            tipPercentage /= 100f;
            MathHelper.Clamp(tipPercentage, 0f, 1f);
            float toothTipStepSize = (stepSize / 2f) * tipPercentage;

            float toothAngleStepSize = (stepSize - (toothTipStepSize * 2f)) / 2f;

            for (int i = numberOfTeeth - 1; i >= 0; --i)
            {
                if (toothTipStepSize > 0f)
                {
                    vertices.Add(
                        new Vector2(radius *
                                    (float) Math.Cos(stepSize * i + toothAngleStepSize * 2f + toothTipStepSize),
                                    -radius *
                                    (float) Math.Sin(stepSize * i + toothAngleStepSize * 2f + toothTipStepSize)));

                    vertices.Add(
                        new Vector2((radius + toothHeight) *
                                    (float) Math.Cos(stepSize * i + toothAngleStepSize + toothTipStepSize),
                                    -(radius + toothHeight) *
                                    (float) Math.Sin(stepSize * i + toothAngleStepSize + toothTipStepSize)));
                }

                vertices.Add(new Vector2((radius + toothHeight) *
                                         (float) Math.Cos(stepSize * i + toothAngleStepSize),
                                         -(radius + toothHeight) *
                                         (float) Math.Sin(stepSize * i + toothAngleStepSize)));

                vertices.Add(new Vector2(radius * (float) Math.Cos(stepSize * i),
                                         -radius * (float) Math.Sin(stepSize * i)));
            }

            return vertices;
        }

        public static Vertices CreatePolygon(uint[] data, int width, int height)
        {
            return TextureConverter.CreateVertices(data, width, height);
        }

        public static Vertices CreatePolygon(uint[] data, int width, int height, bool holeDetection)
        {
            return TextureConverter.CreateVertices(data, width, height, holeDetection);
        }

        public static List<Vertices> CreatePolygon(uint[] data, int width, int height, float hullTolerance,
                                                   byte alphaTolerance, bool multiPartDetection, bool holeDetection)
        {
            return TextureConverter.CreateVertices(data, width, height, hullTolerance, alphaTolerance,
                                                   multiPartDetection, holeDetection);
        }
    }
}