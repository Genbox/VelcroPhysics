#region Using Statements
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
using FarseerGames.FarseerPhysicsDemos.Components;
#endregion

namespace FarseerGames.FarseerPhysicsDemos {
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class FarseerPhysicsGame : Microsoft.Xna.Framework.Game {
        GraphicsDeviceManager graphics;
        ContentManager content;
        ScreenManager screenManager;

        public FarseerPhysicsGame() {
            Window.Title = "Farseer Physics Engine Samples Framework";
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(Services);

            graphics.SynchronizeWithVerticalRetrace = false;

            this.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 10); 
            this.IsFixedTimeStep = true;            

#if !XBOX
            //windowed
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.IsFullScreen = false;

            //fullscreen
            //graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            //graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            //graphics.IsFullScreen = true;

            IsMouseVisible = true;
#else
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.IsFullScreen = true;
#endif      
            //Set window defaults. Parent game can override in constructor
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new System.EventHandler(Window_ClientSizeChanged);

            //new-up components and add to Game.Components
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);

            FrameRateCounter frameRateCounter = new FrameRateCounter(this);
            frameRateCounter.DrawOrder = 101;
            Components.Add(frameRateCounter);

            //SafeAreaComponent safeAreaComponent = new SafeAreaComponent(this);
            //safeAreaComponent.DrawOrder = 100;
            //Components.Add(safeAreaComponent);

            screenManager.AddScreen(new LogoScreen());
        }

        public ScreenManager ScreenManager {
            get {
                return this.screenManager;
            }
            set {
                this.screenManager = value;
            }
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // TODO: Add your initialization logic here
            graphics.ApplyChanges();
            base.Initialize();
        }


        /// <summary>
        /// Load your graphics content.  If loadAllContent is true, you should
        /// load content from both ResourceManagementMode pools.  Otherwise, just
        /// load ResourceManagementMode.Manual content.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void LoadContent() {

        }


        /// <summary>
        /// Unload your graphics content.  If unloadAllContent is true, you should
        /// unload content from both ResourceManagementMode pools.  Otherwise, just
        /// unload ResourceManagementMode.Manual content.  Manual content will get
        /// Disposed by the GraphicsDevice during a Reset.
        /// </summary>
        /// <param name="unloadAllContent">Which type of content to unload.</param>
        protected override void UnloadContent(){

        }

        public bool updatedOnce = false;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
            updatedOnce = true;
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            if (!updatedOnce) return;

            ScreenManager.GraphicsDevice.Clear(Color.SteelBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        void Window_ClientSizeChanged(object sender, System.EventArgs e) {
            if (Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0) {
                graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            }

        }
    }
}
