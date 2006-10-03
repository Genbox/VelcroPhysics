using System;

namespace FarseerDemo1 {
    partial class DemoGame {
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.graphics = new Microsoft.Xna.Framework.Components.GraphicsComponent();
            this.keyboardInputComponent = new FarseerGames.Components.KeyboardInputComponent();
            // 
            // graphics
            // 
            this.graphics.BackBufferHeight = 600;
            this.graphics.BackBufferWidth = 800;
            this.graphics.DeviceReset += new System.EventHandler(this.graphics_DeviceReset);
            // 
            // keyboardInputComponent
            // 
            this.keyboardInputComponent.LeftKeyPressed += new System.EventHandler<FarseerGames.Components.KeyEventArgs>(this.keyboardInputComponent_LeftKeyPressed);
            this.keyboardInputComponent.SKeyPressed += new System.EventHandler<FarseerGames.Components.KeyEventArgs>(this.keyboardInputComponent_SKeyPressed);
            this.keyboardInputComponent.AKeyPressed += new System.EventHandler<FarseerGames.Components.KeyEventArgs>(this.keyboardInputComponent_AKeyPressed);
            this.keyboardInputComponent.RightKeyPressed += new System.EventHandler<FarseerGames.Components.KeyEventArgs>(this.keyboardInputComponent_RightKeyPressed);
            this.keyboardInputComponent.EscapeKeyDown += new System.EventHandler<FarseerGames.Components.KeyEventArgs>(this.keyboardInputComponent_EscapeKeyDown);
            this.keyboardInputComponent.WKeyPressed += new System.EventHandler<FarseerGames.Components.KeyEventArgs>(this.keyboardInputComponent_WKeyPressed);
            this.keyboardInputComponent.DKeyPressed += new System.EventHandler<FarseerGames.Components.KeyEventArgs>(this.keyboardInputComponent_DKeyPressed);
            this.GameComponents.Add(this.graphics);
            this.GameComponents.Add(this.keyboardInputComponent);

        }

        private Microsoft.Xna.Framework.Components.GraphicsComponent graphics;
        private FarseerGames.Components.KeyboardInputComponent keyboardInputComponent;
    }
}
