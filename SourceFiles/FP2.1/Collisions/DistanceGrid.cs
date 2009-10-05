using System;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Interfaces;

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
    public class DistanceGrid : INarrowPhaseCollider
    {
        private static DistanceGrid _instance;

        private DistanceGrid()
        {
        }

        public static DistanceGrid Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DistanceGrid();
                }
                return _instance;
            }
        }

        private Dictionary<int, DistanceGridData> _distanceGrids = new Dictionary<int, DistanceGridData>();

        /// <summary>
        ///used to calculate a cell size from the AABB whenever the collisionGridCellSize
        ///is not set explicitly. The more sharp corners a body has, the smaller this Value will 
        ///need to be. 
        /// </summary>
        private const float _gridCellSizeAABBFactor = .1f;

        /// <summary>
        /// Finds the contactpoints between the two geometries.
        /// </summary>
        /// <param name="geomA">The first geom.</param>
        /// <param name="geomB">The second geom.</param>
        /// <param name="contactList">The contact list.</param>
        public void Collide(Geom geomA, Geom geomB, ContactList contactList)
        {
            int vertexIndex = -1;

            //Lookup distancegrid A data from list
            DistanceGridData geomAGridData = _distanceGrids[geomA.id];

            //Iterate the second geometry vertices
            for (int i = 0; i < geomB.worldVertices.Count; i++)
            {
                if (contactList.Count == PhysicsSimulator.MaxContactsToDetect)
                    break;

                vertexIndex += 1;
                _vertRef = geomB.WorldVertices[i];
                geomA.TransformToLocalCoordinates(ref _vertRef, out _localVertex);

                //The geometry intersects when distance <= 0
                //Continue in the list if the current vector does not intersect
                if (!geomAGridData.Intersect(ref _localVertex, out _feature))
                    continue;

                //If the geometries collide, create a new contact and add it to the contact list.
                if (_feature.Distance < 0f)
                {
                    geomA.TransformNormalToWorld(ref _feature.Normal, out _feature.Normal);

                    Contact contact = new Contact(geomB.WorldVertices[i], _feature.Normal, _feature.Distance, new ContactId(geomB.id, vertexIndex, geomA.id));
                    contactList.Add(contact);
                }
            }

            //Lookup distancegrid B data from list
            DistanceGridData geomBGridData = _distanceGrids[geomB.id];

            //Iterate the first geometry vertices
            for (int i = 0; i < geomA.WorldVertices.Count; i++)
            {
                if (contactList.Count == PhysicsSimulator.MaxContactsToDetect)
                    break;

                vertexIndex += 1;
                _vertRef = geomA.WorldVertices[i];
                geomB.TransformToLocalCoordinates(ref _vertRef, out _localVertex);

                if (!geomBGridData.Intersect(ref _localVertex, out _feature))
                    continue;

                if (_feature.Distance < 0f)
                {
                    geomB.TransformNormalToWorld(ref _feature.Normal, out _feature.Normal);
                    _feature.Normal = -_feature.Normal;

                    Contact contact = new Contact( geomA.WorldVertices[i], _feature.Normal, _feature.Distance, new ContactId(geomA.id, vertexIndex, geomB.id));
                    contactList.Add(contact);
                }
            }
        }

        /// <summary>
        /// Computes the grid.
        /// </summary>
        /// <param name="geom">The geometry.</param>
        /// <exception cref="ArgumentNullException"><c>geometry</c> is null.</exception>
        public void CreateDistanceGrid(Geom geom)
        {
            if (geom == null)
                throw new ArgumentNullException("geom", "Geometry can't be null");

            //Don't create distancegrid for geometry that already have one.
            //NOTE: This should be used to -update- the geometry's distance grid (if grid cell size has changed).
            if (_distanceGrids.ContainsKey(geom.id))
                return;

            //By default, calculate the gridcellsize from the AABB
            if (geom.GridCellSize <= 0)
                geom.GridCellSize = CalculateGridCellSizeFromAABB(ref geom.AABB);

            //Prepare the geometry. Reset the geometry matrix
            Matrix old = geom.Matrix;
            geom.Matrix = Matrix.Identity;

            //Create data needed for gridcalculations
            AABB aabb = new AABB(ref geom.AABB);
            float gridCellSizeInv = 1 / geom.GridCellSize;

            //Note: Physics2d have +2
            int xSize = (int)Math.Ceiling((double)(aabb.Max.X - aabb.Min.X) * gridCellSizeInv) + 1;
            int ySize = (int)Math.Ceiling((double)(aabb.Max.Y - aabb.Min.Y) * gridCellSizeInv) + 1;

            float[,] nodes = new float[xSize, ySize];
            Vector2 vector = aabb.Min;
            for (int x = 0; x < xSize; ++x, vector.X += geom.GridCellSize)
            {
                vector.Y = aabb.Min.Y;
                for (int y = 0; y < ySize; ++y, vector.Y += geom.GridCellSize)
                {
                    nodes[x, y] = geom.GetNearestDistance(ref vector); // shape.GetDistance(vector);
                }
            }
            //restore the geometry
            geom.Matrix = old;

            DistanceGridData distanceGridData = new DistanceGridData();
            distanceGridData.AABB = aabb;
            distanceGridData.GridCellSize = geom.GridCellSize;
            distanceGridData.GridCellSizeInv = gridCellSizeInv;
            distanceGridData.Nodes = nodes;

            _distanceGrids.Add(geom.id, distanceGridData);
        }

        /// <summary>
        /// Removes a distance grid from the cache.
        /// </summary>
        /// <param name="geom">The geom.</param>
        public void RemoveDistanceGrid(Geom geom)
        {
            _distanceGrids.Remove(geom.id);
        }

        /// <summary>
        /// Copies the distance grid from one id to another. This is used in cloning of geometries.
        /// </summary>
        /// <param name="fromId">From id.</param>
        /// <param name="toId">To id.</param>
        public void Copy(int fromId, int toId)
        {
            _distanceGrids.Add(toId, _distanceGrids[fromId]);
        }

        /// <summary>
        /// Calculates the grid cell size from AABB.
        /// </summary>
        /// <param name="aabb">The AABB.</param>
        /// <returns></returns>
        private float CalculateGridCellSizeFromAABB(ref AABB aabb)
        {
            return aabb.GetShortestSide() * _gridCellSizeAABBFactor;
        }

        /// <summary>
        /// Checks if the specified geom intersect the specified point.
        /// </summary>
        /// <param name="geom">The geom.</param>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public bool Intersect(Geom geom, ref Vector2 point)
        {
            //Lookup the geometry's distancegrid
            DistanceGridData gridData = _distanceGrids[geom.id];

            Feature feature;
            return gridData.Intersect(ref point, out feature);
        }

        #region Collide variables

        private Feature _feature;
        private Vector2 _localVertex;
        private Vector2 _vertRef;

        #endregion
    }

    /// <summary>
    /// Class that holds the distancegrid data
    /// </summary>
    public struct DistanceGridData
    {
        public AABB AABB;
        public float GridCellSize;
        public float GridCellSizeInv;
        public float[,] Nodes;

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
            if (AABB.Contains(ref vector))
            {
                int x = (int)Math.Floor((vector.X - AABB.Min.X) * GridCellSizeInv);
                int y = (int)Math.Floor((vector.Y - AABB.Min.Y) * GridCellSizeInv);

                float bottomLeft = Nodes[x, y];
                float bottomRight = Nodes[x + 1, y];
                float topLeft = Nodes[x, y + 1];
                float topRight = Nodes[x + 1, y + 1];

                if (bottomLeft <= 0 ||
                    bottomRight <= 0 ||
                    topLeft <= 0 ||
                    topRight <= 0)
                {
                    float xPercent = (vector.X - (GridCellSize * x + AABB.Min.X)) * GridCellSizeInv;
                    float yPercent = (vector.Y - (GridCellSize * y + AABB.Min.Y)) * GridCellSizeInv;

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

                        //Uncommented by Daniel Pramel 08/17/08
                        //make sure the normal is not zero length.
                        if (normal.X != 0 || normal.Y != 0)
                        {
                            Vector2.Normalize(ref normal, out normal);
                            feature = new Feature(ref vector, ref normal, distance);
                            return true;
                        }
                    }
                }
            }

            feature = new Feature();
            return false;
        }
    }
}