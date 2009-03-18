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
    /// TODO: Write
    /// Grid is used to test for intersection.
    /// Computation of the grid may take a long time, depending on the grid cell size provided.
    /// </summary>
    public class DistanceGrid : INarrowPhaseCollider
    {
        private Dictionary<int, DistanceGridData> _distanceGrids = new Dictionary<int, DistanceGridData>();

        /// <summary>
        ///used to calculate a cell size from the AABB whenever the collisionGridCellSize
        ///is not set explicitly. The more sharp corners a body has, the smaller this Value will 
        ///need to be. 
        /// </summary>
        private const float _gridCellSizeAABBFactor = .1f;
        private PhysicsSimulator _physicsSimulator;

        public DistanceGrid(PhysicsSimulator physicsSimulator)
        {
            _physicsSimulator = physicsSimulator;
        }

        public void Collide(Geom geomA, Geom geomB, ContactList contactList)
        {
            int vertexIndex = -1;

            //Lookup distancegrid data from list
            DistanceGridData geomAGridData = _distanceGrids[geomA.Id];

            //Iterate the second geometry vertices
            for (int i = 0; i < geomB.worldVertices.Count; i++)
            {
                if (contactList.Count == _physicsSimulator.maxContactsToDetect)
                    break;

                //Can be null for "one-way" collision (points)
                //if (geometry1.narrowPhaseCollider == null)
                //    break;

                vertexIndex += 1;
                _vertRef = geomB.WorldVertices[i];
                geomA.TransformToLocalCoordinates(ref _vertRef, out _localVertex);

                //The geometry intersects when distance <= 0
                //Continue in the list if the current vector does not intersect
                if (!geomAGridData.Intersect(ref _localVertex, out _feature))
                    continue;

                //If the second geometry's current vector intersects with the first geometry
                //create a new contact and add it to the contact list.
                if (_feature.Distance < 0f)
                {
                    geomA.TransformNormalToWorld(ref _feature.Normal, out _feature.Normal);
                    Contact contact = new Contact(geomB.WorldVertices[i], _feature.Normal, _feature.Distance,
                                                  new ContactId(1, vertexIndex, 2));
                    contactList.Add(contact);
                }
            }

            //Lookup distancegrid data from list
            DistanceGridData geomBGridData = _distanceGrids[geomA.Id];

            //Iterate the first geometry vertices
            for (int i = 0; i < geomA.WorldVertices.Count; i++)
            {
                if (contactList.Count == _physicsSimulator.maxContactsToDetect)
                    break;

                //Can be null for "one-way" collision (points)
                //if (geomB.narrowPhaseCollider == null)
                //    break;

                vertexIndex += 1;
                _vertRef = geomA.WorldVertices[i];
                geomB.TransformToLocalCoordinates(ref _vertRef, out _localVertex);

                if (!geomBGridData.Intersect(ref _localVertex, out _feature))
                    continue;

                if (_feature.Distance < 0f)
                {
                    geomB.TransformNormalToWorld(ref _feature.Normal, out _feature.Normal);
                    _feature.Normal = -_feature.Normal;
                    Contact contact = new Contact(geomA.WorldVertices[i], _feature.Normal, _feature.Distance,
                                                  new ContactId(2, vertexIndex, 1));
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
            if (_distanceGrids.ContainsKey(geom.Id))
                return;

            //By default, calculate the gridcellsize from the AABB
            float gridCellSize = CalculateGridCellSizeFromAABB(geom.AABB);

            //Prepare the geometry. Reset the geometry matrix
            Matrix old = geom.Matrix;
            geom.Matrix = Matrix.Identity;

            //Create data needed for gridcalculations
            AABB aabb = new AABB(geom.AABB);
            float gridCellSizeInv = 1 / gridCellSize;

            //Note: Physics2d have +2
            int xSize = (int)Math.Ceiling((double)(aabb.Max.X - aabb.Min.X) * gridCellSizeInv) + 1;
            int ySize = (int)Math.Ceiling((double)(aabb.Max.Y - aabb.Min.Y) * gridCellSizeInv) + 1;

            float[,] nodes = new float[xSize, ySize];
            Vector2 vector = aabb.Min;
            for (int x = 0; x < xSize; ++x, vector.X += gridCellSize)
            {
                vector.Y = aabb.Min.Y;
                for (int y = 0; y < ySize; ++y, vector.Y += gridCellSize)
                {
                    nodes[x, y] = geom.GetNearestDistance(vector); // shape.GetDistance(vector);
                }
            }
            //restore the geometry
            geom.Matrix = old;

            DistanceGridData distanceGridData = new DistanceGridData();
            distanceGridData.AABB = aabb;
            distanceGridData.GridCellSize = gridCellSize;
            distanceGridData.GridCellSizeInv = gridCellSizeInv;
            distanceGridData.Nodes = nodes;

            _distanceGrids.Add(geom.Id, distanceGridData);
        }

        public void RemoveDistanceGrid(Geom geom)
        {
            _distanceGrids.Remove(geom.Id);
        }

        /// <summary>
        /// Calculates the grid cell size from AABB.
        /// </summary>
        /// <param name="aabb">The AABB.</param>
        /// <returns></returns>
        private float CalculateGridCellSizeFromAABB(AABB aabb)
        {
            return aabb.GetShortestSide() * _gridCellSizeAABBFactor;
        }

        #region Collide variables

        private Feature _feature;
        private Vector2 _localVertex;
        private Vector2 _vertRef;

        #endregion
    }

    public sealed class DistanceGridData
    {
        public AABB AABB;
        public float GridCellSize;
        public float GridCellSizeInv;
        public float[,] Nodes;

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public DistanceGridData Clone()
        {
            DistanceGridData grid = new DistanceGridData();
            grid.GridCellSize = GridCellSize;
            grid.GridCellSizeInv = GridCellSizeInv;
            grid.AABB = AABB;
            grid.Nodes = (float[,])Nodes.Clone();
            return grid;
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
                            feature = new Feature(vector, normal, distance);
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