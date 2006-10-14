using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Collisions {
    public class Vertices : List<Vector2> {
        public Vertices() { }

        public Vertices(Vector2[] vector2) {
            for (int i = 0; i < vector2.Length; i++) {
                Add(vector2[i]);                
            }
        }

        public Vertices(Vertices vertices) {
            for (int i = 0; i < vertices.Count; i++) {
                Add(vertices[i]);
            }
        }

        public static Vertices CreateRectangle(float width, float height) {
            Vertices vertices = new Vertices();
            vertices.Add(new Vector2(-width * .5f, -height * .5f));
            vertices.Add(new Vector2(-width * .5f, -height * .25f));
            vertices.Add(new Vector2(-width * .5f, 0));
            vertices.Add(new Vector2(-width * .5f, height * .25f));
            vertices.Add(new Vector2(-width * .5f, height * .5f));
            vertices.Add(new Vector2(-width * .25f, height * .5f));
            vertices.Add(new Vector2(0, height * .5f));
            vertices.Add(new Vector2(width * .25f, height * .5f));
            vertices.Add(new Vector2(width * .5f, height * .5f));
            vertices.Add(new Vector2(width * .5f, height * .25f));
            vertices.Add(new Vector2(width * .5f, 0));
            vertices.Add(new Vector2(width * .5f, -height * .25f));
            vertices.Add(new Vector2(width * .5f, -height * .5f));
            vertices.Add(new Vector2(width * .25f, -height * .5f));
            vertices.Add(new Vector2(0, -height * .5f));
            vertices.Add(new Vector2(-width * .25f, -height * .5f));
            return vertices;
        }

        public static Vertices CreateCircle(float radius, int edgeCount) {
            Vertices vertices = new Vertices();

            float stepSize = Mathematics.Calculator.PiX2 / (float)edgeCount;
            vertices.Add(new Vector2(radius, 0));
            for (int i = 1; i < edgeCount; i++) {
                vertices.Add(new Vector2(radius * Calculator.Cos(stepSize * i), -radius*Calculator.Sin(stepSize*i)));
            }
            return vertices;
        }
    }
}
