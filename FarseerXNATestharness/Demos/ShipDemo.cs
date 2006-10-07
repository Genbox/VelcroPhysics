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
        private ShipEntity ship;
        private ShipEntityView shipView;
        private ShipKeyboardEntityController shipKeyboardController;

        public ShipDemo() {
            InitializeComponent();
        }

        public override void Start() {
            // TODO: Add your start up code here
            base.Start();
            ship = new ShipEntity(physicsSimulator);
            shipView = new ShipEntityView(ship, "sprite.png", spriteManager);
            shipKeyboardController = new ShipKeyboardEntityController(ship,keyboardInputService);

            ship.Position = new Vector2(400, 400);
        }

        //public override void Update() {
        //    // TODO: Add your update code here
        //}

        //public override void Draw() {
        //    // TODO: Add your drawing code here
            
        //}
    }
}