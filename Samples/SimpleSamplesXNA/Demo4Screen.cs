using System.Text;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.DemoShare;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.SimpleSamplesXNA
{
    internal class Demo4Screen : PhysicsGameScreen, IDemoScreen
    {
#if XBOX
    //Xbox360 can't handle as many geometries
        private const int PyramidBaseBodyCount = 10;
#else
        private const int PyramidBaseBodyCount = 14;
#endif

        private Agent _agent;

        public override void LoadContent()
        {
            World = new World(new Vector2(0, -20));
            base.LoadContent();

            _agent = new Agent(World, new Vector2(5, 10));

            Vertices box = PolygonTools.CreateRectangle(0.75f, 0.75f);
            PolygonShape shape = new PolygonShape(box, 1);

            Vector2 x = new Vector2(-14.0f, -23.0f);
            Vector2 deltaX = new Vector2(1.0f, 1.50f);
            Vector2 deltaY = new Vector2(2, 0.0f);

            DemoMaterial matBox = new DemoMaterial(MaterialType.Blank)
                                      {
                                          Color = Color.WhiteSmoke
                                      };

            for (int i = 0; i < PyramidBaseBodyCount; ++i)
            {
                Vector2 y = x;

                for (int j = i; j < PyramidBaseBodyCount; ++j)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = y;
                    body.CreateFixture(shape, matBox);

                    y += deltaY;
                }

                x += deltaX;
            }
        }

        public string GetTitle()
        {
            return "Demo4: Stacked Objects";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows the stacking stability of the engine.");
            sb.AppendLine("It shows a stack of rectangular bodies stacked in");
            sb.AppendLine("the shape of a pyramid.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate: left and right triggers");
            sb.AppendLine("  -Move: left thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }

        public override void HandleInput(InputHelper input)
        {
            if (input.CurrentGamepadState.IsConnected)
            {
                Vector2 force = 1000 * input.CurrentGamepadState.ThumbSticks.Left;
                _agent.Body.ApplyForce(force);

                float rotation = 400 * input.CurrentGamepadState.Triggers.Left;
                _agent.Body.ApplyTorque(rotation);

                rotation = -400 * input.CurrentGamepadState.Triggers.Right;
                _agent.Body.ApplyTorque(rotation);
            }

            base.HandleInput(input);
        }
    }
}