using VelcroPhysics.Shared;

namespace VelcroPhysics.MonoGame.ContentPipelines.SVGImport.Objects
{
    public class VerticesExt : Vertices
    {
        public bool Closed;

        public VerticesExt()
        {
            
        }

        public VerticesExt(Vertices v, bool closed) : base(v)
        {
            Closed = closed;
        }
    }
}