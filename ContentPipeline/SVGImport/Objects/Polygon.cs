using VelcroPhysics.Shared;

namespace VelcroPhysics.ContentPipelines.SVGImport.Objects
{
    public struct Polygon
    {
        public Vertices Vertices;
        public bool Closed;

        public Polygon(Vertices v, bool closed)
        {
            Vertices = v;
            Closed = closed;
        }
    }
}