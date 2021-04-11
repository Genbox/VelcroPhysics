using System;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo
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