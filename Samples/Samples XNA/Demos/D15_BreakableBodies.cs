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
using FarseerPhysics.Samples.Demos.Prefabs;
using FarseerPhysics.Samples.ScreenSystem;
using FarseerPhysics.Samples.MediaSystem;
#endregion

namespace FarseerPhysics.Samples.Demos
{
  internal class D15_BreakableBodies : PhysicsDemoScreen
  {
    private Border _border;
    private List<List<Sprite>> _breakableSprite;
    private List<BreakableBody> _breakableBody;

    #region Demo description
    public override string GetTitle()
    {
      return "Breakable bodies and explosions";
    }

    public override string GetDetails()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("This demo shows breakable bodies, created from a texture.");
      sb.AppendLine(string.Empty);
      sb.AppendLine("GamePad:");
      sb.AppendLine("  - Explosion (at cursor): B button");
      sb.AppendLine("  - Move cursor: Left thumbstick");
      sb.AppendLine("  - Grab object (beneath cursor): A button");
      sb.AppendLine("  - Drag grabbed object: Left thumbstick");
      sb.AppendLine("  - Exit to demo selection: Back button");
#if WINDOWS
      sb.AppendLine(string.Empty);
      sb.AppendLine("Keyboard:");
      sb.AppendLine("  - Exit to demo selection: Escape");
      sb.AppendLine(string.Empty);
      sb.AppendLine("Mouse");
      sb.AppendLine("  - Explosion (at cursor): Right click");
      sb.AppendLine("  - Grab object (beneath cursor): Left click");
      sb.AppendLine("  - Drag grabbed object: Move mouse");
#endif
      return sb.ToString();
    }
    #endregion

    public override void LoadContent()
    {
      base.LoadContent();

      World.Gravity = Vector2.Zero;

      _border = new Border(World, Lines, Framework.GraphicsDevice);

      _breakableSprite = new List<List<Sprite>>();
      _breakableBody = new List<BreakableBody>();

      Texture2D letters = ContentWrapper.GetTexture("breakableObjects");

      uint[] data = new uint[letters.Width * letters.Height];
      letters.GetData(data);

      List<Vertices> list = PolygonTools.CreatePolygon(data, letters.Width, 3.5f, 20, true, true);
      for (int i = 0; i < list.Count; i++)
      {
        Vector2 centroid = -list[i].GetCentroid();
        list[i].Translate(ref centroid);
        list[i] = SimplifyTools.ReduceByDistance(list[i], 2);
        Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1));
        list[i].Scale(ref vertScale);
      }

      //                           Cube, f, a, r, s, e, e, r, p, h, y, s,  i,  c, s 
      short[] Indexes = new short[] { 0, 1, 2, 3, 4, 5, 5, 3, 6, 7, 8, 9, 10, 11, 9 };

      float xOffset = 2.8f;
      float xStart = -xOffset * (float)(Indexes.Length - 1) / 2f;
      for (int i = 0; i < Indexes.Length; i++)
      {
        Vector2 postion = new Vector2(xStart + xOffset * i, 0f);
        if (i == 0)
        {
          postion.X -= 2.5f;
          postion.Y -= 3f;
        }
        List<Vertices> triangulated = BayazitDecomposer.ConvexPartition(list[Indexes[i]]);

        BreakableBody breakableBody = new BreakableBody(triangulated, World, 1);
        breakableBody.MainBody.Position = postion;
        breakableBody.Strength = 90;
        breakableBody.MainBody.UserData = i;
        World.AddBreakableBody(breakableBody);
        _breakableBody.Add(breakableBody);

        List<Sprite> _parts = new List<Sprite>();
        for (int j = 0; j < breakableBody.Parts.Count; j++)
        {
          AABB bounds;
          Transform transform;
          breakableBody.Parts[j].Body.GetTransform(out transform);
          breakableBody.Parts[j].Shape.ComputeAABB(out bounds, ref transform, 0);
          Vector2 origin = ConvertUnits.ToDisplayUnits(breakableBody.Parts[j].Body.Position - bounds.LowerBound);

          _parts.Add(new Sprite(ContentWrapper.TextureFromShape(breakableBody.Parts[j].Shape, ContentWrapper.Black, ContentWrapper.Black), origin));
        }
        _breakableSprite.Add(_parts);
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
                          fv *= 40;
                          fixture.Body.ApplyLinearImpulse(ref fv);
                          return true;
                        }, ref aabb);
      }

      base.HandleInput(input, gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
      Sprites.Begin(0, null, null, null, null, null, Camera.View);
      for (int i = 0; i < _breakableBody.Count; i++)
      {
        for (int j = 0; j < _breakableBody[i].Parts.Count; j++)
        {
          Body b = _breakableBody[i].Parts[j].Body;
          Sprites.Draw(_breakableSprite[i][j].Image, ConvertUnits.ToDisplayUnits(b.Position), null, Color.White, b.Rotation, _breakableSprite[i][j].Origin, 1f, SpriteEffects.None, 0f);
        }
      }
      Sprites.End();

      _border.Draw(Camera.SimProjection, Camera.SimView);

      base.Draw(gameTime);
    }
  }
}