using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerXNAGame.Sprites;

using FarseerGames.FarseerXNAPhysics;


namespace FarseerGames.FarseerXNAGame.Sprites {
    public class Sprite : IIsDisposable  {
        private Texture2D spriteTexture;
        private TextureInformation textureInformation;
        private Vector2 origin;
        private string textureName;

        private Vector2 position = Vector2.Zero;
        private float orientation = 0;

        public Sprite(string textureName, SpriteManager spriteManager) {
            this.textureName = textureName;
            LoadResources(spriteManager.GraphicsDevice);
            spriteManager.Add(this);
        }

        public Vector2 Position {
            get { return position; }
            set { position = value; }
        }

        public float Orientation {
            get { return orientation; }
            set { orientation = value; }
        }

        public virtual void Draw(SpriteBatch spriteBatch) {
            //draw the sprite using the position and orientation from the rigid body.
            spriteBatch.Draw(spriteTexture, position, null, Color.White, orientation, origin, 1f, SpriteEffects.None, 0f);
        }

        public void LoadResources(GraphicsDevice graphicsDevice) {
            string fullTexturePath = @"Textures\" + textureName;
            spriteTexture = Texture2D.FromFile(graphicsDevice, fullTexturePath);
            textureInformation = Texture2D.GetTextureInformation(fullTexturePath);
            origin.X = textureInformation.Width / 2f;
            origin.Y = textureInformation.Height / 2f;
        }

        private bool isDisposed = false;
        public bool IsDisposed {
            get { return isDisposed; }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!isDisposed) {
                if (disposing) {
                    spriteTexture.Dispose();
                }
                isDisposed = true;
            }
        }
    }
}
