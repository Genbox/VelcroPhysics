using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace FarseerPhysics.ContentPipeline
{
    [ContentTypeWriter]
    public class FarseerBodyContainerWriter : ContentTypeWriter<BodyContainer>
    {
        protected override void Write(ContentWriter output, BodyContainer container)
        {
            output.Write(container.Count);
            foreach (KeyValuePair<string, BodyTemplate> p in container)
            {
                output.Write(p.Key);
                output.Write(p.Value.Mass);
                output.Write((int)p.Value.BodyType);
                output.Write(p.Value.Fixtures.Count);
                foreach (FixtureTemplate f in p.Value.Fixtures)
                {
                    output.Write(f.Name);
                    output.Write(f.Restitution);
                    output.Write(f.Friction);
                    output.Write((int)f.Shape.ShapeType);
                    switch (f.Shape.ShapeType)
                    {
                        case ShapeType.Circle:
                            {
                                CircleShape circle = (CircleShape)f.Shape;
                                output.Write(circle.Density);
                                output.Write(circle.Radius);
                                output.Write(circle.Position);
                            } break;
                        case ShapeType.Polygon:
                            {
                                PolygonShape poly = (PolygonShape)f.Shape;
                                output.Write(poly.Density);
                                output.Write(poly.Vertices.Count);
                                foreach (Vector2 v in poly.Vertices)
                                {
                                    output.Write(v);
                                }
                                output.Write(poly.MassData.Centroid);
                            } break;
                        case ShapeType.Edge:
                            {
                                EdgeShape edge = (EdgeShape)f.Shape;
                                output.Write(edge.Vertex1);
                                output.Write(edge.Vertex2);
                                output.Write(edge.HasVertex0);
                                if (edge.HasVertex0)
                                {
                                    output.Write(edge.Vertex0);
                                }
                                output.Write(edge.HasVertex3);
                                if (edge.HasVertex3)
                                {
                                    output.Write(edge.Vertex3);
                                }
                            } break;
                        case ShapeType.Chain:
                            {
                                ChainShape chain = (ChainShape)f.Shape;
                                output.Write(chain.Vertices.Count);
                                foreach (Vector2 v in chain.Vertices)
                                {
                                    output.Write(v);
                                }
                            } break;
                        default:
                            throw new Exception("Shape type not supported!");
                    }
                }
            }
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(FarseerBodyContainerReader).AssemblyQualifiedName;
        }
    }
}
