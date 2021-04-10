using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Shared;

namespace VelcroPhysics.Templates.Shapes
{
    public class PolygonShapeTemplate : ShapeTemplate
    {
        public PolygonShapeTemplate() : base(ShapeType.Polygon) { }

        public Vertices Vertices { get; set; }
    }
}