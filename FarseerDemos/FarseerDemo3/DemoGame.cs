using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using FarseerGames.FarseerXNAPhysics;
using FarseerGames.FarseerXNAPhysics.Collisions;
using FarseerGames.FarseerXNAPhysics.Dynamics;

namespace FarseerDemo3 {
    /// <summary>

    /// </summary>
    partial class DemoGame : Microsoft.Xna.Framework.Game {
        private bool _exiting = false;

        private SpriteBatch spriteBatch;
        //used to contain all rigid bodies within the screen
        private ScreenCollisionBorder screenCollisionBorder;

        //these obejects combine sprite and rigid body logic into a single class
        private BottleSprite bottleSprite;
        private BottleSprite bottleSprite2;
        private BottleSprite bottleSprite3;
        private BottleSprite bottleSprite4;
        private BottleSprite bottleSprite5;

        private RectangleSprite xRectangleSprite;
        private RectangleSprite nRectangleSprite;
        private RectangleSprite aRectangleSprite;
        private RectangleSprite farseerRectangleSprite;
        private RectangleSprite xRectangleSprite2;
        private RectangleSprite nRectangleSprite2;
        private RectangleSprite aRectangleSprite2;
        private RectangleSprite farseerRectangleSprite2;
        
        //simple variables to keep track of gravity state and strength
        private int gravityState = 0;
        private float gravityAcceleration = 100f;

        //declare variables for PhysicsSimulator and a single RigidBody
        private PhysicsSimulator physicsSimulator;
        private float forceMagnitude = 60; //linear force magnitude applied to body on key press
        private float torqueMagnitude = 500; //rotational torque magnitude applied to body on key press
        

        public DemoGame() {
            InitializeComponent();
        }

        protected override void OnStarting() {
            base.OnStarting();
            this.Window.Title = "Farseer Physics Demo 3 -- Keys: (A,S,D,W) (Left Arrow, Right Arrow) (Space)";
   
            //new-up an instance of the PhysicsSimulator.
            Vector2 gravity = new Vector2(0,0);
            physicsSimulator = new PhysicsSimulator(gravity);
            physicsSimulator.AllowedPenetrations = .2f; //advanced parameter that affcts collision response (see paper by Erin Catto)
            physicsSimulator.BiasFactor = .4f; //another advanced parameter that affects collision response (see paper by Erin Catto)

            LoadResources();
        }

        private void LoadResources() {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            //new-up the screen border collision object.
            screenCollisionBorder = new ScreenCollisionBorder(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height, physicsSimulator);


            //new-up the rigid body sprites. physicsSimulator is passed in so that the rigid body can add
            //itself to the physicsSimulator. This was done mostly for code cleanliness.
            bottleSprite = new BottleSprite("sprite.png", graphics.GraphicsDevice, physicsSimulator);
            bottleSprite.Position = new Vector2(400, 60);

            bottleSprite2 = new BottleSprite("sprite.png", graphics.GraphicsDevice, physicsSimulator);
            bottleSprite2.Position = new Vector2(500, 120);

            bottleSprite3 = new BottleSprite("sprite.png", graphics.GraphicsDevice, physicsSimulator);
            bottleSprite3.Position = new Vector2(300, 120);

            bottleSprite4 = new BottleSprite("sprite.png", graphics.GraphicsDevice, physicsSimulator);
            bottleSprite4.Position = new Vector2(550, 120);

            bottleSprite5 = new BottleSprite("sprite.png", graphics.GraphicsDevice, physicsSimulator);
            bottleSprite5.Position = new Vector2(250, 120);

            farseerRectangleSprite2 = new RectangleSprite("FPE.jpg", graphics.GraphicsDevice, physicsSimulator);
            farseerRectangleSprite2.Position = new Vector2(400, 168);

            xRectangleSprite2 = new RectangleSprite("X.jpg", graphics.GraphicsDevice, physicsSimulator);
            xRectangleSprite2.Position = new Vector2(400, 216);

            nRectangleSprite2 = new RectangleSprite("N.jpg", graphics.GraphicsDevice, physicsSimulator);
            nRectangleSprite2.Position = new Vector2(400, 280);

            aRectangleSprite2 = new RectangleSprite("A.jpg", graphics.GraphicsDevice, physicsSimulator);
            aRectangleSprite2.Position = new Vector2(400, 344);

            farseerRectangleSprite = new RectangleSprite("FPE.jpg", graphics.GraphicsDevice, physicsSimulator);
            farseerRectangleSprite.Position = new Vector2(400, 392);

            xRectangleSprite = new RectangleSprite("X.jpg", graphics.GraphicsDevice, physicsSimulator);
            xRectangleSprite.Position = new Vector2(400, 440);

            nRectangleSprite = new RectangleSprite("N.jpg", graphics.GraphicsDevice, physicsSimulator);
            nRectangleSprite.Position = new Vector2(400, 504);

            aRectangleSprite = new RectangleSprite("A.jpg", graphics.GraphicsDevice, physicsSimulator);
            aRectangleSprite.Position = new Vector2(400, 568);

        }

        private void ResetResources() {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            xRectangleSprite.Reset(graphics.GraphicsDevice);
            nRectangleSprite.Reset(graphics.GraphicsDevice);
            aRectangleSprite.Reset(graphics.GraphicsDevice);
            farseerRectangleSprite.Reset(graphics.GraphicsDevice);
        }

        protected override void Update() {
            if (_exiting) { return; }
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

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            //draw the sprites.
            bottleSprite.Draw(spriteBatch);
            bottleSprite2.Draw(spriteBatch);
            bottleSprite3.Draw(spriteBatch);
            bottleSprite4.Draw(spriteBatch);
            bottleSprite5.Draw(spriteBatch);

            xRectangleSprite.Draw(spriteBatch);
            nRectangleSprite.Draw(spriteBatch);
            aRectangleSprite.Draw(spriteBatch);
            farseerRectangleSprite.Draw(spriteBatch);

            xRectangleSprite2.Draw(spriteBatch);
            nRectangleSprite2.Draw(spriteBatch);
            aRectangleSprite2.Draw(spriteBatch);
            farseerRectangleSprite2.Draw(spriteBatch);

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
            bottleSprite.ApplyForce(new Vector2(-forceMagnitude, 0));
        }

        private void keyboardInputComponent_DKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            bottleSprite.ApplyForce(new Vector2(forceMagnitude, 0));
        }

        private void keyboardInputComponent_SKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            bottleSprite.ApplyForce(new Vector2(0, forceMagnitude));
        }

        private void keyboardInputComponent_WKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            bottleSprite.ApplyForce(new Vector2(0, -forceMagnitude));
        }

        private void keyboardInputComponent_LeftKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            bottleSprite.ApplyTorque(-torqueMagnitude);
        }

        private void keyboardInputComponent_RightKeyPressed(object sender, FarseerGames.Components.KeyEventArgs e) {
            bottleSprite.ApplyTorque(torqueMagnitude);
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
            _exiting = true;
        }
    }
}   