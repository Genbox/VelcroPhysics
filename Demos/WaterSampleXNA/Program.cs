using System;

namespace WaterSampleXNA
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (FarseerPhysicsGame game = new FarseerPhysicsGame())
            {
                game.Run();
            }
        }
    }
}

