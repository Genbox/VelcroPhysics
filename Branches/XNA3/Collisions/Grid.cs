using System;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Collisions
{
    public sealed class Grid
    {
        private AABB aabb;
        private float gridCellSize;
        private float gridCellSizeInv;
        private float[,] nodes;
        private Vector2[] points;

        public Vector2[] Points
        {
            get { return points; }
        }

        public Grid Clone()
        {
            Grid grid = new Grid();
            grid.gridCellSize = gridCellSize;
            grid.gridCellSizeInv = gridCellSizeInv;
            grid.aabb = aabb;
            grid.nodes = (float[,]) nodes.Clone();
            grid.points = (Vector2[]) points.Clone();
            return grid;
        }

        public void ComputeGrid(Geom geometry, float gridCellSize)
        {
            //prepare the geometry.
            Matrix old = geometry.Matrix;
            Matrix identity = Matrix.Identity;
            geometry.Matrix = identity;

            aabb = new AABB(geometry.AABB);
            this.gridCellSize = gridCellSize;
            gridCellSizeInv = 1/gridCellSize;

            int xSize = (int) Math.Ceiling(Convert.ToDouble((aabb.Max.X - aabb.Min.X)*gridCellSizeInv)) + 1;
            int ySize = (int) Math.Ceiling(Convert.ToDouble((aabb.Max.Y - aabb.Min.Y)*gridCellSizeInv)) + 1;

            nodes = new float[xSize,ySize];
            points = new Vector2[xSize*ySize];
            int i = 0;
            Vector2 vector = aabb.Min;
            for (int x = 0; x < xSize; ++x, vector.X += gridCellSize)
            {
                vector.Y = aabb.Min.Y;
                for (int y = 0; y < ySize; ++y, vector.Y += gridCellSize)
                {
                    nodes[x, y] = geometry.GetNearestDistance(vector); // shape.GetDistance(vector);
                    points[i] = vector;
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
            if (aabb.Contains(ref vector))
            {
                int x = (int) Math.Floor((vector.X - aabb.Min.X)*gridCellSizeInv);
                int y = (int) Math.Floor((vector.Y - aabb.Min.Y)*gridCellSizeInv);


                float xPercent = (vector.X - (gridCellSize*x + aabb.Min.X))*gridCellSizeInv;
                float yPercent = (vector.Y - (gridCellSize*y + aabb.Min.Y))*gridCellSizeInv;

                float bottomLeft = nodes[x, y];
                float bottomRight = nodes[x + 1, y];
                float topLeft = nodes[x, y + 1];
                float topRight = nodes[x + 1, y + 1];

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