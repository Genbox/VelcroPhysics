using VelcroPhysics.Collision.Shapes;

namespace VelcroPhysics.Templates.Shapes
{
    public class ShapeTemplate
    {
        public ShapeTemplate(ShapeType type)
        {
            ShapeType = type;
        }

        /// <summary>
        /// Gets or sets the density.
        /// </summary>
        public float Density { get; set; }

        /// <summary>
        /// Radius of the Shape
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// Get the type of this shape.
        /// </summary>
        public ShapeType ShapeType { get; }
    }
}