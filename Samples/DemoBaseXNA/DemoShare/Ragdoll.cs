using System.Collections.Generic;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseXNA.DemoShare
{
    public class Ragdoll
    {
        private const float ArmDensity = 10;
        private const float LegDensity = 15;
        private const float LimbAngularDamping = 7;

        private List<Fixture> _body;
        private Fixture _head;

        private List<Fixture> _lowerLeftArm;
        private List<Fixture> _lowerLeftLeg;
        private List<Fixture> _lowerRightArm;
        private List<Fixture> _lowerRightLeg;

        private List<Fixture> _upperLeftArm;
        private List<Fixture> _upperLeftLeg;
        private List<Fixture> _upperRightArm;
        private List<Fixture> _upperRightLeg;


        public Ragdoll(World world, Vector2 position)
        {
            CreateBody(world, position);
            CreateJoints(world);
        }

        public Body Body
        {
            get { return _body[0].Body; }
        }

        //Torso
        private void CreateBody(World world, Vector2 position)
        {
            DebugMaterial matHead = new DebugMaterial(MaterialType.Face)
                                        {
                                            Color = Color.DeepSkyBlue,
                                            Scale = 2f
                                        };
            DebugMaterial matBody = new DebugMaterial(MaterialType.Squares)
                                        {
                                            Color = Color.DeepSkyBlue,
                                            Scale = 8f
                                        };

            //Head
            _head = FixtureFactory.CreateCircle(world, .9f, 10, matHead);
            _head.Body.BodyType = BodyType.Dynamic;
            _head.Body.AngularDamping = LimbAngularDamping;
            _head.Body.Mass = 2;
            _head.Body.Position = position;

            //Body
            _body = FixtureFactory.CreateRoundedRectangle(world, 2, 4, .5f, .7f, 2, 10, matBody);
            _body[0].Body.BodyType = BodyType.Dynamic;
            _body[0].Body.Mass = 2;
            _body[0].Body.Position = position + new Vector2(0, -3);

            //Left Arm
            _lowerLeftArm = FixtureFactory.CreateCapsule(world, 1, .45f, ArmDensity, matBody);
            _lowerLeftArm[0].Body.BodyType = BodyType.Dynamic;
            _lowerLeftArm[0].Body.AngularDamping = LimbAngularDamping;
            _lowerLeftArm[0].Body.Mass = 2;
            _lowerLeftArm[0].Body.Rotation = -1.4f;
            _lowerLeftArm[0].Body.Position = position + new Vector2(-4, -2.2f);

            _upperLeftArm = FixtureFactory.CreateCapsule(world, 1, .45f, ArmDensity, matBody);
            _upperLeftArm[0].Body.BodyType = BodyType.Dynamic;
            _upperLeftArm[0].Body.AngularDamping = LimbAngularDamping;
            _upperLeftArm[0].Body.Mass = 2;
            _upperLeftArm[0].Body.Rotation = -1.4f;
            _upperLeftArm[0].Body.Position = position + new Vector2(-2, -1.8f);

            //Right Arm
            _lowerRightArm = FixtureFactory.CreateCapsule(world, 1, .45f, ArmDensity, matBody);
            _lowerRightArm[0].Body.BodyType = BodyType.Dynamic;
            _lowerRightArm[0].Body.AngularDamping = LimbAngularDamping;
            _lowerRightArm[0].Body.Mass = 2;
            _lowerRightArm[0].Body.Rotation = 1.4f;
            _lowerRightArm[0].Body.Position = position + new Vector2(4, -2.2f);

            _upperRightArm = FixtureFactory.CreateCapsule(world, 1, .45f, ArmDensity, matBody);
            _upperRightArm[0].Body.BodyType = BodyType.Dynamic;
            _upperRightArm[0].Body.AngularDamping = LimbAngularDamping;
            _upperRightArm[0].Body.Mass = 2;
            _upperRightArm[0].Body.Rotation = 1.4f;
            _upperRightArm[0].Body.Position = position + new Vector2(2, -1.8f);

            //Left Leg
            _lowerLeftLeg = FixtureFactory.CreateCapsule(world, 1, .5f, LegDensity, matBody);
            _lowerLeftLeg[0].Body.BodyType = BodyType.Dynamic;
            _lowerLeftLeg[0].Body.AngularDamping = LimbAngularDamping;
            _lowerLeftLeg[0].Body.Mass = 2;
            _lowerLeftLeg[0].Body.Position = position + new Vector2(-0.6f, -8);

            _upperLeftLeg = FixtureFactory.CreateCapsule(world, 1, .5f, LegDensity, matBody);
            _upperLeftLeg[0].Body.BodyType = BodyType.Dynamic;
            _upperLeftLeg[0].Body.AngularDamping = LimbAngularDamping;
            _upperLeftLeg[0].Body.Mass = 2;
            _upperLeftLeg[0].Body.Position = position + new Vector2(-0.6f, -6);

            //Right Leg
            _lowerRightLeg = FixtureFactory.CreateCapsule(world, 1, .5f, LegDensity, matBody);
            _lowerRightLeg[0].Body.BodyType = BodyType.Dynamic;
            _lowerRightLeg[0].Body.AngularDamping = LimbAngularDamping;
            _lowerRightLeg[0].Body.Mass = 2;
            _lowerRightLeg[0].Body.Position = position + new Vector2(0.6f, -8);

            _upperRightLeg = FixtureFactory.CreateCapsule(world, 1, .5f, LegDensity, matBody);
            _upperRightLeg[0].Body.BodyType = BodyType.Dynamic;
            _upperRightLeg[0].Body.AngularDamping = LimbAngularDamping;
            _upperRightLeg[0].Body.Mass = 2;
            _upperRightLeg[0].Body.Position = position + new Vector2(0.6f, -6);
        }

        private void CreateJoints(World world)
        {
            const float dampingRatio = 1;
            const float frequency = 25;

            //head -> body
            DistanceJoint jHeadBody = new DistanceJoint(_head.Body, _body[0].Body,
                                                        new Vector2(0, -1), new Vector2(0, 2));
            jHeadBody.CollideConnected = true;
            jHeadBody.DampingRatio = dampingRatio;
            jHeadBody.Frequency = frequency;
            jHeadBody.Length = 0.025f;
            world.AddJoint(jHeadBody);

            //lowerLeftArm -> upperLeftArm
            DistanceJoint jLeftArm = new DistanceJoint(_lowerLeftArm[0].Body, _upperLeftArm[0].Body,
                                                       new Vector2(0, 1), new Vector2(0, -1));
            jLeftArm.CollideConnected = true;
            jLeftArm.DampingRatio = dampingRatio;
            jLeftArm.Frequency = frequency;
            jLeftArm.Length = 0.02f;
            world.AddJoint(jLeftArm);

            //upperLeftArm -> body
            DistanceJoint jLeftArmBody = new DistanceJoint(_upperLeftArm[0].Body, _body[0].Body,
                                                           new Vector2(0, 1), new Vector2(-1, 1.5f));
            jLeftArmBody.CollideConnected = true;
            jLeftArmBody.DampingRatio = dampingRatio;
            jLeftArmBody.Frequency = frequency;
            jLeftArmBody.Length = 0.02f;
            world.AddJoint(jLeftArmBody);

            //lowerRightArm -> upperRightArm
            DistanceJoint jRightArm = new DistanceJoint(_lowerRightArm[0].Body, _upperRightArm[0].Body,
                                                        new Vector2(0, 1), new Vector2(0, -1));
            jRightArm.CollideConnected = true;
            jRightArm.DampingRatio = dampingRatio;
            jRightArm.Frequency = frequency;
            jRightArm.Length = 0.02f;
            world.AddJoint(jRightArm);

            //upperRightArm -> body
            DistanceJoint jRightArmBody = new DistanceJoint(_upperRightArm[0].Body, _body[0].Body,
                                                            new Vector2(0, 1), new Vector2(1, 1.5f));

            jRightArmBody.CollideConnected = true;
            jRightArmBody.DampingRatio = dampingRatio;
            jRightArmBody.Frequency = 25;
            jRightArmBody.Length = 0.02f;
            world.AddJoint(jRightArmBody);

            //lowerLeftLeg -> upperLeftLeg
            DistanceJoint jLeftLeg = new DistanceJoint(_lowerLeftLeg[0].Body, _upperLeftLeg[0].Body,
                                                       new Vector2(0, 1.1f), new Vector2(0, -1));
            jLeftLeg.CollideConnected = true;
            jLeftLeg.DampingRatio = dampingRatio;
            jLeftLeg.Frequency = frequency;
            jLeftLeg.Length = 0.05f;
            world.AddJoint(jLeftLeg);

            //upperLeftLeg -> body
            DistanceJoint jLeftLegBody = new DistanceJoint(_upperLeftLeg[0].Body, _body[0].Body,
                                                           new Vector2(0, 1.1f), new Vector2(-0.8f, -1.9f));
            jLeftLegBody.CollideConnected = true;
            jLeftLegBody.DampingRatio = dampingRatio;
            jLeftLegBody.Frequency = frequency;
            jLeftLegBody.Length = 0.02f;
            world.AddJoint(jLeftLegBody);

            //lowerRightleg -> upperRightleg
            DistanceJoint jRightLeg = new DistanceJoint(_lowerRightLeg[0].Body, _upperRightLeg[0].Body,
                                                        new Vector2(0, 1.1f), new Vector2(0, -1));
            jRightLeg.CollideConnected = true;
            jRightLeg.DampingRatio = dampingRatio;
            jRightLeg.Frequency = frequency;
            jRightLeg.Length = 0.05f;
            world.AddJoint(jRightLeg);

            //upperRightleg -> body
            DistanceJoint jRightLegBody = new DistanceJoint(_upperRightLeg[0].Body, _body[0].Body,
                                                            new Vector2(0, 1.1f), new Vector2(0.8f, -1.9f));
            jRightLegBody.CollideConnected = true;
            jRightLegBody.DampingRatio = dampingRatio;
            jRightLegBody.Frequency = frequency;
            jRightLegBody.Length = 0.02f;
            world.AddJoint(jRightLegBody);
        }
    }
}