using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using VelcroPhysics.ContentPipelines.SVGImport.Objects;

namespace VelcroPhysics.ContentPipelines.SVGImport
{
    [ContentTypeWriter]
    public class VerticesContainerWriter : ContentTypeWriter<VerticesContainer>
    {
        protected override void Write(ContentWriter output, VerticesContainer container)
        {
            output.Write(container.Count);
            foreach (KeyValuePair<string, List<VerticesExt>> p in container)
            {
                output.Write(p.Key);
                output.Write(p.Value.Count);

                foreach (VerticesExt ext in p.Value)
                {
                    output.Write(ext.Closed);
                    output.Write(ext.Count);

                    foreach (Vector2 vec in ext)
                    {
                        output.Write(vec);
                    }
                }
            }
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(VerticesContainerReader).AssemblyQualifiedName;
        }
    }
}