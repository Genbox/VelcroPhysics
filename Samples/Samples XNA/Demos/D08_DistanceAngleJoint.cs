using System.Text;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Samples.Demos.Prefabs;
using FarseerPhysics.Samples.MediaSystem;
using FarseerPhysics.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.Samples.Demos
{
    internal class D08_DistanceAngleJoint : PhysicsDemoScreen
    {
        private Border _border;
        private Body _obstacles;
        private Body[] _angleBody = new Body[3];
        private Body[] _distanceBody = new Body[4];

        private Sprite _angleCube;
        private Sprite _distanceCube;

        #region Demo description
        public override string GetTitle()
        {
            return "Distance & angle joints";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows several bodies connected by distance and angle joints.");
            sb.AppendLine("Orange bodies are forced to have the same angle at all times.");
            sb.AppendLine();
            sb.AppendLine("Striped bodies are forced to have the same distance at all times.");
            sb.AppendLine("Two of them have a rigid distance joint.");
            sb.AppendLine("The other two have a soft (spring-like) distance joint.");
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

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, 20f);

            _border = new Border(World, Lines, Framework.GraphicsDevice);

            _obstacles = new Body(World);
            FixtureFactory.AttachEdge(new Vector2(-16f, -1f), new Vector2(-14f, 1f), _obstacles);
            FixtureFactory.AttachEdge(new Vector2(-14f, 1f), new Vector2(-12f, -1f), _obstacles);

            FixtureFactory.AttachEdge(new Vector2(14f, -1f), new Vector2(12f, 5f), _obstacles);
            FixtureFactory.AttachEdge(new Vector2(14f, -1f), new Vector2(16f, 5f), _obstacles);

            _angleBody[0] = BodyFactory.CreateRectangle(World, 1.5f, 1.5f, 1f);
            _angleBody[0].BodyType = BodyType.Dynamic;
            _angleBody[0].Friction = 0.7f;
            _angleBody[0].Position = new Vector2(-15f, -5f);
            _angleBody[1] = BodyFactory.CreateRectangle(World, 1.5f, 1.5f, 1f);
            _angleBody[1].BodyType = BodyType.Dynamic;
            _angleBody[1].Friction = 0.7f;
            _angleBody[1].Position = new Vector2(-18f, 5f);
            _angleBody[2] = BodyFactory.CreateRectangle(World, 1.5f, 1.5f, 1f);
            _angleBody[2].BodyType = BodyType.Dynamic;
            _angleBody[2].Friction = 0.7f;
            _angleBody[2].Position = new Vector2(-10f, 5f);

            World.AddJoint(new AngleJoint(_angleBody[0], _angleBody[1]));
            World.AddJoint(new AngleJoint(_angleBody[0], _angleBody[2]));

            _distanceBody[0] = BodyFactory.CreateRectangle(World, 1.5f, 1.5f, 1f);
            _distanceBody[0].BodyType = BodyType.Dynamic;
            _distanceBody[0].Friction = 0.7f;
            _distanceBody[0].Position = new Vector2(11.5f, -4f);
            _distanceBody[1] = BodyFactory.CreateRectangle(World, 1.5f, 1.5f, 1f);
            _distanceBody[1].BodyType = BodyType.Dynamic;
            _distanceBody[1].Friction = 0.7f;
            _distanceBody[1].Position = new Vector2(16.5f, -4f);
            _distanceBody[2] = BodyFactory.CreateRectangle(World, 1.5f, 1.5f, 1f);
            _distanceBody[2].BodyType = BodyType.Dynamic;
            _distanceBody[2].Friction = 0.7f;
            _distanceBody[2].Position = new Vector2(11.5f, -6f);
            _distanceBody[3] = BodyFactory.CreateRectangle(World, 1.5f, 1.5f, 1f);
            _distanceBody[3].BodyType = BodyType.Dynamic;
            _distanceBody[3].Friction = 0.7f;
            _distanceBody[3].Position = new Vector2(16.5f, -6f);

            DistanceJoint softDistance = new DistanceJoint(_distanceBody[0], _distanceBody[1], Vector2.Zero, Vector2.Zero, false);
            softDistance.DampingRatio = 0.3f;
            softDistance.Frequency = 5f;
            World.AddJoint(softDistance);
            World.AddJoint(new DistanceJoint(_distanceBody[2], _distanceBody[3], Vector2.Zero, Vector2.Zero, false));

            // create sprites based on bodies
            _angleCube = new Sprite(ContentWrapper.TextureFromShape(_angleBody[0].FixtureList[0].Shape, "Square", ContentWrapper.Gold, ContentWrapper.Orange, ContentWrapper.Grey, 1f));
            _distanceCube = new Sprite(ContentWrapper.TextureFromShape(_distanceBody[0].FixtureList[0].Shape, "Stripe", ContentWrapper.Red, ContentWrapper.Blue, ContentWrapper.Grey, 4f));
        }

        public override void Draw(GameTime gameTime)
        {
            Lines.Begin(Camera.SimProjection, Camera.SimView);

            foreach (Fixture f in _obstacles.FixtureList)
            {
                Lines.DrawLine(_distanceBody[0].Position, _distanceBody[1].Position, ContentWrapper.Black);
                Lines.DrawLine(_distanceBody[2].Position, _distanceBody[3].Position, ContentWrapper.Black);
            }

            Lines.End();
            Sprites.Begin(0, null, null, null, null, null, Camera.View);

            for (int i = 0; i < 3; i++)
            {
                Sprites.Draw(_angleCube.Image, ConvertUnits.ToDisplayUnits(_angleBody[i].Position), null, Color.White, _angleBody[i].Rotation, _angleCube.Origin, 1f, SpriteEffects.None, 0f);
            }

            for (int i = 0; i < 4; i++)
            {
                Sprites.Draw(_distanceCube.Image, ConvertUnits.ToDisplayUnits(_distanceBody[i].Position), null, Color.White, _distanceBody[i].Rotation, _distanceCube.Origin, 1f, SpriteEffects.None, 0f);
            }

            Sprites.End();
            Lines.Begin(Camera.SimProjection, Camera.SimView);
            
            foreach (Fixture f in _obstacles.FixtureList)
            {
                Lines.DrawLineShape(f.Shape, ContentWrapper.Black);
            }

            Lines.End();

            _border.Draw(Camera.SimProjection, Camera.SimView);

            base.Draw(gameTime);
        }
    }
}