using System;
using System.Collections.Generic;
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

        #region Rectangles
        /// <summary>
        /// Creates a rectangle geometry.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The geometry</returns>
        public Geom CreateRectangleGeom(PhysicsSimulator physicsSimulator, Body body, float width, float height)
        {
            return CreateRectangleGeom(physicsSimulator, body, width, height, Vector2.Zero, 0, 0);
        }

        /// <summary>
        /// Creates a rectangle geometry.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The geometry</returns>
        public Geom CreateRectangleGeom(Body body, float width, float height)
        {
            return CreateRectangleGeom(body, width, height, Vector2.Zero, 0, 0);
        }

        /// <summary>
        /// Creates a rectangle geometry.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="positionOffset">The position offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <returns>The geometry</returns>
        public Geom CreateRectangleGeom(PhysicsSimulator physicsSimulator, Body body, float width, float height, Vector2 positionOffset, float rotationOffset)
        {
            return CreateRectangleGeom(physicsSimulator, body, width, height, positionOffset, rotationOffset, 0);
        }

        /// <summary>
        /// Creates a rectangle geometry.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="positionOffset">The position offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <returns>The geometry</returns>
        public Geom CreateRectangleGeom(Body body, float width, float height, Vector2 positionOffset, float rotationOffset)
        {
            return CreateRectangleGeom(body, width, height, positionOffset, rotationOffset, 0);
        }

        /// <summary>
        /// Creates the rectangle geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="collisionGridSize">Size of the collision grid cells. Pass in 0 or less automatically calculate the grid cell size. - not used with SAT</param>
        /// <returns></returns>
        public Geom CreateRectangleGeom(PhysicsSimulator physicsSimulator, Body body, float width, float height, float collisionGridSize)
        {
            return CreateRectangleGeom(physicsSimulator, body, width, height, Vector2.Zero, 0, collisionGridSize);
        }

        /// <summary>
        /// Creates the rectangle geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="collisionGridSize">Size of the collision grid. - not used with SAT</param>
        /// <returns></returns>
        public Geom CreateRectangleGeom(Body body, float width, float height, float collisionGridSize)
        {
            return CreateRectangleGeom(body, width, height, Vector2.Zero, 0, collisionGridSize);
        }

        /// <summary>
        /// Creates the rectangle geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="positionOffset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <param name="collisionGridSize">Size of the collision grid. - not used with SAT</param>
        /// <returns></returns>
        public Geom CreateRectangleGeom(PhysicsSimulator physicsSimulator, Body body, float width, float height, Vector2 positionOffset, float rotationOffset, float collisionGridSize)
        {
            Geom geometry = CreateRectangleGeom(body, width, height, positionOffset, rotationOffset, collisionGridSize);
            physicsSimulator.Add(geometry);
            return geometry;
        }

        /// <summary>
        /// Creates the rectangle geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="positionOffset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <param name="collisionGridSize">Size of the collision grid. - not used with SAT</param>
        /// <returns></returns>
        public Geom CreateRectangleGeom(Body body, float width, float height, Vector2 positionOffset, float rotationOffset, float collisionGridSize)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", "Width must be more than 0");

            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", "Height must be more than 0");

            Vertices vertices = Vertices.CreateRectangle(width, height);

            Geom geometry = new Geom(body, vertices, positionOffset, rotationOffset, collisionGridSize);
            return geometry;
        }
        #endregion

        #region Ellipses
        /// <summary>
        /// Creates a ellipse geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="xRadius">The x radius.</param>
        /// <param name="yRadius">The y radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <returns></returns>
        public Geom CreateEllipseGeom(PhysicsSimulator physicsSimulator, Body body, float xRadius, float yRadius, int numberOfEdges)
        {
            return CreateEllipseGeom(physicsSimulator, body, xRadius, yRadius, numberOfEdges, Vector2.Zero, 0, 0);
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
            return CreateEllipseGeom(body, xRadius, yRadius, numberOfEdges, Vector2.Zero, 0, 0);
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
        public Geom CreateEllipseGeom(PhysicsSimulator physicsSimulator, Body body, float xRadius, float yRadius, int numberOfEdges, Vector2 offset, float rotationOffset)
        {
            return CreateEllipseGeom(physicsSimulator, body, xRadius, yRadius, numberOfEdges, offset, rotationOffset, 0);
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
        public Geom CreateEllipseGeom(Body body, float xRadius, float yRadius, int numberOfEdges, Vector2 offset, float rotationOffset)
        {
            return CreateEllipseGeom(body, xRadius, yRadius, numberOfEdges, offset, rotationOffset, 0);
        }

        /// <summary>
        /// Creates a ellipse geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="xRadius">The x radius.</param>
        /// <param name="yRadius">The y radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="collisionGridSize">Size of the collision grid. - not used with SAT</param>
        /// <returns></returns>
        public Geom CreateEllipseGeom(PhysicsSimulator physicsSimulator, Body body, float xRadius, float yRadius, int numberOfEdges, float collisionGridSize)
        {
            return CreateEllipseGeom(physicsSimulator, body, xRadius, yRadius, numberOfEdges, Vector2.Zero, 0,
                                     collisionGridSize);
        }

        /// <summary>
        /// Creates an ellipse geometry
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="xRadius">The x radius.</param>
        /// <param name="yRadius">The y radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="collisionGridSize">Size of the collision grid. - not used with SAT</param>
        /// <returns></returns>
        public Geom CreateEllipseGeom(Body body, float xRadius, float yRadius, int numberOfEdges, float collisionGridSize)
        {
            return CreateEllipseGeom(body, xRadius, yRadius, numberOfEdges, Vector2.Zero, 0, collisionGridSize);
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
        /// <param name="collisionGridSize">Size of the collision grid. - not used with SAT</param>
        /// <returns></returns>
        public Geom CreateEllipseGeom(PhysicsSimulator physicsSimulator, Body body, float xRadius, float yRadius, int numberOfEdges, Vector2 offset, float rotationOffset, float collisionGridSize)
        {
            Geom geometry = CreateEllipseGeom(body, xRadius, yRadius, numberOfEdges, offset, rotationOffset,
                                              collisionGridSize);
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
        /// <param name="collisionGridSize">Size of the collision grid. - not used with SAT</param>
        /// <returns></returns>
        public Geom CreateEllipseGeom(Body body, float xRadius, float yRadius, int numberOfEdges, Vector2 offset, float rotationOffset, float collisionGridSize)
        {
            if (xRadius <= 0)
                throw new ArgumentOutOfRangeException("xRadius", "xRadius must be more than 0");

            if (yRadius <= 0)
                throw new ArgumentOutOfRangeException("yRadius", "yRadius must be more than 0");

            Vertices vertices = Vertices.CreateEllipse(xRadius, yRadius, numberOfEdges);

            Geom geometry = new Geom(body, vertices, offset, rotationOffset, collisionGridSize);
            return geometry;
        }
        #endregion

        #region Circles
        /// <summary>
        /// Creates a circle geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <returns></returns>
        public Geom CreateCircleGeom(PhysicsSimulator physicsSimulator, Body body, float radius, int numberOfEdges)
        {
            return CreateCircleGeom(physicsSimulator, body, radius, numberOfEdges, Vector2.Zero, 0, 0);
        }

        /// <summary>
        /// Creates a circle geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <returns></returns>
        public Geom CreateCircleGeom(Body body, float radius, int numberOfEdges)
        {
            return CreateCircleGeom(body, radius, numberOfEdges, Vector2.Zero, 0, 0);
        }

        /// <summary>
        /// Creates a circle geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <returns></returns>
        public Geom CreateCircleGeom(PhysicsSimulator physicsSimulator, Body body, float radius, int numberOfEdges,
                                     Vector2 offset, float rotationOffset)
        {
            return CreateCircleGeom(physicsSimulator, body, radius, numberOfEdges, offset, rotationOffset, 0);
        }

        /// <summary>
        /// Creates a circle geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <returns></returns>
        public Geom CreateCircleGeom(Body body, float radius, int numberOfEdges, Vector2 offset, float rotationOffset)
        {
            return CreateCircleGeom(body, radius, numberOfEdges, offset, rotationOffset, 0);
        }

        /// <summary>
        /// Creates a circle geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="collisionGridSize">Size of the collision grid.</param>
        /// <returns></returns>
        public Geom CreateCircleGeom(PhysicsSimulator physicsSimulator, Body body, float radius, int numberOfEdges,
                                     float collisionGridSize)
        {
            return CreateCircleGeom(physicsSimulator, body, radius, numberOfEdges, Vector2.Zero, 0,
                                    collisionGridSize);
        }

        /// <summary>
        /// Creates a circle geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="collisionGridSize">Size of the collision grid. - not used with SAT</param>
        /// <returns></returns>
        public Geom CreateCircleGeom(Body body, float radius, int numberOfEdges, float collisionGridSize)
        {
            return CreateCircleGeom(body, radius, numberOfEdges, Vector2.Zero, 0, collisionGridSize);
        }

        /// <summary>
        /// Creates a circle geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <param name="collisionGridSize">Size of the collision grid. - not used with SAT</param>
        /// <returns></returns>
        public Geom CreateCircleGeom(PhysicsSimulator physicsSimulator, Body body, float radius, int numberOfEdges,
                                     Vector2 offset, float rotationOffset, float collisionGridSize)
        {
            Geom geometry = CreateCircleGeom(body, radius, numberOfEdges, offset, rotationOffset, collisionGridSize);
            physicsSimulator.Add(geometry);
            return geometry;
        }

        /// <summary>
        /// Creates a circle geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="numberOfEdges">The number of edges.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <param name="collisionGridSize">Size of the collision grid. - not used with SAT</param>
        /// <returns></returns>
        public Geom CreateCircleGeom(Body body, float radius, int numberOfEdges, Vector2 offset, float rotationOffset,
                                     float collisionGridSize)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0");

            Vertices vertices = Vertices.CreateCircle(radius, numberOfEdges);

            Geom geometry = new Geom(body, vertices, offset, rotationOffset, collisionGridSize);
            return geometry;
        }
        #endregion

        #region Polygons
        /// <summary>
        /// Creates a polygon geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="vertices">The vertices.</param>
        /// <param name="collisionGridSize">Size of the collision grid. - not used with SAT
        /// Put in 0 or less to make the engine calculate a grid cell size.</param>
        /// <returns></returns>
        public Geom CreatePolygonGeom(PhysicsSimulator physicsSimulator, Body body, Vertices vertices, float collisionGridSize)
        {
            return CreatePolygonGeom(physicsSimulator, body, vertices, Vector2.Zero, 0, collisionGridSize);
        }

        /// <summary>
        /// Creates a polygon geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="vertices">The vertices.</param>
        /// <param name="collisionGridSize">Size of the collision grid. - not used with SAT
        /// Put in 0 or less to make the engine calculate a grid cell size.</param>
        /// <returns></returns>
        public Geom CreatePolygonGeom(Body body, Vertices vertices, float collisionGridSize)
        {
            return CreatePolygonGeom(body, vertices, Vector2.Zero, 0, collisionGridSize);
        }

        /// <summary>
        /// Creates a polygon geom.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="vertices">The vertices.</param>
        /// <param name="positionOffset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <param name="collisionGridSize">Size of the collision grid. - not used with SAT
        /// Put in 0 or less to make the engine calculate a grid cell size.</param>
        /// <returns></returns>
        public Geom CreatePolygonGeom(PhysicsSimulator physicsSimulator, Body body, Vertices vertices, Vector2 positionOffset, float rotationOffset, float collisionGridSize)
        {
            Geom geometry = CreatePolygonGeom(body, vertices, positionOffset, rotationOffset, collisionGridSize);
            physicsSimulator.Add(geometry);
            return geometry;
        }

        /// <summary>
        /// Creates a polygon geom.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="vertices">The vertices.</param>
        /// <param name="positionOffset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <param name="collisionGridSize">Size of the collision grid. - not used with SAT
        /// Put in 0 or less to make the engine calculate a grid cell size.</param>
        /// <returns></returns>
        public Geom CreatePolygonGeom(Body body, Vertices vertices, Vector2 positionOffset, float rotationOffset, float collisionGridSize)
        {
            if (body == null)
                throw new ArgumentNullException("body", "Body must not be null");

            if (vertices == null)
                throw new ArgumentNullException("vertices", "Vertices must not be null");
            
            //Adjust the verts to be relative to the centroid.
            Vector2 centroid = vertices.GetCentroid();

            Vector2.Multiply(ref centroid, -1, out centroid);

            vertices.Translate(ref centroid);

            Geom geometry = new Geom(body, vertices, positionOffset, rotationOffset, collisionGridSize);
            return geometry;
        }

        /// <summary>
        /// Creates a polygon geometry.
        /// Use this if you use the SAT narrow phase collider. It will automatically decompose concave geometries using auto-divide.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="vertices">The vertices.</param>
        /// <param name="maxGeoms">The number of geometries to split the geometry into. It is needed to make SAT support concave polygons. The engine will try to reach the desired number of geometries.</param>
        /// <returns></returns>
        public List<Geom> CreateSATPolygonGeom(PhysicsSimulator physicsSimulator, Body body, Vertices vertices, int maxGeoms)
        {
            List<Geom> geometries = CreateSATPolygonGeom(body, vertices, maxGeoms);
            foreach (Geom geom in geometries)
            {
                physicsSimulator.Add(geom);
            }

            return geometries;
        }

        /// <summary>
        /// Creates a polygon geometry.
        /// Use this if you use the SAT narrow phase collider. It will automatically decompose concave geometries using auto-divide.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="vertices">The vertices.</param>
        /// <param name="numberOfGeoms">The number of geoms.</param>
        /// <returns></returns>
        public List<Geom> CreateSATPolygonGeom(Body body, Vertices vertices, int numberOfGeoms)
        {
            if (body == null)
                throw new ArgumentNullException("body", "Body must not be null");

            if (vertices == null)
                throw new ArgumentNullException("vertices", "Vertices must not be null");

            List<Geom> geometries = Vertices.DecomposeGeom(vertices, body, numberOfGeoms);

            return geometries;
        }

        #endregion

        #region Clones
        /// <summary>
        /// Creates a clone of a geometry.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="body">The body.</param>
        /// <param name="geometry">The geometry to clone.</param>
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
        /// <param name="geometry">The geometry to clone.</param>
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
        /// <param name="geometry">The geometry to clone.</param>
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
        /// <param name="geometry">The geometry to clone.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <returns></returns>
        public Geom CreateGeom(Body body, Geom geometry, Vector2 offset, float rotationOffset)
        {
            Geom geometryClone = new Geom(body, geometry, offset, rotationOffset);
            return geometryClone;
        }
        #endregion

    }
}