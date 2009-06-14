namespace FarseerGames.AdvancedSamplesXNA
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            using (FarseerPhysicsGame game = new FarseerPhysicsGame())
            {
                game.Run();
            }
        }
    }
}