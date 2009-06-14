using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.SimpleSamplesXNA.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.SimpleSamplesXNA.Demos.DemoShare
{
    public class AngularSpringLever
    {
        private Body _angleSpringleverBody;
        private int _attachPoint; //0=left, 1=top, 2=right,3=bottom
        private Geom _circleGeom;
        private Texture2D _circleTexture;
        private int _collisionGroup;
        private float _dampningConstant = 1;
        private Vector2 _position;
        private Geom _rectangleGeom;
        private int _rectangleHeight = 20;
        private Texture2D _rectangleTexture;
        private int _rectangleWidth = 100;

        private FixedRevoluteJoint _revoluteJoint;
        private float _springConstant = 1;

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public int AttachPoint
        {
            get { return _attachPoint; }
            set { _attachPoint = value; }
        }

        public int RectangleWidth
        {
            get { return _rectangleWidth; }
            set { _rectangleWidth = value; }
        }

        public int RectangleHeight
        {
            get { return _rectangleHeight; }
            set { _rectangleHeight = value; }
        }

        public float SpringConstant
        {
            get { return _springConstant; }
            set { _springConstant = value; }
        }

        public float DampingConstant
        {
            get { return _dampningConstant; }
            set { _dampningConstant = value; }
        }


        public int CollisionGroup
        {
            get { return _collisionGroup; }
            set { _collisionGroup = value; }
        }

        public Body Body
        {
            get { return _angleSpringleverBody; }
        }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _rectangleTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, _rectangleWidth, _rectangleHeight,
                                                                     Color.White, Color.Black);
            int radius;
            if (_attachPoint == 0 | _attachPoint == 2)
            {
                radius = _rectangleHeight;
            }
            else
            {
                radius = _rectangleWidth;
            }
            _circleTexture = DrawingHelper.CreateCircleTexture(graphicsDevice, radius, Color.White, Color.Black);

            //body is created as rectangle so that it has the moment of inertia closer to the final shape of the object.
            _angleSpringleverBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _rectangleWidth,
                                                                             _rectangleHeight, 1f);

            _rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _angleSpringleverBody,
                                                                      _rectangleWidth, _rectangleHeight);
            _rectangleGeom.FrictionCoefficient = .5f;
            _rectangleGeom.CollisionGroup = _collisionGroup;

            Vector2 offset = Vector2.Zero;
            switch (_attachPoint)
            {
                case 0:
                    {
                        offset = new Vector2(-_rectangleWidth/2f, 0); //offset to rectangle to left
                        break;
                    }
                case 1:
                    {
                        offset = new Vector2(0, -_rectangleHeight/2f); //offset to rectangle to top
                        break;
                    }
                case 2:
                    {
                        offset = new Vector2(_rectangleWidth/2f, 0); //offset to rectangle to right
                        break;
                    }
                case 3:
                    {
                        offset = new Vector2(0, _rectangleHeight/2f); //offset to rectangle to bottom
                        break;
                    }
            }

            _angleSpringleverBody.Position = _position - offset;

            _circleGeom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, _angleSpringleverBody, radius, 20,
                                                                offset, 0);
            _circleGeom.FrictionCoefficient = .5f;
            _circleGeom.CollisionGroup = _collisionGroup;

            _revoluteJoint = JointFactory.Instance.CreateFixedRevoluteJoint(physicsSimulator, _angleSpringleverBody,
                                                                            _position);
            physicsSimulator.Add(_revoluteJoint);
            SpringFactory.Instance.CreateFixedAngleSpring(physicsSimulator, _angleSpringleverBody,
                                                          _springConstant, _dampningConstant);
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