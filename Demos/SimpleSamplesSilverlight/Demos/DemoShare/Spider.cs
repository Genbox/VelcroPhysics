using System;
using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using SM = System.Windows.Media;

namespace FarseerGames.SimpleSamplesSilverlight.Demos.DemoShare
{
    public class Spider
    {
        private const int _spiderBodyRadius = 20;
        private CollisionCategory _collidesWith = CollisionCategory.All;
        private CollisionCategory _collisionCategory = CollisionCategory.All;
        private int _collisionGroup;
        private bool _kneeFlexed;
        private float _kneeTargetAngle = .4f;
        private AngleJoint _leftKneeAngleJoint;
        private Body _leftLowerLegBody;
        private Geom _leftLowerLegGeom;
        private AngleJoint _leftShoulderAngleJoint;
        private Body _leftUpperLegBody;
        private Geom _leftUpperLegGeom;
        private Vector2 _lowerLegSize = new Vector2(30, 12);
        private Vector2 _position;

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
        private Vector2 _upperLegSize = new Vector2(40, 12); //x=width, y=height

        public Spider(Vector2 position)
        {
            _position = position;
        }

        public Body Body
        {
            get { return _spiderBody; }
        }

        public CollisionCategory CollisionCategory
        {
            get { return _collisionCategory; }
            set { _collisionCategory = value; }
        }

        public CollisionCategory CollidesWith
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

        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator)
        {
            //Load bodies
            _spiderBody = BodyFactory.Instance.CreateCircleBody(physicsSimulator, _spiderBodyRadius, 1);
            _spiderBody.Position = _position;
            _spiderBody.IsStatic = false;
            view.AddCircleToCanvas(_spiderBody, _spiderBodyRadius);
            _leftUpperLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _upperLegSize.X,
                                                                         _upperLegSize.Y,
                                                                         1);
            _leftUpperLegBody.Position = _spiderBody.Position - new Vector2(_spiderBodyRadius, 0) -
                                         new Vector2(_upperLegSize.X/2, 0);
            view.AddRectangleToCanvas(_leftUpperLegBody, Colors.White, new Vector2(_upperLegSize.X, _upperLegSize.Y));

            _leftLowerLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _lowerLegSize.X,
                                                                         _lowerLegSize.Y,
                                                                         1);
            _leftLowerLegBody.Position = _spiderBody.Position - new Vector2(_spiderBodyRadius, 0) -
                                         new Vector2(_upperLegSize.X, 0) - new Vector2(_lowerLegSize.X/2, 0);
            view.AddRectangleToCanvas(_leftLowerLegBody, Colors.Red, new Vector2(_lowerLegSize.X, _lowerLegSize.Y));

            _rightUpperLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _upperLegSize.X,
                                                                          _upperLegSize.Y, 1);
            _rightUpperLegBody.Position = _spiderBody.Position + new Vector2(_spiderBodyRadius, 0) +
                                          new Vector2(_upperLegSize.X/2, 0);
            view.AddRectangleToCanvas(_rightUpperLegBody, Colors.White, new Vector2(_upperLegSize.X, _upperLegSize.Y));

            _rightLowerLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _lowerLegSize.X,
                                                                          _lowerLegSize.Y, 1);
            _rightLowerLegBody.Position = _spiderBody.Position + new Vector2(_spiderBodyRadius, 0) +
                                          new Vector2(_upperLegSize.X, 0) + new Vector2(_lowerLegSize.X/2, 0);
            view.AddRectangleToCanvas(_rightLowerLegBody, Colors.Red, new Vector2(_lowerLegSize.X, _lowerLegSize.Y));

            //load geometries
            _spiderGeom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, _spiderBody, _spiderBodyRadius, 14);
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
                                                      new Vector2(_spiderBodyRadius, 0));
            _leftShoulderAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, _spiderBody,
                                                                             _leftUpperLegBody);
            _leftShoulderAngleJoint.TargetAngle = -.4f;
            _leftShoulderAngleJoint.MaxImpulse = 300;

            JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, _spiderBody,
                                                      _rightUpperLegBody,
                                                      _spiderBody.Position +
                                                      new Vector2(_spiderBodyRadius, 0));
            _rightShoulderAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, _spiderBody,
                                                                              _rightUpperLegBody);
            _rightShoulderAngleJoint.TargetAngle = .4f;
            _leftShoulderAngleJoint.MaxImpulse = 300;

            JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, _leftUpperLegBody,
                                                      _leftLowerLegBody,
                                                      _spiderBody.Position -
                                                      new Vector2(_spiderBodyRadius, 0) -
                                                      new Vector2(_upperLegSize.X, 0));
            _leftKneeAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, _leftUpperLegBody,
                                                                         _leftLowerLegBody);
            _leftKneeAngleJoint.TargetAngle = -_kneeTargetAngle;
            _leftKneeAngleJoint.MaxImpulse = 300;

            JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, _rightUpperLegBody,
                                                      _rightLowerLegBody,
                                                      _spiderBody.Position +
                                                      new Vector2(_spiderBodyRadius, 0) +
                                                      new Vector2(_upperLegSize.X, 0));
            _rightKneeAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, _rightUpperLegBody,
                                                                          _rightLowerLegBody);
            _rightKneeAngleJoint.TargetAngle = _kneeTargetAngle;
            _rightKneeAngleJoint.MaxImpulse = 300;
        }

        public void Update(TimeSpan elapsedTime)
        {
            _s += elapsedTime.Milliseconds;
            if (_s > 4000)
            {
                _s = 0;

                _kneeFlexed = !_kneeFlexed;
                _shoulderFlexed = !_shoulderFlexed;

                _kneeTargetAngle = _kneeFlexed ? 1.4f : .4f;

                _shoulderTargetAngle = _kneeFlexed ? 1.2f : .2f;
            }
            _leftKneeAngleJoint.TargetAngle = -_kneeTargetAngle;
            _rightKneeAngleJoint.TargetAngle = _kneeTargetAngle;

            _leftShoulderAngleJoint.TargetAngle = -_shoulderTargetAngle;
            _rightShoulderAngleJoint.TargetAngle = _shoulderTargetAngle;
        }
    }
}