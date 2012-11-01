using System.Text;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.DemoShare;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.ScreenSystem;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Samples
{
    internal class Demo4Screen : GameScreen, IDemoScreen
    {
#if XBOX
    //Xbox360 can't handle as many geometries
        private const int PyramidBaseBodyCount = 10;
#else
        private const int PyramidBaseBodyCount = 16;
#endif

        private Agent _agent;

        public override void Initialize()
        {
            World = new World(new Vector2(0, -20));

            base.Initialize();
        }

        public override void LoadContent()
        {
            _agent = new Agent(World, new Vector2(5, 10));

            Vertices box = PolygonTools.CreateRectangle(0.5f, 0.5f);
            PolygonShape shape = new PolygonShape(box, 1);

            Vector2 x = new Vector2(-8.0f, -17.0f);
            Vector2 deltaX = new Vector2(0.5625f, 1.25f);
            Vector2 deltaY = new Vector2(1.125f, 0.0f);

            for (int i = 0; i < PyramidBaseBodyCount; ++i)
            {
                Vector2 y = x;

                for (int j = i; j < PyramidBaseBodyCount; ++j)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = y;
                    body.CreateFixture(shape);

                    y += deltaY;
                }

                x += deltaX;
            }

            base.LoadContent();
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
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }
    }
}