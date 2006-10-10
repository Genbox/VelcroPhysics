using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using FarseerGames.FarseerXNATestharness.Ships;

namespace FarseerGames.FarseerXNATestharness.Demos {
    public partial class ShipDemo : FarseerDemoComponent {
        private Ship ship;
        private ShipKeyboardEntityController shipKeyboardController;

        public ShipDemo() {
            InitializeComponent();
        }

        public override void Start() {
            // TODO: Add your start up code here
            base.Start();
            shipKeyboardController = new ShipKeyboardEntityController(keyboardInputService);
            ship = new Ship(physicsSimulator, spriteManager,shipKeyboardController);
        }
    }
}