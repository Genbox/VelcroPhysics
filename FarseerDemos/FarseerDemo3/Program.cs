using System;

namespace FarseerDemo3 {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args) {
            using (DemoGame game = new DemoGame()) {
                game.Run();
            }
        }
    }
}

