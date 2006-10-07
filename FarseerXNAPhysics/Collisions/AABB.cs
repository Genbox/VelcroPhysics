using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Collisions {
    public class AABB {
        private Vector2 _min = Vector2.Zero;
        private Vector2 _max = Vector2.Zero;

        public Vector2 Min {
            get { return _min; }
        }

        public Vector2 Max {
            get { return _max; }
        }

        public float Width {
            get { return _max.X - _min.X; }
        }

        public float Height {
            get { return _max.Y - _min.Y; }
        }

        public AABB() { }

        public AABB(Vector2 min, Vector2 max) {
            _min = min;
            _max = max;
        }

        public void Update(Vertices vertices) {
            _min = vertices[0];
            _max = _min;
            for (int i = 0; i < vertices.Count; i++) {
                if (vertices[i].X < _min.X) _min.X = vertices[i].X;
                if (vertices[i].X > _max.X) _max.X = vertices[i].X;
                if (vertices[i].Y < _min.Y) _min.Y = vertices[i].Y;
                if (vertices[i].Y > _max.Y) _max.Y = vertices[i].Y;
            }
        }

        public static bool Intersect(AABB aabb1, AABB aabb2) {
            if (aabb1.Min.X > aabb2.Max.X || aabb2.Min.X > aabb1.Max.X) {
                return false;
            }
            else if (aabb1.Min.Y > aabb2.Max.Y || aabb2.Min.Y > aabb1.Max.Y) {
                return false;
            }
            return true;
        }
    }
}
