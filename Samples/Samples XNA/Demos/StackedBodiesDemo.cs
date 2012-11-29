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
  internal class StackedBodiesDemo : PhysicsGameScreen
  {
#if XBOX
    private const int PyramidBaseBodyCount = 10;
#else
    private const int PyramidBaseBodyCount = 14;
#endif

    private Agent _agent;
    private Pyramid _pyramid;
    private Border _border;

    #region Demo description

    public override string GetTitle()
    {
      return "Stacked Objects";
    }

    public override string GetDetails()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("This demo shows the stacking stability of the engine.");
      sb.AppendLine("It shows a stack of rectangular bodies stacked in the shape of a pyramid.");
      sb.AppendLine(string.Empty);
      sb.AppendLine("GamePad:");
      sb.AppendLine("  - Rotate agent: left and right triggers");
      sb.AppendLine("  - Move agent: right thumbstick");
      sb.AppendLine("  - Move cursor: left thumbstick");
      sb.AppendLine("  - Grab object (beneath cursor): A button");
      sb.AppendLine("  - Drag grabbed object: left thumbstick");
      sb.AppendLine("  - Exit to menu: Back button");
      sb.AppendLine(string.Empty);
      sb.AppendLine("Keyboard:");
      sb.AppendLine("  - Rotate agent: left and right arrows");
      sb.AppendLine("  - Move agent: A,S,D,W");
      sb.AppendLine("  - Exit to menu: Escape");
      sb.AppendLine(string.Empty);
      sb.AppendLine("Mouse / Touchscreen");
      sb.AppendLine("  - Grab object (beneath cursor): Left click");
      sb.AppendLine("  - Drag grabbed object: move mouse / finger");
      return sb.ToString();
    }

    public override int GetIndex()
    {
      return 4;
    }

    #endregion

    public override void LoadContent()
    {
      base.LoadContent();

      World.Gravity = new Vector2(0f, 20f);

      _border = new Border(World, Lines, Framework.GraphicsDevice);

      _agent = new Agent(World, new Vector2(5f, -10f));

      _pyramid = new Pyramid(World, new Vector2(0f, 15f), PyramidBaseBodyCount, 1f);

      SetUserAgent(_agent.Body, 1000f, 400f);
    }

    public override void Draw(GameTime gameTime)
    {
      Sprites.Begin(0, null, null, null, null, null, Camera.View);
      _agent.Draw(Sprites);
      _pyramid.Draw(Sprites);
      Sprites.End();

      _border.Draw(Camera.SimProjection, Camera.SimView);

      base.Draw(gameTime);
    }
  }
}