#region Using System
using System;
using System.Text;
using System.Collections.Generic;
#endregion
#region Using XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion
#region Using Farseer
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Content;
using FarseerPhysics.Samples.Demos.Prefabs;
using FarseerPhysics.Samples.ScreenSystem;
using FarseerPhysics.Samples.MediaSystem;
#endregion

namespace FarseerPhysics.Samples.Demos
{
  internal class D18_BreakableBody : PhysicsDemoScreen
  {
    private Border _border;
    private List<Sprite> _breakableSprite;
    private BreakableBody _breakableBody;

    #region Demo description
    public override string GetTitle()
    {
      return "Breakable body and explosions";
    }

    public override string GetDetails()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("This demo shows a breakable cookie, imported from a SVG.");
      sb.AppendLine();
      sb.AppendLine("GamePad:");
      sb.AppendLine("  - Explosion (at cursor): B button");
      sb.AppendLine("  - Move cursor: Left thumbstick");
      sb.AppendLine("  - Grab object (beneath cursor): A button");
      sb.AppendLine("  - Drag grabbed object: Left thumbstick");
      sb.Append("  - Exit to demo selection: Back button");
#if WINDOWS
      sb.AppendLine();
      sb.AppendLine();
      sb.AppendLine("Keyboard:");
      sb.AppendLine("  - Exit to demo selection: Escape");
      sb.AppendLine();
      sb.AppendLine("Mouse");
      sb.AppendLine("  - Explosion (at cursor): Right click");
      sb.AppendLine("  - Grab object (beneath cursor): Left click");
      sb.Append("  - Drag grabbed object: Move mouse");
#endif
      return sb.ToString();
    }
    #endregion

    public override void LoadContent()
    {
      base.LoadContent();

      World.Gravity = Vector2.Zero;

      _border = new Border(World, Lines, Framework.GraphicsDevice);
      _breakableBody = Framework.Content.Load<BodyContainer>("pipeline/farseerBreakableBody")["cookie"].CreateBreakable(World);
      _breakableBody.Strength = 80f;

      _breakableSprite = new List<Sprite>();
      for (int i = 0; i < _breakableBody.Parts.Count; i++)
      {
        AABB bounds;
        Transform transform;
        _breakableBody.Parts[i].Body.GetTransform(out transform);
        _breakableBody.Parts[i].Shape.ComputeAABB(out bounds, ref transform, 0);
        Vector2 origin = ConvertUnits.ToDisplayUnits(_breakableBody.Parts[i].Body.Position - bounds.LowerBound);

        _breakableSprite.Add(new Sprite(ContentWrapper.CustomPolygonTexture(((PolygonShape)_breakableBody.Parts[i].Shape).Vertices, "cookie"), origin));
      }
    }

    public override void HandleInput(InputHelper input, GameTime gameTime)
    {
      if (input.IsNewMouseButtonPress(MouseButtons.RightButton) ||
          input.IsNewButtonPress(Buttons.B))
      {
        Vector2 cursorPos = Camera.ConvertScreenToWorld(input.Cursor);

        Vector2 min = cursorPos - new Vector2(10, 10);
        Vector2 max = cursorPos + new Vector2(10, 10);

        AABB aabb = new AABB(ref min, ref max);

        World.QueryAABB(fixture =>
                        {
                          Vector2 fv = fixture.Body.Position - cursorPos;
                          fv.Normalize();
                          fv *= 80;
                          fixture.Body.ApplyLinearImpulse(ref fv);
                          return true;
                        }, ref aabb);
      }

      base.HandleInput(input, gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
      Sprites.Begin(0, null, null, null, null, null, Camera.View);
      for (int i = 0; i < _breakableBody.Parts.Count; i++)
      {
        Body b = _breakableBody.Parts[i].Body;
        Sprites.Draw(_breakableSprite[i].Image, ConvertUnits.ToDisplayUnits(b.Position), null, Color.White, b.Rotation, _breakableSprite[i].Origin, 1f, SpriteEffects.None, 0f);
      }
      Sprites.End();

      _border.Draw(Camera.SimProjection, Camera.SimView);

      base.Draw(gameTime);
    }
  }
}