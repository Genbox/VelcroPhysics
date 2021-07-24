using System.Collections.Generic;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem.Graphics;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.ScreenSystem;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos.Prefabs
{
    public class TheoJansenWalker
    {
        private readonly Sprite _body;
        private readonly Body _chassis;
        private readonly Sprite _engine;
        private readonly Sprite _leftLeg;
        private readonly Body[] _leftLegs;
        private readonly Sprite _leftShoulder;
        private readonly Body[] _leftShoulders;

        private readonly RevoluteJoint _motorJoint;

        private readonly bool _motorOn;

        private readonly Vector2 _position;
        private readonly Sprite _rightLeg;
        private readonly Body[] _rightLegs;
        private readonly Sprite _rightShoulder;
        private readonly Body[] _rightShoulders;

        private readonly Color[] _walkerColors = { Colors.Brown, Colors.Orange, Colors.Gold };
        private readonly List<DistanceJoint> _walkerJoints = new List<DistanceJoint>();
        private readonly Body _wheel;
        private float _motorSpeed;

        public TheoJansenWalker(World world, Vector2 position)
        {
            _position = position;
            _motorSpeed = 2.0f;
            _motorOn = true;

            _leftShoulders = new Body[3];
            _leftLegs = new Body[3];

            _rightShoulders = new Body[3];
            _rightLegs = new Body[3];

            Vector2 pivot = new Vector2(0f, -0.8f);

            // Chassis
            PolygonShape box = new PolygonShape(1f);
            box.Vertices = PolygonUtils.CreateRectangle(2.5f, 1.0f);
            _body = new Sprite(Managers.TextureManager.TextureFromShape(box, _walkerColors[0], Colors.Black));

            _chassis = BodyFactory.CreateBody(world);
            _chassis.BodyType = BodyType.Dynamic;
            _chassis.Position = pivot + _position;

            Fixture bodyFixture = _chassis.AddFixture(box);
            bodyFixture.CollisionGroup = -1;

            // Wheel
            CircleShape circle = new CircleShape(1.6f, 1f);
            _engine = new Sprite(Managers.TextureManager.TextureFromShape(circle, "Stripe", _walkerColors[1] * 0.6f, _walkerColors[2] * 0.8f, Colors.Black, 3f));

            _wheel = BodyFactory.CreateBody(world);
            _wheel.BodyType = BodyType.Dynamic;
            _wheel.Position = pivot + _position;

            Fixture wheelFixture = _wheel.AddFixture(circle);
            wheelFixture.CollisionGroup = -1;

            // Physics
            _motorJoint = new RevoluteJoint(_wheel, _chassis, _chassis.Position, true);
            _motorJoint.CollideConnected = false;
            _motorJoint.MotorSpeed = _motorSpeed;
            _motorJoint.MaxMotorTorque = 400f;
            _motorJoint.MotorEnabled = _motorOn;
            world.AddJoint(_motorJoint);

            Vector2 wheelAnchor = pivot + new Vector2(0f, 0.8f);

            CreateLeg(world, -1f, wheelAnchor, 0);
            CreateLeg(world, 1f, wheelAnchor, 0);

            _wheel.SetTransform(_wheel.Position, 120f * MathConstants.Pi / 180f);
            CreateLeg(world, -1f, wheelAnchor, 1);
            CreateLeg(world, 1f, wheelAnchor, 1);

            _wheel.SetTransform(_wheel.Position, -120f * MathConstants.Pi / 180f);
            CreateLeg(world, -1f, wheelAnchor, 2);
            CreateLeg(world, 1f, wheelAnchor, 2);

            // GFX
            Vector2[] points =
            {
                new Vector2(-5.4f, 6.1f),
                new Vector2(-7.2f, 1.2f),
                new Vector2(-4.3f, 1.9f),
                new Vector2(-2.9f, -0.7f),
                new Vector2(0.6f, -2.9f)
            };

            _leftShoulder = new Sprite(Managers.TextureManager.PolygonTexture(new[] { Vector2.Zero, points[3], points[4] }, Color.White * 0.6f, Colors.Black));
            _leftShoulder.Origin = Managers.TextureManager.CalculateOrigin(_leftShoulders[0]);

            _leftLeg = new Sprite(Managers.TextureManager.PolygonTexture(new[] { points[0], points[1], points[2] }, Color.White * 0.6f, Colors.Black));
            _leftLeg.Origin = Managers.TextureManager.CalculateOrigin(_leftLegs[0]);

            for (int i = 0; i < points.Length; i++)
            {
                points[i].X *= -1f;
            }

            _rightShoulder = new Sprite(Managers.TextureManager.PolygonTexture(new[] { Vector2.Zero, points[4], points[3] }, Color.White * 0.6f, Colors.Black));
            _rightShoulder.Origin = Managers.TextureManager.CalculateOrigin(_rightShoulders[0]);

            _rightLeg = new Sprite(Managers.TextureManager.PolygonTexture(new[] { points[0], points[2], points[1] }, Color.White * 0.6f, Colors.Black));
            _rightLeg.Origin = Managers.TextureManager.CalculateOrigin(_rightLegs[0]);
        }

        private void CreateLeg(World world, float direction, Vector2 wheelAnchor, int index)
        {
            Vector2[] points =
            {
                new Vector2(5.4f * direction, 6.1f),
                new Vector2(7.2f * direction, 1.2f),
                new Vector2(4.3f * direction, 1.9f),
                new Vector2(3.1f * direction, -0.8f),
                new Vector2(6.0f * direction, -1.5f),
                new Vector2(2.5f * direction, -3.7f)
            };

            PolygonShape legPolygon = new PolygonShape(1f);
            PolygonShape shoulderPolygon = new PolygonShape(1f);

            if (direction < 0f)
            {
                legPolygon.Vertices = new Vertices(new[] { points[0], points[1], points[2] });
                shoulderPolygon.Vertices = new Vertices(new[] { Vector2.Zero, points[4] - points[3], points[5] - points[3] });
            }

            if (direction > 0f)
            {
                legPolygon.Vertices = new Vertices(new[] { points[0], points[2], points[1] });
                shoulderPolygon.Vertices = new Vertices(new[] { Vector2.Zero, points[5] - points[3], points[4] - points[3] });
            }

            Body leg = BodyFactory.CreateBody(world);
            leg.BodyType = BodyType.Dynamic;
            leg.Position = _position;
            leg.AngularDamping = 10f;

            if (direction < 0f)
                _leftLegs[index] = leg;

            if (direction > 0f)
                _rightLegs[index] = leg;

            Body shoulder = BodyFactory.CreateBody(world);
            shoulder.BodyType = BodyType.Dynamic;
            shoulder.Position = points[3] + _position;
            shoulder.AngularDamping = 10f;

            if (direction < 0f)
                _leftShoulders[index] = shoulder;

            if (direction > 0f)
                _rightShoulders[index] = shoulder;

            Fixture legFixture = leg.AddFixture(legPolygon);
            legFixture.CollisionGroup = -1;

            Fixture shoulderFixture = shoulder.AddFixture(shoulderPolygon);
            shoulderFixture.CollisionGroup = -1;

            float dampingRatio = 0.5f;
            float frequencyHz = 10.0f;

            // Using a soft distancejoint can reduce some jitter.
            // It also makes the structure seem a bit more fluid by
            // acting like a suspension system.
            DistanceJoint djd = new DistanceJoint(leg, shoulder, points[1] + _position, points[4] + _position, true);
            JointHelper.LinearStiffness(frequencyHz, dampingRatio, djd.BodyA, djd.BodyB, out float stiffness, out float damping);
            djd.Damping = damping;
            djd.Stiffness = stiffness;

            world.AddJoint(djd);
            _walkerJoints.Add(djd);

            DistanceJoint djd2 = new DistanceJoint(leg, shoulder, points[2] + _position, points[3] + _position, true);
            djd2.Damping = damping;
            djd2.Stiffness = stiffness;

            world.AddJoint(djd2);
            _walkerJoints.Add(djd2);

            DistanceJoint djd3 = new DistanceJoint(leg, _wheel, points[2] + _position, wheelAnchor + _position, true);
            djd3.Damping = damping;
            djd3.Stiffness = stiffness;

            world.AddJoint(djd3);
            _walkerJoints.Add(djd3);

            DistanceJoint djd4 = new DistanceJoint(shoulder, _wheel, points[5] + _position, wheelAnchor + _position, true);
            djd4.Damping = damping;
            djd4.Stiffness = stiffness;

            world.AddJoint(djd4);
            _walkerJoints.Add(djd4);

            RevoluteJoint rjd = new RevoluteJoint(shoulder, _chassis, points[3] + _position, true);
            world.AddJoint(rjd);
        }

        public void Reverse()
        {
            _motorSpeed *= -1f;
            _motorJoint.MotorSpeed = _motorSpeed;
        }

        public void Draw(SpriteBatch batch, LineBatch lines, Camera2D camera)
        {
            batch.Begin(0, null, null, null, null, null, camera.View);
            batch.Draw(_body.Image, ConvertUnits.ToDisplayUnits(_chassis.Position), null, Color.White, _chassis.Rotation, _body.Origin, 1f, SpriteEffects.None, 0f);
            batch.End();
            for (int i = 0; i < 3; i++)
            {
                batch.Begin(0, null, null, null, null, null, camera.View);
                batch.Draw(_leftLeg.Image, ConvertUnits.ToDisplayUnits(_leftLegs[i].Position), null, _walkerColors[i], _leftLegs[i].Rotation, _leftLeg.Origin, 1f, SpriteEffects.None, 0f);
                batch.Draw(_leftShoulder.Image, ConvertUnits.ToDisplayUnits(_leftShoulders[i].Position), null, _walkerColors[i], _leftShoulders[i].Rotation, _leftShoulder.Origin, 1f, SpriteEffects.None, 0f);
                batch.Draw(_rightLeg.Image, ConvertUnits.ToDisplayUnits(_rightLegs[i].Position), null, _walkerColors[i], _rightLegs[i].Rotation, _rightLeg.Origin, 1f, SpriteEffects.None, 0f);
                batch.Draw(_rightShoulder.Image, ConvertUnits.ToDisplayUnits(_rightShoulders[i].Position), null, _walkerColors[i], _rightShoulders[i].Rotation, _rightShoulder.Origin, 1f, SpriteEffects.None, 0f);
                batch.End();

                lines.Begin(ref camera.SimProjection, ref camera.SimView);

                for (int j = 0; j < 8; j++)
                {
                    lines.DrawLine(_walkerJoints[8 * i + j].WorldAnchorA, _walkerJoints[8 * i + j].WorldAnchorB, Colors.Grey);
                }

                lines.End();
            }

            batch.Begin(0, null, null, null, null, null, camera.View);
            batch.Draw(_engine.Image, ConvertUnits.ToDisplayUnits(_wheel.Position), null, Color.White, _wheel.Rotation, _engine.Origin, 1f, SpriteEffects.None, 0f);
            batch.End();
        }
    }
}