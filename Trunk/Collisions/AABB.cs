using System;
using System.Collections.Generic;
using System.Text;

#if (XNA)
using Microsoft.Xna.Framework; 
#endif

using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Collisions {
    public class AABB {
        private float epsilon = .00001f;
        internal Vector2 min = Vector2.Zero;
        internal Vector2 max = Vector2.Zero;

        public AABB() { }

        public AABB(AABB aabb) {
            min = aabb.Min;
            max = aabb.Max;
        }

        public AABB(Vector2 min, Vector2 max) {
            this.min = min;
            this.max = max;
        }

        public AABB(Vertices vertices) {
            Update(ref vertices);
        }

        public Vector2 Min {
            get { return min; }
        }

        public Vector2 Max {
            get { return max; }
        }

        public float Width {
            get { return max.X - min.X; }
        }

        public float Height {
            get { return max.Y - min.Y; }
        }

        public Vertices GetVertices() {
            Vertices vertices = new Vertices();
            vertices.Add(Min);
            vertices.Add(new Vector2(Min.X, Max.Y));
            vertices.Add(Max);
            vertices.Add(new Vector2(Max.X, Min.Y));
            return vertices;
        }

        public Vector2 GetCenter() {
            return new Vector2(Min.X + (Max.X - Min.X) / 2, Min.Y + (Max.Y - Min.Y) / 2);
        }

        public float GetShortestSide() {
            float width = max.X - min.X;
            float height = max.Y - min.Y;
            return Math.Min(width, height);
        }

        Vector2 vertice;
        public void Update(ref Vertices vertices) {
            min = vertices[0];
            max = min;
            for (int i = 0; i < vertices.Count; i++) {
                vertice = vertices[i]; 
                if (vertice.X < min.X) min.X = vertice.X;
                if (vertice.X > max.X) max.X = vertice.X;
                if (vertice.Y < min.Y) min.Y = vertice.Y;
                if (vertice.Y > max.Y) max.Y = vertice.Y;
            }
        }

        public bool Contains(Vector2 point) {
            //using epsilon to try and gaurd against float rounding errors.
            if ((point.X > (min.X + epsilon) && point.X < (max.X - epsilon) && (point.Y > (min.Y + epsilon) && point.Y < (max.Y - epsilon)))) {
                return true;
            }
            else {
                return false;
            }
        }

        public bool Contains(ref Vector2 point) {
            //using epsilon to try and gaurd against float rounding errors.
            if ((point.X > (min.X + epsilon) && point.X < (max.X - epsilon) && (point.Y > (min.Y + epsilon) && point.Y < (max.Y - epsilon)))) {
                return true;
            }
            else {
                return false;
            }
        }

        public static bool Intersect(AABB aabb1, AABB aabb2) {
            if (aabb1.min.X > aabb2.max.X || aabb2.min.X > aabb1.max.X) {
                return false;
            }
            else if (aabb1.min.Y > aabb2.Max.Y || aabb2.min.Y > aabb1.Max.Y) {
                return false;
            }
            return true;
        }
    }
}
