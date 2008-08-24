using System;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Collisions
{
    public sealed class Grid
    {
        private AABB _aabb;
        private float _gridCellSize;
        private float _gridCellSizeInv;
        private float[,] _nodes;

        public Vector2[] Points { get; private set; }

        public Grid Clone()
        {
            Grid grid = new Grid();
            grid._gridCellSize = _gridCellSize;
            grid._gridCellSizeInv = _gridCellSizeInv;
            grid._aabb = _aabb;
            grid._nodes = (float[,]) _nodes.Clone();
            grid.Points = (Vector2[]) Points.Clone();
            return grid;
        }

        public void ComputeGrid(Geom geometry, float gridCellSize)
        {
            //prepare the geometry.
            Matrix old = geometry.Matrix;
            Matrix identity = Matrix.Identity;
            geometry.Matrix = identity;

            _aabb = new AABB(geometry.AABB);
            _gridCellSize = gridCellSize;
            _gridCellSizeInv = 1/gridCellSize;

            int xSize = (int) Math.Ceiling(Convert.ToDouble((_aabb.Max.X - _aabb.Min.X)*_gridCellSizeInv)) + 1;
            int ySize = (int) Math.Ceiling(Convert.ToDouble((_aabb.Max.Y - _aabb.Min.Y)*_gridCellSizeInv)) + 1;

            _nodes = new float[xSize,ySize];
            Points = new Vector2[xSize*ySize];
            int i = 0;
            Vector2 vector = _aabb.Min;
            for (int x = 0; x < xSize; ++x, vector.X += gridCellSize)
            {
                vector.Y = _aabb.Min.Y;
                for (int y = 0; y < ySize; ++y, vector.Y += gridCellSize)
                {
                    _nodes[x, y] = geometry.GetNearestDistance(vector); // shape.GetDistance(vector);
                    Points[i] = vector;
                    i += 1;
                }
            }
            //restore the geometry
            geometry.Matrix = old;
        }

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
            //feature = null;
            return false;
        }
    }
}