using System;
using System.Collections.Generic;
using System.Text;

#if (XNA)
using Microsoft.Xna.Framework; 
#endif

using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.FarseerPhysics.Dynamics;

namespace FarseerGames.FarseerPhysics.Collisions {
    public class GeomFactory {
        private static GeomFactory instance;
        /// <summary>
        ///used to calculate a cell size from the aabb whenever the collisionGridCellSize
        ///is not set explicitly. The more sharp corners a body has, the smaller this value will 
        ///need to be. 
        /// </summary>
        private float gridCellSizeAABBFactor = .1f; 

        private GeomFactory() { }

        public static GeomFactory  Instance {
            get {
                if (instance == null) { instance = new GeomFactory(); }
                return instance;
            }
        }	

        public float GridCellSizeAABBFactor {
            get { return gridCellSizeAABBFactor; }
            set { gridCellSizeAABBFactor = value; }
        }	

        //rectangles
        public Geom CreateRectangleGeom(PhysicsSimulator physicsSimulator,Body body, float width, float height) {
            return CreateRectangleGeom(physicsSimulator,body, width, height, Vector2.Zero, 0, 0);
        }

        public Geom CreateRectangleGeom(Body body, float width, float height) {
            return CreateRectangleGeom(body, width, height, Vector2.Zero, 0, 0);
        }

        public Geom CreateRectangleGeom(PhysicsSimulator physicsSimulator, Body body, float width, float height, Vector2 offset, float rotationOffset) {
            return CreateRectangleGeom(physicsSimulator,body, width, height, offset, rotationOffset, 0);
        }

        public Geom CreateRectangleGeom(Body body, float width, float height, Vector2 offset, float rotationOffset) {
            return CreateRectangleGeom(body, width, height, offset, rotationOffset, 0);
        }

        public Geom CreateRectangleGeom(PhysicsSimulator physicsSimulator, Body body, float width, float height, float collisionGridCellSize) {
            return CreateRectangleGeom(physicsSimulator, body, width, height, Vector2.Zero,0, collisionGridCellSize);
        }

        public Geom CreateRectangleGeom(Body body, float width, float height, float collisionGridCellSize) {
            return CreateRectangleGeom(body, width, height, Vector2.Zero, 0, collisionGridCellSize);
        }

        public Geom CreateRectangleGeom(PhysicsSimulator physicsSimulator, Body body, float width, float height, Vector2 offset, float rotationOffset, float collisionGridCellSize) {
            Geom geometry = CreateRectangleGeom(body, width, height, offset, rotationOffset, collisionGridCellSize);            
            physicsSimulator.Add(geometry);
            return geometry;
        }

        public Geom CreateRectangleGeom(Body body, float width, float height, Vector2 offset, float rotationOffset, float collisionGridCellSize) {
            Vertices vertices = Vertices.CreateRectangle(width, height);
            if (collisionGridCellSize <= 0) { collisionGridCellSize = CalculateGridCellSizeFromAABB(vertices); }
            Geom geometry = new Geom(body, vertices, offset, rotationOffset, collisionGridCellSize);
            return geometry;
        }

        //circles
        public Geom CreateCircleGeom(PhysicsSimulator physicsSimulator, Body body, float radius, int numberOfEdges) {
            return CreateCircleGeom(physicsSimulator, body, radius, numberOfEdges,Vector2.Zero,0, 0);
        }
        
        public Geom CreateCircleGeom(Body body, float radius, int numberOfEdges) {
            return CreateCircleGeom(body, radius, numberOfEdges, Vector2.Zero, 0, 0);
        }

        public Geom CreateCircleGeom(PhysicsSimulator physicsSimulator, Body body, float radius, int numberOfEdges, Vector2 offset, float rotationOffset) {
            return CreateCircleGeom(physicsSimulator,body, radius, numberOfEdges, offset, rotationOffset, 0);
        }

        public Geom CreateCircleGeom(Body body, float radius, int numberOfEdges, Vector2 offset, float rotationOffset) {
            return CreateCircleGeom(body, radius, numberOfEdges, offset,rotationOffset, 0);
        }

