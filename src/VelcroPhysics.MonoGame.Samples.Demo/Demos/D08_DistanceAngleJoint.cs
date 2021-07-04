using System.Text;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem.Graphics;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.Screens;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos
{
    internal class D08_DistanceAngleJoint : PhysicsDemoScreen
    {
        private readonly Body[] _angleBody = new Body[3];
        private readonly Body[] _distanceBody = new Body[4];

        private Sprite _angleCube;
        private Sprite _distanceCube;
        private Body _obstacles;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, 20f);

            _obstacles = BodyFactory.CreateBody(World);
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

            DistanceJoint softDistance = new DistanceJoint(_distanceBody[0], _distanceBody[1], Vector2.Zero, Vector2.Zero);
            JointHelper.LinearStiffness(5f, 0.3f, softDistance.BodyA, softDistance.BodyB, out var stiffness, out var damping);
            softDistance.Damping = damping;
            softDistance.Stiffness = stiffness;
            World.AddJoint(softDistance);
            World.AddJoint(new DistanceJoint(_distanceBody[2], _distanceBody[3], Vector2.Zero, Vector2.Zero));

            // create sprites based on bodies
            _angleCube = new Sprite(Managers.TextureManager.TextureFromShape(_angleBody[0].FixtureList[0].Shape, "Square", Colors.Gold, Colors.Orange, Colors.Grey, 1f));
            _distanceCube = new Sprite(Managers.TextureManager.TextureFromShape(_distanceBody[0].FixtureList[0].Shape, "Stripe", Colors.Red, Colors.Blue, Colors.Grey, 4f));
        }

        public override void Draw()
        {
            Lines.Begin(ref Camera.SimProjection, ref Camera.SimView);

            foreach (Fixture f in _obstacles.FixtureList)
            {
                Lines.DrawLine(_distanceBody[0].Position, _distanceBody[1].Position, Colors.Black);
                Lines.DrawLine(_distanceBody[2].Position, _distanceBody[3].Position, Colors.Black);
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
            Lines.Begin(ref Camera.SimProjection, ref Camera.SimView);

            foreach (Fixture f in _obstacles.FixtureList)
            {
                Lines.DrawLineShape(f.Shape, Colors.Black);
            }

            Lines.End();

            base.Draw();
        }

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
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to demo selection: Escape");
            sb.AppendLine();
            sb.AppendLine("Mouse:");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: Move mouse");
#elif XBOX
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Move cursor: Left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: Left thumbstick");
            sb.AppendLine("  - Exit to demo selection: Back button");
#endif
            return sb.ToString();
        }
    }
}