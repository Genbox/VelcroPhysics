using System.Collections.Generic;
using System.Text;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.SamplesFramework
{
    internal class AdvancedDemo2 : PhysicsGameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Path generator";
        }

        public string GetDetails()
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

        #endregion

        private Border _border;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0, 9.82f);

            _border = new Border(World, this, ScreenManager.GraphicsDevice.Viewport);

            /* Bridge */
            //We make a path using 2 points.
            Path bridgePath = new Path();
            bridgePath.Add(new Vector2(-15, 5));
            bridgePath.Add(new Vector2(15, 5));
            bridgePath.Closed = false;

            Vertices box = PolygonTools.CreateRectangle(0.125f, 0.5f);
            PolygonShape shape = new PolygonShape(box, 20);

            List<Body> bridgeBodies = PathManager.EvenlyDistributeShapesAlongPath(World, bridgePath, shape,
                                                                                  BodyType.Dynamic, 29);

            //Attach the first and last fixtures to the world
            JointFactory.CreateFixedRevoluteJoint(World, bridgeBodies[0], new Vector2(0f, -0.5f), bridgeBodies[0].Position);
            JointFactory.CreateFixedRevoluteJoint(World, bridgeBodies[bridgeBodies.Count - 1], new Vector2(0, 0.5f),
                                                  bridgeBodies[bridgeBodies.Count - 1].Position);

            PathManager.AttachBodiesWithRevoluteJoint(World, bridgeBodies, new Vector2(0f, -0.5f), new Vector2(0f, 0.5f),
                                                      false, true);

            /* Soft body */
            //We make a rectangular path.
            Path rectanglePath = new Path();
            rectanglePath.Add(new Vector2(-6, -11));
            rectanglePath.Add(new Vector2(-6, 1));
            rectanglePath.Add(new Vector2(6, 1));
            rectanglePath.Add(new Vector2(6, -11));
            rectanglePath.Closed = true;

            //Creating two shapes. A circle to form the circle and a rectangle to stabilize the soft body.
            List<Shape> shapes = new List<Shape>(2);
            shapes.Add(new PolygonShape(PolygonTools.CreateRectangle(0.5f, 0.5f, new Vector2(-0.1f, 0f), 0f), 1f));
            shapes.Add(new CircleShape(0.5f, 1f));

            //We distribute the shapes in the rectangular path.
            List<Body> bodies = PathManager.EvenlyDistributeShapesAlongPath(World, rectanglePath, shapes,
                                                                            BodyType.Dynamic, 30);

            //Attach the bodies together with revolute joints. The rectangular form will converge to a circular form.
            PathManager.AttachBodiesWithRevoluteJoint(World, bodies, new Vector2(0f, -0.5f), new Vector2(0f, 0.5f), true,
                                                      true);
        }

        public override void Draw(GameTime gameTime)
        {
            _border.Draw();
            base.Draw(gameTime);
        }

    }
}