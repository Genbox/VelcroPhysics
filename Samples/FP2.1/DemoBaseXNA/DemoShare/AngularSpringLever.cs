using DemoBaseXNA.DrawingSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DemoShare
{
    public class AngularSpringLever
    {
        private Geom _circleGeom;
        private Texture2D _circleTexture;
        private Geom _rectangleGeom;
        private Texture2D _rectangleTexture;

        private FixedRevoluteJoint _revoluteJoint;

        public AngularSpringLever()
        {
            DampingConstant = 1;
            SpringConstant = 1;
            RectangleHeight = 20;
            RectangleWidth = 100;
        }

        public Vector2 Position { get; set; }

        public int AttachPoint { get; set; }

        public int RectangleWidth { get; set; }

        public int RectangleHeight { get; set; }

        public float SpringConstant { get; set; }

        public float DampingConstant { get; set; }

        public int CollisionGroup { get; set; }

        public Body Body { get; private set; }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _rectangleTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, RectangleWidth, RectangleHeight,
                                                                     Color.White, Color.Black);
            int radius;
            if (AttachPoint == 0 | AttachPoint == 2)
            {
                radius = RectangleHeight;
            }
            else
            {
                radius = RectangleWidth;
            }
            _circleTexture = DrawingHelper.CreateCircleTexture(graphicsDevice, radius, Color.White, Color.Black);

            //body is created as rectangle so that it has the moment of inertia closer to the final shape of the object.
            Body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, RectangleWidth,
                                                                             RectangleHeight, 1f);

            _rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, Body,
                                                                      RectangleWidth, RectangleHeight);
            _rectangleGeom.FrictionCoefficient = .5f;
            _rectangleGeom.CollisionGroup = CollisionGroup;

            Vector2 offset = Vector2.Zero;
            switch (AttachPoint)
            {
                case 0:
                    {
                        offset = new Vector2(-RectangleWidth/2f, 0); //offset to rectangle to left
                        break;
                    }
                case 1:
                    {
                        offset = new Vector2(0, -RectangleHeight/2f); //offset to rectangle to top
                        break;
                    }
                case 2:
                    {
                        offset = new Vector2(RectangleWidth/2f, 0); //offset to rectangle to right
                        break;
                    }
                case 3:
                    {
                        offset = new Vector2(0, RectangleHeight/2f); //offset to rectangle to bottom
                        break;
                    }
            }

            Body.Position = Position - offset;

            _circleGeom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, Body, radius, 20,
                                                                offset, 0);
            _circleGeom.FrictionCoefficient = .5f;
            _circleGeom.CollisionGroup = CollisionGroup;

            _revoluteJoint = JointFactory.Instance.CreateFixedRevoluteJoint(physicsSimulator, Body,
                                                                            Position);
            physicsSimulator.Add(_revoluteJoint);
            SpringFactory.Instance.CreateFixedAngleSpring(physicsSimulator, Body,
                                                          SpringConstant, DampingConstant);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_rectangleTexture, _rectangleGeom.Position, null, Color.White, _rectangleGeom.Rotation,
                             new Vector2(_rectangleTexture.Width/2f, _rectangleTexture.Height/2f), 1, SpriteEffects.None,
                             0);
            spriteBatch.Draw(_circleTexture, _circleGeom.Position, null, Color.White, _circleGeom.Rotation,
                             new Vector2(_circleTexture.Width/2f, _circleTexture.Height/2f), 1, SpriteEffects.None, 0);
        }
    }
}