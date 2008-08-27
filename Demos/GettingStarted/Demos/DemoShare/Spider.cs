using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.Demos.DemoShare
{
    public class Spider
    {
        private readonly Vector2 position;
        private CollisionCategories collidesWith = CollisionCategories.All;
        private CollisionCategories collisionCategory = CollisionCategories.All;
        private int collisionGroup;
        private bool kneeFlexed;
        private float kneeTargetAngle = .4f;
        private AngleJoint leftKneeAngleJoint;
        private Body leftLowerLegBody;
        private Geom leftLowerLegGeom;
        private AngleJoint leftShoulderAngleJoint;
        private Body leftUpperLegBody;
        private Geom leftUpperLegGeom;
        private Vector2 lowerLegOrigin;
        private Vector2 lowerLegSize = new Vector2(30, 12);
        private Texture2D lowerLegTexture;

        private AngleJoint rightKneeAngleJoint;
        private Body rightLowerLegBody;
        private Geom rightLowerLegGeom;
        private AngleJoint rightShoulderAngleJoint;
        private Body rightUpperLegBody;
        private Geom rightUpperLegGeom;
        private float s;
        private bool shoulderFlexed;
        private float shoulderTargetAngle = .2f;
        private Body spiderBody;

        private const int spiderBodyRadius = 20;
        private Geom spiderGeom;
        private Vector2 spiderOrigin;
        private Texture2D spiderTexture;
        private Vector2 upperLegOrigin;
        private Vector2 upperLegSize = new Vector2(40, 12); //x=width, y=height
        private Texture2D upperLegTexture;

        public Spider(Vector2 position)
        {
            this.position = position;
        }

        public Body Body
        {
            get { return spiderBody; }
        }

        public CollisionCategories CollisionCategory
        {
            get { return collisionCategory; }
            set { collisionCategory = value; }
        }

        public CollisionCategories CollidesWith
        {
            get { return collidesWith; }
            set { collidesWith = value; }
        }

        public int CollisionGroup
        {
            get { return collisionGroup; }
            set { collisionGroup = value; }
        }

        public void ApplyForce(Vector2 force)
        {
            spiderBody.ApplyForce(force);
        }

        public void ApplyTorque(float torque)
        {
            spiderBody.ApplyTorque(torque);
        }


        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            spiderTexture = DrawingHelper.CreateCircleTexture(graphicsDevice, spiderBodyRadius, Color.White, Color.Black);
            spiderOrigin = new Vector2(spiderTexture.Width/2f, spiderTexture.Height/2f);

            upperLegTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, (int) upperLegSize.X,
                                                                   (int) upperLegSize.Y, Color.White, Color.Black);
            upperLegOrigin = new Vector2(upperLegTexture.Width/2f, upperLegTexture.Height/2f);

            lowerLegTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, (int) lowerLegSize.X,
                                                                   (int) lowerLegSize.Y, Color.Red, Color.Black);
            lowerLegOrigin = new Vector2(lowerLegTexture.Width/2f, lowerLegTexture.Height/2f);

            //Load bodies
            spiderBody = BodyFactory.Instance.CreateCircleBody(physicsSimulator, spiderBodyRadius, 1);
            spiderBody.Position = position;
            spiderBody.IsStatic = false;

            leftUpperLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, upperLegSize.X, upperLegSize.Y,
                                                                        1);
            leftUpperLegBody.Position = spiderBody.Position - new Vector2(spiderBodyRadius, 0) -
                                        new Vector2(upperLegSize.X/2, 0);

            leftLowerLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, lowerLegSize.X, lowerLegSize.Y,
                                                                        1);
            leftLowerLegBody.Position = spiderBody.Position - new Vector2(spiderBodyRadius, 0) -
                                        new Vector2(upperLegSize.X, 0) - new Vector2(lowerLegSize.X/2, 0);

            rightUpperLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, upperLegSize.X,
                                                                         upperLegSize.Y, 1);
            rightUpperLegBody.Position = spiderBody.Position + new Vector2(spiderBodyRadius, 0) +
                                         new Vector2(upperLegSize.X/2, 0);

            rightLowerLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, lowerLegSize.X,
                                                                         lowerLegSize.Y, 1);
            rightLowerLegBody.Position = spiderBody.Position + new Vector2(spiderBodyRadius, 0) +
                                         new Vector2(upperLegSize.X, 0) + new Vector2(lowerLegSize.X/2, 0);

            //load geometries
            spiderGeom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, spiderBody, spiderBodyRadius, 14);
            leftUpperLegGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, leftUpperLegBody,
                                                                        upperLegSize.X, upperLegSize.Y);
            leftLowerLegGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, leftLowerLegBody,
                                                                        lowerLegSize.X, lowerLegSize.Y);
            rightUpperLegGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rightUpperLegBody,
                                                                         upperLegSize.X, upperLegSize.Y);
            rightLowerLegGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rightLowerLegBody,
                                                                         lowerLegSize.X, lowerLegSize.Y);
            spiderGeom.CollisionGroup = collisionGroup;
            leftUpperLegGeom.CollisionGroup = collisionGroup;
            leftLowerLegGeom.CollisionGroup = collisionGroup;
            rightUpperLegGeom.CollisionGroup = collisionGroup;
            rightLowerLegGeom.CollisionGroup = collisionGroup;

            //load joints
            JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, spiderBody,
                                                                                  leftUpperLegBody,
                                                                                  spiderBody.Position -
                                                                                  new Vector2(spiderBodyRadius, 0));
            leftShoulderAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, spiderBody,
                                                                            leftUpperLegBody);
            leftShoulderAngleJoint.TargetAngle = -.4f;
            leftShoulderAngleJoint.MaxImpulse = 300;

            JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, spiderBody,
                                                                                   rightUpperLegBody,
                                                                                   spiderBody.Position +
                                                                                   new Vector2(spiderBodyRadius, 0));
            rightShoulderAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, spiderBody,
                                                                             rightUpperLegBody);
            rightShoulderAngleJoint.TargetAngle = .4f;
            leftShoulderAngleJoint.MaxImpulse = 300;

            JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, leftUpperLegBody,
                                                                              leftLowerLegBody,
                                                                              spiderBody.Position -
                                                                              new Vector2(spiderBodyRadius, 0) -
                                                                              new Vector2(upperLegSize.X, 0));
            leftKneeAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, leftUpperLegBody,
                                                                        leftLowerLegBody);
            leftKneeAngleJoint.TargetAngle = -kneeTargetAngle;
            leftKneeAngleJoint.MaxImpulse = 300;

            JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, rightUpperLegBody,
                                                                               rightLowerLegBody,
                                                                               spiderBody.Position +
                                                                               new Vector2(spiderBodyRadius, 0) +
                                                                               new Vector2(upperLegSize.X, 0));
            rightKneeAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, rightUpperLegBody,
                                                                         rightLowerLegBody);
            rightKneeAngleJoint.TargetAngle = kneeTargetAngle;
            rightKneeAngleJoint.MaxImpulse = 300;
        }

        public void Update(GameTime gameTime)
        {
            s += gameTime.ElapsedGameTime.Milliseconds;
            if (s > 4000)
            {
                s = 0;

                kneeFlexed = !kneeFlexed;
                shoulderFlexed = !shoulderFlexed;

                if (kneeFlexed)
                    kneeTargetAngle = 1.4f;
                else
                    kneeTargetAngle = .4f;

                if (kneeFlexed)
                    shoulderTargetAngle = 1.2f;
                else
                    shoulderTargetAngle = .2f;
            }
            leftKneeAngleJoint.TargetAngle = -kneeTargetAngle;
            rightKneeAngleJoint.TargetAngle = kneeTargetAngle;

            leftShoulderAngleJoint.TargetAngle = -shoulderTargetAngle;
            rightShoulderAngleJoint.TargetAngle = shoulderTargetAngle;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(upperLegTexture, leftUpperLegGeom.Position, null, Color.White, leftUpperLegGeom.Rotation,
                             upperLegOrigin, 1, SpriteEffects.None, 0f);
            spriteBatch.Draw(lowerLegTexture, leftLowerLegGeom.Position, null, Color.White, leftLowerLegGeom.Rotation,
                             lowerLegOrigin, 1, SpriteEffects.None, 0f);
            spriteBatch.Draw(upperLegTexture, rightUpperLegGeom.Position, null, Color.White, rightUpperLegGeom.Rotation,
                             upperLegOrigin, 1, SpriteEffects.None, 0f);
            spriteBatch.Draw(lowerLegTexture, rightLowerLegGeom.Position, null, Color.White, rightLowerLegGeom.Rotation,
                             lowerLegOrigin, 1, SpriteEffects.None, 0f);
            spriteBatch.Draw(spiderTexture, spiderGeom.Position, null, Color.White, spiderGeom.Rotation, spiderOrigin, 1,
                             SpriteEffects.None, 0f);
        }
    }
}