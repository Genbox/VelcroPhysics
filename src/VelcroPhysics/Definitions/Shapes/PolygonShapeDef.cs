using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Shared;

namespace Genbox.VelcroPhysics.Definitions.Shapes
{
    public sealed class PolygonShapeDef : ShapeDef
    {
        public PolygonShapeDef() : base(ShapeType.Polygon)
        {
            SetDefaults();
        }

        public Vertices Vertices { get; set; }

        public override void SetDefaults()
        {
            Vertices = null;
            base.SetDefaults();
        }
    }
}