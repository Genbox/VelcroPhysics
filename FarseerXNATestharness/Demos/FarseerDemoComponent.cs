using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using FarseerGames.FarseerXNAGame.Sprites;
using FarseerGames.FarseerXNAGame.Input;

using FarseerGames.FarseerXNAPhysics;

namespace FarseerGames.FarseerXNATestharness.Demos {
    public partial class FarseerDemoComponent : Microsoft.Xna.Framework.GameComponent {
        protected PhysicsSimulator physicsSimulator;
        protected SpriteManager spriteManager;
        protected IKeyboardInputService keyboardInputService;

        public FarseerDemoComponent() {
            InitializeComponent();
        }

        public override void Start() {
            this.physicsSimulator = Game.GameServices.GetService<PhysicsSimulator>();
            this.spriteManager = Game.GameServices.GetService<SpriteManager>();
            this.keyboardInputService = Game.GameServices.GetService<IKeyboardInputService>();
        }

        public override void Update() {
            // TODO: Add your update code here
        }

        public override void Draw() {
            // TODO: Add your drawing code here
        }
    }
}