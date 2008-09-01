using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.Demos.DemoShare
{
    public class AngularSpringLever
    {
        private Body angleSpringleverBody;
        private int attachPoint; //0=left, 1=top, 2=right,3=bottom
        private Geom circleGeom;
        private Texture2D circleTexture;
        private int collisionGroup;
        private float dampningConstant = 1;
        private Vector2 position;
        private Geom rectangleGeom;
        private int rectangleHeight = 20;
        private Texture2D rectangleTexture;
        private int rectangleWidth = 100;

        private FixedRevoluteJoint revoluteJoint;
        private float springConstant = 1;


        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public int AttachPoint
        {
            get { return attachPoint; }
            set { attachPoint = value; }
        }

        public int RectangleWidth
        {
            get { return rectangleWidth; }
            set { rectangleWidth = value; }
        }

        public int RectangleHeight
        {
            get { return rectangleHeight; }
            set { rectangleHeight = value; }
        }

        public float SpringConstant
        {
            get { return springConstant; }
            set { springConstant = value; }
        }

        public float DampningConstant
        {
            get { return dampningConstant; }
            set { dampningConstant = value; }
        }


        public int CollisionGroup
        {
            get { return collisionGroup; }
            set { collisionGroup = value; }
        }

        public Body Body
        {
            get { return angleSpringleverBody; }
        }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            rectangleTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, rectangleWidth, rectangleHeight,
                                                                    Color.White, Color.Black);
            int radius;
            if (attachPoint == 0 | attachPoint == 2)
            {
                radius = rectangleHeight;
            }
            else
            {
                radius = rectangleWidth;
            }
            circleTexture = DrawingHelper.CreateCircleTexture(graphicsDevice, radius, Color.White, Color.Black);

            //body is created as rectangle so that it has the moment of inertia closer to the final shape of the object.
            angleSpringleverBody = BodyFactory.Instance.CreateBody(physicsSimulator, 1,
                                                                   BodyFactory.MOIForRectangle(rectangleWidth,
                                                                                               rectangleHeight, 1f));

            rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, angleSpringleverBody,
                                                                     rectangleWidth, rectangleHeight);
            rectangleGeom.FrictionCoefficient = .5f;
            rectangleGeom.CollisionGroup = collisionGroup;

            Vector2 offset = Vector2.Zero;
            switch (attachPoint)
            {
                case 0:
                    {
                        offset = new Vector2(-rectangleWidth/2f, 0); //offset to rectangle to left
                        break;
                    }
                case 1:
                    {
                        offset = new Vector2(0, -rectangleHeight/2f); //offset to rectangle to top
                        break;
                    }
                case 2:
                    {
                        offset = new Vector2(rectangleWidth/2f, 0); //offset to rectangle to right
                        break;
                    }
                case 3:
                    {
                        offset = new Vector2(0, rectangleHeight/2f); //offset to rectangle to bottom
                        break;
                    }
            }

            angleSpringleverBody.Position = position - offset;

            circleGeom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, angleSpringleverBody, radius, 20,
                                                               offset, 0);
            circleGeom.FrictionCoefficient = .5f;
            circleGeom.CollisionGroup = collisionGroup;

            revoluteJoint = JointFactory.Instance.CreateFixedRevoluteJoint(physicsSimulator, angleSpringleverBody,
                                                                           position);
            physicsSimulator.Add(revoluteJoint);
            ControllerFactory.Instance.CreateFixedAngleSpring(physicsSimulator, angleSpringleverBody,
                                                              springConstant, dampningConstant);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(rectangleTexture, rectangleGeom.Position, null, Color.White, rectangleGeom.Rotation,
                             new Vector2(rectangleTexture.Width/2f, rectangleTexture.Height/2f), 1, SpriteEffects.None,
                             0);
            spriteBatch.Draw(circleTexture, circleGeom.Position, null, Color.White, circleGeom.Rotation,
                             new Vector2(circleTexture.Width/2f, circleTexture.Height/2f), 1, SpriteEffects.None, 0);
        }
    }
}