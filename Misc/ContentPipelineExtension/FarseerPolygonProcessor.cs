using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace FarseerPhysics.ContentPipeline
{
    [ContentProcessor(DisplayName = "Farseer Polygon Processor")]
    class FarseerPolygonProcessor : ContentProcessor<List<RawBodyTemplate>, PolygonContainer>
    {
        private float _scaleFactor = 1f;
        private int _bezierIterations = 3;
        private bool _decompose = false;

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
        public int BezierIterations
        {
            get { return _bezierIterations; }
            set { _bezierIterations = value; }
        }

        [DisplayName("Decompose paths")]
        [Description("Decompose paths into convex polygons.")]
        [DefaultValue(false)]
        public bool DecomposePaths
        {
            get { return _decompose; }
            set { _decompose = value; }
        }

        public override PolygonContainer Process(List<RawBodyTemplate> input, ContentProcessorContext context)
        {
            if (ScaleFactor < 1)
            {
                throw new Exception("Pixel to meter ratio must be greater than zero.");
            }
            if (BezierIterations < 1)
            {
                throw new Exception("Cubic bézier iterations must be greater than zero.");
            }

            Matrix matScale = Matrix.CreateScale(_scaleFactor, _scaleFactor, 1f);
            SVGPathParser parser = new SVGPathParser(_bezierIterations);
            PolygonContainer polygons = new PolygonContainer();

            foreach (RawBodyTemplate body in input)
            {
                foreach (RawFixtureTemplate fixture in body.Fixtures)
                {
                    List<Polygon> paths = parser.ParseSVGPath(fixture.Path, fixture.Transformation * matScale);
                    if (paths.Count == 1)
                    {
                        polygons.Add(fixture.Name, paths[0]);
                    }
                    else
                    {
                        for (int i = 0; i < paths.Count; i++)
                        {
                            polygons.Add(fixture.Name + i.ToString(), paths[i]);
                        }
                    }
                }
            }
            if (_decompose)
            {
                polygons.Decompose();
            }
            return polygons;
        }
    }
}