#region Using System
using System;
#endregion
#region Using XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion
#region Using Farseer
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Samples.MediaSystem;
using FarseerPhysics.Samples.ScreenSystem;
#endregion

namespace FarseerPhysics.Samples.Demos.Prefabs
{
  public class Ragdoll
  {
    private const float ArmDensity = 10;
    private const float LegDensity = 15;
    private const float LimbAngularDamping = 7;
    private const float DampingRatio = 1f;
    private const float Frequency = 25f;

    private Body _head;
    private Body _body;
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
    private Sprite _upperArm;
    private Sprite _lowerArm;
    private Sprite _upperLeg;
    private Sprite _lowerLeg;

    public Ragdoll(World world, Vector2 position)
    {
      // Physics
      // Head
      _head = BodyFactory.CreateCircle(world, 0.9f, 10f);
      _head.BodyType = BodyType.Dynamic;
      _head.AngularDamping = LimbAngularDamping;
      _head.Mass = 2f;
      _head.Position = position;

      // Torso
      _body = BodyFactory.CreateRoundedRectangle(world, 2f, 4f, 0.5f, 0.7f, 2, 10f);
      _body.BodyType = BodyType.Dynamic;
      _body.Mass = 2f;
      _body.Position = position + new Vector2(0f, 3f);

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

      // head -> body
      DistanceJoint jointHeadBody = new DistanceJoint(_head, _body, new Vector2(0f, 1f), new Vector2(0f, -2f));
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

      // upperLeftArm -> body
      DistanceJoint jointLeftArmBody = new DistanceJoint(_upperLeftArm, _body, new Vector2(0f, -1f), new Vector2(-1f, -1.5f));
      jointLeftArmBody.CollideConnected = true;
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

      // upperRightArm -> body
      DistanceJoint jointRightArmBody = new DistanceJoint(_upperRightArm, _body, new Vector2(0f, -1f), new Vector2(1f, -1.5f));
      jointRightArmBody.CollideConnected = true;
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

      // upperLeftLeg -> body
      DistanceJoint jointLeftLegBody = new DistanceJoint(_upperLeftLeg, _body, new Vector2(0f, -1.1f), new Vector2(-0.8f, 1.9f));
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

      // upperRightleg -> body
      DistanceJoint jointRightLegBody = new DistanceJoint(_upperRightLeg, _body, new Vector2(0f, -1.1f), new Vector2(0.8f, 1.9f));
      jointRightLegBody.CollideConnected = true;
      jointRightLegBody.DampingRatio = DampingRatio;
      jointRightLegBody.Frequency = Frequency;
      jointRightLegBody.Length = 0.02f;
      world.AddJoint(jointRightLegBody);

      // GFX
      _face = new Sprite(AssetCreator.CircleTexture(0.9f, "square", AssetCreator.Lime, AssetCreator.Teal, AssetCreator.Grey, 1f));
      _torso = new Sprite(AssetCreator.PolygonTexture(PolygonTools.CreateRoundedRectangle(2f, 4f, 0.5f, 0.7f, 2), "stripe", AssetCreator.Lime, AssetCreator.Teal, AssetCreator.Black, 2.0f));
      _upperArm = new Sprite(AssetCreator.PolygonTexture(PolygonTools.CreateCapsule(1.9f, 0.45f, 16), "square", AssetCreator.Lime, AssetCreator.Teal, AssetCreator.Black, 1f));
      _lowerArm = new Sprite(AssetCreator.PolygonTexture(PolygonTools.CreateCapsule(1.9f, 0.45f, 16), "square", AssetCreator.Lime, AssetCreator.Teal, AssetCreator.Black, 1f));
      _upperLeg = new Sprite(AssetCreator.PolygonTexture(PolygonTools.CreateCapsule(2f, 0.5f, 16), "square", AssetCreator.Lime, AssetCreator.Teal, AssetCreator.Black, 1f));
      _lowerLeg = new Sprite(AssetCreator.PolygonTexture(PolygonTools.CreateCapsule(2f, 0.5f, 16), "square", AssetCreator.Lime, AssetCreator.Teal, AssetCreator.Black, 1f));
    }

    public Body Body
    {
      get { return _body; }
    }

    public void Draw(SpriteBatch batch)
    {
      batch.Draw(_lowerLeg.Image, ConvertUnits.ToDisplayUnits(_lowerLeftLeg.Position), null,
                 Color.White, _lowerLeftLeg.Rotation, _lowerLeg.Origin, 1f, SpriteEffects.None, 0f);
      batch.Draw(_lowerLeg.Image, ConvertUnits.ToDisplayUnits(_lowerRightLeg.Position), null,
                 Color.White, _lowerRightLeg.Rotation, _lowerLeg.Origin, 1f, SpriteEffects.None, 0f);

      batch.Draw(_upperLeg.Image, ConvertUnits.ToDisplayUnits(_upperLeftLeg.Position), null,
                 Color.White, _upperLeftLeg.Rotation, _upperLeg.Origin, 1f, SpriteEffects.None, 0f);
      batch.Draw(_upperLeg.Image, ConvertUnits.ToDisplayUnits(_upperRightLeg.Position), null,
                 Color.White, _upperRightLeg.Rotation, _upperLeg.Origin, 1f, SpriteEffects.None, 0f);

      batch.Draw(_lowerArm.Image, ConvertUnits.ToDisplayUnits(_lowerLeftArm.Position), null,
                 Color.White, _lowerLeftArm.Rotation, _lowerArm.Origin, 1f, SpriteEffects.None, 0f);
      batch.Draw(_lowerArm.Image, ConvertUnits.ToDisplayUnits(_lowerRightArm.Position), null,
                 Color.White, _lowerRightArm.Rotation, _lowerArm.Origin, 1f, SpriteEffects.None, 0f);

      batch.Draw(_upperArm.Image, ConvertUnits.ToDisplayUnits(_upperLeftArm.Position), null,
                 Color.White, _upperLeftArm.Rotation, _upperArm.Origin, 1f, SpriteEffects.None, 0f);
      batch.Draw(_upperArm.Image, ConvertUnits.ToDisplayUnits(_upperRightArm.Position), null,
                 Color.White, _upperRightArm.Rotation, _upperArm.Origin, 1f, SpriteEffects.None, 0f);

      batch.Draw(_torso.Image, ConvertUnits.ToDisplayUnits(_body.Position), null,
                 Color.White, _body.Rotation, _torso.Origin, 1f, SpriteEffects.None, 0f);

      batch.Draw(_face.Image, ConvertUnits.ToDisplayUnits(_head.Position), null,
                 Color.White, _head.Rotation, _face.Origin, 1f, SpriteEffects.None, 0f);
    }
  }
}