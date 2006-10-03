using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using FarseerGames.FarseerXNAGame;

using FarseerGames.FarseerXNAPhysics.Collisions;
using FarseerGames.FarseerXNAPhysics.Dynamics;
using FarseerGames.FarseerXNAPhysics;

using FarseerGames.FarseerXNATestharness.Entities;

namespace FarseerGames.FarseerXNATestharness {
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    partial class FarseerXNATestharness : FarseerGame  {
        private bool _exiting = false;
        private bool isDrawn = false;
        public TimeSpan ElapsedGameTime;
        
        public FarseerXNATestharness() {
            InitializeComponent();
        }

        protected override void OnStarting() {
            base.OnStarting();
            Window.Title = "Farseer XNA Testharness";
        }

        private void LoadResources() {
            
        }

        protected override void Update() {
            if (!isDrawn) { return; }
            if (_exiting) { return; }
            ElapsedGameTime = this.ElapsedTime;

            UpdateComponents();
        }

        protected override void Draw() {
            isDrawn = true;
            if (!farseerGraphicsComponent.EnsureDevice())
                return;

            farseerGraphicsComponent.GraphicsDevice.Clear(Color.DarkSlateBlue);
            farseerGraphicsComponent.GraphicsDevice.BeginScene();
  
            // Let the GameComponents draw
            DrawComponents();

            farseerGraphicsComponent.GraphicsDevice.EndScene();
            farseerGraphicsComponent.GraphicsDevice.Present();
        }

        private void keyboardInputComponent1_EscapeKeyDown(object sender, FarseerGames.FarseerXNAGame.Components.KeyEventArgs e) {
            Exit();
            _exiting = true;
        }

        private void farseerGraphicsComponent_DeviceCreated(object sender, EventArgs e) {

        }

        private void farseerGraphicsComponent_DeviceReset(object sender, EventArgs e) {

        }
    }
}