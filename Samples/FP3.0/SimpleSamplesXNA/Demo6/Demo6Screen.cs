using System.Text;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;

namespace SimpleSamplesXNA.Demo6
{
    internal class Demo6Screen : GameScreen
    {
        public string GetTitle()
        {
            return "Demo6: Linear and Angular Springs";
        }

        private string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows the use of angular and linear");
            sb.AppendLine("springs");
            sb.AppendLine(string.Empty);
            sb.AppendLine("The levers are connected to the walls using");
            sb.AppendLine("revolute joints and they each have an angular");
            sb.AppendLine("spring attached.");
            sb.AppendLine("The hanging squares are connected by linear");
            sb.AppendLine("springs.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate: left and right triggers");
            sb.AppendLine("  -Move: left thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate: left and right arrows");
            sb.AppendLine("  -Move: A,S,D,W");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }

    }
}