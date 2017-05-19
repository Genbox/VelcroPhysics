using System;

namespace VelcroPhysics.Samples.Samples2
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            using (Game1 game = new Game1())
                game.Run();
        }
    }
}