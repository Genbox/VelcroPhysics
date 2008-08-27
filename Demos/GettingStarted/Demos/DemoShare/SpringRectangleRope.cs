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
    public class SpringRectangleRope {
        Texture2D rectangleTexture;
        Body[] rectangleBody;
        Geom[] rectangleGeom;
        LinearSpring[] linearSpring;

        int rectangleWidth = 20;
        int rectangleHeight = 20;
        float rectangleMass = 1;
        Vector2 position = Vector2.Zero;
        int rectangleCount = 20;

        float springLength = 25;
        float springConstant = 1;
        float dampningConstant = 1;

        int collisionGroup = 0;


        public SpringRectangleRope() {
        }

        public Body FirstBody {
            get { return rectangleBody[0]; }
            set { rectangleBody[0] = value; }
        }

        public Vector2 Position {
            get { return position; }
            set { position = value; }
        }

        public int RectangleCount {
            get { return rectangleCount; }
            set { rectangleCount = value; }
        }

        public int RectangleWidth {
            get { return rectangleWidth; }
            set { rectangleWidth = value; }
        }

        public int RectangleHeight {
            get { return rectangleHeight; }
            set { rectangleHeight = value; }
        }

        public float RectangleMass {
            get { return rectangleMass; }
            set { rectangleMass = value; }
        }

        public float SpringLength {
            get { return springLength; }
            set { springLength = value; }
        }

        public float  SpringConstant {
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

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator) {
            rectangleTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, rectangleWidth, rectangleHeight, Color.White, Color.Black);

            linearSpring = new LinearSpring[rectangleCount-1];
            rectangleBody = new Body[rectangleCount];
            rectangleBody[0] = BodyFactory.Instance.CreateRectangleBody(physicsSimulator,rectangleWidth, rectangleHeight, rectangleMass);
            rectangleBody[0].Position = position;
            for (int i = 1; i < rectangleBody.Length; i++) {
                rectangleBody[i] = BodyFactory.Instance.CreateBody(physicsSimulator,rectangleBody[0]);
                rectangleBody[i].Position = rectangleBody[i - 1].Position + new Vector2(0, springLength);
            }

            rectangleGeom = new Geom[rectangleCount];
            rectangleGeom[0] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator,rectangleBody[0], rectangleWidth, rectangleHeight);
            rectangleGeom[0].CollisionGroup = collisionGroup;
            for (int j = 1; j < rectangleGeom.Length; j++) {
                rectangleGeom[j] = GeomFactory.Instance.CreateGeom(physicsSimulator,rectangleBody[j], rectangleGeom[0]);
            }

            for (int k = 0; k < linearSpring.Length; k++) {
                linearSpring[k] = ControllerFactory.Instance.CreateLinearSpring(physicsSimulator,rectangleBody[k], Vector2.Zero, rectangleBody[k + 1], Vector2.Zero, springConstant, dampningConstant); 
            }
        }

        public void Draw(SpriteBatch spriteBatch) {
            for (int i = 0; i < rectangleGeom.Length; i++) {
                spriteBatch.Draw(rectangleTexture, rectangleGeom[i].Position, null, Color.White, rectangleGeom[i].Rotation, new Vector2(rectangleTexture.Width / 2, rectangleTexture.Height / 2), 1, SpriteEffects.None, 0);
            }
        }
    }
}
