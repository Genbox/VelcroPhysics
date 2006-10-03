using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using FarseerGames.FarseerXNAPhysics;
using FarseerGames.FarseerXNAPhysics.Dynamics;

namespace FarseerDemo1 {
    /// <summary>
    /// DEMO1(HELLO WORLD):
    /// This is the "Hello World" demo for the 1st release of the Farseer Physics Engine.
    /// This is a modified version of the "How To: Rotate a Sprite" demo from the XNA 
    /// documentation.
    /// HOW IT WORKS: 
    /// A sprite is created and a rigid body is created. The rigid body, once
    /// added to the physics simulator, is moved around and rotated by applying forces and torques
    /// in response to key-press events.  
    /// The sprite simply applies the position and rotation of the rigid body to itself and draws itself every
    /// loop.            
    /// </summary>
    partial class DemoGame : Microsoft.Xna.Framework.Game {
        private bool exiting = false;

        private SpriteBatch spriteBatch;
        private Texture2D xSpriteTexture;
        private Vector2 origin;

        //declare variables for PhysicsSimulator and a single RigidBody
        private PhysicsSimulator physicsSimulator;
        private RigidBody xRigidBody;
        private float forceMagnitude = 100;
        private float torqueMagnitude = 500; 
        

        public DemoGame() {
            InitializeComponent();
        }

        protected override void OnStarting() {
            base.OnStarting();
            this.Window.Title = "Farseer Physics Demo 1 -- Keys: (A,S,D,W) (Left Arrow, Right Arrow)";
   
            //new-up an instance of the PhysicsSimulator.
            Vector2 gravity = new Vector2(0,0);
            physicsSimulator = new PhysicsSimulator(gravity);

            LoadResources();
        }

        private void LoadResources(){
            //sprite stuff
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            xSpriteTexture = Texture2D.FromFile(graphics.GraphicsDevice, @"Textures\X.jpg");
            TextureInformation textureInfo = Texture2D.GetTextureInformation(@"Textures\X.jpg");
            origin.X = textureInfo.Width / 2f;
            origin.Y = textureInfo.Height / 2f;

            //new-up a rectangular RigidBody that matches the dimenstions of the sprite
            xRigidBody = new RectangleRigidBody(textureInfo.Width, textureInfo.Height, 1);
            xRigidBody.Position = new Vector2(400, 300);

            //add the RigidBody to the PhysicsSimulator
            physicsSimulator.Add(xRigidBody);
        }

        protected override void Update() {
            if (exiting) { return; }
            float elapsed = (float)ElapsedTime.TotalSeconds;

            //Update the PhysicsSimulator
            physicsSimulator.Update(elapsed);

            UpdateComponents();
        }

        protected override void Draw() {
            if (!graphics.EnsureDevice())
                return;

            graphics.GraphicsDevice.Clear(Color.DarkSlateBlue);
            graphics.GraphicsDevice.BeginScene();

            spriteBatch.Begin();
            //Draw the sprite using position and orientation from the RigidBody
            spriteBatch.Draw(xSpriteTexture, xRigidBody.Position, null, Color.White, xRigidBody.Orientation,origin,1f,SpriteEffects.None,0f);
            spriteBatch.End();

            DrawComponents();

            graphics.GraphicsDevice.EndScene();
            graphics.GraphicsDevice.Present();
        }

        private void graphics_DeviceReset(object sender, EventArgs e) {
            LoadResources();
        }

        //apply forces and torques based on key presses. A,S,D,W and Left, Right
        private void keyboardInputComponent_AKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            xRigidBody.ApplyForce(new Vector2(-forceMagnitude,0));
        }

        private void keyboardInputComponent_DKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            xRigidBody.ApplyForce(new Vector2(forceMagnitude, 0));
        }

        private void keyboardInputComponent_SKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            xRigidBody.ApplyForce(new Vector2(0,forceMagnitude));
        }

        private void keyboardInputComponent_WKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            xRigidBody.ApplyForce(new Vector2(0,-forceMagnitude));
        }

        private void keyboardInputComponent_LeftKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            xRigidBody.ApplyTorque(-torqueMagnitude);
        }

        private void keyboardInputComponent_RightKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            xRigidBody.ApplyTorque(torqueMagnitude);
        }

        private void keyboardInputComponent_EscapeKeyDown(object sender, FarseerGames.Components.KeyEventArgs e) {
            Exit();
            exiting = true;
        }
    }
}