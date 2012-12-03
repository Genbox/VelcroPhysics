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
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Samples.MediaSystem;
using FarseerPhysics.Samples.Demos.Prefabs;
using FarseerPhysics.Samples.ScreenSystem;
#endregion

namespace FarseerPhysics.Samples.Demos
{
  internal class SoftBodyDemo : PhysicsDemoScreen
  {
    private Border _border;

    private List<Body> _bridgeBodies;
    private List<Body> _softBodies;

    private Sprite _bridgeBox;
    private Sprite _softBodyBox;
    private Sprite _softBodyCircle;

    #region Demo description

    public override string GetTitle()
    {
      return "Soft body & Path generator";
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
      return 11;
    }

    #endregion

    public override void LoadContent()
    {
      base.LoadContent();

      World.Gravity = new Vector2(0, 9.82f);

      _border = new Border(World, Lines, Framework.GraphicsDevice);

      // Bridge
      // We make a path using 2 points.
      Path bridgePath = new Path();
      bridgePath.Add(new Vector2(-15, 5));
      bridgePath.Add(new Vector2(15, 5));
      bridgePath.Closed = false;

      Vertices box = PolygonTools.CreateRectangle(0.125f, 0.5f);
      PolygonShape shape = new PolygonShape(box, 20);

      _bridgeBodies = PathManager.EvenlyDistributeShapesAlongPath(World, bridgePath, shape, BodyType.Dynamic, 29);

      // Attach the first and last fixtures to the world
      ////JointFactory.CreateFixedRevoluteJoint(World, _bridgeBodies[0], new Vector2(0f, -0.5f),
      ////                                      _bridgeBodies[0].Position);
      ////JointFactory.CreateFixedRevoluteJoint(World, _bridgeBodies[_bridgeBodies.Count - 1], new Vector2(0, 0.5f),
      ////                                      _bridgeBodies[_bridgeBodies.Count - 1].Position);

      PathManager.AttachBodiesWithRevoluteJoint(World, _bridgeBodies, new Vector2(0f, -0.5f), new Vector2(0f, 0.5f), false, true);

      // Soft body
      // We make a rectangular path.
      Path rectanglePath = new Path();
      rectanglePath.Add(new Vector2(-6, -11));
      rectanglePath.Add(new Vector2(-6, 1));
      rectanglePath.Add(new Vector2(6, 1));
      rectanglePath.Add(new Vector2(6, -11));
      rectanglePath.Closed = true;

      // Creating two shapes. A circle to form the circle and a rectangle to stabilize the soft body.
      Shape[] shapes = new Shape[2];
      shapes[0] = new PolygonShape(PolygonTools.CreateRectangle(0.5f, 0.5f, new Vector2(-0.1f, 0f), 0f), 1f);
      shapes[1] = new CircleShape(0.5f, 1f);

      // We distribute the shapes in the rectangular path.
      _softBodies = PathManager.EvenlyDistributeShapesAlongPath(World, rectanglePath, shapes, BodyType.Dynamic, 30);

      // Attach the bodies together with revolute joints. The rectangular form will converge to a circular form.
      PathManager.AttachBodiesWithRevoluteJoint(World, _softBodies, new Vector2(0f, -0.5f), new Vector2(0f, 0.5f), true, true);

      // GFX
      _bridgeBox = new Sprite(ContentWrapper.TextureFromShape(shape, ContentWrapper.Orange, ContentWrapper.Brown));
      _softBodyBox = new Sprite(ContentWrapper.TextureFromShape(shapes[0], ContentWrapper.Green, ContentWrapper.Black));
      _softBodyBox.Origin += new Vector2(ConvertUnits.ToDisplayUnits(0.1f), 0f);
      _softBodyCircle = new Sprite(ContentWrapper.TextureFromShape(shapes[1], ContentWrapper.Lime, ContentWrapper.Grey));
    }

    public override void Draw(GameTime gameTime)
    {
      Sprites.Begin(0, null, null, null, null, null, Camera.View);
      foreach (Body body in _softBodies)
      {
        Sprites.Draw(_softBodyBox.Image, ConvertUnits.ToDisplayUnits(body.Position), null, Color.White, body.Rotation, _softBodyBox.Origin, 1f, SpriteEffects.None, 0f);
      }
      foreach (Body body in _softBodies)
      {
        Sprites.Draw(_softBodyCircle.Image, ConvertUnits.ToDisplayUnits(body.Position), null, Color.White, body.Rotation, _softBodyCircle.Origin, 1f, SpriteEffects.None, 0f);
      }
      foreach (Body body in _bridgeBodies)
      {
        Sprites.Draw(_bridgeBox.Image, ConvertUnits.ToDisplayUnits(body.Position), null, Color.White, body.Rotation, _bridgeBox.Origin, 1f, SpriteEffects.None, 0f);
      }
      Sprites.End();

      _border.Draw(Camera.SimProjection, Camera.SimView);

      base.Draw(gameTime);
    }
  }
}