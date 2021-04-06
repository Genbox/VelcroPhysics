using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Shared;

namespace VelcroPhysics.Templates.Shapes
{
    /// <summary>
    /// A chain shape is a free form sequence of line segments.
    /// The chain has two-sided collision, so you can use inside and outside collision.
    /// Therefore, you may use any winding order.
    /// Connectivity information is used to create smooth collisions.
    /// <remarks>WARNING: The chain will not collide properly if there are self-intersections.</remarks>
    /// </summary>
    public class ChainShapeTemplate : ShapeTemplate
    {
        public ChainShapeTemplate() : base(ShapeType.Chain) { }

        /// <summary>
        /// Establish connectivity to a vertex that follows the last vertex.
        /// <remarks>Don't call this for loops.</remarks>
        /// </summary>
        public Vector2 NextVertex { get; set; }

        /// <summary>
        /// Establish connectivity to a vertex that precedes the first vertex.
        /// <remarks>Don't call this for loops.</remarks>
        /// </summary>
        public Vector2 PrevVertex { get; set; }

        /// <summary>
        /// The vertices. These are not owned/freed by the chain Shape.
        /// </summary>
        public Vertices Vertices { get; set; }
    }
}