using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Definitions.Shapes
{
    /// <summary>A chain shape is a free form sequence of line segments. The chain has two-sided collision, so you can use
    /// inside and outside collision. Therefore, you may use any winding order. Connectivity information is used to create
    /// smooth collisions.
    /// <remarks>WARNING: The chain will not collide properly if there are self-intersections.</remarks>
    /// </summary>
    public sealed class ChainShapeDef : ShapeDef
    {
        public ChainShapeDef() : base(ShapeType.Chain)
        {
            SetDefaults();
        }

        /// <summary>Establish connectivity to a vertex that follows the last vertex.
        /// <remarks>Don't call this for loops.</remarks>
        /// </summary>
        public Vector2 NextVertex { get; set; }

        /// <summary>Establish connectivity to a vertex that precedes the first vertex.
        /// <remarks>Don't call this for loops.</remarks>
        /// </summary>
        public Vector2 PrevVertex { get; set; }

        /// <summary>The vertices. These are not owned/freed by the chain Shape.</summary>
        public Vertices Vertices { get; set; }

        public override void SetDefaults()
        {
            NextVertex = Vector2.Zero;
            PrevVertex = Vector2.Zero;
            Vertices = null;

            base.SetDefaults();
        }
    }
}