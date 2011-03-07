using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.SamplesFramework
{
    public class Spider
    {
        private const float SpiderBodyRadius = 0.65f;
        private bool _kneeFlexed;
        private float _kneeTargetAngle = -0.4f;
        private AngleJoint _leftKneeAngleJoint;
        private AngleJoint _leftShoulderAngleJoint;
        private Vector2 _lowerLegSize = new Vector2(1.8f, 0.3f);

        private AngleJoint _rightKneeAngleJoint;
        private AngleJoint _rightShoulderAngleJoint;
        private float _s;
        private bool _shoulderFlexed;
        private float _shoulderTargetAngle = -0.2f;
        private Vector2 _upperLegSize = new Vector2(1.8f, 0.3f);

        private Body circle;
        private Body leftUpper;
        private Body leftLower;
        private Body rightUpper;
        private Body rightLower;

        private PhysicsGameScreen _screen;
        private Sprite _torso;
        private Sprite _upperLeg;
        private Sprite _lowerLeg;

        public Spider(World world, PhysicsGameScreen screen, Vector2 position)
        {
            //Load bodies
            circle = BodyFactory.CreateCircle(world, SpiderBodyRadius, 0.1f, position);
            circle.BodyType = BodyType.Dynamic;

            //Left upper leg
            leftUpper = BodyFactory.CreateRectangle(world, _upperLegSize.X, _upperLegSize.Y, 0.1f,
                                                        circle.Position - new Vector2(SpiderBodyRadius, 0f) -
                                                        new Vector2(_upperLegSize.X / 2f, 0f));
            leftUpper.BodyType = BodyType.Dynamic;

            //Left lower leg
            leftLower = BodyFactory.CreateRectangle(world, _lowerLegSize.X, _lowerLegSize.Y, 0.1f,
                                                        circle.Position - new Vector2(SpiderBodyRadius, 0f) -
                                                        new Vector2(_upperLegSize.X, 0f) -
                                                        new Vector2(_lowerLegSize.X / 2f, 0f));
            leftLower.BodyType = BodyType.Dynamic;

            //Right upper leg
            rightUpper = BodyFactory.CreateRectangle(world, _upperLegSize.X, _upperLegSize.Y, 0.1f,
                                                         circle.Position + new Vector2(SpiderBodyRadius, 0f) +
                                                         new Vector2(_upperLegSize.X / 2f, 0f));
            rightUpper.BodyType = BodyType.Dynamic;

            //Right lower leg
            rightLower = BodyFactory.CreateRectangle(world, _lowerLegSize.X, _lowerLegSize.Y, 0.1f,
                                                         circle.Position + new Vector2(SpiderBodyRadius, 0f) +
                                                         new Vector2(_upperLegSize.X, 0f) +
                                                         new Vector2(_lowerLegSize.X / 2f, 0f));
            rightLower.BodyType = BodyType.Dynamic;

            //Create joints
            JointFactory.CreateRevoluteJoint(world, circle, leftUpper, new Vector2(_upperLegSize.X / 2f, 0f));
            _leftShoulderAngleJoint = JointFactory.CreateAngleJoint(world, circle, leftUpper);
            _leftShoulderAngleJoint.MaxImpulse = 3f;

            JointFactory.CreateRevoluteJoint(world, circle, rightUpper, new Vector2(-_upperLegSize.X / 2f, 0f));
            _rightShoulderAngleJoint = JointFactory.CreateAngleJoint(world, circle, rightUpper);
            _rightShoulderAngleJoint.MaxImpulse = 3f;

            JointFactory.CreateRevoluteJoint(world, leftUpper, leftLower, new Vector2(_lowerLegSize.X / 2f, 0f));
            _leftKneeAngleJoint = JointFactory.CreateAngleJoint(world, leftUpper, leftLower);
            _leftKneeAngleJoint.MaxImpulse = 3f;

            JointFactory.CreateRevoluteJoint(world, rightUpper, rightLower, new Vector2(-_lowerLegSize.X / 2f, 0f));
            _rightKneeAngleJoint = JointFactory.CreateAngleJoint(world, rightUpper, rightLower);
            _rightKneeAngleJoint.MaxImpulse = 3;

            _screen = screen;

            //GFX
            AssetCreator _creator = _screen.ScreenManager.Assets;
            _torso = new Sprite(_creator.CircleTexture(SpiderBodyRadius, MaterialType.Waves, Color.Gray, 1f));
            _upperLeg = new Sprite(_creator.TextureFromShape(leftUpper.FixtureList[0].Shape,
                                                             MaterialType.Blank, Color.DimGray, 1f));
            _lowerLeg = new Sprite(_creator.TextureFromShape(leftLower.FixtureList[0].Shape,
                                                             MaterialType.Blank, Color.DarkSlateGray, 1f));
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
                    _kneeTargetAngle = -1.4f;
                else
                    _kneeTargetAngle = -0.4f;

                if (_kneeFlexed)
                    _shoulderTargetAngle = -1.2f;
                else
                    _shoulderTargetAngle = -0.2f;
            }

            _leftKneeAngleJoint.TargetAngle = _kneeTargetAngle;
            _rightKneeAngleJoint.TargetAngle = -_kneeTargetAngle;

            _leftShoulderAngleJoint.TargetAngle = _shoulderTargetAngle;
            _rightShoulderAngleJoint.TargetAngle = -_shoulderTargetAngle;
        }

        public void Draw()
        {
            SpriteBatch _batch = _screen.ScreenManager.SpriteBatch;

            _batch.Draw(_lowerLeg.texture, ConvertUnits.ToDisplayUnits(leftLower.Position), null,
                        Color.White, leftLower.Rotation, _lowerLeg.origin, 1f, SpriteEffects.None, 0f);
            _batch.Draw(_lowerLeg.texture, ConvertUnits.ToDisplayUnits(rightLower.Position), null,
                        Color.White, rightLower.Rotation, _lowerLeg.origin, 1f, SpriteEffects.None, 0f);

            _batch.Draw(_upperLeg.texture, ConvertUnits.ToDisplayUnits(leftUpper.Position), null,
                        Color.White, leftUpper.Rotation, _upperLeg.origin, 1f, SpriteEffects.None, 0f);
            _batch.Draw(_upperLeg.texture, ConvertUnits.ToDisplayUnits(rightUpper.Position), null,
                        Color.White, rightUpper.Rotation, _upperLeg.origin, 1f, SpriteEffects.None, 0f);

            _batch.Draw(_torso.texture, ConvertUnits.ToDisplayUnits(circle.Position), null,
                        Color.White, circle.Rotation, _torso.origin, 1f, SpriteEffects.None, 0f);
        }
    }
}