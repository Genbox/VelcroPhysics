using System;
using System.Text;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.SamplesFramework
{
    internal class SimpleDemo8 : PhysicsGameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Restitution";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows several bodys with varying restitution.");
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

        private Body[] _circle = new Body[6];

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, -20f);

            new Border(World, ScreenManager.GraphicsDevice.Viewport);

            Vector2 _position = new Vector2(-15f, 8f);
            float _restitution = 0f;

            for (int i = 0; i < 6; ++i)
            {
                _circle[i] = BodyFactory.CreateCircle(World, 1.5f, 1f, _position);
                _circle[i].BodyType = BodyType.Dynamic;
                _circle[i].Restitution = _restitution;
                _position.X += 6f;
                _restitution += 0.2f;
            }
        }
    }
}