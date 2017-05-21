using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using VelcroPhysics.ContentPipelines.SVGImport.Objects;
using VelcroPhysics.Shared;

namespace VelcroPhysics.ContentPipelines.SVGImport
{
    public class VerticesContainerReader : ContentTypeReader<Dictionary<string, VerticesExt>>
    {
        protected override Dictionary<string, VerticesExt> Read(ContentReader input, Dictionary<string, VerticesExt> existingInstance)
        {
            Dictionary<string, VerticesExt> paths = existingInstance ?? new Dictionary<string, VerticesExt>();

            int count = input.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string name = input.ReadString();
                bool closed = input.ReadBoolean();
                int vertsCount = input.ReadInt32();

                Vertices verts = new Vertices(vertsCount);
                for (int j = 0; j < vertsCount; j++)
                {
                    verts.Add(input.ReadVector2());
                }

                paths[name] = new VerticesExt(verts, closed);
            }

            return paths;
        }
    }
}