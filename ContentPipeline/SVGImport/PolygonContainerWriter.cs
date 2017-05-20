using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using VelcroPhysics.ContentPipelines.SVGImport.Objects;

namespace VelcroPhysics.ContentPipelines.SVGImport
{
    [ContentTypeWriter]
    public class PolygonContainerWriter : ContentTypeWriter<Dictionary<string, VerticesExt>>
    {
        protected override void Write(ContentWriter output, Dictionary<string, VerticesExt> container)
        {
            output.Write(container.Count);
            foreach (KeyValuePair<string, VerticesExt> p in container)
            {
                output.Write(p.Key);
                output.Write(p.Value.Closed);
                output.Write(p.Value.Count);
                foreach (Vector2 vec in p.Value)
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