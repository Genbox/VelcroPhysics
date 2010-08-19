using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseXNA.DemoShare
{
    public class Ragdoll
    {
        private const float armDensity = 10;
        private const float legDensity = 15f;
        private const float limbAngularDamping = 10f;
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


        //BODY
        private void CreateBody(World world, Vector2 position)
        {
            //Head
            _head = FixtureFactory.CreateCircle(world, .9f, 10);
            _head.Body.BodyType = BodyType.Dynamic;
            _head.Body.Mass = 2;
            _head.Body.Position = position;

            //Body
            _body = FixtureFactory.CreateRoundedRectangle(world, 2, 4, .5f, .7f, 2, 10);
            _body[0].Body.BodyType = BodyType.Dynamic;
            _body[0].Body.Mass = 2;
            _body[0].Body.Position = position + new Vector2(0, -3);

            //Left Arm
            _lowerLeftArm = FixtureFactory.CreateCapsule(world, 1, .45f, armDensity);
            _lowerLeftArm[0].Body.BodyType = BodyType.Dynamic;
            _lowerLeftArm[0].Body.AngularDamping = limbAngularDamping;
            _lowerLeftArm[0].Body.Mass = 2;
            _lowerLeftArm[0].Body.Position = position + new Vector2(-4, -1);

            _upperLeftArm = FixtureFactory.CreateCapsule(world, 1, .45f, armDensity);
            _upperLeftArm[0].Body.BodyType = BodyType.Dynamic;
            _upperLeftArm[0].Body.AngularDamping = limbAngularDamping;
            _upperLeftArm[0].Body.Mass = 2;
            _upperLeftArm[0].Body.Position = position + new Vector2(-2, -3);

            //Right Arm
            _lowerRightArm = FixtureFactory.CreateCapsule(world, 1, .45f, armDensity);
            _lowerRightArm[0].Body.BodyType = BodyType.Dynamic;
            _lowerRightArm[0].Body.AngularDamping = limbAngularDamping;
            _lowerRightArm[0].Body.Mass = 2;
            _lowerRightArm[0].Body.Position = position + new Vector2(4, -1);

            _upperRightArm = FixtureFactory.CreateCapsule(world, 1, .45f, armDensity);
            _upperRightArm[0].Body.BodyType = BodyType.Dynamic;
            _upperRightArm[0].Body.AngularDamping = limbAngularDamping;
            _upperRightArm[0].Body.Mass = 2;
            _upperRightArm[0].Body.Position = position + new Vector2(2, -3);

            //Left Leg
            _lowerLeftLeg = FixtureFactory.CreateCapsule(world, 1, .5f, legDensity);
            _lowerLeftLeg[0].Body.BodyType = BodyType.Dynamic;
            _lowerLeftLeg[0].Body.AngularDamping = limbAngularDamping;
            _lowerLeftLeg[0].Body.Mass = 2;
            _lowerLeftLeg[0].Body.Position = position + new Vector2(-2, -4);

            _upperLeftLeg = FixtureFactory.CreateCapsule(world, 1, .5f, legDensity);
            _upperLeftLeg[0].Body.BodyType = BodyType.Dynamic;
            _upperLeftLeg[0].Body.AngularDamping = limbAngularDamping;
            _upperLeftLeg[0].Body.Mass = 2;
            _upperLeftLeg[0].Body.Position = position + new Vector2(-2, -7);

            //Right Leg
            _lowerRightLeg = FixtureFactory.CreateCapsule(world, 1, .5f, legDensity);
            _lowerRightLeg[0].Body.BodyType = BodyType.Dynamic;
            _lowerRightLeg[0].Body.AngularDamping = limbAngularDamping;
            _lowerRightLeg[0].Body.Mass = 2;
            _lowerRightLeg[0].Body.Position = position + new Vector2(2, -4);

            _upperRightLeg = FixtureFactory.CreateCapsule(world, 1, .5f, legDensity);
            _upperRightLeg[0].Body.BodyType = BodyType.Dynamic;
            _upperRightLeg[0].Body.AngularDamping = limbAngularDamping;
            _upperRightLeg[0].Body.Mass = 2;
            _upperRightLeg[0].Body.Position = position + new Vector2(2, -7);
        }

        //JOINTS
        private void CreateJoints(World world)
        {
            const float dp = 1;
            const float frequency = 25;
            //head -> body
            DistanceJoint jHeadBody = new DistanceJoint(_head.Body, _body[0].Body, new Vector2(0, -1),
                                                        new Vector2(0, 2f));
            jHeadBody.CollideConnected = true;
            jHeadBody.DampingRatio = dp;
            jHeadBody.Frequency = frequency;
            jHeadBody.Length = 0.025f;
            world.AddJoint(jHeadBody);

            //lowerLeftArm -> upperLeftArm
            DistanceJoint jLeftArm = new DistanceJoint(_lowerLeftArm[0].Body, _upperLeftArm[0].Body, new Vector2(0, 1),
                                                       new Vector2(0, -1f));
            jLeftArm.CollideConnected = true;
            jLeftArm.DampingRatio = dp;
            jLeftArm.Frequency = frequency;
            jLeftArm.Length = 0.02f;
            world.AddJoint(jLeftArm);

            //upperLeftArm -> body
            DistanceJoint jLeftArmBody = new DistanceJoint(_upperLeftArm[0].Body, _body[0].Body, new Vector2(0, 1),
                                                           new Vector2(-1, 1.5f));
            jLeftArmBody.CollideConnected = true;
            jLeftArmBody.DampingRatio = dp;
            jLeftArmBody.Frequency = frequency;
            jLeftArmBody.Length = 0.02f;
            world.AddJoint(jLeftArmBody);

            //lowerRightArm -> upperRightArm
            DistanceJoint jRightArm = new DistanceJoint(_lowerRightArm[0].Body, _upperRightArm[0].Body,
                                                        new Vector2(0, 1), new Vector2(0, -1));
            jRightArm.CollideConnected = true;
            jRightArm.DampingRatio = dp;
            jRightArm.Frequency = frequency;
            jRightArm.Length = 0.02f;
            world.AddJoint(jRightArm);

            //upperRightArm -> body
            DistanceJoint jRightArmBody = new DistanceJoint(_upperRightArm[0].Body, _body[0].Body, new Vector2(0, 1),
                                                            new Vector2(1, 1.5f));
            jRightArmBody.CollideConnected = true;
            jRightArmBody.DampingRatio = dp;
            jRightArmBody.Frequency = 25;
            jRightArmBody.Length = 0.02f;
            world.AddJoint(jRightArmBody);

            //lowerLeftLeg -> upperLeftLeg
            DistanceJoint jLeftLeg = new DistanceJoint(_lowerLeftLeg[0].Body, _upperLeftLeg[0].Body,
                                                       new Vector2(0, 1.1f), new Vector2(0, -1));
            jLeftLeg.CollideConnected = true;
            jLeftLeg.DampingRatio = dp;
            jLeftLeg.Frequency = frequency;
            jLeftLeg.Length = 0.05f;
            world.AddJoint(jLeftLeg);

            //upperLeftLeg -> body
            DistanceJoint jLeftLegBody = new DistanceJoint(_upperLeftLeg[0].Body, _body[0].Body, new Vector2(0, 1.1f),
                                                           new Vector2(-0.8f, -1.9f));
            jLeftLegBody.CollideConnected = true;
            jLeftLegBody.DampingRatio = dp;
            jLeftLegBody.Frequency = frequency;
            jLeftLegBody.Length = 0.05f;
            world.AddJoint(jLeftLegBody);

            //lowerRightleg -> upperRightleg
            DistanceJoint jRightLeg = new DistanceJoint(_lowerRightLeg[0].Body, _upperRightLeg[0].Body,
                                                        new Vector2(0, 1.1f), new Vector2(0, -1));
            jRightLeg.CollideConnected = true;
            jRightLeg.DampingRatio = dp;
            jRightLeg.Frequency = frequency;
            jRightLeg.Length = 0.05f;
            world.AddJoint(jRightLeg);

            //upperRightleg -> body
            DistanceJoint jRightLegBody = new DistanceJoint(_upperRightLeg[0].Body, _body[0].Body, new Vector2(0, 1.1f),
                                                            new Vector2(0.8f, -1.9f));
            jRightLegBody.CollideConnected = true;
            jRightLegBody.DampingRatio = dp;
            jRightLegBody.Frequency = frequency;
            jRightLegBody.Length = 0.05f;
            world.AddJoint(jRightLegBody);
        }
    }
}