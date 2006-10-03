using System;

namespace FarseerGames.FarseerXNATestharness {
    partial class FarseerXNATestharness {
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FarseerXNATestharness));
            this.framerateComponent = new FarseerGames.FarseerXNAGame.Components.FrameRateComponent();
            this.keyboardInputComponent1 = new FarseerGames.FarseerXNAGame.Components.KeyboardInputComponent();
            this.farseerGraphicsComponent = new FarseerGames.FarseerXNAGame.FarseerGraphicsComponent();
            this.farseerPhysicsComponent = new FarseerGames.FarseerXNATestharness.Components.FarseerPhysicsComponent();
            this.farseerDemo21 = new FarseerGames.FarseerXNATestharness.Samples.FarseerDemo2();
            // 
            // keyboardInputComponent1
            // 
            this.keyboardInputComponent1.EscapeKeyDown += new System.EventHandler<FarseerGames.FarseerXNAGame.Components.KeyEventArgs>(this.keyboardInputComponent1_EscapeKeyDown);
            // 
            // farseerGraphicsComponent
            // 
            this.farseerGraphicsComponent.BackBufferHeight = 768;
            this.farseerGraphicsComponent.BackBufferWidth = 1024;
            // 
            // farseerPhysicsComponent
            // 
            this.farseerPhysicsComponent.AllowedCollisionPenetration = 0.1F;
            this.farseerPhysicsComponent.Gravity = ((Microsoft.Xna.Framework.Vector2)(resources.GetObject("farseerPhysicsComponent.Gravity")));
            this.farseerPhysicsComponent.ImpulseBiasFactor = 0.8F;
            this.farseerPhysicsComponent.IterationsPerCollision = 5;
            // 
            // farseerDemo21
            // 
            this.farseerDemo21.KeyboardInputComponent = this.keyboardInputComponent1;
            this.farseerDemo21.ShipHeight = 1F;
            this.farseerDemo21.ShipStartPosition = ((Microsoft.Xna.Framework.Vector2)(resources.GetObject("farseerDemo21.ShipStartPosition")));
            this.farseerDemo21.ShipThrust = 100F;
            this.farseerDemo21.ShipTurningTorque = 10F;
            this.farseerDemo21.ShipWidth = 1F;
            // 
            // FarseerXNATestharness
            // 
            this.AllowUserResizing = true;
            this.IsFixedTimeStep = false;
            this.IsMouseVisible = true;
            this.GameComponents.Add(this.framerateComponent);
            this.GameComponents.Add(this.farseerPhysicsComponent);
            this.GameComponents.Add(this.keyboardInputComponent1);
            this.GameComponents.Add(this.farseerDemo21);
            this.GameComponents.Add(this.farseerGraphicsComponent);

        }
        private FarseerGames.FarseerXNAGame.Components.FrameRateComponent framerateComponent;
        private FarseerGames.FarseerXNATestharness.Components.FarseerPhysicsComponent farseerPhysicsComponent;
        private FarseerGames.FarseerXNAGame.Components.KeyboardInputComponent keyboardInputComponent1;
        private FarseerGames.FarseerXNATestharness.Samples.FarseerDemo2 farseerDemo21;
        private FarseerGames.FarseerXNAGame.FarseerGraphicsComponent farseerGraphicsComponent;
    }
}
