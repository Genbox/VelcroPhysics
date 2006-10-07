using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using FarseerGames.FarseerXNAGame.Utilities;
using FarseerGames.FarseerXNAGame.Fonts;

namespace FarseerGames.FarseerXNAGame.Components {
    public partial class FrameRateComponent : Microsoft.Xna.Framework.GameComponent {

        private static BitmapFont _font;
        IGraphicsDeviceService graphics;
        private FrameRate _frameRate;
        
        public FrameRateComponent() {
            InitializeComponent();
        }

        public override void Start() {
            this.graphics = this.Game.GameServices.GetService<IGraphicsDeviceService>();
            this.graphics.DeviceReset += new EventHandler(graphics_DeviceReset);

            _font = new BitmapFont("Fonts/comic.xml");
            LoadResources();
            _frameRate = new FrameRate(3,.5f);
        }

        public override void Update() {
            _frameRate.Update((float)Game.ElapsedTime.TotalSeconds);             
        }            
           
        public override void Draw() {
            _font.DrawString(10, 25, Color.Yellow, _frameRate.FrameTime.ToString("0.0000"));
            _font.DrawString(10, 5, Color.Yellow, _frameRate.FramesPerSecond.ToString("0"));
        }

        void graphics_DeviceReset(object sender, EventArgs e) {
            LoadResources();
        }

        void LoadResources() {
            _font.Reset(this.graphics.GraphicsDevice); 
        }
    }
}