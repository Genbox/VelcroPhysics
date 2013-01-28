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
  [ContentProcessor(DisplayName = "Farseer Body Processor")]
  class FarseerBodyProcessor : ContentProcessor<List<BodyTemplate>, List<BodyTemplate>>
  {
    public override List<BodyTemplate> Process(List<BodyTemplate> input, ContentProcessorContext context)
    {
      return input;
    }
  }
}