using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using FarseerGames.FarseerXNAGame;
using FarseerGames.FarseerXNAPhysics;


namespace FarseerGames.FarseerXNATestharness.Samples {
    public partial class FarseerDemo : GameComponent {
        protected PhysicsSimulator _physicsSimulator;

        public FarseerDemo() {
            InitializeComponent();
        }

        public override void Start() {
            _physicsSimulator = Game.GameServices.GetService<PhysicsSimulator>();           
        }

        public override void Update() {
            // TODO: Add your update code here
        }

        public override void Draw() {
            // TODO: Add your drawing code here
        }
    }
}