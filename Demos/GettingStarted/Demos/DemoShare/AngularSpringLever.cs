using System;
using System.Collections.Generic;
using System.Text;

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

namespace FarseerGames.FarseerPhysicsDemos.Demos.DemoShare {
    public class AngularSpringLever {
        Texture2D rectangleTexture;
        Texture2D circleTexture;

        int attachPoint = 0; //0=left, 1=top, 2=right,3=bottom
        int rectangleWidth = 100;
        int rectangleHeight = 20;
        Vector2 position;

        float springConstant = 1;
        float dampningConstant = 1;

        Body angleSpringleverBody;
        Geom circleGeom;
        Geom rectangleGeom;

        FixedRevoluteJoint revoluteJoint;
        FixedAngleSpring fixedAngleSpring;

        int collisionGroup = 0;


        public AngularSpringLever() {
        }

        public Vector2 Position {
            get { return position; }
            set { position = value; }
        }

        public int AttachPoint {
            get { return attachPoint; }
            set { attachPoint = value; }
        }
        public int RectangleWidth {
            get { return rectangleWidth; }
            set { rectangleWidth = value; }
        }

        public int RectangleHeight {
            get { return rectangleHeight; }
            set { rectangleHeight = value; }
        }

        public float SpringConstant {
            get { return springConstant; }
            set { springConstant = value; }
        }

        public float DampningConstant {
            get { return dampningConstant; }
            set { dampningConstant = value; }
        }
	

        public int CollisionGroup {
            get { return collisionGroup; }
            set { collisionGroup = value; }
        }

        public Body Body {
            get { return angleSpringleverBody; } 
        }	

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator) {
            rectangleTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, rectangleWidth, rectangleHeight, Color.White, Color.Black);
            int radius;
            if(attachPoint ==0 | attachPoint==2){
                radius = rectangleHeight;
            }else{
                radius = rectangleWidth;
            }
            circleTexture = DrawingHelper.CreateCircleTexture(graphicsDevice,radius,Color.White,Color.Black);

            //body is created as rectangle so that it has the moment of inertia closer to the final shape of the object.
            angleSpringleverBody = BodyFactory.Instance.CreateBody(physicsSimulator,1,BodyFactory.MOIForRectangle(rectangleWidth,rectangleHeight,1f));
            
            rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator,angleSpringleverBody, rectangleWidth, rectangleHeight);
            rectangleGeom.FrictionCoefficient = .5f;
            rectangleGeom.CollisionGroup = collisionGroup;
            
            Vector2 offset = Vector2.Zero;
            switch(attachPoint){
                case 0 : {
                    offset = new Vector2(-rectangleWidth/2,0); //offset to rectangle to left
                    break;
                }
                case 1 : {
                    offset = new Vector2(0,-rectangleHeight/2); //offset to rectangle to top
                    break;
                }
                case 2 : {
                    offset = new Vector2(rectangleWidth/2,0); //offset to rectangle to right
                    break;
                }
                case 3 : {
                    offset = new Vector2(0,rectangleHeight/2); //offset to rectangle to bottom
                    break;
                }
            }

            angleSpringleverBody.Position = position - offset;

            circleGeom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator,angleSpringleverBody, radius, 20, offset, 0);
            circleGeom.FrictionCoefficient = .5f;
            circleGeom.CollisionGroup = collisionGroup;

            revoluteJoint = JointFactory.Instance.CreateFixedRevoluteJoint(physicsSimulator,angleSpringleverBody, position);
            physicsSimulator.Add(revoluteJoint);
            fixedAngleSpring = ControllerFactory.Instance.CreateFixedAngleSpring(physicsSimulator,angleSpringleverBody, springConstant,dampningConstant);
            //fixedAngleSpring.MaxTorque = 200000;
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(rectangleTexture, rectangleGeom.Position, null, Color.White, rectangleGeom.Rotation, new Vector2(rectangleTexture.Width / 2, rectangleTexture.Height / 2), 1, SpriteEffects.None, 0);
            spriteBatch.Draw(circleTexture, circleGeom.Position, null, Color.White, circleGeom.Rotation, new Vector2(circleTexture.Width / 2, circleTexture.Height / 2), 1, SpriteEffects.None, 0); 
        }
    }
}
