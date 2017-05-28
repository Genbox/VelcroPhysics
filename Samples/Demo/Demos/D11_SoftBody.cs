using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Dynamics.Joints;
using VelcroPhysics.Factories;
using VelcroPhysics.Samples.Demo.Demos.Prefabs;
using VelcroPhysics.Samples.Demo.MediaSystem;
using VelcroPhysics.Samples.Demo.ScreenSystem;
using VelcroPhysics.Shared;
using VelcroPhysics.Tools.PathGenerator;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Samples.Demo.Demos
{
    internal class D11_SoftBody : PhysicsDemoScreen
    {
        private Border _border;

        private List<Body> _bridgeBodies;

        private Sprite _bridgeBox;
        private List<Body> _softBodies;
        private Sprite _softBodyBox;
        private Sprite _softBodyCircle;

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

            Vertices box = PolygonUtils.CreateRectangle(0.125f, 0.5f);
            PolygonShape shape = new PolygonShape(box, 20);

            _bridgeBodies = PathManager.EvenlyDistributeShapesAlongPath(World, bridgePath, shape, BodyType.Dynamic, 29);

            // Attach the first and last fixtures to the world
            Body anchor = BodyFactory.CreateBody(World);
            anchor.BodyType = BodyType.Static;
            World.AddJoint(new RevoluteJoint(_bridgeBodies[0], anchor, _bridgeBodies[0].Position - new Vector2(0.5f, 0f), true));
            World.AddJoint(new RevoluteJoint(_bridgeBodies[_bridgeBodies.Count - 1], anchor, _bridgeBodies[_bridgeBodies.Count - 1].Position + new Vector2(0.5f, 0f), true));

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
            shapes[0] = new PolygonShape(PolygonUtils.CreateRectangle(0.5f, 0.5f, new Vector2(-0.1f, 0f), 0f), 1f);
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

        #region Demo description

        public override string GetTitle()
        {
            return "Soft body & path generator";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how a soft body and a bridge can be created, using");
            sb.AppendLine("the path generator and bodies connected with revolute joints.");
            sb.AppendLine();
            sb.AppendLine("GamePad:");
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
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.Append("  - Drag grabbed object: Move mouse");
#endif
            return sb.ToString();
        }

        #endregion
    }
}