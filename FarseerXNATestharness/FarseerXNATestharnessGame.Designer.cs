using System;

namespace FarseerGames.FarseerXNATestharness {
    partial class FarseerXNATestharnessGame {
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FarseerXNATestharnessGame));
            this.graphics = new Microsoft.Xna.Framework.Components.GraphicsComponent();
            this.farseerPhysicsComponent = new FarseerGames.FarseerXNAGame.Components.FarseerPhysicsComponent();
            this.spriteManagerComponent = new FarseerGames.FarseerXNAGame.Components.SpriteManagerComponent();
            this.shipDemo = new FarseerGames.FarseerXNATestharness.Demos.ShipDemo();
            this.keyboardInputComponent = new FarseerGames.FarseerXNAGame.Components.KeyboardInputComponent();
            // 
            // farseerPhysicsComponent
            // 
            this.farseerPhysicsComponent.AllowedCollisionPenetration = 0.01F;
            this.farseerPhysicsComponent.Gravity = ((Microsoft.Xna.Framework.Vector2)(resources.GetObject("farseerPhysicsComponent.Gravity")));
            this.farseerPhysicsComponent.ImpulseBiasFactor = 0.8F;
            this.farseerPhysicsComponent.IterationsPerCollision = 3;
            this.GameComponents.Add(this.keyboardInputComponent);
            this.GameComponents.Add(this.graphics);
            this.GameComponents.Add(this.farseerPhysicsComponent);
            this.GameComponents.Add(this.spriteManagerComponent);
            this.GameComponents.Add(this.shipDemo);

        }

        private Microsoft.Xna.Framework.Components.GraphicsComponent graphics;
        private FarseerXNAGame.Components.FarseerPhysicsComponent farseerPhysicsComponent;
        private FarseerGames.FarseerXNAGame.Components.SpriteManagerComponent spriteManagerComponent;
        private FarseerGames.FarseerXNATestharness.Demos.ShipDemo shipDemo;
        private FarseerGames.FarseerXNAGame.Components.KeyboardInputComponent keyboardInputComponent;
    }
}
