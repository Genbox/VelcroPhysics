using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoShare
{
    public class Ragdoll
    {
        private const float ArmDensity = 10;
        private const float LegDensity = 15;
        private const float LimbAngularDamping = 7;

        private Body _body;
        private Body _head;

        private Body _lowerLeftArm;
        private Body _lowerLeftLeg;
        private Body _lowerRightArm;
        private Body _lowerRightLeg;
        
        private Body _upperLeftArm;
        private Body _upperLeftLeg;
        private Body _upperRightArm;
        private Body _upperRightLeg;


        public Ragdoll(World world, Vector2 position)
        {
            CreateBody(world, position);
            CreateJoints(world);
        }

        public Body Body
        {
            get { return _body; }
        }

        //Torso
        private void CreateBody(World world, Vector2 position)
        {
            //Head
            _head = BodyFactory.CreateCircle(world, .9f, 10);
            _head.BodyType = BodyType.Dynamic;
            _head.AngularDamping = LimbAngularDamping;
            _head.Mass = 2;
            _head.Position = position;

            //Body
            _body = BodyFactory.CreateRoundedRectangle(world, 2, 4, .5f, .7f, 2, 10);
            _body.BodyType = BodyType.Dynamic;
            _body.Mass = 2;
            _body.Position = position + new Vector2(0, -3);

            //Left Arm
            _lowerLeftArm = BodyFactory.CreateCapsule(world, 1, .45f, ArmDensity);
            _lowerLeftArm.BodyType = BodyType.Dynamic;
            _lowerLeftArm.AngularDamping = LimbAngularDamping;
            _lowerLeftArm.Mass = 2;
            _lowerLeftArm.Rotation = -1.4f;
            _lowerLeftArm.Position = position + new Vector2(-4, -2.2f);

            _upperLeftArm = BodyFactory.CreateCapsule(world, 1, .45f, ArmDensity);
            _upperLeftArm.BodyType = BodyType.Dynamic;
            _upperLeftArm.AngularDamping = LimbAngularDamping;
            _upperLeftArm.Mass = 2;
            _upperLeftArm.Rotation = -1.4f;
            _upperLeftArm.Position = position + new Vector2(-2, -1.8f);

            //Right Arm
            _lowerRightArm = BodyFactory.CreateCapsule(world, 1, .45f, ArmDensity);
            _lowerRightArm.BodyType = BodyType.Dynamic;
            _lowerRightArm.AngularDamping = LimbAngularDamping;
            _lowerRightArm.Mass = 2;
            _lowerRightArm.Rotation = 1.4f;
            _lowerRightArm.Position = position + new Vector2(4, -2.2f);

            _upperRightArm = BodyFactory.CreateCapsule(world, 1, .45f, ArmDensity);
            _upperRightArm.BodyType = BodyType.Dynamic;
            _upperRightArm.AngularDamping = LimbAngularDamping;
            _upperRightArm.Mass = 2;
            _upperRightArm.Rotation = 1.4f;
            _upperRightArm.Position = position + new Vector2(2, -1.8f);

            //Left Leg
            _lowerLeftLeg = BodyFactory.CreateCapsule(world, 1, .5f, LegDensity);
            _lowerLeftLeg.BodyType = BodyType.Dynamic;
            _lowerLeftLeg.AngularDamping = LimbAngularDamping;
            _lowerLeftLeg.Mass = 2;
            _lowerLeftLeg.Position = position + new Vector2(-0.6f, -8);

            _upperLeftLeg = BodyFactory.CreateCapsule(world, 1, .5f, LegDensity);
            _upperLeftLeg.BodyType = BodyType.Dynamic;
            _upperLeftLeg.AngularDamping = LimbAngularDamping;
            _upperLeftLeg.Mass = 2;
            _upperLeftLeg.Position = position + new Vector2(-0.6f, -6);

            //Right Leg
            _lowerRightLeg = BodyFactory.CreateCapsule(world, 1, .5f, LegDensity);
            _lowerRightLeg.BodyType = BodyType.Dynamic;
            _lowerRightLeg.AngularDamping = LimbAngularDamping;
            _lowerRightLeg.Mass = 2;
            _lowerRightLeg.Position = position + new Vector2(0.6f, -8);

            _upperRightLeg = BodyFactory.CreateCapsule(world, 1, .5f, LegDensity);
            _upperRightLeg.BodyType = BodyType.Dynamic;
            _upperRightLeg.AngularDamping = LimbAngularDamping;
            _upperRightLeg.Mass = 2;
            _upperRightLeg.Position = position + new Vector2(0.6f, -6);
        }

        private void CreateJoints(World world)
        {
            const float dampingRatio = 1;
            const float frequency = 25;

            //head -> body
            DistanceJoint jHeadBody = new DistanceJoint(_head, _body,
                                                        new Vector2(0, -1), new Vector2(0, 2));
            jHeadBody.CollideConnected = true;
            jHeadBody.DampingRatio = dampingRatio;
            jHeadBody.Frequency = frequency;
            jHeadBody.Length = 0.025f;
            world.AddJoint(jHeadBody);

            //lowerLeftArm -> upperLeftArm
            DistanceJoint jLeftArm = new DistanceJoint(_lowerLeftArm, _upperLeftArm,
                                                       new Vector2(0, 1), new Vector2(0, -1));
            jLeftArm.CollideConnected = true;
            jLeftArm.DampingRatio = dampingRatio;
            jLeftArm.Frequency = frequency;
            jLeftArm.Length = 0.02f;
            world.AddJoint(jLeftArm);

            //upperLeftArm -> body
            DistanceJoint jLeftArmBody = new DistanceJoint(_upperLeftArm, _body,
                                                           new Vector2(0, 1), new Vector2(-1, 1.5f));
            jLeftArmBody.CollideConnected = true;
            jLeftArmBody.DampingRatio = dampingRatio;
            jLeftArmBody.Frequency = frequency;
            jLeftArmBody.Length = 0.02f;
            world.AddJoint(jLeftArmBody);

            //lowerRightArm -> upperRightArm
            DistanceJoint jRightArm = new DistanceJoint(_lowerRightArm, _upperRightArm,
                                                        new Vector2(0, 1), new Vector2(0, -1));
            jRightArm.CollideConnected = true;
            jRightArm.DampingRatio = dampingRatio;
            jRightArm.Frequency = frequency;
            jRightArm.Length = 0.02f;
            world.AddJoint(jRightArm);

            //upperRightArm -> body
            DistanceJoint jRightArmBody = new DistanceJoint(_upperRightArm, _body,
                                                            new Vector2(0, 1), new Vector2(1, 1.5f));

            jRightArmBody.CollideConnected = true;
            jRightArmBody.DampingRatio = dampingRatio;
            jRightArmBody.Frequency = 25;
            jRightArmBody.Length = 0.02f;
            world.AddJoint(jRightArmBody);

            //lowerLeftLeg -> upperLeftLeg
            DistanceJoint jLeftLeg = new DistanceJoint(_lowerLeftLeg, _upperLeftLeg,
                                                       new Vector2(0, 1.1f), new Vector2(0, -1));
            jLeftLeg.CollideConnected = true;
            jLeftLeg.DampingRatio = dampingRatio;
            jLeftLeg.Frequency = frequency;
            jLeftLeg.Length = 0.05f;
            world.AddJoint(jLeftLeg);

            //upperLeftLeg -> body
            DistanceJoint jLeftLegBody = new DistanceJoint(_upperLeftLeg, _body,
                                                           new Vector2(0, 1.1f), new Vector2(-0.8f, -1.9f));
            jLeftLegBody.CollideConnected = true;
            jLeftLegBody.DampingRatio = dampingRatio;
            jLeftLegBody.Frequency = frequency;
            jLeftLegBody.Length = 0.02f;
            world.AddJoint(jLeftLegBody);

            //lowerRightleg -> upperRightleg
            DistanceJoint jRightLeg = new DistanceJoint(_lowerRightLeg, _upperRightLeg,
                                                        new Vector2(0, 1.1f), new Vector2(0, -1));
            jRightLeg.CollideConnected = true;
            jRightLeg.DampingRatio = dampingRatio;
            jRightLeg.Frequency = frequency;
            jRightLeg.Length = 0.05f;
            world.AddJoint(jRightLeg);

            //upperRightleg -> body
            DistanceJoint jRightLegBody = new DistanceJoint(_upperRightLeg, _body,
                                                            new Vector2(0, 1.1f), new Vector2(0.8f, -1.9f));
            jRightLegBody.CollideConnected = true;
            jRightLegBody.DampingRatio = dampingRatio;
            jRightLegBody.Frequency = frequency;
            jRightLegBody.Length = 0.02f;
            world.AddJoint(jRightLegBody);
        }
    }
}