using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using VelcroPhysics.ContentPipelines.SVGImport.Objects;
using VelcroPhysics.Shared;

namespace VelcroPhysics.ContentPipelines.SVGImport
{
    public class VerticesContainerReader : ContentTypeReader<VerticesContainer>
    {
        protected override VerticesContainer Read(ContentReader input, VerticesContainer existingInstance)
        {
            VerticesContainer container = existingInstance ?? new VerticesContainer();

            int count = input.ReadInt32(); //container.Count
            for (int i = 0; i < count; i++)
            {
                string name = input.ReadString();
                int listCount = input.ReadInt32();

                List<VerticesExt> exts = new List<VerticesExt>();

                for (int j = 0; j < listCount; j++)
                {
                    bool closed = input.ReadBoolean();
                    int vertCount = input.ReadInt32();

                    Vertices verts = new Vertices(vertCount);
                    for (int x = 0; x < vertCount; x++)
                    {
                        verts.Add(input.ReadVector2());
                    }

                    exts.Add(new VerticesExt(verts, closed));
                }

                container.Add(name, exts);
            }

            return container;
        }
    }
}