        public Geom CreateCircleGeom(PhysicsSimulator physicsSimulator, Body body, float radius, int numberOfEdges, float collisionGridCellSize) {
            return CreateCircleGeom(physicsSimulator,body, radius, numberOfEdges, Vector2.Zero, 0, collisionGridCellSize);
        }

        public Geom CreateCircleGeom(Body body, float radius, int numberOfEdges, float collisionGridCellSize) {
            return CreateCircleGeom(body, radius, numberOfEdges, Vector2.Zero, 0, collisionGridCellSize);
        }

        public Geom CreateCircleGeom(PhysicsSimulator physicsSimulator, Body body, float radius, int numberOfEdges, Vector2 offset, float rotationOffset, float collisionGridCellSize) {
            Geom geometry =  CreateCircleGeom(body, radius,numberOfEdges, offset, rotationOffset, collisionGridCellSize);
            physicsSimulator.Add(geometry);
            return geometry;
        }

        public Geom CreateCircleGeom(Body body, float radius, int numberOfEdges, Vector2 offset, float rotationOffset, float collisionGridCellSize) {
            Vertices vertices = Vertices.CreateCircle(radius, numberOfEdges);
            if (collisionGridCellSize <= 0) { collisionGridCellSize = CalculateGridCellSizeFromAABB(vertices); }
            Geom geometry = new Geom(body, vertices, offset, rotationOffset, collisionGridCellSize);
            return geometry;
        }

        //polygons
        public Geom CreatePolygonGeom(PhysicsSimulator physicsSimulator, Body body, Vertices vertices, float collisionGridCellSize) {
            return CreatePolygonGeom(physicsSimulator, body,vertices,Vector2.Zero,0,collisionGridCellSize);
        }

        public Geom CreatePolygonGeom(Body body, Vertices vertices, float collisionGridCellSize) {
            return CreatePolygonGeom(body, vertices, Vector2.Zero, 0, collisionGridCellSize);
        }

        public Geom CreatePolygonGeom(PhysicsSimulator physicsSimulator, Body body, Vertices vertices, Vector2 offset, float rotationOffset, float collisionGridCellSize) {
            Geom geometry = CreatePolygonGeom(body, vertices,offset, rotationOffset,collisionGridCellSize);
            physicsSimulator.Add(geometry);
            return geometry;
        }

        public Geom CreatePolygonGeom(Body body, Vertices vertices, Vector2 offset, float rotationOffset, float collisionGridCellSize) {
            //adjust the verts to be relative to 0,0
            Vector2 centroid = vertices.GetCentroid();
            vertices.Translate(-centroid);

            if (collisionGridCellSize <= 0) { collisionGridCellSize = CalculateGridCellSizeFromAABB(vertices); }
            Geom geometry = new Geom(body, vertices, offset, rotationOffset, collisionGridCellSize);
            return geometry;
        }
        
        public Geom CreateGeom(PhysicsSimulator physicsSimulator, Body body, Geom geometry) {
            Geom geometryClone = CreateGeom(body, geometry);
            physicsSimulator.Add(geometryClone);
            return geometryClone;
        }

        public Geom CreateGeom(Body body, Geom geometry) {
            Geom geometryClone = new Geom(body, geometry);
            return geometryClone;
        }

        public Geom CreateGeom(PhysicsSimulator physicsSimulator, Body body, Geom geometry, Vector2 offset, float rotationOffset) {
            Geom geometryClone = CreateGeom(body, geometry, offset, rotationOffset);
            physicsSimulator.Add(geometryClone);
            return geometryClone;
        }

        public Geom CreateGeom(Body body, Geom geometry, Vector2 offset, float rotationOffset) {
            Geom geometryClone = new Geom(body, geometry, offset, rotationOffset);
            return geometryClone;
        }

        //misc
        public float CalculateGridCellSizeFromAABB(Vertices vertices) {
            AABB aabb = new AABB(vertices);
            return aabb.GetShortestSide() * gridCellSizeAABBFactor;         
        }
    }
}
