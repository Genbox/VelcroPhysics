using System.Text;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos.Prefabs;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem.Graphics;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.Screens;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos
{
    internal class D03_StaticBodies : PhysicsDemoScreen
    {
        private readonly Body[] _obstacles = new Body[5];
        private Agent _agent;
        private Sprite _obstacle;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, 20f);

            _agent = new Agent(World, new Vector2(-6.9f, -11f));

            // Obstacles
            for (int i = 0; i < 5; i++)
            {
                _obstacles[i] = BodyFactory.CreateRectangle(World, 5f, 1f, 1f);
                _obstacles[i].BodyType = BodyType.Static;
                _obstacles[i].Restitution = 0.2f;
                _obstacles[i].Friction = 0.2f;
            }

            _obstacles[0].Position = new Vector2(-5f, 9f);
            _obstacles[1].Position = new Vector2(15f, 6f);
            _obstacles[2].Position = new Vector2(10f, -3f);
            _obstacles[3].Position = new Vector2(-10f, -9f);
            _obstacles[4].Position = new Vector2(-17f, 0f);

            // create sprite based on body
            _obstacle = new Sprite(Managers.TextureManager.TextureFromShape(_obstacles[0].FixtureList[0].Shape, "Stripe", Colors.Gold, Colors.Black, Colors.Black, 1.5f));

            SetUserAgent(_agent.Body, 1000f, 400f);
        }

        public override void Draw()
        {
            Sprites.Begin(0, null, null, null, null, null, Camera.View);

            for (int i = 0; i < 5; ++i)
            {
                Sprites.Draw(_obstacle.Image, ConvertUnits.ToDisplayUnits(_obstacles[i].Position), null, Color.White, _obstacles[i].Rotation, _obstacle.Origin, 1f, SpriteEffects.None, 0f);
            }

            _agent.Draw(Sprites);
            Sprites.End();

            base.Draw();
        }

        public override string GetTitle()
        {
            return "Multiple fixtures and static bodies";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with multiple attached fixtures");
            sb.AppendLine("and different shapes attached.");
            sb.AppendLine("Several static bodies are placed as obstacles in the environment.");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Rotate object: Q, E");
            sb.AppendLine("  - Move object: W, S, A, D");
            sb.AppendLine("  - Exit to demo selection: Escape");
            sb.AppendLine();
            sb.AppendLine("Mouse:");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: Move mouse");
#elif XBOX
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Rotate object: Left and right trigger");
            sb.AppendLine("  - Move object: Right thumbstick");
            sb.AppendLine("  - Move cursor: Left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: Left thumbstick");
            sb.AppendLine("  - Exit to demo selection: Back button");
#endif
            return sb.ToString();
        }
    }
}