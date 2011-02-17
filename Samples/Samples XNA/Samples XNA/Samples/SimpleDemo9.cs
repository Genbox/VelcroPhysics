using System.Text;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.SamplesFramework
{
    internal class SimpleDemo9 : PhysicsGameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Friction";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows several bodys with varying friction.");
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

        private Body[] _rectangle = new Body[5];

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, 20f);

            new Border(World, ScreenManager.GraphicsDevice.Viewport);

            Body _temp;
            _temp = BodyFactory.CreateEdge(World, new Vector2(-20f, -11.2f), new Vector2(10f, -3.8f));
            _temp = BodyFactory.CreateEdge(World, new Vector2(12f, -5.6f), new Vector2(12f, -3.2f));

            _temp = BodyFactory.CreateEdge(World, new Vector2(-10f, 4.4f), new Vector2(20f, -1.4f));
            _temp = BodyFactory.CreateEdge(World, new Vector2(-12f, 2.6f), new Vector2(-12f, 5f));

            _temp = BodyFactory.CreateEdge(World, new Vector2(-20f, 6.8f), new Vector2(10f, 11.5f));

            float[] friction = new[] { 0.75f, 0.45f, 0.28f, 0.17f, 0.0f };
            for (int i = 0; i < 5; ++i)
            {
                _rectangle[i] = BodyFactory.CreateRectangle(World, 1.5f, 1.5f, 1f);
                _rectangle[i].BodyType = BodyType.Dynamic;
                _rectangle[i].Position = new Vector2(-18f + 5.2f * i, -13.0f + 1.282f * i);
                _rectangle[i].Friction = friction[i];
            }
        }
    }
}