using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseXNA.DemoShare
{
    public class Spider
    {
        private const float SpiderBodyRadius = 1.1f;
        private bool _kneeFlexed;
        private float _kneeTargetAngle = .4f;
        private AngleJoint _leftKneeAngleJoint;
        private AngleJoint _leftShoulderAngleJoint;
        private Vector2 _lowerLegSize = new Vector2(3, 0.5f);

        private AngleJoint _rightKneeAngleJoint;
        private AngleJoint _rightShoulderAngleJoint;
        private float _s;
        private bool _shoulderFlexed;
        private float _shoulderTargetAngle = .2f;
        private Vector2 _upperLegSize = new Vector2(3, 0.5f); //x=width, y=height

        public Spider(World world, Vector2 position)
        {
            DebugMaterial matHead = new DebugMaterial(MaterialType.Face)
                                        {
                                            Color = Color.ForestGreen,
                                            Scale = 2f
                                        };
            DebugMaterial matBody = new DebugMaterial(MaterialType.Blank)
                                        {
                                            Color = Color.YellowGreen
                                        };
            DebugMaterial matLeg = new DebugMaterial(MaterialType.Blank)
                                       {
                                           Color = Color.DarkGreen
                                       };

            //Load bodies
            Fixture circle = FixtureFactory.CreateCircle(world, SpiderBodyRadius, 0.1f, position, matHead);
            circle.Body.BodyType = BodyType.Dynamic;

            //Left upper leg
            Fixture leftUpper = FixtureFactory.CreateRectangle(world, _upperLegSize.X, _upperLegSize.Y, 0.1f,
                                                               circle.Body.Position - new Vector2(SpiderBodyRadius, 0) -
                                                               new Vector2(_upperLegSize.X/2, 0), matBody);
            leftUpper.Body.BodyType = BodyType.Dynamic;

            //Left lower leg
            Fixture leftLower = FixtureFactory.CreateRectangle(world, _lowerLegSize.X, _lowerLegSize.Y, 0.1f,
                                                               circle.Body.Position - new Vector2(SpiderBodyRadius, 0) -
                                                               new Vector2(_upperLegSize.X, 0) -
                                                               new Vector2(_lowerLegSize.X/2, 0), matLeg);
            leftLower.Body.BodyType = BodyType.Dynamic;

            //Right upper leg
            Fixture rightUpper = FixtureFactory.CreateRectangle(world, _upperLegSize.X, _upperLegSize.Y, 0.1f,
                                                                circle.Body.Position + new Vector2(SpiderBodyRadius, 0) +
                                                                new Vector2(_upperLegSize.X/2, 0), matBody);
            rightUpper.Body.BodyType = BodyType.Dynamic;

            //Right lower leg
            Fixture rightLower = FixtureFactory.CreateRectangle(world, _lowerLegSize.X, _lowerLegSize.Y, 0.1f,
                                                                circle.Body.Position + new Vector2(SpiderBodyRadius, 0) +
                                                                new Vector2(_upperLegSize.X, 0) +
                                                                new Vector2(_lowerLegSize.X/2, 0), matLeg);
            rightLower.Body.BodyType = BodyType.Dynamic;

            //Create joints
            JointFactory.CreateRevoluteJoint(world, circle.Body, leftUpper.Body, new Vector2(SpiderBodyRadius, 0));

            _leftShoulderAngleJoint = JointFactory.CreateAngleJoint(world, circle.Body, leftUpper.Body);
            _leftShoulderAngleJoint.MaxImpulse = 3;

            JointFactory.CreateRevoluteJoint(world, circle.Body, rightUpper.Body, new Vector2(-SpiderBodyRadius, 0));

            _rightShoulderAngleJoint = JointFactory.CreateAngleJoint(world, circle.Body, rightUpper.Body);
            _rightShoulderAngleJoint.MaxImpulse = 3;

            JointFactory.CreateRevoluteJoint(world, leftUpper.Body, leftLower.Body, new Vector2(_upperLegSize.X/2, 0));
            _leftKneeAngleJoint = JointFactory.CreateAngleJoint(world, leftUpper.Body, leftLower.Body);
            _leftKneeAngleJoint.MaxImpulse = 3;

            JointFactory.CreateRevoluteJoint(world, rightUpper.Body, rightLower.Body,
                                             -new Vector2(_upperLegSize.X/2, 0));
            _rightKneeAngleJoint = JointFactory.CreateAngleJoint(world, rightUpper.Body, rightLower.Body);
            _rightKneeAngleJoint.MaxImpulse = 3;
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

            _leftKneeAngleJoint.TargetAngle = _kneeTargetAngle;
            _rightKneeAngleJoint.TargetAngle = -_kneeTargetAngle;

            _leftShoulderAngleJoint.TargetAngle = _shoulderTargetAngle;
            _rightShoulderAngleJoint.TargetAngle = -_shoulderTargetAngle;
        }
    }
}