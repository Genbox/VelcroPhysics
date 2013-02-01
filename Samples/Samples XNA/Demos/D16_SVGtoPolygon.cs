#region Using System
using System;
using System.Text;
using System.Collections.Generic;
#endregion
#region Using XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion
#region Using Farseer
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Content;
using FarseerPhysics.Samples.Demos.Prefabs;
using FarseerPhysics.Samples.ScreenSystem;
using FarseerPhysics.Samples.MediaSystem;
#endregion

namespace FarseerPhysics.Samples.Demos
{
  internal class D16_SVGtoPolygon : PhysicsDemoScreen
  {
    #region Demo description
    public override string GetTitle()
    {
      return "SVG Importer to polygons";
    }

    public override string GetDetails()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("This demo shows how to load vertices from a SVG.");
      sb.AppendLine();
      sb.AppendLine("GamePad:");
      sb.Append("  - Exit to demo selection: Back button");
#if WINDOWS
      sb.AppendLine();
      sb.AppendLine();
      sb.AppendLine("Keyboard:");
      sb.AppendLine("  - Exit to demo selection: Escape");
#endif
      return sb.ToString();
    }
    #endregion

    private PolygonContainer _farseerPoly;

    public override void LoadContent()
    {
      base.LoadContent();

      World.Gravity = Vector2.Zero;

      _farseerPoly = Framework.Content.Load<PolygonContainer>("Pipeline/farseerPolygon");
    }

    public override void Draw(GameTime gameTime)
    {
      Matrix projection = Camera.SimProjection;
      Matrix view = Camera.SimView;

      DebugView.BeginCustomDraw(ref projection, ref view);
      foreach (Polygon p in _farseerPoly.Values)
      {
        DebugView.DrawPolygon(p.vertices.ToArray(), p.vertices.Count, Color.Black, p.closed);
      }
      DebugView.EndCustomDraw();

      base.Draw(gameTime);
    }
  }
}