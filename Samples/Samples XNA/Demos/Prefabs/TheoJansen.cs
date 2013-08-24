using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Samples.DrawingSystem;
using FarseerPhysics.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.Samples.Demos.Prefabs
{
    public class TheoJansenWalker
    {
        private Sprite _body;
        private Body _chassis;
        private Sprite _engine;
        private Sprite _leftLeg;
        private Body[] _leftLegs;
        private Sprite _leftShoulder;
        private Body[] _leftShoulders;
        private RevoluteJoint _motorJoint;
        private float _motorSpeed;
        private Vector2 _position;
        private Sprite _rightLeg;
        private Body[] _rightLegs;

        private Sprite _rightShoulder;
        private Body[] _rightShoulders;
        private SpriteBatch _spriteBatch;
        private LineBatch _lineBatch;
        private Camera2D _camera;

        private List<DistanceJoint> _walkerJoints;
        private Body _wheel;

        public TheoJansenWalker(World world, ScreenManager screenManager, Camera2D camera, Vector2 position)
        {
            _position = position;
            _motorSpeed = 2.0f;
            _spriteBatch = screenManager.SpriteBatch;
            _lineBatch = screenManager.LineBatch;
            _camera = camera;

            _walkerJoints = new List<DistanceJoint>();

            _leftShoulders = new Body[3];
            _rightShoulders = new Body[3];
            _leftLegs = new Body[3];
            _rightLegs = new Body[3];

            Vector2 pivot = new Vector2(0f, -0.8f);

            CreateChassis(world, pivot, screenManager.Assets);

            Vector2 wheelAnchor = pivot + new Vector2(0f, 0.8f);

            CreateLegTextures(screenManager.Assets);

            CreateLeg(world, -1f, wheelAnchor, 0);
            CreateLeg(world, 1f, wheelAnchor, 0);

            _leftLeg.Origin = AssetCreator.CalculateOrigin(_leftLegs[0]);
            _leftShoulder.Origin = AssetCreator.CalculateOrigin(_leftShoulders[0]);
            _rightLeg.Origin = AssetCreator.CalculateOrigin(_rightLegs[0]);
            _rightShoulder.Origin = AssetCreator.CalculateOrigin(_rightShoulders[0]);

            _wheel.SetTransform(_wheel.Position, 120f * Settings.Pi / 180f);
            CreateLeg(world, -1f, wheelAnchor, 1);
            CreateLeg(world, 1f, wheelAnchor, 1);

            _wheel.SetTransform(_wheel.Position, -120f * Settings.Pi / 180f);
            CreateLeg(world, -1f, wheelAnchor, 2);
            CreateLeg(world, 1f, wheelAnchor, 2);
        }

        private void CreateChassis(World world, Vector2 pivot, AssetCreator assets)
        {
            {
                PolygonShape shape = new PolygonShape(1f);
                shape.Vertices = PolygonTools.CreateRectangle(2.5f, 1.0f);

                _body = new Sprite(assets.TextureFromShape(shape, MaterialType.Blank, Color.Beige, 1f));

                _chassis = BodyFactory.CreateBody(world);
                _chassis.BodyType = BodyType.Dynamic;
                _chassis.Position = pivot + _position;

                Fixture fixture = _chassis.CreateFixture(shape);
                fixture.CollisionGroup = -1;
            }

            {
                CircleShape shape = new CircleShape(1.6f, 1f);
                _engine = new Sprite(assets.TextureFromShape(shape, MaterialType.Waves, Color.Beige * 0.8f, 1f));

                _wheel = BodyFactory.CreateBody(world);
                _wheel.BodyType = BodyType.Dynamic;
                _wheel.Position = pivot + _position;

                Fixture fixture = _wheel.CreateFixture(shape);
                fixture.CollisionGroup = -1;
            }

            {
                _motorJoint = new RevoluteJoint(_wheel, _chassis, _wheel.GetLocalPoint(_chassis.Position), Vector2.Zero);
                _motorJoint.CollideConnected = false;
                _motorJoint.MotorSpeed = _motorSpeed;
                _motorJoint.MaxMotorTorque = 400f;
                _motorJoint.MotorEnabled = true;
                world.AddJoint(_motorJoint);
            }
        }

        public void Reverse()
        {
            _motorSpeed *= -1f;
            _motorJoint.MotorSpeed = _motorSpeed;
        }

        private void CreateLeg(World world, float direction, Vector2 wheelAnchor, int index)
        {
            Vector2 p1 = new Vector2(5.4f * direction, 6.1f);
            Vector2 p2 = new Vector2(7.2f * direction, 1.2f);
            Vector2 p3 = new Vector2(4.3f * direction, 1.9f);
            Vector2 p4 = new Vector2(3.1f * direction, -0.8f);
            Vector2 p5 = new Vector2(6.0f * direction, -1.5f);
            Vector2 p6 = new Vector2(2.5f * direction, -3.7f);

            PolygonShape shoulderPolygon;
            PolygonShape legPolygon;

            Vertices vertices = new Vertices(3);

            if (direction < 0f)
            {
                vertices.Add(p1);
                vertices.Add(p2);
                vertices.Add(p3);
                shoulderPolygon = new PolygonShape(vertices, 1);

                vertices[0] = Vector2.Zero;
                vertices[1] = p5 - p4;
                vertices[2] = p6 - p4;
                legPolygon = new PolygonShape(vertices, 2);
            }
            else
            {
                vertices.Add(p1);
                vertices.Add(p3);
                vertices.Add(p2);
                shoulderPolygon = new PolygonShape(vertices, 1);

                vertices[0] = Vector2.Zero;
                vertices[1] = p6 - p4;
                vertices[2] = p5 - p4;
                legPolygon = new PolygonShape(vertices, 2);
            }

            Body leg = BodyFactory.CreateBody(world);
            leg.BodyType = BodyType.Dynamic;
            leg.Position = _position;
            leg.AngularDamping = 10f;

            if (direction < 0f)
                _leftLegs[index] = leg;
            else
                _rightLegs[index] = leg;

            Body shoulder = BodyFactory.CreateBody(world);
            shoulder.BodyType = BodyType.Dynamic;
            shoulder.Position = p4 + _position;
            shoulder.AngularDamping = 10f;

            if (direction < 0f)
                _leftShoulders[index] = shoulder;
            else
                _rightShoulders[index] = shoulder;

            Fixture f1 = leg.CreateFixture(shoulderPolygon);
            f1.CollisionGroup = -1;

            Fixture f2 = shoulder.CreateFixture(legPolygon);
            f2.CollisionGroup = -1;

            // Using a soft distanceraint can reduce some jitter.
            // It also makes the structure seem a bit more fluid by
            // acting like a suspension system.
            DistanceJoint djd = new DistanceJoint(leg, shoulder, leg.GetLocalPoint(p2 + _position), shoulder.GetLocalPoint(p5 + _position));
            djd.DampingRatio = 0.5f;
            djd.Frequency = 10f;

            world.AddJoint(djd);
            _walkerJoints.Add(djd);

            DistanceJoint djd2 = new DistanceJoint(leg, shoulder, leg.GetLocalPoint(p3 + _position), shoulder.GetLocalPoint(p4 + _position));
            djd2.DampingRatio = 0.5f;
            djd2.Frequency = 10f;

            world.AddJoint(djd2);
            _walkerJoints.Add(djd2);

            DistanceJoint djd3 = new DistanceJoint(leg, _wheel, leg.GetLocalPoint(p3 + _position), _wheel.GetLocalPoint(wheelAnchor + _position));
            djd3.DampingRatio = 0.5f;
            djd3.Frequency = 10f;

            world.AddJoint(djd3);
            _walkerJoints.Add(djd3);

            DistanceJoint djd4 = new DistanceJoint(shoulder, _wheel, shoulder.GetLocalPoint(p6 + _position), _wheel.GetLocalPoint(wheelAnchor + _position));
            djd4.DampingRatio = 0.5f;
            djd4.Frequency = 10f;

            world.AddJoint(djd4);
            _walkerJoints.Add(djd4);

            Vector2 anchor = p4 - new Vector2(0f, -0.8f);
            RevoluteJoint rjd = new RevoluteJoint(shoulder, _chassis, shoulder.GetLocalPoint(_chassis.GetWorldPoint(anchor)), anchor);
            world.AddJoint(rjd);
        }

        private void CreateLegTextures(AssetCreator assets)
        {
            Vector2 p1 = new Vector2(-5.4f, 6.1f);
            Vector2 p2 = new Vector2(-7.2f, 1.2f);
            Vector2 p3 = new Vector2(-4.3f, 1.9f);
            Vector2 p4 = new Vector2(-2.9f, -0.7f);
            Vector2 p5 = new Vector2(0.6f, -2.9f);

            Vertices vertices = new Vertices(3);

            vertices.Add(p1);
            vertices.Add(p2);
            vertices.Add(p3);
            _leftLeg = new Sprite(assets.TextureFromVertices(vertices, MaterialType.Blank, Color.IndianRed * 0.8f, 1f));

            vertices[0] = Vector2.Zero;
            vertices[1] = p4;
            vertices[2] = p5;
            _leftShoulder = new Sprite(assets.TextureFromVertices(vertices, MaterialType.Blank, Color.Beige * 0.8f, 1f));

            p1.X *= -1f;
            p2.X *= -1f;
            p3.X *= -1f;
            p4.X *= -1f;
            p5.X *= -1f;

            vertices[0] = p1;
            vertices[1] = p3;
            vertices[2] = p2;
            _rightLeg = new Sprite(assets.TextureFromVertices(vertices, MaterialType.Blank, Color.IndianRed * 0.8f, 1f));

            vertices[0] = Vector2.Zero;
            vertices[1] = p5;
            vertices[2] = p4;
            _rightShoulder = new Sprite(assets.TextureFromVertices(vertices, MaterialType.Blank, Color.Beige * 0.8f, 1f));
        }

        public void Draw()
        {
            _spriteBatch.Begin(0, null, null, null, null, null, _camera.View);
            _spriteBatch.Draw(_body.Texture, ConvertUnits.ToDisplayUnits(_chassis.Position), null, Color.White, _chassis.Rotation, _body.Origin, 1f, SpriteEffects.None, 0f);
            _spriteBatch.End();

            _lineBatch.Begin(_camera.SimProjection, _camera.SimView);
            for (int i = 0; i < _walkerJoints.Count; ++i)
            {
                _lineBatch.DrawLine(_walkerJoints[i].WorldAnchorA, _walkerJoints[i].WorldAnchorB, Color.DarkRed);
            }
            _lineBatch.End();

            _spriteBatch.Begin(0, null, null, null, null, null, _camera.View);
            for (int i = 0; i < 3; ++i)
            {
                _spriteBatch.Draw(_leftLeg.Texture, ConvertUnits.ToDisplayUnits(_leftLegs[i].Position), null, Color.White, _leftLegs[i].Rotation, _leftLeg.Origin, 1f, SpriteEffects.None, 0f);
                _spriteBatch.Draw(_leftShoulder.Texture, ConvertUnits.ToDisplayUnits(_leftShoulders[i].Position), null, Color.White, _leftShoulders[i].Rotation, _leftShoulder.Origin, 1f, SpriteEffects.None, 0f);
                _spriteBatch.Draw(_rightLeg.Texture, ConvertUnits.ToDisplayUnits(_rightLegs[i].Position), null, Color.White, _rightLegs[i].Rotation, _rightLeg.Origin, 1f, SpriteEffects.None, 0f);
                _spriteBatch.Draw(_rightShoulder.Texture, ConvertUnits.ToDisplayUnits(_rightShoulders[i].Position), null, Color.White, _rightShoulders[i].Rotation, _rightShoulder.Origin, 1f, SpriteEffects.None, 0f);
            }
            _spriteBatch.Draw(_engine.Texture, ConvertUnits.ToDisplayUnits(_wheel.Position), null, Color.White, _wheel.Rotation, _engine.Origin, 1f, SpriteEffects.None, 0f);
            _spriteBatch.End();
        }
    }
}