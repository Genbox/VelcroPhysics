using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Shared;

namespace Genbox.VelcroPhysics.Templates.Shapes
{
    public class PolygonShapeTemplate : ShapeTemplate
    {
        public PolygonShapeTemplate() : base(ShapeType.Polygon) { }

        public Vertices Vertices { get; set; }
    }
}