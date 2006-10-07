using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerXNAGame.Sprites {
    public class SpriteManager : ISpriteManagerService  {
        private GraphicsDevice graphicsDevice;
        private List<Sprite> spriteList;
        private SpriteBatch spriteBatch;

        public GraphicsDevice  GraphicsDevice {
            get { return graphicsDevice; }
            set { graphicsDevice = value; }
        }	

        public SpriteManager(GraphicsDevice graphicsDevice) {
            spriteList = new List<Sprite>();
            LoadResources(graphicsDevice);
        }

        public void Draw() {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteList.ForEach(DrawSprite);
            spriteBatch.End();            
        }

        private void DrawSprite(Sprite sprite) {
            sprite.Draw(spriteBatch);
        }

        public void Add(Sprite sprite) {
            spriteList.Add(sprite);
        }

        public void Remove(Sprite sprite) {
            spriteList.Remove(sprite);
        }

        public void LoadResources(GraphicsDevice graphicsDevice) {
            this.graphicsDevice = graphicsDevice;
            spriteBatch = new SpriteBatch(graphicsDevice);
            spriteList.ForEach(LoadSpriteResource);
        }

        private void LoadSpriteResource(Sprite sprite) {
            sprite.LoadResources(this.graphicsDevice);
        }
    }
}
