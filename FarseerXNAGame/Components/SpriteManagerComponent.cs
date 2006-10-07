using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using FarseerGames.FarseerXNAGame.Sprites;

namespace FarseerGames.FarseerXNAGame.Components {
    public partial class SpriteManagerComponent : Microsoft.Xna.Framework.GameComponent {
        private SpriteManager spriteManager;
        private IGraphicsDeviceService graphicsDeviceService;

        public SpriteManagerComponent() {
            InitializeComponent();
        }

        public override void Start() {     
            graphicsDeviceService = (IGraphicsDeviceService)Game.GameServices.GetService<IGraphicsDeviceService>();
            spriteManager = new SpriteManager(graphicsDeviceService.GraphicsDevice);
            graphicsDeviceService.DeviceReset += new EventHandler(GraphicsDevice_Reset);
            Game.GameServices.AddService(typeof(SpriteManager), spriteManager);
        }

        public override void Update() {
            // TODO: Add your update code here
        }

        public override void Draw() {
            spriteManager.Draw();
        }

        private void GraphicsDevice_Reset(Object sender, EventArgs args){
            LoadResources();    
        }

        public void LoadResources() {
            spriteManager.LoadResources(graphicsDeviceService.GraphicsDevice);
        }
    }
}