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
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Samples.Demos.Prefabs;
using FarseerPhysics.Samples.ScreenSystem;
using FarseerPhysics.Samples.MediaSystem;
#endregion

namespace FarseerPhysics.Samples.Demos
{
  internal class D14_TextureToShapes : PhysicsDemoScreen
  {
    private Border _border;
    private Body _compound;
    private Sprite _objectSprite;

    #region Demo description
    public override string GetTitle()
    {
      return "Texture to collision shapes";
    }

    public override string GetDetails()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("This demo shows how to create collision shapes from a texture.");
      sb.AppendLine("These are added to a single body with multiple fixtures.");
      sb.AppendLine(string.Empty);
      sb.AppendLine("GamePad:");
      sb.AppendLine("  - Rotate object: Left and right trigger");
      sb.AppendLine("  - Move object: Right thumbstick");
      sb.AppendLine("  - Move cursor: Left thumbstick");
      sb.AppendLine("  - Grab object (beneath cursor): A button");
      sb.AppendLine("  - Drag grabbed object: Left thumbstick");
      sb.AppendLine("  - Exit to demo selection: Back button");
#if WINDOWS
      sb.AppendLine(string.Empty);
      sb.AppendLine("Keyboard:");
      sb.AppendLine("  - Rotate Object: Q, E");
      sb.AppendLine("  - Move Object: W, S, A, D");
      sb.AppendLine("  - Exit to demo selection: Escape");
      sb.AppendLine(string.Empty);
      sb.AppendLine("Mouse");
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

      // Load texture that will represent the physics body
      Texture2D polygonTexture = ContentWrapper.GetTexture("object");

      // Create an array to hold the data from the texture
      uint[] data = new uint[polygonTexture.Width * polygonTexture.Height];

      // Transfer the texture data to the array
      polygonTexture.GetData(data);

      // Find the vertices that makes up the outline of the shape in the texture
      Vertices textureVertices = PolygonTools.CreatePolygon(data, polygonTexture.Width, false);

      // The tool returns vertices as they were found in the texture.
      // We need to find the real center (centroid) of the vertices for 2 reasons:

      // 1. To translate the vertices so the polygon is centered around the centroid.
      Vector2 centroid = -textureVertices.GetCentroid();
      textureVertices.Translate(ref centroid);

      // 2. To draw the texture the correct place.
      Vector2 origin = -centroid;

      // We simplify the vertices found in the texture.
      textureVertices = SimplifyTools.DouglasPeuckerSimplify(textureVertices, 0.1f);

      // Since it is a concave polygon, we need to partition it into several smaller convex polygons
      List<Vertices> list = BayazitDecomposer.ConvexPartition(textureVertices);

      // Scale the vertices from graphics space to sim space
      Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1));
      foreach (Vertices vertices in list)
      {
        vertices.Scale(ref vertScale);
      }

      // Create a single body with multiple fixtures
      _compound = BodyFactory.CreateCompoundPolygon(World, list, 1f, BodyType.Dynamic);
      _compound.BodyType = BodyType.Dynamic;

      SetUserAgent(_compound, 200f, 200f);

      // GFX
      origin.X -= 1f;
      origin.Y -= 1f;
      _objectSprite = new Sprite(ContentWrapper.GetTexture("objectImage"), origin);
    }

    public override void Draw(GameTime gameTime)
    {
      Sprites.Begin(0, null, null, null, null, null, Camera.View);
      Sprites.Draw(_objectSprite.Image, ConvertUnits.ToDisplayUnits(_compound.Position),
                   null, Color.White, _compound.Rotation, _objectSprite.Origin, 1f, SpriteEffects.None, 0f);
      Sprites.End();

      _border.Draw(Camera.SimProjection, Camera.SimView);

      base.Draw(gameTime);
    }
  }
}