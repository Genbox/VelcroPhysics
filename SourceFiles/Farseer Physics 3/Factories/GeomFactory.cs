using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;

// If this is an XNA project then we use math from the XNA framework.
#if XNA
using Microsoft.Xna.Framework;
#endif

namespace FarseerPhysics.Factories
{
    /// <summary>
    /// An easy to use factory for creating geometries
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

        /// <summary>
        /// Creates a rectangle geometry and attaches it to a body.
        /// </summary>
        /// <param name="body">Body to attach rectangle too.</param>
        /// <param name="width">Width of rectangle.</param>
        /// <param name="height">Height of rectangle.</param>
        /// <returns></returns>
        public Shape CreateRectangleGeom(Body body, float width, float height)
        {
            PolygonDef geomDef = new PolygonDef();

            // we divide by 2 here because SetAsBox takes half sizes
            geomDef.SetAsBox(width / 2.0f, height / 2.0f);

            // add the shape to the body
            Shape geom = body.CreateShape(geomDef);

            // set the mass from the all the shapes associated with the body
            body.SetMassFromShapes();

            // return the shape for the user
            return geom;
        }


        /// <summary>
        /// Creates a circle geometry and attaches it to a body.
        /// </summary>
        /// <param name="body">Body to attach circle too.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <returns></returns>
        public Shape CreateCircleGeom(Body body, float radius)
        {
            CircleDef geomDef = new CircleDef();

            geomDef.Radius = radius;

            // add the shape to the body
            Shape geom = body.CreateShape(geomDef);

            // set the mass from the all the shapes associated with the body
            body.SetMassFromShapes();

            return geom;
        }

        /// <summary>
        /// Creates a simple polygon geometry.
        /// NOTE: this accepts only convex polygons.
        /// </summary>
        /// <param name="body">Body to attach geometry too.</param>
        /// <param name="vertices">Array of vertices to create the polygon with.</param>
        /// <param name="numOfVertices">Number of verrtices in the array.</param>
        /// <returns></returns>
        public Shape CreateSimplePolygonGeom(Body body, Vector2[] vertices, int numOfVertices)
        {
            PolygonDef geomDef = new PolygonDef();

            geomDef.Vertices = vertices;
            geomDef.VertexCount = numOfVertices;

            Shape geom = body.CreateShape(geomDef);

            body.SetMassFromShapes();

            return geom;
        }

        /// <summary>
        /// Creates a simple polygon geometry.
        /// NOTE: this accepts only convex polygons.
        /// </summary>
        /// <param name="body">Body to attach geometry too.</param>
        /// <param name="vertices">Vertices to create the polygon with.</param>
        /// <returns></returns>
        /*public Shape CreateSimplePolygonGeom(Body body, Vertices vertices)
        {
            PolygonDef geomDef = new PolygonDef();
          
            geomDef.Vertices = vertices.ToArray();
            geomDef.VertexCount = vertices.Count;

            Shape geom = body.CreateShape(geomDef);

            body.SetMassFromShapes();

            return geom;
        }*/

        /// <summary>
        /// Creates a complex polygon geometry.
        /// NOTE: this will split concave polygons
        /// into a set of convex polygons.
        /// </summary>
        /// <param name="body">Body to attach geometry too.</param>
        /// <param name="vertices">Array of vertices to create the polygon with.</param>
        /// <param name="numOfVertices">Number of verrtices in the array.</param>
        /// <returns></returns>
        //public Shape[] CreateComplexPolygonGeom(Body body, Vector2[] vertices, int numOfVertices)
        //{
            // 
        //}
    }
}