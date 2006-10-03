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

namespace FarseerDemo2 {
    /// <summary>
    /// DEMO2(SIMPLE COLLISION):
    /// This demo adds 3 rigid body objects and demonstrates simple collision detection and response.
    /// HOW IT WORKS: 
    /// For this demo, the sprite and rigid body logic were combined into a "RigidBodySprite" class.  This 
    /// keeps the code a bit more clean in the "Game" object and it provides better re-use.
    /// There are 3 boxes on the screen, you can move the "N" box with the ASDW keys and rotate it using the 
    /// left and right arrow keys.  This demo also demonstrates gravity. Hitting the space bar will cycle gravity
    /// from DOWN to UP to RIGHT to LEFT and back to NONE.
    /// One last thing, this demo adds a simple ScreenCollisionBorder object. This is just a set of static rigid 
    /// bodies placed at the 4 screen edges to keep all the rigid bodies on screen.
    /// </summary>
    partial class DemoGame : Microsoft.Xna.Framework.Game {
        private bool exiting = false;

        private SpriteBatch spriteBatch;
        //used to contain all rigid bodies within the screen
        private ScreenCollisionBorder screenCollisionBorder;

        //these obejects combine sprite and rigid body logic into a single class
        private RigidBodySprite xRigidBodySprite;
        private RigidBodySprite nRigidBodySprite;
        private RigidBodySprite aRigidBodySprite;

        //simple variables to keep track of gravity state and strength
        private int gravityState = 0;
        private float gravityAcceleration = 200f;

        //declare variables for PhysicsSimulator and a single RigidBody
        private PhysicsSimulator physicsSimulator;
        private float forceMagnitude = 300; //linear force magnitude applied to body on key press
        private float torqueMagnitude = 500; //rotational torque magnitude applied to body on key press
        

        public DemoGame() {
            InitializeComponent();
        }

        protected override void OnStarting() {
            base.OnStarting();
            this.Window.Title = "Farseer Physics Demo 2 -- Keys: (A,S,D,W) (Left Arrow, Right Arrow) (Space)";
   
            //new-up an instance of the PhysicsSimulator.
            Vector2 gravity = new Vector2(0,0);
            physicsSimulator = new PhysicsSimulator(gravity);
            physicsSimulator.AllowedPenetrations = 1f; //advanced parameter that affcts collision response (see paper by Erin Catto)
            physicsSimulator.BiasFactor = .8f; //another advanced parameter that affects collision response (see paper by Erin Catto)

            LoadResources();
        }

        private void LoadResources(){
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            //new-up the screen border collision object.
            screenCollisionBorder = new ScreenCollisionBorder(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height,physicsSimulator);

            //new-up the rigid body sprites. physicsSimulator is passed in so that the rigid body can add
            //itself to the physicsSimulator. This was done mostly for code cleanliness.
            xRigidBodySprite = new RigidBodySprite("X.jpg", graphics.GraphicsDevice, physicsSimulator);
            xRigidBodySprite.Position = new Vector2(200, 100);

            nRigidBodySprite = new RigidBodySprite("N.jpg", graphics.GraphicsDevice, physicsSimulator);
            nRigidBodySprite.Position = new Vector2(400, 300);

            aRigidBodySprite = new RigidBodySprite("A.jpg", graphics.GraphicsDevice, physicsSimulator);
            aRigidBodySprite.Position = new Vector2(600, 500);
        }

        private void ResetResources() {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            xRigidBodySprite.Reset(graphics.GraphicsDevice);
            nRigidBodySprite.Reset(graphics.GraphicsDevice);
            aRigidBodySprite.Reset(graphics.GraphicsDevice);
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

            //draw the sprites.
            xRigidBodySprite.Draw(spriteBatch);
            nRigidBodySprite.Draw(spriteBatch);
            aRigidBodySprite.Draw(spriteBatch);

            spriteBatch.End();

            DrawComponents();

            graphics.GraphicsDevice.EndScene();
            graphics.GraphicsDevice.Present();
        }

        private void graphics_DeviceReset(object sender, EventArgs e) {
            ResetResources();
        }

        //apply forces and torques based on key presses. A,S,D,W and Left, Right
        private void keyboardInputComponent_AKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            nRigidBodySprite.ApplyForce(new Vector2(-forceMagnitude, 0));
        }

        private void keyboardInputComponent_DKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            nRigidBodySprite.ApplyForce(new Vector2(forceMagnitude, 0));
        }

        private void keyboardInputComponent_SKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            nRigidBodySprite.ApplyForce(new Vector2(0, forceMagnitude));
        }

        private void keyboardInputComponent_WKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            nRigidBodySprite.ApplyForce(new Vector2(0, -forceMagnitude));
        }

        private void keyboardInputComponent_LeftKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            nRigidBodySprite.ApplyTorque(-torqueMagnitude);
        }

        private void keyboardInputComponent_RightKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            nRigidBodySprite.ApplyTorque(torqueMagnitude);
        }

        private void keyboardInputComponent_SpaceKeyDown(object sender, FarseerGames.Components.KeyEventArgs e) {
            gravityState += 1;
            if (gravityState > 4) { gravityState = 0; }
            switch (gravityState) {
                case 0:
                    physicsSimulator.Gravity = new Vector2(0, 0);
                    break;
                case 1:
                    physicsSimulator.Gravity = new Vector2(0, gravityAcceleration);
                    break;
                case 2:
                    physicsSimulator.Gravity = new Vector2(0, -gravityAcceleration);
                    break;
                case 3:
                    physicsSimulator.Gravity = new Vector2(gravityAcceleration,0);
                    break;
                case 4:
                    physicsSimulator.Gravity = new Vector2(-gravityAcceleration,0);
                    break;
                default:
                    break;
            }

        }

        private void keyboardInputComponent_EscapeKeyDown(object sender, FarseerGames.Components.KeyEventArgs e) {
            Exit();
            exiting = true;
        }
    }
}   