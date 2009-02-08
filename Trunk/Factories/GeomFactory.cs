using System;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Factories
{
    /// <summary>
    /// An easy to use factory for creating geoms
    /// </summary>
    public class GeomFactory
    {
        private static GeomFactory _instance;

        /// <summary>
        ///used to calculate a cell size from the AABB whenever the collisionGridCellSize
        ///is not set explicitly. The more sharp corners a body has, the smaller this Value will 
        ///need to be. 
        /// </summary>
        public float GridCellSizeAABBFactor = .1f;

        private GeomFactory()
        {
        }

        public static GeomFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GeomFactory();
                }
                return _instance;
            }
        }

        //rectangles
        public Geom CreateRectangleGeom(PhysicsSimulator physicsSimulator, Body body, float width, float height)
        {
            return CreateRectangleGeom(physicsSimulator, body, width, height, Vector2.Zero, 0, new ColliderData());
        }

        public Geom CreateRectangleGeom(Body body, float width, float height)
        {
            return CreateRectangleGeom(body, width, height, Vector2.Zero, 0, new ColliderData());
        }

        public Geom CreateRectangleGeom(PhysicsSimulator physicsSimulator, Body body, float width, float height,
                                        Vector2 offset, float rotationOffset)
        {
            return CreateRectangleGeom(physicsSimulator, body, width, height, offset, rotationOffset, new ColliderData());
        }

        public Geom CreateRectangleGeom(Body body, float width, float height, Vector2 offset, float rotationOffset)
        {
            return CreateRectangleGeom(body, width, height, offset, rotationOffset, new ColliderData());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="physicsSimulator"></param>
        /// <param name="body"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="collisionGridCellSize">Pass in 0 or less to make Farseer calculate the grid cell size</param>
        /// <returns></returns>
        public Geom CreateRectangleGeom(PhysicsSimulator physicsSimulator, Body body, float width, float height,
                                        float collisionGridCellSize)
        {
            return CreateRectangleGeom(physicsSimulator, body, width, height, Vector2.Zero, 0, new ColliderData());
        }

        /// <summary>
        /// Creates the rectangle geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Geom CreateRectangleGeom(Body body, float width, float height, ColliderData data)
        {
            return CreateRectangleGeom(body, width, height, Vector2.Zero, 0, data);
        }

        /// <summary>
        /// Creates the rectangle geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Geom CreateRectangleGeom(PhysicsSimulator physicsSimulator, Body body, float width, float height,
                                        Vector2 offset, float rotationOffset, ColliderData data)
        {
            Geom geometry = CreateRectangleGeom(body, width, height, offset, rotationOffset, data);
            physicsSimulator.Add(geometry);
            return geometry;
        }

        /// <summary>
        /// Creates the rectangle geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Geom CreateRectangleGeom(Body body, float width, float height, Vector2 offset, float rotationOffset,
                                        ColliderData data)
        {
            Vertices vertices = Vertices.CreateRectangle(width, height);

            if (data.GridCellSize <= 0)
            {
                data.GridCellSize = CalculateGridCellSizeFromAABB(vertices);
            }

            Geom geometry = new Geom(body, vertices, offset, rotationOffset, data);
            return geometry;
        }

        //circles
        public Geom CreateCircleGeom(PhysicsSimulator physicsSimulator, Body body, float radius, int numberOfEdges)
        {
            return CreateCircleGeom(physicsSimulator, body, radius, numberOfEdges, Vector2.Zero, 0, new ColliderData());
        }

        public Geom CreateCircleGeom(Body body, float radius, int numberOfEdges)
        {
            return CreateCircleGeom(body, radius, numberOfEdges, Vector2.Zero, 0, new ColliderData());
        }

        public Geom CreateCircleGeom(PhysicsSimulator physicsSimulator, Body body, float radius, int numberOfEdges,
                                     Vector2 offset, float rotationOffset)
        {
            return CreateCircleGeom(physicsSimulator, body, radius, numberOfEdges, offset, rotationOffset, new ColliderData());
        }

        public Geom CreateCircleGeom(Body body, float radius, int numberOfEdges, Vector2 offset, float rotationOffset)
        {
            return CreateCircleGeom(body, radius, numberOfEdges, offset, rotationOffset, new ColliderData());
        }

        public Geom CreateCircleGeom(PhysicsSimulator physicsSimulator, Body body, float radius, int numberOfEdges,
                                     ColliderData data)
        {
            return CreateCircleGeom(physicsSimulator, body, radius, numberOfEdges, Vector2.Zero, 0,
                                    data);
        }

        /// <summary>
        /// Creates the circle geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Geom CreateCircleGeom(Body body, float radius, int numberOfEdges, ColliderData data)
        {
            return CreateCircleGeom(body, radius, numberOfEdges, Vector2.Zero, 0, data);
        }

        /// <summary>
        /// Creates the circle geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Geom CreateCircleGeom(PhysicsSimulator physicsSimulator, Body body, float radius, int numberOfEdges,
                                     Vector2 offset, float rotationOffset, ColliderData data)
        {
            Geom geometry = CreateCircleGeom(body, radius, numberOfEdges, offset, rotationOffset, data);
            physicsSimulator.Add(geometry);
            return geometry;
        }

        /// <summary>
        /// Creates the circle geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Geom CreateCircleGeom(Body body, float radius, int numberOfEdges, Vector2 offset, float rotationOffset,
                                     ColliderData data)
        {
            Vertices vertices = Vertices.CreateCircle(radius, numberOfEdges);
            if (data.GridCellSize <= 0)
            {
                data.GridCellSize = CalculateGridCellSizeFromAABB(vertices);
            }

            Geom geometry = new Geom(body, vertices, offset, rotationOffset, data);
            return geometry;
        }

        //polygons
        /// <summary>
        /// Creates the polygon geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="vertices">The vertices.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Geom CreatePolygonGeom(PhysicsSimulator physicsSimulator, Body body, Vertices vertices,
                                      ColliderData data)
        {
            return CreatePolygonGeom(physicsSimulator, body, vertices, Vector2.Zero, 0, data);
        }

        /// <summary>
        /// Creates the polygon geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="vertices">The vertices.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Geom CreatePolygonGeom(Body body, Vertices vertices, ColliderData data)
        {
            return CreatePolygonGeom(body, vertices, Vector2.Zero, 0, data);
        }

        public Geom CreatePolygonGeom(PhysicsSimulator physicsSimulator, Body body, Vertices verts)
        {
            return CreatePolygonGeom(physicsSimulator, body, verts, new ColliderData());
        }

        /// <summary>
        /// Creates the polygon geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="vertices">The vertices.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Geom CreatePolygonGeom(PhysicsSimulator physicsSimulator, Body body, Vertices vertices, Vector2 offset,
                                      float rotationOffset, ColliderData data)
        {
            Geom geometry = CreatePolygonGeom(body, vertices, offset, rotationOffset, data);
            physicsSimulator.Add(geometry);
            return geometry;
        }

        /// <summary>
        /// Creates the polygon geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="vertices">The vertices.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Geom CreatePolygonGeom(Body body, Vertices vertices, Vector2 offset, float rotationOffset,
                                      ColliderData data)
        {
            if (body == null)
                throw new ArgumentNullException("body", "Body must not be null");

            if (vertices == null)
                throw new ArgumentNullException("vertices", "Vertices must not be null");

            //adjust the verts to be relative to 0,0
            Vector2 centroid = vertices.GetCentroid();
            vertices.Translate(-centroid);

            if (data.GridCellSize <= 0)
            {
                data.GridCellSize = CalculateGridCellSizeFromAABB(vertices);
            }

            Geom geometry = new Geom(body, vertices, offset, rotationOffset, data);
            return geometry;
        }

        /// <summary>
        /// Creates a clone of a geometry.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="geometry">The geometry.</param>
        /// <returns></returns>
        public Geom CreateGeom(PhysicsSimulator physicsSimulator, Body body, Geom geometry)
        {
            Geom geometryClone = CreateGeom(body, geometry);
            physicsSimulator.Add(geometryClone);
            return geometryClone;
        }

        /// <summary>
        /// Creates a clone of a geometry.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="geometry">The geometry.</param>
        /// <returns></returns>
        public Geom CreateGeom(Body body, Geom geometry)
        {
            Geom geometryClone = new Geom(body, geometry);
            return geometryClone;
        }

        /// <summary>
        /// Creates a clone of a geometry.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="geometry">The geometry.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <returns></returns>
        public Geom CreateGeom(PhysicsSimulator physicsSimulator, Body body, Geom geometry, Vector2 offset,
                               float rotationOffset)
        {
            Geom geometryClone = CreateGeom(body, geometry, offset, rotationOffset);
            physicsSimulator.Add(geometryClone);
            return geometryClone;
        }

        /// <summary>
        /// Creates a clone of a geometry.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="geometry">The geometry.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <returns></returns>
        public Geom CreateGeom(Body body, Geom geometry, Vector2 offset, float rotationOffset)
        {
            Geom geometryClone = new Geom(body, geometry, offset, rotationOffset);
            return geometryClone;
        }

        //ellipses
        /// <summary>
        /// Creates a ellipse geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="xRadius">The x radius.</param>
        /// <param name="yRadius">The y radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <returns></returns>
        public Geom CreateEllipseGeom(PhysicsSimulator physicsSimulator, Body body, float xRadius, float yRadius,
                                      int numberOfEdges)
        {
            return CreateEllipseGeom(physicsSimulator, body, xRadius, yRadius, numberOfEdges, Vector2.Zero, 0, new ColliderData());
        }

        /// <summary>
        /// Creates a ellipse geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="xRadius">The x radius.</param>
        /// <param name="yRadius">The y radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <returns></returns>
        public Geom CreateEllipseGeom(Body body, float xRadius, float yRadius, int numberOfEdges)
        {
            return CreateEllipseGeom(body, xRadius, yRadius, numberOfEdges, Vector2.Zero, 0, new ColliderData());
        }

        /// <summary>
        /// Creates a ellipse geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="xRadius">The x radius.</param>
        /// <param name="yRadius">The y radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <returns></returns>
        public Geom CreateEllipseGeom(PhysicsSimulator physicsSimulator, Body body, float xRadius, float yRadius,
                                      int numberOfEdges,
                                      Vector2 offset, float rotationOffset)
        {
            return CreateEllipseGeom(physicsSimulator, body, xRadius, yRadius, numberOfEdges, offset, rotationOffset, new ColliderData());
        }

        /// <summary>
        /// Creates a ellipse geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="xRadius">The x radius.</param>
        /// <param name="yRadius">The y radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <returns></returns>
        public Geom CreateEllipseGeom(Body body, float xRadius, float yRadius, int numberOfEdges, Vector2 offset,
                                      float rotationOffset)
        {
            return CreateEllipseGeom(body, xRadius, yRadius, numberOfEdges, offset, rotationOffset, new ColliderData());
        }

        /// <summary>
        /// Creates a ellipse geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="xRadius">The x radius.</param>
        /// <param name="yRadius">The y radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Geom CreateEllipseGeom(PhysicsSimulator physicsSimulator, Body body, float xRadius, float yRadius,
                                      int numberOfEdges,
                                      ColliderData data)
        {
            return CreateEllipseGeom(physicsSimulator, body, xRadius, yRadius, numberOfEdges, Vector2.Zero, 0,
                                     data);
        }

        /// <summary>
        /// Creates an ellipse geometry
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="xRadius">The x radius.</param>
        /// <param name="yRadius">The y radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Geom CreateEllipseGeom(Body body, float xRadius, float yRadius, int numberOfEdges,
                                      ColliderData data)
        {
            return CreateEllipseGeom(body, xRadius, yRadius, numberOfEdges, Vector2.Zero, 0, data);
        }

        /// <summary>
        /// Creates the ellipse geometry.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="xRadius">The x radius.</param>
        /// <param name="yRadius">The y radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Geom CreateEllipseGeom(PhysicsSimulator physicsSimulator, Body body, float xRadius, float yRadius,
                                      int numberOfEdges,
                                      Vector2 offset, float rotationOffset, ColliderData data)
        {
            Geom geometry = CreateEllipseGeom(body, xRadius, yRadius, numberOfEdges, offset, rotationOffset,
                                              data);
            physicsSimulator.Add(geometry);
            return geometry;
        }

        /// <summary>
        /// Creates the ellipse geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="xRadius">The x radius.</param>
        /// <param name="yRadius">The y radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Geom CreateEllipseGeom(Body body, float xRadius, float yRadius, int numberOfEdges, Vector2 offset,
                                      float rotationOffset,
                                      ColliderData data)
        {
            Vertices vertices = Vertices.CreateEllipse(xRadius, yRadius, numberOfEdges);
            if (data.GridCellSize <= 0)
            {
                data.GridCellSize = CalculateGridCellSizeFromAABB(vertices);
            }

            Geom geometry = new Geom(body, vertices, offset, rotationOffset, data);
            return geometry;
        }

        //misc
        /// <summary>
        /// Calculates the grid cell size from AABB.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <returns></returns>
        public float CalculateGridCellSizeFromAABB(Vertices vertices)
        {
            AABB aabb = new AABB(vertices);
            return aabb.GetShortestSide() * GridCellSizeAABBFactor;
        }
    }
}