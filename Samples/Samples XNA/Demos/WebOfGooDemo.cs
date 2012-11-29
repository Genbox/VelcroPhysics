#region Using System
using System;
using System.Text;
#endregion
#region Using XNA
using Microsoft.Xna.Framework;
#endregion
#region Using Farseer
using FarseerPhysics.Samples.Demos.Prefabs;
using FarseerPhysics.Samples.ScreenSystem;
#endregion

namespace FarseerPhysics.Samples.Demos
{
  internal class WebOfGooDemo : PhysicsGameScreen
  {
    private Border _border;
    private WebOfGoo _webOfGoo;

    #region Demo description

    public override string GetTitle()
    {
      return "Advanced dynamics";
    }

    public override string GetDetails()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("TODO: Add sample description!");
      sb.AppendLine(string.Empty);
      sb.AppendLine("GamePad:");
      sb.AppendLine("  - Move cursor: left thumbstick");
      sb.AppendLine("  - Grab object (beneath cursor): A button");
      sb.AppendLine("  - Drag grabbed object: left thumbstick");
      sb.AppendLine("  - Exit to menu: Back button");
      sb.AppendLine(string.Empty);
      sb.AppendLine("Keyboard:");
      sb.AppendLine("  - Exit to menu: Escape");
      sb.AppendLine(string.Empty);
      sb.AppendLine("Mouse / Touchscreen");
      sb.AppendLine("  - Grab object (beneath cursor): Left click");
      sb.AppendLine("  - Drag grabbed object: move mouse / finger");
      return sb.ToString();
    }

    public override int GetIndex()
    {
      return 13;
    }

    #endregion

    public override void LoadContent()
    {
      base.LoadContent();

      World.Gravity = new Vector2(0, 9.82f);

      _border = new Border(World, Lines, Framework.GraphicsDevice);

      _webOfGoo = new WebOfGoo(World, Vector2.Zero, ConvertUnits.ToSimUnits(12), 5, 12);
    }

    public override void Draw(GameTime gameTime)
    {
      Sprites.Begin(0, null, null, null, null, null, Camera.View);
      _webOfGoo.Draw(Sprites);
      Sprites.End();

      _border.Draw(Camera.SimProjection, Camera.SimView);

      base.Draw(gameTime);
    }
  }
}