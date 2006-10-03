using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerXNAPhysics;
using FarseerGames.FarseerXNAPhysics.Dynamics;

namespace FarseerDemo3  {
    /// <summary>
    /// Simple class that encapsulates both sprite and rigid body logic.
    /// </summary>
    public class RectangleSprite    {
        private RectangleRigidBody rigidBody;
        private Texture2D spriteTexture;
        private TextureInformation textureInformation;
        private Vector2 origin;
        private string textureName;
        private float mass = 1;
        
        public RectangleSprite(string textureName, GraphicsDevice graphicsDevice,PhysicsSimulator physicsSimulator) {
            this.textureName = textureName;
            Reset(graphicsDevice);
            rigidBody = new RectangleRigidBody(Width, Height, mass);
            //setup some default physics parameters for all rigid body sprites
            rigidBody.RotationalDragCoefficient = 50;
            rigidBody.LinearDragCoefficient = .0001f;
            rigidBody.FrictionCoefficient = .3f;
            rigidBody.RestitutionCoefficient = .1f;
            //add rigid body to physics simulator
            physicsSimulator.Add(rigidBody);
        }

        public Vector2  Position {
            get { return rigidBody.Position; }
            set { rigidBody.Position = value; }
        }

        public float Orientation {
            get { return rigidBody.Orientation; }
            set { rigidBody.Orientation = value; }
        }	

        public float Width{
            get{return textureInformation.Width;}
        }

        public float Height {
            get { return textureInformation.Height; }
        }

        public float Mass {
            get { return mass; }
            set { 
                mass = value;
                rigidBody.Mass = mass;            
            }
        }

        public void ApplyForce(Vector2 force) {
            rigidBody.ApplyForce(force);
        }

        public void ApplyTorque(float torque) {
            rigidBody.ApplyTorque(torque);
        }

        public void Draw(SpriteBatch spriteBatch) {  
            //draw the sprite using the position and orientation from the rigid body.
            spriteBatch.Draw(spriteTexture, rigidBody.Position, null, Color.White, rigidBody.Orientation, origin, 1f, SpriteEffects.None, 0f);
        }

        public void Reset(GraphicsDevice graphicsDevice) {
            string fullTexturePath = @"Textures\" + textureName;
            spriteTexture = Texture2D.FromFile(graphicsDevice, fullTexturePath);
            textureInformation = Texture2D.GetTextureInformation(fullTexturePath);
            origin.X = textureInformation.Width / 2f;
            origin.Y = textureInformation.Height / 2f;            
        }
    }
}
