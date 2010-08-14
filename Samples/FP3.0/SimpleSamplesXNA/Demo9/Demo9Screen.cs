using System.Text;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;

namespace SimpleSamplesXNA.Demo9
{
    internal class Demo9Screen : GameScreen
    {
        public string GetTitle()
        {
            return "Demo9: Ragdoll";
        }

        private string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to combine physics objects");
            sb.AppendLine("to create a ragdoll");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }
    }
}