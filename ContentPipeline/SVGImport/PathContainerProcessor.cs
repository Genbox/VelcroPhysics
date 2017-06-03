using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using VelcroPhysics.ContentPipelines.SVGImport.Objects;

namespace VelcroPhysics.ContentPipelines.SVGImport
{
    [ContentProcessor(DisplayName = "PathContainer Processor")]
    public class PathContainerProcessor : ContentProcessor<List<PathDefinition>, VerticesContainer>
    {
        private float _scaleFactor = 1f;

        [DisplayName("Pixel to meter ratio")]
        [Description("The length of one physics simulation unit in pixels.")]
        [DefaultValue(1)]
        public int ScaleFactor
        {
            get { return (int)(1f / _scaleFactor); }
            set { _scaleFactor = 1f / value; }
        }

        [DisplayName("Cubic bézier iterations")]
        [Description("Amount of subdivisions for decomposing cubic bézier curves into line segments.")]
        [DefaultValue(3)]
        public int BezierIterations { get; set; } = 3;

        public override VerticesContainer Process(List<PathDefinition> input, ContentProcessorContext context)
        {
            if (ScaleFactor < 1)
                throw new Exception("Pixel to meter ratio must be greater than zero.");

            if (BezierIterations < 1)
                throw new Exception("Cubic bézier iterations must be greater than zero.");

            Matrix matScale = Matrix.CreateScale(_scaleFactor, _scaleFactor, 1f);
            SVGPathParser parser = new SVGPathParser(BezierIterations);
            VerticesContainer container = new VerticesContainer();

            foreach (PathDefinition d in input)
            {
                List<VerticesExt> vertices = parser.ParseSVGPath(d.Path, d.Transformation * matScale);
                List<VerticesExt> c = container.ContainsKey(d.Id) ? container[d.Id] : (container[d.Id] = new List<VerticesExt>());

                if (vertices.Count == 1)
                    c.Add(vertices[0]);
                else
                {
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        c.Add(vertices[i]);
                    }
                }
            }

            return container;
        }
    }
}