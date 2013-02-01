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
  internal class D17_SVGtoBody : PhysicsDemoScreen
  {
    #region Demo description
    public override string GetTitle()
    {
      return "SVG Importer to bodies";
    }

    public override string GetDetails()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("This demo shows how to load bodies from a SVG.");
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

    private BodyContainer _farseerBody;
    private Border _border;
    private Body _heartBody;
    private Body _clubBody;
    private Body _spadeBody;
    private Body _diamondBody;
    private Sprite _heart;
    private Sprite _club;
    private Sprite _spade;
    private Sprite _diamond;

    public override void LoadContent()
    {
      base.LoadContent();

      World.Gravity = new Vector2(0f, 10f);
      _border = new Border(World, Lines, Framework.GraphicsDevice);

      _farseerBody = Framework.Content.Load<BodyContainer>("Pipeline/farseerBody");

      _heartBody = _farseerBody["heart"].Create(World);
      _heart = new Sprite(ContentWrapper.GetTexture("heart"), ContentWrapper.CalculateOrigin(_heartBody));

      _clubBody = _farseerBody["club"].Create(World);
      _club = new Sprite(ContentWrapper.GetTexture("club"), ContentWrapper.CalculateOrigin(_clubBody));

      _spadeBody = _farseerBody["spade"].Create(World);
      _spade = new Sprite(ContentWrapper.GetTexture("spade"), ContentWrapper.CalculateOrigin(_spadeBody));

      _diamondBody = _farseerBody["diamond"].Create(World);
      _diamond = new Sprite(ContentWrapper.GetTexture("diamond"), ContentWrapper.CalculateOrigin(_diamondBody));
    }

    public override void Draw(GameTime gameTime)
    {
      Sprites.Begin(0, null, null, null, null, null, Camera.View);
      Sprites.Draw(_heart.Image, ConvertUnits.ToDisplayUnits(_heartBody.Position),
                   null, Color.White, _heartBody.Rotation, _heart.Origin, 1f, SpriteEffects.None, 0f);
      Sprites.Draw(_club.Image, ConvertUnits.ToDisplayUnits(_clubBody.Position),
                   null, Color.White, _clubBody.Rotation, _club.Origin, 1f, SpriteEffects.None, 0f);
      Sprites.Draw(_spade.Image, ConvertUnits.ToDisplayUnits(_spadeBody.Position),
                   null, Color.White, _spadeBody.Rotation, _spade.Origin, 1f, SpriteEffects.None, 0f);
      Sprites.Draw(_diamond.Image, ConvertUnits.ToDisplayUnits(_diamondBody.Position),
                   null, Color.White, _diamondBody.Rotation, _diamond.Origin, 1f, SpriteEffects.None, 0f);
      Sprites.End();

      _border.Draw(Camera.SimProjection, Camera.SimView);
      base.Draw(gameTime);
    }
  }
}