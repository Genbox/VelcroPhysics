#region Using System
using System;
using System.Text;
#endregion
#region Using XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion
#region Using Farseer
using FarseerPhysics.Samples.Demos.Prefabs;
using FarseerPhysics.Samples.ScreenSystem;
#endregion

namespace FarseerPhysics.Samples.Demos
{
  internal class Demo12 : PhysicsGameScreen
  {
    private Border _border;
    private TheoJansenWalker _walker;

    #region Demo description

    public override string GetTitle()
    {
      return "Theo Jansen's walker";
    }

    public override string GetDetails()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("TODO: Add sample description!");
      sb.AppendLine(string.Empty);
      sb.AppendLine("GamePad:");
      sb.AppendLine("  - Switch walker direction: B button");
      sb.AppendLine("  - Exit to menu: Back button");
      sb.AppendLine(string.Empty);
      sb.AppendLine("Keyboard:");
      sb.AppendLine("  - Switch walker direction: Space");
      sb.AppendLine("  - Exit to menu: Escape");
      sb.AppendLine(string.Empty);
      sb.AppendLine("Mouse / Touchscreen");
      sb.AppendLine("  - Switch walker direction: Right click");
      return sb.ToString();
    }

    public override int GetIndex()
    {
      return 12;
    }

    #endregion

    public override void LoadContent()
    {
      base.LoadContent();

      HasCursor = false;

      World.Gravity = new Vector2(0, 9.82f);

      _border = new Border(World, Lines, Framework.GraphicsDevice);

      _walker = new TheoJansenWalker(World, Vector2.Zero);
    }

    public override void HandleInput(InputHelper input, GameTime gameTime)
    {
      if (input.IsNewButtonPress(Buttons.B) ||
          input.IsNewMouseButtonPress(MouseButtons.RightButton) ||
          input.IsNewKeyPress(Keys.Space))
      {
        _walker.Reverse();
      }

      base.HandleInput(input, gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
      _walker.Draw(Sprites, Lines, Camera);

      _border.Draw(Camera.SimProjection, Camera.SimView);

      base.Draw(gameTime);
    }
  }
}