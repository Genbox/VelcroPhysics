using System;

namespace FarseerGames.FarseerXNATestharness {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args) {
            using (FarseerXNATestharnessGame game = new FarseerXNATestharnessGame()) {
                game.Run();
            }
        }
    }
}

