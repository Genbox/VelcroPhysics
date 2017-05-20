using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Shapes;

namespace VelcroPhysics.Templates.Shapes
{
    /// <summary>
    /// A line segment (edge) shape. These can be connected in chains or loops to other edge shapes.
    /// The connectivity information is used to ensure correct contact normals.
    /// </summary>
    public class EdgeShapeTemplate : ShapeTemplate
    {
        public EdgeShapeTemplate() : base(ShapeType.Edge) { }

        /// <summary>
        /// Is true if the edge is connected to an adjacent vertex before vertex 1.
        /// </summary>
        public bool HasVertex0 { get; set; }

        /// <summary>
        /// Is true if the edge is connected to an adjacent vertex after vertex2.
        /// </summary>
        public bool HasVertex3 { get; set; }

        /// <summary>
        /// Optional adjacent vertices. These are used for smooth collision.
        /// </summary>
        public Vector2 Vertex0 { get; set; }

        /// <summary>
        /// These are the edge vertices
        /// </summary>
        public Vector2 Vertex1 { get; set; }

        /// <summary>
        /// These are the edge vertices
        /// </summary>
        public Vector2 Vertex2 { get; set; }

        /// <summary>
        /// Optional adjacent vertices. These are used for smooth collision.
        /// </summary>
        public Vector2 Vertex3 { get; set; }
    }
}