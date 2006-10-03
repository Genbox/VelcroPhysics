using System;

namespace FarseerGames.FarseerXNATestharness {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args) {
            using (FarseerXNATestharness game = new FarseerXNATestharness()) {
                game.Run();
            }
        }
    }
}

