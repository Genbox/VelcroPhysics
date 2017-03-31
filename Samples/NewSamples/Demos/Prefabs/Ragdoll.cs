using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Samples.MediaSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.Samples.Demos.Prefabs
{
    public class Ragdoll
    {
        private const float ArmDensity = 10f;
        private const float LegDensity = 15f;
        private const float LimbAngularDamping = 2f;
        private const float DampingRatio = 1f;
        private const float Frequency = 25f;

        private Body _head;
        private Body _upperBody;
        private Body _middleBody;
        private Body _lowerBody;
        private Body _upperLeftArm;
        private Body _lowerLeftArm;
        private Body _upperRightArm;
        private Body _lowerRightArm;
        private Body _upperLeftLeg;
        private Body _lowerLeftLeg;
        private Body _upperRightLeg;
        private Body _lowerRightLeg;

        private Sprite _face;
        private Sprite _torso;
        private Sprite _upperLimb;
        private Sprite _lowerLimb;

        public Ragdoll(World world, Vector2 position)
        {
            // Physics
            // Head
            _head = BodyFactory.CreateCircle(world, 0.75f, 10f);
            _head.BodyType = BodyType.Dynamic;
            _head.AngularDamping = LimbAngularDamping;
            _head.Mass = 2f;
            _head.Position = position;

            // Torso
            _upperBody = BodyFactory.CreateCapsule(world, 0.5f, 0.75f, LegDensity);
            _upperBody.BodyType = BodyType.Dynamic;
            _upperBody.Mass = 1f;
            _upperBody.SetTransform(position + new Vector2(0f, 1.75f), MathHelper.Pi / 2f);
            _middleBody = BodyFactory.CreateCapsule(world, 0.5f, 0.75f, LegDensity);
            _middleBody.BodyType = BodyType.Dynamic;
            _middleBody.Mass = 1f;
            _middleBody.SetTransform(position + new Vector2(0f, 3f), MathHelper.Pi / 2f);
            _lowerBody = BodyFactory.CreateCapsule(world, 0.5f, 0.75f, LegDensity);
            _lowerBody.BodyType = BodyType.Dynamic;
            _lowerBody.Mass = 1f;
            _lowerBody.SetTransform(position + new Vector2(0f, 4.25f), MathHelper.Pi / 2f);

            // Left Arm
            _lowerLeftArm = BodyFactory.CreateCapsule(world, 1f, 0.45f, ArmDensity);
            _lowerLeftArm.BodyType = BodyType.Dynamic;
            _lowerLeftArm.AngularDamping = LimbAngularDamping;
            _lowerLeftArm.Mass = 2f;
            _lowerLeftArm.Rotation = 1.4f;
            _lowerLeftArm.Position = position + new Vector2(-4f, 2.2f);

            _upperLeftArm = BodyFactory.CreateCapsule(world, 1f, 0.45f, ArmDensity);
            _upperLeftArm.BodyType = BodyType.Dynamic;
            _upperLeftArm.AngularDamping = LimbAngularDamping;
            _upperLeftArm.Mass = 2f;
            _upperLeftArm.Rotation = 1.4f;
            _upperLeftArm.Position = position + new Vector2(-2f, 1.8f);

            // Right Arm
            _lowerRightArm = BodyFactory.CreateCapsule(world, 1f, 0.45f, ArmDensity);
            _lowerRightArm.BodyType = BodyType.Dynamic;
            _lowerRightArm.AngularDamping = LimbAngularDamping;
            _lowerRightArm.Mass = 2f;
            _lowerRightArm.Rotation = -1.4f;
            _lowerRightArm.Position = position + new Vector2(4f, 2.2f);

            _upperRightArm = BodyFactory.CreateCapsule(world, 1f, 0.45f, ArmDensity);
            _upperRightArm.BodyType = BodyType.Dynamic;
            _upperRightArm.AngularDamping = LimbAngularDamping;
            _upperRightArm.Mass = 2f;
            _upperRightArm.Rotation = -1.4f;
            _upperRightArm.Position = position + new Vector2(2f, 1.8f);

            // Left Leg
            _lowerLeftLeg = BodyFactory.CreateCapsule(world, 1f, 0.5f, LegDensity);
            _lowerLeftLeg.BodyType = BodyType.Dynamic;
            _lowerLeftLeg.AngularDamping = LimbAngularDamping;
            _lowerLeftLeg.Mass = 2f;
            _lowerLeftLeg.Position = position + new Vector2(-0.6f, 8f);

            _upperLeftLeg = BodyFactory.CreateCapsule(world, 1f, 0.5f, LegDensity);
            _upperLeftLeg.BodyType = BodyType.Dynamic;
            _upperLeftLeg.AngularDamping = LimbAngularDamping;
            _upperLeftLeg.Mass = 2f;
            _upperLeftLeg.Position = position + new Vector2(-0.6f, 6f);

            // Right Leg
            _lowerRightLeg = BodyFactory.CreateCapsule(world, 1f, 0.5f, LegDensity);
            _lowerRightLeg.BodyType = BodyType.Dynamic;
            _lowerRightLeg.AngularDamping = LimbAngularDamping;
            _lowerRightLeg.Mass = 2f;
            _lowerRightLeg.Position = position + new Vector2(0.6f, 8f);

            _upperRightLeg = BodyFactory.CreateCapsule(world, 1f, 0.5f, LegDensity);
            _upperRightLeg.BodyType = BodyType.Dynamic;
            _upperRightLeg.AngularDamping = LimbAngularDamping;
            _upperRightLeg.Mass = 2f;
            _upperRightLeg.Position = position + new Vector2(0.6f, 6f);

            // head -> upper body
            DistanceJoint jointHeadBody = new DistanceJoint(_head, _upperBody, new Vector2(0f, 1f), new Vector2(-0.75f, 0f));
            jointHeadBody.CollideConnected = true;
            jointHeadBody.DampingRatio = DampingRatio;
            jointHeadBody.Frequency = Frequency;
            jointHeadBody.Length = 0.025f;
            world.AddJoint(jointHeadBody);

            // lowerLeftArm -> upperLeftArm
            DistanceJoint jointLeftArm = new DistanceJoint(_lowerLeftArm, _upperLeftArm, new Vector2(0f, -1f), new Vector2(0f, 1f));
            jointLeftArm.CollideConnected = true;
            jointLeftArm.DampingRatio = DampingRatio;
            jointLeftArm.Frequency = Frequency;
            jointLeftArm.Length = 0.02f;
            world.AddJoint(jointLeftArm);

            // upperLeftArm -> upper body
            DistanceJoint jointLeftArmBody = new DistanceJoint(_upperLeftArm, _upperBody, new Vector2(0f, -1f), new Vector2(-0.15f, 1f));
            jointLeftArmBody.DampingRatio = DampingRatio;
            jointLeftArmBody.Frequency = Frequency;
            jointLeftArmBody.Length = 0.02f;
            world.AddJoint(jointLeftArmBody);

            // lowerRightArm -> upperRightArm
            DistanceJoint jointRightArm = new DistanceJoint(_lowerRightArm, _upperRightArm, new Vector2(0f, -1f), new Vector2(0f, 1f));
            jointRightArm.CollideConnected = true;
            jointRightArm.DampingRatio = DampingRatio;
            jointRightArm.Frequency = Frequency;
            jointRightArm.Length = 0.02f;
            world.AddJoint(jointRightArm);

            // upperRightArm -> upper body
            DistanceJoint jointRightArmBody = new DistanceJoint(_upperRightArm, _upperBody, new Vector2(0f, -1f), new Vector2(-0.15f, -1f));
            jointRightArmBody.DampingRatio = DampingRatio;
            jointRightArmBody.Frequency = 25;
            jointRightArmBody.Length = 0.02f;
            world.AddJoint(jointRightArmBody);

            // lowerLeftLeg -> upperLeftLeg
            DistanceJoint jointLeftLeg = new DistanceJoint(_lowerLeftLeg, _upperLeftLeg, new Vector2(0f, -1.1f), new Vector2(0f, 1f));
            jointLeftLeg.CollideConnected = true;
            jointLeftLeg.DampingRatio = DampingRatio;
            jointLeftLeg.Frequency = Frequency;
            jointLeftLeg.Length = 0.05f;
            world.AddJoint(jointLeftLeg);

            // upperLeftLeg -> lower body
            DistanceJoint jointLeftLegBody = new DistanceJoint(_upperLeftLeg, _lowerBody, new Vector2(0f, -1.1f), new Vector2(0.7f, 0.8f));
            jointLeftLegBody.CollideConnected = true;
            jointLeftLegBody.DampingRatio = DampingRatio;
            jointLeftLegBody.Frequency = Frequency;
            jointLeftLegBody.Length = 0.02f;
            world.AddJoint(jointLeftLegBody);

            // lowerRightleg -> upperRightleg
            DistanceJoint jointRightLeg = new DistanceJoint(_lowerRightLeg, _upperRightLeg, new Vector2(0f, -1.1f), new Vector2(0f, 1f));
            jointRightLeg.CollideConnected = true;
            jointRightLeg.DampingRatio = DampingRatio;
            jointRightLeg.Frequency = Frequency;
            jointRightLeg.Length = 0.05f;
            world.AddJoint(jointRightLeg);

            // upperRightleg -> lower body
            DistanceJoint jointRightLegBody = new DistanceJoint(_upperRightLeg, _lowerBody, new Vector2(0f, -1.1f), new Vector2(0.7f, -0.8f));
            jointRightLegBody.CollideConnected = true;
            jointRightLegBody.DampingRatio = DampingRatio;
            jointRightLegBody.Frequency = Frequency;
            jointRightLegBody.Length = 0.02f;
            world.AddJoint(jointRightLegBody);

            // upper body -> middle body
            RevoluteJoint jointUpperTorso = new RevoluteJoint(_upperBody, _middleBody, _upperBody.Position + new Vector2(0f, 0.625f), true);
            jointUpperTorso.LimitEnabled = true;
            jointUpperTorso.SetLimits(-MathHelper.Pi / 16f, MathHelper.Pi / 16f);
            world.AddJoint(jointUpperTorso);

            // middle body -> lower body
            RevoluteJoint jointLowerTorso = new RevoluteJoint(_middleBody, _lowerBody, _middleBody.Position + new Vector2(0f, 0.625f), true);
            jointLowerTorso.LimitEnabled = true;
            jointLowerTorso.SetLimits(-MathHelper.Pi / 8f, MathHelper.Pi / 8f);
            world.AddJoint(jointLowerTorso);

            // GFX
            _face = new Sprite(ContentWrapper.CircleTexture(0.75f, "Square", ContentWrapper.Gold, ContentWrapper.Orange, ContentWrapper.Grey, 1f));
            _torso = new Sprite(ContentWrapper.PolygonTexture(PolygonTools.CreateRoundedRectangle(1.5f, 2f, 0.75f, 0.75f, 2), "Stripe", ContentWrapper.Gold, ContentWrapper.Orange, ContentWrapper.Black, 2.0f));
            _upperLimb = new Sprite(ContentWrapper.PolygonTexture(PolygonTools.CreateCapsule(1.9f, 0.45f, 16), "Square", ContentWrapper.Gold, ContentWrapper.Orange, ContentWrapper.Black, 1f));
            _lowerLimb = new Sprite(ContentWrapper.PolygonTexture(PolygonTools.CreateCapsule(2f, 0.5f, 16), "Square", ContentWrapper.Gold, ContentWrapper.Orange, ContentWrapper.Black, 1f));
        }

        public Body Body
        {
            get { return _upperBody; }
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(_lowerLimb.Image, ConvertUnits.ToDisplayUnits(_lowerLeftLeg.Position), null, Color.White, _lowerLeftLeg.Rotation, _lowerLimb.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_lowerLimb.Image, ConvertUnits.ToDisplayUnits(_lowerRightLeg.Position), null, Color.White, _lowerRightLeg.Rotation, _lowerLimb.Origin, 1f, SpriteEffects.None, 0f);

            batch.Draw(_lowerLimb.Image, ConvertUnits.ToDisplayUnits(_upperLeftLeg.Position), null, Color.White, _upperLeftLeg.Rotation, _lowerLimb.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_lowerLimb.Image, ConvertUnits.ToDisplayUnits(_upperRightLeg.Position), null, Color.White, _upperRightLeg.Rotation, _lowerLimb.Origin, 1f, SpriteEffects.None, 0f);

            batch.Draw(_upperLimb.Image, ConvertUnits.ToDisplayUnits(_lowerLeftArm.Position), null, Color.White, _lowerLeftArm.Rotation, _upperLimb.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_upperLimb.Image, ConvertUnits.ToDisplayUnits(_lowerRightArm.Position), null, Color.White, _lowerRightArm.Rotation, _upperLimb.Origin, 1f, SpriteEffects.None, 0f);

            batch.Draw(_upperLimb.Image, ConvertUnits.ToDisplayUnits(_upperLeftArm.Position), null, Color.White, _upperLeftArm.Rotation, _upperLimb.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_upperLimb.Image, ConvertUnits.ToDisplayUnits(_upperRightArm.Position), null, Color.White, _upperRightArm.Rotation, _upperLimb.Origin, 1f, SpriteEffects.None, 0f);

            batch.Draw(_torso.Image, ConvertUnits.ToDisplayUnits(_lowerBody.Position), null, Color.White, _lowerBody.Rotation, _torso.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_torso.Image, ConvertUnits.ToDisplayUnits(_middleBody.Position), null, Color.White, _middleBody.Rotation, _torso.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_torso.Image, ConvertUnits.ToDisplayUnits(_upperBody.Position), null, Color.White, _upperBody.Rotation, _torso.Origin, 1f, SpriteEffects.None, 0f);

            batch.Draw(_face.Image, ConvertUnits.ToDisplayUnits(_head.Position), null, Color.White, _head.Rotation, _face.Origin, 1f, SpriteEffects.None, 0f);
        }
    }
}