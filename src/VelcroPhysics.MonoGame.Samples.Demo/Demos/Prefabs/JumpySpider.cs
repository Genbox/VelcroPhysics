using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem.Graphics;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos.Prefabs
{
    public class JumpySpider
    {
        private const float FlexTime = 5000f;
        private const float RelaxTime = 5000f;

        private const float ShoulderFlexed = -1.2f;
        private const float ShoulderRelaxed = -0.2f;

        private const float KneeFlexed = -1.4f;
        private const float KneeRelaxed = -0.4f;

        private const float SpiderBodyRadius = 0.65f;

        private readonly Body _circle;
        private readonly AngleJoint _leftKneeAngleJoint;
        private readonly Body _leftLower;

        private readonly AngleJoint _leftShoulderAngleJoint;
        private readonly Body _leftUpper;
        private readonly Sprite _lowerLeg;
        private readonly Vector2 _lowerLegSize = new Vector2(1.8f, 0.3f);
        private readonly AngleJoint _rightKneeAngleJoint;
        private readonly Body _rightLower;
        private readonly AngleJoint _rightShoulderAngleJoint;
        private readonly Body _rightUpper;

        private readonly Sprite _torso;
        private readonly Sprite _upperLeg;
        private readonly Vector2 _upperLegSize = new Vector2(1.8f, 0.3f);

        private bool _flexed;

        private float _timer;

        public JumpySpider(World world, Vector2 position)
        {
            // Body
            _circle = BodyFactory.CreateCircle(world, SpiderBodyRadius, 0.1f, position);
            _circle.BodyType = BodyType.Dynamic;

            // Left upper leg
            _leftUpper = BodyFactory.CreateRectangle(world, _upperLegSize.X, _upperLegSize.Y, 0.1f, _circle.Position - new Vector2(SpiderBodyRadius, 0f) - new Vector2(_upperLegSize.X / 2f, 0f));
            _leftUpper.BodyType = BodyType.Dynamic;

            // Left lower leg
            _leftLower = BodyFactory.CreateRectangle(world, _lowerLegSize.X, _lowerLegSize.Y, 0.1f, _circle.Position - new Vector2(SpiderBodyRadius, 0f) - new Vector2(_upperLegSize.X, 0f) - new Vector2(_lowerLegSize.X / 2f, 0f));
            _leftLower.BodyType = BodyType.Dynamic;

            // Right upper leg
            _rightUpper = BodyFactory.CreateRectangle(world, _upperLegSize.X, _upperLegSize.Y, 0.1f, _circle.Position + new Vector2(SpiderBodyRadius, 0f) + new Vector2(_upperLegSize.X / 2f, 0f));
            _rightUpper.BodyType = BodyType.Dynamic;

            // Right lower leg
            _rightLower = BodyFactory.CreateRectangle(world, _lowerLegSize.X, _lowerLegSize.Y, 0.1f, _circle.Position + new Vector2(SpiderBodyRadius, 0f) + new Vector2(_upperLegSize.X, 0f) + new Vector2(_lowerLegSize.X / 2f, 0f));
            _rightLower.BodyType = BodyType.Dynamic;

            //Create joints
            JointFactory.CreateRevoluteJoint(world, _circle, _leftUpper, new Vector2(_upperLegSize.X / 2f, 0f));
            _leftShoulderAngleJoint = JointFactory.CreateAngleJoint(world, _circle, _leftUpper);
            _leftShoulderAngleJoint.MaxImpulse = 3f;

            JointFactory.CreateRevoluteJoint(world, _circle, _rightUpper, new Vector2(-_upperLegSize.X / 2f, 0f));
            _rightShoulderAngleJoint = JointFactory.CreateAngleJoint(world, _circle, _rightUpper);
            _rightShoulderAngleJoint.MaxImpulse = 3f;

            JointFactory.CreateRevoluteJoint(world, _leftUpper, _leftLower, new Vector2(_lowerLegSize.X / 2f, 0f));
            _leftKneeAngleJoint = JointFactory.CreateAngleJoint(world, _leftUpper, _leftLower);
            _leftKneeAngleJoint.MaxImpulse = 3f;

            JointFactory.CreateRevoluteJoint(world, _rightUpper, _rightLower, new Vector2(-_lowerLegSize.X / 2f, 0f));
            _rightKneeAngleJoint = JointFactory.CreateAngleJoint(world, _rightUpper, _rightLower);
            _rightKneeAngleJoint.MaxImpulse = 3;

            //GFX
            _torso = new Sprite(Managers.TextureManager.CircleTexture(SpiderBodyRadius, "Square", Colors.Grey, Colors.Gold, Colors.Black, 1f));
            _upperLeg = new Sprite(Managers.TextureManager.TextureFromShape(_leftUpper.FixtureList[0].Shape, Colors.Grey, Colors.Black));
            _lowerLeg = new Sprite(Managers.TextureManager.TextureFromShape(_leftLower.FixtureList[0].Shape, Colors.Gold, Colors.Black));

            _flexed = false;
            _timer = 0f;

            _leftShoulderAngleJoint.TargetAngle = ShoulderRelaxed;
            _leftKneeAngleJoint.TargetAngle = KneeRelaxed;

            _rightShoulderAngleJoint.TargetAngle = -ShoulderRelaxed;
            _rightKneeAngleJoint.TargetAngle = -KneeRelaxed;
        }

        public void Update(GameTime gameTime)
        {
            _timer += gameTime.ElapsedGameTime.Milliseconds;
            if (_flexed)
            {
                if (_timer >= FlexTime)
                {
                    _timer = 0;
                    _flexed = false;

                    _leftShoulderAngleJoint.TargetAngle = ShoulderRelaxed;
                    _leftKneeAngleJoint.TargetAngle = KneeRelaxed;

                    _rightShoulderAngleJoint.TargetAngle = -ShoulderRelaxed;
                    _rightKneeAngleJoint.TargetAngle = -KneeRelaxed;
                }
            }
            else if (_timer >= RelaxTime)
            {
                _timer = 0f;
                _flexed = true;

                _leftShoulderAngleJoint.TargetAngle = ShoulderFlexed;
                _leftKneeAngleJoint.TargetAngle = KneeFlexed;

                _rightShoulderAngleJoint.TargetAngle = -ShoulderFlexed;
                _rightKneeAngleJoint.TargetAngle = -KneeFlexed;
            }
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(_lowerLeg.Image, ConvertUnits.ToDisplayUnits(_leftLower.Position), null, Color.White, _leftLower.Rotation, _lowerLeg.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_lowerLeg.Image, ConvertUnits.ToDisplayUnits(_rightLower.Position), null, Color.White, _rightLower.Rotation, _lowerLeg.Origin, 1f, SpriteEffects.None, 0f);

            batch.Draw(_upperLeg.Image, ConvertUnits.ToDisplayUnits(_leftUpper.Position), null, Color.White, _leftUpper.Rotation, _upperLeg.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_upperLeg.Image, ConvertUnits.ToDisplayUnits(_rightUpper.Position), null, Color.White, _rightUpper.Rotation, _upperLeg.Origin, 1f, SpriteEffects.None, 0f);

            batch.Draw(_torso.Image, ConvertUnits.ToDisplayUnits(_circle.Position), null, Color.White, _circle.Rotation, _torso.Origin, 1f, SpriteEffects.None, 0f);
        }
    }
}