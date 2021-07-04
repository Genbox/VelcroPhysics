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
    public class Ragdoll
    {
        private const float ArmDensity = 10f;
        private const float LegDensity = 15f;
        private const float LimbAngularDamping = 2f;
        private const float DampingRatio = 1f;
        private const float Frequency = 25f;

        private readonly Sprite _face;

        private readonly Body _head;
        private readonly Body _lowerBody;
        private readonly Body _lowerLeftArm;
        private readonly Body _lowerLeftLeg;
        private readonly Sprite _lowerLimb;
        private readonly Body _lowerRightArm;
        private readonly Body _lowerRightLeg;
        private readonly Body _middleBody;
        private readonly Sprite _torso;
        private readonly Body _upperLeftArm;
        private readonly Body _upperLeftLeg;
        private readonly Sprite _upperLimb;
        private readonly Body _upperRightArm;
        private readonly Body _upperRightLeg;

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
            Body = BodyFactory.CreateCapsule(world, 0.5f, 0.75f, LegDensity);
            Body.BodyType = BodyType.Dynamic;
            Body.Mass = 1f;
            Body.SetTransform(position + new Vector2(0f, 1.75f), MathHelper.Pi / 2f);

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
            DistanceJoint jointHeadBody = new DistanceJoint(_head, Body, new Vector2(0f, 1f), new Vector2(-0.75f, 0f));
            JointHelper.LinearStiffness(Frequency, DampingRatio, jointHeadBody.BodyA, jointHeadBody.BodyB, out var stiffness, out var damping);
            jointHeadBody.CollideConnected = true;
            jointHeadBody.Damping = damping;
            jointHeadBody.Stiffness = stiffness;
            jointHeadBody.Length = 0.025f;
            world.AddJoint(jointHeadBody);

            // lowerLeftArm -> upperLeftArm
            DistanceJoint jointLeftArm = new DistanceJoint(_lowerLeftArm, _upperLeftArm, new Vector2(0f, -1f), new Vector2(0f, 1f));
            JointHelper.LinearStiffness(Frequency, DampingRatio, jointLeftArm.BodyA, jointLeftArm.BodyB, out stiffness, out damping);
            jointLeftArm.CollideConnected = true;
            jointLeftArm.Damping = damping;
            jointLeftArm.Stiffness = stiffness;
            jointLeftArm.Length = 0.02f;
            world.AddJoint(jointLeftArm);

            // upperLeftArm -> upper body
            DistanceJoint jointLeftArmBody = new DistanceJoint(_upperLeftArm, Body, new Vector2(0f, -1f), new Vector2(-0.15f, 1f));
            JointHelper.LinearStiffness(Frequency, DampingRatio, jointLeftArmBody.BodyA, jointLeftArmBody.BodyB, out stiffness, out damping);
            jointLeftArmBody.Damping = damping;
            jointLeftArmBody.Stiffness = stiffness;
            jointLeftArmBody.Length = 0.02f;
            world.AddJoint(jointLeftArmBody);

            // lowerRightArm -> upperRightArm
            DistanceJoint jointRightArm = new DistanceJoint(_lowerRightArm, _upperRightArm, new Vector2(0f, -1f), new Vector2(0f, 1f));
            JointHelper.LinearStiffness(Frequency, DampingRatio, jointRightArm.BodyA, jointRightArm.BodyB, out stiffness, out damping);
            jointRightArm.CollideConnected = true;
            jointRightArm.Damping = damping;
            jointRightArm.Stiffness = stiffness;
            jointRightArm.Length = 0.02f;
            world.AddJoint(jointRightArm);

            // upperRightArm -> upper body
            DistanceJoint jointRightArmBody = new DistanceJoint(_upperRightArm, Body, new Vector2(0f, -1f), new Vector2(-0.15f, -1f));
            JointHelper.LinearStiffness(Frequency, DampingRatio, jointRightArmBody.BodyA, jointRightArmBody.BodyB, out stiffness, out damping);
            jointRightArmBody.Damping = damping;
            jointRightArmBody.Stiffness = stiffness;
            jointRightArmBody.Length = 0.02f;
            world.AddJoint(jointRightArmBody);

            // lowerLeftLeg -> upperLeftLeg
            DistanceJoint jointLeftLeg = new DistanceJoint(_lowerLeftLeg, _upperLeftLeg, new Vector2(0f, -1.1f), new Vector2(0f, 1f));
            JointHelper.LinearStiffness(Frequency, DampingRatio, jointLeftLeg.BodyA, jointLeftLeg.BodyB, out stiffness, out damping);
            jointLeftLeg.CollideConnected = true;
            jointLeftLeg.Damping = damping;
            jointLeftLeg.Stiffness = stiffness;
            jointLeftLeg.Length = 0.05f;
            world.AddJoint(jointLeftLeg);

            // upperLeftLeg -> lower body
            DistanceJoint jointLeftLegBody = new DistanceJoint(_upperLeftLeg, _lowerBody, new Vector2(0f, -1.1f), new Vector2(0.7f, 0.8f));
            JointHelper.LinearStiffness(Frequency, DampingRatio, jointLeftLegBody.BodyA, jointLeftLegBody.BodyB, out stiffness, out damping);
            jointLeftLegBody.CollideConnected = true;
            jointLeftLegBody.Damping = damping;
            jointLeftLegBody.Stiffness = stiffness;
            jointLeftLegBody.Length = 0.02f;
            world.AddJoint(jointLeftLegBody);

            // lowerRightleg -> upperRightleg
            DistanceJoint jointRightLeg = new DistanceJoint(_lowerRightLeg, _upperRightLeg, new Vector2(0f, -1.1f), new Vector2(0f, 1f));
            JointHelper.LinearStiffness(Frequency, DampingRatio, jointRightLeg.BodyA, jointRightLeg.BodyB, out stiffness, out damping);
            jointRightLeg.CollideConnected = true;
            jointRightLeg.Damping = damping;
            jointRightLeg.Stiffness = stiffness;
            jointRightLeg.Length = 0.05f;
            world.AddJoint(jointRightLeg);

            // upperRightleg -> lower body
            DistanceJoint jointRightLegBody = new DistanceJoint(_upperRightLeg, _lowerBody, new Vector2(0f, -1.1f), new Vector2(0.7f, -0.8f));
            JointHelper.LinearStiffness(Frequency, DampingRatio, jointRightLegBody.BodyA, jointRightLegBody.BodyB, out stiffness, out damping);
            jointRightLegBody.CollideConnected = true;
            jointRightLegBody.Damping = damping;
            jointRightLegBody.Stiffness = stiffness;
            jointRightLegBody.Length = 0.02f;
            world.AddJoint(jointRightLegBody);

            // upper body -> middle body
            RevoluteJoint jointUpperTorso = new RevoluteJoint(Body, _middleBody, Body.Position + new Vector2(0f, 0.625f), true);
            jointUpperTorso.LimitEnabled = true;
            jointUpperTorso.SetLimits(-MathHelper.Pi / 16f, MathHelper.Pi / 16f);
            world.AddJoint(jointUpperTorso);

            // middle body -> lower body
            RevoluteJoint jointLowerTorso = new RevoluteJoint(_middleBody, _lowerBody, _middleBody.Position + new Vector2(0f, 0.625f), true);
            jointLowerTorso.LimitEnabled = true;
            jointLowerTorso.SetLimits(-MathHelper.Pi / 8f, MathHelper.Pi / 8f);
            world.AddJoint(jointLowerTorso);

            // GFX
            _face = new Sprite(Managers.TextureManager.CircleTexture(0.75f, "Square", Colors.Gold, Colors.Orange, Colors.Grey, 1f));
            _torso = new Sprite(Managers.TextureManager.PolygonTexture(PolygonUtils.CreateRoundedRectangle(1.5f, 2f, 0.75f, 0.75f, 2), "Stripe", Colors.Gold, Colors.Orange, Colors.Black, 2.0f));
            _upperLimb = new Sprite(Managers.TextureManager.PolygonTexture(PolygonUtils.CreateCapsule(1.9f, 0.45f, 16), "Square", Colors.Gold, Colors.Orange, Colors.Black, 1f));
            _lowerLimb = new Sprite(Managers.TextureManager.PolygonTexture(PolygonUtils.CreateCapsule(2f, 0.5f, 16), "Square", Colors.Gold, Colors.Orange, Colors.Black, 1f));
        }

        public Body Body { get; }

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
            batch.Draw(_torso.Image, ConvertUnits.ToDisplayUnits(Body.Position), null, Color.White, Body.Rotation, _torso.Origin, 1f, SpriteEffects.None, 0f);

            batch.Draw(_face.Image, ConvertUnits.ToDisplayUnits(_head.Position), null, Color.White, _head.Rotation, _face.Origin, 1f, SpriteEffects.None, 0f);
        }
    }
}