using System.Text;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;

namespace SimpleSamplesXNA.Demo5
{
    internal class Demo5Screen : GameScreen
    {
        public string GetTitle()
        {
            return "Demo5: Collision Categories";
        }

        private string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to setup complex collision");
            sb.AppendLine("scenerios.");
            sb.AppendLine("In this demo:");
            sb.AppendLine("-Red, Green, and Blue are set to only collide with");
            sb.AppendLine(" their own color.");
            sb.AppendLine("-Black is set to collide with itself, Red, Green, ");
            sb.AppendLine(" and Blue.");
            sb.AppendLine("-The 'Agent' (the cross thing) is set to collide");
            sb.AppendLine(" with all but Black");
            sb.AppendLine(string.Empty);
            sb.AppendLine("NOTE: If two objects define conflicting");
            sb.AppendLine("collision status, collide wins over not colliding.");
            sb.AppendLine("This is the case with Black vs. the Red, Green, ");
            sb.AppendLine("and Blue circles");
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