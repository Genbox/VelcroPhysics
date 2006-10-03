using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerXNAPhysics;
using FarseerGames.FarseerXNAPhysics.Collisions;
using FarseerGames.FarseerXNAPhysics.Dynamics;

namespace FarseerDemo3  {
    /// <summary>
    /// This class actualy encapsulates a "PolygonRigidBody" and a sprite object.  A PolygonRigidBody
    /// is used to represent any type of 2d geometry. The vertices must be defined in counter clock-wise
    /// order.
    /// For this particular bottle of soda (which just happens to be a sprite) I got the vertices by 
    /// looking at the image in "Paint.Net" and picking points that mapped to the bottles surface
    /// fairly well.  Normally this could be pre-stored in a custom file along with the image.
    /// </summary>
    public class BottleSprite    {
        private PolygonRigidBody rigidBody;
        private Texture2D spriteTexture;
        private TextureInformation textureInformation;
        private Vector2 origin;
        private string textureName;
        private float mass = 1;

        public BottleSprite(string textureName, GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator) {
            this.textureName = textureName;
            Reset(graphicsDevice);

            //setup the collision geometry to represent the edges of the bottle.
            Vertices bottleVertices = new Vertices();
            bottleVertices.Add(new Vector2(-2, -27));
            bottleVertices.Add(new Vector2(-2, -22));
            bottleVertices.Add(new Vector2(-11, -13));
            bottleVertices.Add(new Vector2(-11, 2));
            bottleVertices.Add(new Vector2(-11, 21));
            bottleVertices.Add(new Vector2(-5, 28));
            bottleVertices.Add(new Vector2(4, 28));
            bottleVertices.Add(new Vector2(9, 21));
            bottleVertices.Add(new Vector2(9, 2));
            bottleVertices.Add(new Vector2(9, -13));
            bottleVertices.Add(new Vector2(2, -22));
            bottleVertices.Add(new Vector2(2, -27));
            rigidBody = new PolygonRigidBody(bottleVertices);
            
            //setup some default physics parameters for all rigid body sprites
            rigidBody.Mass = .2f;
            rigidBody.RotationalDragCoefficient = 10;
            rigidBody.LinearDragCoefficient = .00001f;
            rigidBody.FrictionCoefficient = .1f;
            rigidBody.RestitutionCoefficient = .1f;

            //use the dimenstions of the bottles bounding box to estimate the moment of inertia (using MOI of rectangle as estimate)
            float bbWidth = rigidBody.Geometry.AABB.Width;
            float bbHeight = rigidBody.Geometry.AABB.Height;
            rigidBody.MomentOfInertia = Mass * (bbWidth * bbWidth + bbHeight * bbHeight) / 12f;
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
