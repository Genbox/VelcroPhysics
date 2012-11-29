#region Using System
using System;
using System.Text;
using System.Collections.Generic;
#endregion
#region Using XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion
#region Using Farseer
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Samples.MediaSystem;
using FarseerPhysics.Samples.Demos.Prefabs;
using FarseerPhysics.Samples.ScreenSystem;
#endregion

namespace FarseerPhysics.Samples.Demos
{
  internal class MultipleFixturesDemo : PhysicsGameScreen
  {
    private Border _border;
    private Sprite _rectangleSprite;
    private Body _rectangles;
    private Vector2 _offset;

    #region Demo description

    public override string GetTitle()
    {
      return "Body with two fixtures";
    }

    public override string GetDetails()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("This demo shows a single body with two attached fixtures and shapes.");
      sb.AppendLine("A fixture binds a shape to a body and adds material");
      sb.AppendLine("properties such as density, friction, and restitution.");
      sb.AppendLine(string.Empty);
      sb.AppendLine("GamePad:");
      sb.AppendLine("  - Rotate object: left and right triggers");
      sb.AppendLine("  - Move object: right thumbstick");
      sb.AppendLine("  - Move cursor: left thumbstick");
      sb.AppendLine("  - Grab object (beneath cursor): A button");
      sb.AppendLine("  - Drag grabbed object: left thumbstick");
      sb.AppendLine("  - Exit to menu: Back button");
      sb.AppendLine(string.Empty);
      sb.AppendLine("Keyboard:");
      sb.AppendLine("  - Rotate Object: left and right arrows");
      sb.AppendLine("  - Move Object: A,S,D,W");
      sb.AppendLine("  - Exit to menu: Escape");
      sb.AppendLine(string.Empty);
      sb.AppendLine("Mouse / Touchscreen");
      sb.AppendLine("  - Grab object (beneath cursor): Left click");
      sb.AppendLine("  - Drag grabbed object: move mouse / finger");
      return sb.ToString();
    }

    public override int GetIndex()
    {
      return 2;
    }

    #endregion

    public override void LoadContent()
    {
      base.LoadContent();

      World.Gravity = Vector2.Zero;

      _border = new Border(World, Lines, Framework.GraphicsDevice);

      Vertices rectangle1 = PolygonTools.CreateRectangle(2f, 2f);
      Vertices rectangle2 = PolygonTools.CreateRectangle(2f, 2f);

      Vector2 translation = new Vector2(-2f, 0f);
      rectangle1.Translate(ref translation);
      translation = new Vector2(2f, 0f);
      rectangle2.Translate(ref translation);

      List<Vertices> vertices = new List<Vertices>(2);
      vertices.Add(rectangle1);
      vertices.Add(rectangle2);

      _rectangles = BodyFactory.CreateCompoundPolygon(World, vertices, 1f);
      _rectangles.BodyType = BodyType.Dynamic;

      SetUserAgent(_rectangles, 200f, 200f);

      // create sprite based on rectangle fixture
      _rectangleSprite = new Sprite(AssetCreator.PolygonTexture(rectangle1, "square", AssetCreator.Blue, AssetCreator.Gold, AssetCreator.Black, 1f));
      _offset = new Vector2(ConvertUnits.ToDisplayUnits(2f), 0f);
    }

    public override void Draw(GameTime gameTime)
    {
      Sprites.Begin(0, null, null, null, null, null, Camera.View);
      // draw first rectangle
      Sprites.Draw(_rectangleSprite.Image, ConvertUnits.ToDisplayUnits(_rectangles.Position), null,
                   Color.White, _rectangles.Rotation, _rectangleSprite.Origin + _offset, 1f, SpriteEffects.None, 0f);
      // draw second rectangle
      Sprites.Draw(_rectangleSprite.Image, ConvertUnits.ToDisplayUnits(_rectangles.Position), null,
                   Color.White, _rectangles.Rotation, _rectangleSprite.Origin - _offset, 1f, SpriteEffects.None, 0f);
      Sprites.End();
      
      _border.Draw(Camera.SimProjection, Camera.SimView);

      base.Draw(gameTime);
    }
  }
}