using System;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// Grid is used to test for intersection.
    /// Computation of the grid may take a long time, depending on the grid cell size provided.
    /// </summary>
    public sealed class Grid
    {
        private AABB _aabb;
        private float _gridCellSize;
        private float _gridCellSizeInv;
        private float[,] _nodes;
        private Vector2[] _points;

        /// <summary>
        /// Gets the points.
        /// </summary>
        /// <Value>The points.</Value>
        public Vector2[] Points
        {
            get { return _points; }
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public Grid Clone()
        {
            Grid grid = new Grid();
            grid._gridCellSize = _gridCellSize;
            grid._gridCellSizeInv = _gridCellSizeInv;
            grid._aabb = _aabb;
            grid._nodes = (float[,]) _nodes.Clone();
            grid._points = (Vector2[]) _points.Clone();
            return grid;
        }

        /// <summary>
        /// Computes the grid.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="gridCellSize">Size of the grid cell.</param>
        public void ComputeGrid(Geom geometry, float gridCellSize)
        {
            //Prepare the geometry.
            Matrix old = geometry.Matrix;

            //TODO: Assign geometry.Matrix to Matrix.Identity directly
            Matrix identity = Matrix.Identity;
            geometry.Matrix = identity;

            //Copy the AABB to the grid field
            _aabb = new AABB(geometry.AABB);
            _gridCellSize = gridCellSize;
            _gridCellSizeInv = 1/gridCellSize;

            //NOTE: Using double cast instead of converting.
            int xSize = (int) Math.Ceiling((double) (_aabb.Max.X - _aabb.Min.X)*_gridCellSizeInv) + 1;
            int ySize = (int) Math.Ceiling((double) (_aabb.Max.Y - _aabb.Min.Y)*_gridCellSizeInv) + 1;

            //TODO: Possible optimization (normal)! If the shape is symmetric in X and Y axis, don't calculate the points, replicate them.
            _nodes = new float[xSize,ySize];
            _points = new Vector2[xSize*ySize];
            int i = 0;
            Vector2 vector = _aabb.Min;
            for (int x = 0; x < xSize; ++x, vector.X += gridCellSize)
            {
                vector.Y = _aabb.Min.Y;
                for (int y = 0; y < ySize; ++y, vector.Y += gridCellSize)
                {
                    _nodes[x, y] = geometry.GetNearestDistance(vector); // shape.GetDistance(vector);
                    _points[i] = vector;
                    i += 1;
                }
            }
            //restore the geometry
            geometry.Matrix = old;
        }

        /// <summary>
        /// Checks if the grid intersects with the specified vector.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <param name="feature">The feature.</param>
        /// <returns></returns>
        public bool Intersect(ref Vector2 vector, out Feature feature)
        {
            //TODO: Keep and eye out for floating point accuracy issues here. Possibly some
            //VERY intermittent errors exist?
            if (_aabb.Contains(ref vector))
            {
                int x = (int) Math.Floor((vector.X - _aabb.Min.X)*_gridCellSizeInv);
                int y = (int) Math.Floor((vector.Y - _aabb.Min.Y)*_gridCellSizeInv);

                float xPercent = (vector.X - (_gridCellSize*x + _aabb.Min.X))*_gridCellSizeInv;
                float yPercent = (vector.Y - (_gridCellSize*y + _aabb.Min.Y))*_gridCellSizeInv;

                float bottomLeft = _nodes[x, y];
                float bottomRight = _nodes[x + 1, y];
                float topLeft = _nodes[x, y + 1];
                float topRight = _nodes[x + 1, y + 1];

                if (bottomLeft <= 0 ||
                    bottomRight <= 0 ||
                    topLeft <= 0 ||
                    topRight <= 0)
                {
                    float top = MathHelper.Lerp(topLeft, topRight, xPercent);
                    float bottom = MathHelper.Lerp(bottomLeft, bottomRight, xPercent);
                    float distance = MathHelper.Lerp(bottom, top, yPercent);

                    if (distance <= 0)
                    {
                        float right = MathHelper.Lerp(bottomRight, topRight, yPercent);
                        float left = MathHelper.Lerp(bottomLeft, topLeft, yPercent);

                        Vector2 normal = Vector2.Zero;
                        normal.X = right - left;
                        normal.Y = top - bottom;
                        //make sure the normal is not zero length.

                        #region Uncommented by Daniel Pramel 08/17/08

                        if (normal.X != 0 || normal.Y != 0)
                        {
                            Vector2.Normalize(ref normal, out normal);
                            feature = new Feature(vector, normal, distance);
                            return true;
                        }

                        #endregion
                    }
                }
            }
            feature = new Feature();
            return false;
        }
    }
}