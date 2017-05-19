using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using VelcroPhysics.Templates;

namespace VelcroPhysics.ContentPipelines.SVGImport
{
    [ContentTypeWriter]
    public class PolygonContainerWriter : ContentTypeWriter<PolygonContainer>
    {
        protected override void Write(ContentWriter output, PolygonContainer container)
        {
            output.Write(container.Count);
            foreach (KeyValuePair<string, Polygon> p in container)
            {
                output.Write(p.Key);
                output.Write(p.Value.Closed);
                output.Write(p.Value.Vertices.Count);
                foreach (Vector2 vec in p.Value.Vertices)
                {
                    output.Write(vec);
                }
            }
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(PolygonContainerReader).AssemblyQualifiedName;
        }
    }
}