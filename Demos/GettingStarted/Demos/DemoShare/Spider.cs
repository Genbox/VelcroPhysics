using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.Demos.DemoShare
{
    public class Spider
    {
        private const int spiderBodyRadius = 20;
        private readonly Vector2 _position;
        private CollisionCategories _collidesWith = CollisionCategories.All;
        private CollisionCategories _collisionCategory = CollisionCategories.All;
        private int _collisionGroup;
        private bool _kneeFlexed;
        private float _kneeTargetAngle = .4f;
        private AngleJoint _leftKneeAngleJoint;
        private Body _leftLowerLegBody;
        private Geom _leftLowerLegGeom;
        private AngleJoint _leftShoulderAngleJoint;
        private Body _leftUpperLegBody;
        private Geom _leftUpperLegGeom;
        private Vector2 _lowerLegOrigin;
        private Vector2 _lowerLegSize = new Vector2(30, 12);
        private Texture2D _lowerLegTexture;

        private AngleJoint _rightKneeAngleJoint;
        private Body _rightLowerLegBody;
        private Geom _rightLowerLegGeom;
        private AngleJoint _rightShoulderAngleJoint;
        private Body _rightUpperLegBody;
        private Geom _rightUpperLegGeom;
        private float _s;
        private bool _shoulderFlexed;
        private float _shoulderTargetAngle = .2f;
        private Body _spiderBody;

        private Geom _spiderGeom;
        private Vector2 _spiderOrigin;
        private Texture2D _spiderTexture;
        private Vector2 _upperLegOrigin;
        private Vector2 _upperLegSize = new Vector2(40, 12); //x=width, y=height
        private Texture2D _upperLegTexture;

        public Spider(Vector2 position)
        {
            _position = position;
        }

        public Body Body
        {
            get { return _spiderBody; }
        }

        public CollisionCategories CollisionCategory
        {
            get { return _collisionCategory; }
            set { _collisionCategory = value; }
        }

        public CollisionCategories CollidesWith
        {
            get { return _collidesWith; }
            set { _collidesWith = value; }
        }

        public int CollisionGroup
        {
            get { return _collisionGroup; }
            set { _collisionGroup = value; }
        }

        public void ApplyForce(Vector2 force)
        {
            _spiderBody.ApplyForce(force);
        }

        public void ApplyTorque(float torque)
        {
            _spiderBody.ApplyTorque(torque);
        }


        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _spiderTexture = DrawingHelper.CreateCircleTexture(graphicsDevice, spiderBodyRadius, Color.White, Color.Black);
            _spiderOrigin = new Vector2(_spiderTexture.Width/2f, _spiderTexture.Height/2f);

            _upperLegTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, (int) _upperLegSize.X,
                                                                   (int) _upperLegSize.Y, Color.White, Color.Black);
            _upperLegOrigin = new Vector2(_upperLegTexture.Width/2f, _upperLegTexture.Height/2f);

            _lowerLegTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, (int) _lowerLegSize.X,
                                                                   (int) _lowerLegSize.Y, Color.Red, Color.Black);
            _lowerLegOrigin = new Vector2(_lowerLegTexture.Width/2f, _lowerLegTexture.Height/2f);

            //Load bodies
            _spiderBody = BodyFactory.Instance.CreateCircleBody(physicsSimulator, spiderBodyRadius, 1);
            _spiderBody.Position = _position;
            _spiderBody.IsStatic = false;

            _leftUpperLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _upperLegSize.X, _upperLegSize.Y,
                                                                        1);
            _leftUpperLegBody.Position = _spiderBody.Position - new Vector2(spiderBodyRadius, 0) -
                                        new Vector2(_upperLegSize.X/2, 0);

            _leftLowerLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _lowerLegSize.X, _lowerLegSize.Y,
                                                                        1);
            _leftLowerLegBody.Position = _spiderBody.Position - new Vector2(spiderBodyRadius, 0) -
                                        new Vector2(_upperLegSize.X, 0) - new Vector2(_lowerLegSize.X/2, 0);

            _rightUpperLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _upperLegSize.X,
                                                                         _upperLegSize.Y, 1);
            _rightUpperLegBody.Position = _spiderBody.Position + new Vector2(spiderBodyRadius, 0) +
                                         new Vector2(_upperLegSize.X/2, 0);

            _rightLowerLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _lowerLegSize.X,
                                                                         _lowerLegSize.Y, 1);
            _rightLowerLegBody.Position = _spiderBody.Position + new Vector2(spiderBodyRadius, 0) +
                                         new Vector2(_upperLegSize.X, 0) + new Vector2(_lowerLegSize.X/2, 0);

            //load geometries
            _spiderGeom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, _spiderBody, spiderBodyRadius, 14);
            _leftUpperLegGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _leftUpperLegBody,
                                                                        _upperLegSize.X, _upperLegSize.Y);
            _leftLowerLegGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _leftLowerLegBody,
                                                                        _lowerLegSize.X, _lowerLegSize.Y);
            _rightUpperLegGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _rightUpperLegBody,
                                                                         _upperLegSize.X, _upperLegSize.Y);
            _rightLowerLegGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _rightLowerLegBody,
                                                                         _lowerLegSize.X, _lowerLegSize.Y);
            _spiderGeom.CollisionGroup = _collisionGroup;
            _leftUpperLegGeom.CollisionGroup = _collisionGroup;
            _leftLowerLegGeom.CollisionGroup = _collisionGroup;
            _rightUpperLegGeom.CollisionGroup = _collisionGroup;
            _rightLowerLegGeom.CollisionGroup = _collisionGroup;

            //load joints
            JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, _spiderBody,
                                                      _leftUpperLegBody,
                                                      _spiderBody.Position -
                                                      new Vector2(spiderBodyRadius, 0));
            _leftShoulderAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, _spiderBody,
                                                                            _leftUpperLegBody);
            _leftShoulderAngleJoint.TargetAngle = -.4f;
            _leftShoulderAngleJoint.MaxImpulse = 300;

            JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, _spiderBody,
                                                      _rightUpperLegBody,
                                                      _spiderBody.Position +
                                                      new Vector2(spiderBodyRadius, 0));
            _rightShoulderAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, _spiderBody,
                                                                             _rightUpperLegBody);
            _rightShoulderAngleJoint.TargetAngle = .4f;
            _leftShoulderAngleJoint.MaxImpulse = 300;

            JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, _leftUpperLegBody,
                                                      _leftLowerLegBody,
                                                      _spiderBody.Position -
                                                      new Vector2(spiderBodyRadius, 0) -
                                                      new Vector2(_upperLegSize.X, 0));
            _leftKneeAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, _leftUpperLegBody,
                                                                        _leftLowerLegBody);
            _leftKneeAngleJoint.TargetAngle = -_kneeTargetAngle;
            _leftKneeAngleJoint.MaxImpulse = 300;

            JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, _rightUpperLegBody,
                                                      _rightLowerLegBody,
                                                      _spiderBody.Position +
                                                      new Vector2(spiderBodyRadius, 0) +
                                                      new Vector2(_upperLegSize.X, 0));
            _rightKneeAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, _rightUpperLegBody,
                                                                         _rightLowerLegBody);
            _rightKneeAngleJoint.TargetAngle = _kneeTargetAngle;
            _rightKneeAngleJoint.MaxImpulse = 300;
        }

        public void Update(GameTime gameTime)
        {
            _s += gameTime.ElapsedGameTime.Milliseconds;
            if (_s > 4000)
            {
                _s = 0;

                _kneeFlexed = !_kneeFlexed;
                _shoulderFlexed = !_shoulderFlexed;

                if (_kneeFlexed)
                    _kneeTargetAngle = 1.4f;
                else
                    _kneeTargetAngle = .4f;

                if (_kneeFlexed)
                    _shoulderTargetAngle = 1.2f;
                else
                    _shoulderTargetAngle = .2f;
            }
            _leftKneeAngleJoint.TargetAngle = -_kneeTargetAngle;
            _rightKneeAngleJoint.TargetAngle = _kneeTargetAngle;

            _leftShoulderAngleJoint.TargetAngle = -_shoulderTargetAngle;
            _rightShoulderAngleJoint.TargetAngle = _shoulderTargetAngle;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_upperLegTexture, _leftUpperLegGeom.Position, null, Color.White, _leftUpperLegGeom.Rotation,
                             _upperLegOrigin, 1, SpriteEffects.None, 0f);
            spriteBatch.Draw(_lowerLegTexture, _leftLowerLegGeom.Position, null, Color.White, _leftLowerLegGeom.Rotation,
                             _lowerLegOrigin, 1, SpriteEffects.None, 0f);
            spriteBatch.Draw(_upperLegTexture, _rightUpperLegGeom.Position, null, Color.White, _rightUpperLegGeom.Rotation,
                             _upperLegOrigin, 1, SpriteEffects.None, 0f);
            spriteBatch.Draw(_lowerLegTexture, _rightLowerLegGeom.Position, null, Color.White, _rightLowerLegGeom.Rotation,
                             _lowerLegOrigin, 1, SpriteEffects.None, 0f);
            spriteBatch.Draw(_spiderTexture, _spiderGeom.Position, null, Color.White, _spiderGeom.Rotation, _spiderOrigin, 1,
                             SpriteEffects.None, 0f);
        }
    }
}