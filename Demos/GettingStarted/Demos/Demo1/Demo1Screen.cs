using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using FarseerGames.FarseerPhysics; 
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Collisions;

using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;

namespace FarseerGames.FarseerPhysicsDemos.Demos.Demo1 {
    public class Demo1Screen : GameScreen{
        ContentManager contentManager;
        bool debugViewEnabled = false;
        Texture2D bodyTexture;
        Vector2 origin;

        PhysicsSimulator physicsSimulator;
        PhysicsSimulatorView physicsSimulatorView;

        Body rectangleBody;

        public Demo1Screen() {
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            physicsSimulatorView = new PhysicsSimulatorView(physicsSimulator);
        }

        public override void LoadContent() {
                if (contentManager == null) contentManager = new ContentManager(ScreenManager.Game.Services);
            physicsSimulatorView.LoadContent(ScreenManager.GraphicsDevice,contentManager);

            //load texture that will visually represent the physics body
            bodyTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 128, 128, Color.Gold, Color.Black);
            origin = new Vector2(bodyTexture.Width / 2f, bodyTexture.Height / 2f);

            //use the body factory to create the physics body
            rectangleBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator,128, 128, 1);
            rectangleBody.Position = ScreenManager.ScreenCenter;

        }

        public override void UnloadContent(){
            physicsSimulator.Clear();
        }

        public bool updatedOnce = false;
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            if (IsActive) {
                physicsSimulator.Update(gameTime.ElapsedGameTime.Milliseconds * .001f);
                updatedOnce = true;
            } 

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime) {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            ScreenManager.SpriteBatch.Draw(bodyTexture, rectangleBody.Position, null, Color.White, rectangleBody.Rotation, origin,1, SpriteEffects.None, 0);
            if (debugViewEnabled) {
                physicsSimulatorView.Draw(ScreenManager.SpriteBatch);
            }
            ScreenManager.SpriteBatch.End();
        }

        bool firstRun = true;
        public override void HandleInput(InputState input) {
            if (firstRun) {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
                firstRun = false;
            }
            if (input.PauseGame) {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
            }
            if (input.CurrentGamePadState.IsConnected) {
                HandleGamePadInput(input);
            }
            else {
                HandleKeyboardInput(input);
            }
            base.HandleInput(input);
        }

        private void HandleGamePadInput(InputState input) {
            if (input.LastGamePadState.Buttons.Y != ButtonState.Pressed && input.CurrentGamePadState.Buttons.Y == ButtonState.Pressed) { 
                debugViewEnabled = !debugViewEnabled;
                physicsSimulator.EnableDiagnostics = debugViewEnabled;
            }
            
            Vector2 force = 50 * input.CurrentGamePadState.ThumbSticks.Left;
            force.Y = -force.Y;
            rectangleBody.ApplyForce(force);

            float rotation = -1000 * input.CurrentGamePadState.Triggers.Left;
            rectangleBody.ApplyTorque(rotation);

            rotation = 1000 * input.CurrentGamePadState.Triggers.Right;
            rectangleBody.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input) {
            if (!input.LastKeyboardState.IsKeyDown(Keys.F1) && input.CurrentKeyboardState.IsKeyDown(Keys.F1)) {
                debugViewEnabled = !debugViewEnabled;
                physicsSimulator.EnableDiagnostics = debugViewEnabled;
            }

            float forceAmount = 50;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.A)) { force += new Vector2(-forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S)) { force += new Vector2(0, forceAmount); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D)) { force += new Vector2(forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W)) { force += new Vector2(0, -forceAmount); }

            rectangleBody.ApplyForce(force);

            float torqueAmount = 1000;
            float torque = 0;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left)) { torque -= torqueAmount; }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right)) { torque += torqueAmount; }
            rectangleBody.ApplyTorque(torque);
        }

        public string GetTitle() {
            return "A Single Body";
        }

        public string GetDetails() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with no geometry");
            sb.AppendLine("attached.");
            sb.AppendLine("");
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate : left and right triggers");
            sb.AppendLine("  -Move : left thumbstick");
            sb.AppendLine("");
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate : left and right arrows");
            sb.AppendLine("  -Move : A,S,D,W");
            return sb.ToString();
        }
    }
}
