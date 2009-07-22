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
    public struct AABB : IEquatable<AABB>
    {
        private const float _epsilon = .00001f;
        private Vector2 _vector;
        internal Vector2 max;
        internal Vector2 min;

        public AABB(ref AABB aabb)
        {
            min = aabb.Min;
            max = aabb.Max;
            _vector = Vector2.Zero;
        }

        public AABB(ref Vector2 min, ref Vector2 max)
        {
            this.min = min;
            this.max = max;
            _vector = Vector2.Zero;
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
        /// Gets or sets the position of the AABB
        /// </summary>
        /// <value>The position.</value>
        public Vector2 Position
        {
            get
            {
                return new Vector2((min.X + max.X) / 2f, (min.Y + max.Y) / 2f);
            }
            set
            {
                Vector2 newMin = new Vector2(value.X - (Width / 2f), value.Y - (Height / 2f));
                Vector2 newMax = new Vector2(value.X + (Width / 2f), value.Y + (Height / 2f));

                min = newMin;
                max = newMax;
            }
        }

        /// <summary>
        /// Gets the vertices of the AABB.
        /// </summary>
        /// <returns>The corners of the AABB</returns>
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
            return new Vector2(Min.X + (Max.X - Min.X) / 2, Min.Y + (Max.Y - Min.Y) / 2);
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
        /// Gets the distance to the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>The distance</returns>
        public float GetDistance(ref Vector2 point)
        {
            float result;
            GetDistance(ref point, out result);
            return result;
        }

        /// <summary>
        /// Gets the distance to the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="distance">The distance.</param>
        public void GetDistance(ref Vector2 point, out float distance)
        {
            float xDistance = Math.Abs(point.X - ((max.X + min.X) * .5f)) - (max.X - min.X) * .5f;
            float yDistance = Math.Abs(point.Y - ((max.Y + min.Y) * .5f)) - (max.Y - min.Y) * .5f;

            if (xDistance > 0 && yDistance > 0)
            {
                distance = (float)Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
            }
            else
            {
                distance = Math.Max(xDistance, yDistance);
            }
        }

        /// <summary>
        /// Updates the AABB with the specified vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        internal void Update(ref Vertices vertices)
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
            //Using epsilon to try and gaurd against float rounding errors.
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
            //using epsilon to try and gaurd against float rounding errors.
            if ((point.X > (min.X + _epsilon) && point.X < (max.X - _epsilon) &&
                 (point.Y > (min.Y + _epsilon) && point.Y < (max.Y - _epsilon))))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if 2 AABBs intersects
        /// </summary>
        /// <param name="aabb1">The first AABB.</param>
        /// <param name="aabb2">The second AABB</param>
        /// <returns></returns>
        public static bool Intersect(ref AABB aabb1, ref  AABB aabb2)
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

        #region IEquatable<AABB> Members

        public bool Equals(AABB other)
        {
            return ((min == other.min) && (max == other.max));
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj is AABB)
                return Equals((AABB)obj);

            return false;
        }

        public bool Equals(ref AABB other)
        {
            return ((min == other.min) && (max == other.max));
        }

        public override int GetHashCode()
        {
            return (Min.GetHashCode() + Max.GetHashCode());
        }

        public static bool operator ==(AABB a, AABB b)
        {
            return a.Equals(ref b);
        }

        public static bool operator !=(AABB a, AABB b)
        {
            return !a.Equals(ref b);
        }
    }
}