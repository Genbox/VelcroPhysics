using System;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// Axis Aligned Bounding Box. Can be used to check for intersections with other AABBs.
    /// Use AABB.Intersect() to check for intersections.
    /// </summary>
    public class AABB
    {
        private const float _epsilon = .00001f;
        private Vector2 _vector;
        internal Vector2 max = Vector2.Zero;
        internal Vector2 min = Vector2.Zero;

        public AABB()
        {
        }

        public AABB(AABB aabb)
        {
            min = aabb.Min;
            max = aabb.Max;
        }

        public AABB(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }

        public AABB(Vertices vertices)
        {
            Update(ref vertices);
        }

        /// <summary>
        /// Gets the min.
        /// </summary>
        /// <Value>The min.</Value>
        public Vector2 Min
        {
            get { return min; }
        }

        /// <summary>
        /// Gets the max.
        /// </summary>
        /// <Value>The max.</Value>
        public Vector2 Max
        {
            get { return max; }
        }

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <Value>The width.</Value>
        public float Width
        {
            get { return max.X - min.X; }
        }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <Value>The height.</Value>
        public float Height
        {
            get { return max.Y - min.Y; }
        }

        /// <summary>
        /// Gets the vertices.
        /// </summary>
        /// <returns></returns>
        public Vertices GetVertices()
        {
            Vertices vertices = new Vertices();
            vertices.Add(Min);
            vertices.Add(new Vector2(Min.X, Max.Y));
            vertices.Add(Max);
            vertices.Add(new Vector2(Max.X, Min.Y));
            return vertices;
        }

        /// <summary>
        /// Gets the center.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCenter()
        {
            return new Vector2(Min.X + (Max.X - Min.X)/2, Min.Y + (Max.Y - Min.Y)/2);
        }

        /// <summary>
        /// Gets the shortest side.
        /// </summary>
        /// <returns></returns>
        public float GetShortestSide()
        {
            float width = max.X - min.X;
            float height = max.Y - min.Y;
            return Math.Min(width, height);
        }

        /// <summary>
        /// Updates the AABB with the specified vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        public void Update(ref Vertices vertices)
        {
            min = vertices[0];
            max = min;
            for (int i = 0; i < vertices.Count; i++)
            {
                _vector = vertices[i];
                if (_vector.X < min.X) min.X = _vector.X;
                if (_vector.X > max.X) max.X = _vector.X;
                if (_vector.Y < min.Y) min.Y = _vector.Y;
                if (_vector.Y > max.Y) max.Y = _vector.Y;
            }
        }

        /// <summary>
        /// Determines whether the AAABB contains the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        /// 	<c>true</c> if it contains the specified point; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Vector2 point)
        {
            //using _epsilon to try and gaurd against float rounding errors.
            if ((point.X > (min.X + _epsilon) && point.X < (max.X - _epsilon) &&
                 (point.Y > (min.Y + _epsilon) && point.Y < (max.Y - _epsilon))))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the AAABB contains the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        /// 	<c>true</c> if it contains the specified point; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(ref Vector2 point)
        {
            //using _epsilon to try and gaurd against float rounding errors.
            if ((point.X > (min.X + _epsilon) && point.X < (max.X - _epsilon) &&
                 (point.Y > (min.Y + _epsilon) && point.Y < (max.Y - _epsilon))))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if 2 AABB's intersects
        /// </summary>
        /// <param name="aabb1">The aabb1.</param>
        /// <param name="aabb2">The aabb2.</param>
        /// <returns></returns>
        public static bool Intersect(AABB aabb1, AABB aabb2)
        {
            if (aabb1.min.X > aabb2.max.X || aabb2.min.X > aabb1.max.X)
            {
                return false;
            }

            if (aabb1.min.Y > aabb2.Max.Y || aabb2.min.Y > aabb1.Max.Y)
            {
                return false;
            }
            return true;
        }
    }
}