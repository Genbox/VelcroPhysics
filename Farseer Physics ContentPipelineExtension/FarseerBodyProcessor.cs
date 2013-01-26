using System;
using System.Globalization;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using FarseerPhysics.Common;

namespace FarseerPhysics.ContentPipeline
{
  [ContentProcessor(DisplayName = "Farseer Polygon Processor")]
  public class FarseerPolygonProcessor : ContentProcessor<PolygonContainer, PolygonContainer>
  {
    [DisplayName("Cubic bézier iterations")]
    [Description("Amount of subdivisions for decomposing cubic bézier curves into line segments.")]
    [DefaultValue(3)]
    public int BezierIterations
    {
      get { return _bezierIterations; }
      set { _bezierIterations = value; }
    }
    private int _bezierIterations = 3;

    public override PolygonContainer Process(PolygonContainer input, ContentProcessorContext context)
    {
      return input;
    }
  }
}