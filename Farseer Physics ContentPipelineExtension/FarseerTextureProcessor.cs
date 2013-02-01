﻿using System;
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
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common.PolygonManipulation;


namespace FarseerPhysics.ContentPipeline
{
  [ContentProcessor(DisplayName = "Farseer Texture Processor")]
  class FarseerTextureProcessor : ContentProcessor<Texture2DContent, List<Vertices>>
  {
    [DisplayName("Pixel to meter ratio")]
    [Description("The length of one physics simulation unit in pixels.")]
    [DefaultValue(1)]
    public int ScaleFactor
    {
      get { return (int)(1f / _scaleFactor); }
      set { _scaleFactor = 1f / value; }
    }
    private float _scaleFactor = 1f;

    [DisplayName("Hole detection")]
    [Description("Detect holes in the traced texture.")]
    [DefaultValue(false)]
    public bool HoleDetection
    {
      get { return _holeDetection; }
      set { _holeDetection = value; }
    }
    private bool _holeDetection = false;

    public override List<Vertices> Process(Texture2DContent input, ContentProcessorContext context)
    {
      if (ScaleFactor < 1)
      {
        throw new Exception("Pixel to meter ratio must be greater than zero.");
      }

      PixelBitmapContent<Color> bitmapContent = (PixelBitmapContent<Color>)input.Faces[0][0];
      uint[] colorData = new uint[bitmapContent.Width * bitmapContent.Height];
      for (int i = 0; i < bitmapContent.Height; i++)
      {
        for (int j = 0; j < bitmapContent.Width; j++)
        {
          Color c = bitmapContent.GetPixel(j, i);
          c.R *= c.A;
          c.G *= c.A;
          c.B *= c.A;
          colorData[i * bitmapContent.Width + j] = c.PackedValue;
        }
      }

      Vertices outline = PolygonTools.CreatePolygon(colorData, bitmapContent.Width, _holeDetection);
      Vector2 centroid = -outline.GetCentroid();
      outline.Translate(ref centroid);
      outline = SimplifyTools.DouglasPeuckerSimplify(outline, 0.1f);
      List<Vertices> result = BayazitDecomposer.ConvexPartition(outline);
      Vector2 scale = new Vector2(_scaleFactor);
      foreach (Vertices vertices in result)
      {
        vertices.Scale(ref scale);
      }
      return result;
    }
  }
}