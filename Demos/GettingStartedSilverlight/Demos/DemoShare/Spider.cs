using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using SM = System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerSilverlightDemos.Demos.DemoShare
{
    public class Spider
    {
        Body spiderBody;
        Body leftUpperLegBody;
        Body leftLowerLegBody;
        Body rightUpperLegBody;
        Body rightLowerLegBody;

        Geom spiderGeom;
        Geom leftUpperLegGeom;
        Geom leftLowerLegGeom;
        Geom rightUpperLegGeom;
        Geom rightLowerLegGeom;

        RevoluteJoint leftShoulderRevoluteJoint;
        AngleJoint leftShoulderAngleJoint;

        RevoluteJoint rightShoulderRevoluteJoint;
        AngleJoint rightShoulderAngleJoint;

        RevoluteJoint leftKneeRevoluteJoint;
        AngleJoint leftKneeAngleJoint;

        RevoluteJoint rightKneeRevoluteJoint;
        AngleJoint rightKneeAngleJoint;

        private int spiderBodyRadius = 20;
        private Vector2 upperLegSize = new Vector2(40,12); //x=width, y=height
        private Vector2 lowerLegSize = new Vector2(30,12);

        Enums.CollisionCategories collisionCategory =Enums.CollisionCategories.All;
        Enums.CollisionCategories collidesWith = Enums.CollisionCategories.All;

        Vector2 position;

        public Spider(Vector2 position) {
            this.position = position;
        }

        public Body Body {
            get { return spiderBody; }
        }

        public void ApplyForce(Vector2 force) {
            spiderBody.ApplyForce(force);
        }

        public void ApplyTorque(float torque) {
            spiderBody.ApplyTorque(torque);
        }

        public Enums.CollisionCategories CollisionCategory {
            get { return collisionCategory; }
            set { collisionCategory = value; }
        }

        public Enums.CollisionCategories  CollidesWith {
            get { return collidesWith; }
            set { collidesWith = value; }
        }

        private int collisionGroup;

        public int CollisionGroup {
            get { return collisionGroup; }
            set { collisionGroup = value; }
        }
	

        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator) 
        {
            //Load bodies
            spiderBody = BodyFactory.Instance.CreateCircleBody(physicsSimulator,spiderBodyRadius,1);
            spiderBody.Position = position;
            spiderBody.IsStatic = false;
            view.AddCircleToCanvas(spiderBody, spiderBodyRadius);
            leftUpperLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator,upperLegSize.X, upperLegSize.Y, 1);
            leftUpperLegBody.Position = spiderBody.Position - new Vector2(spiderBodyRadius, 0) - new Vector2(upperLegSize.X/2, 0);
            view.AddRectangleToCanvas(leftUpperLegBody, SM.Colors.White, new Vector2(upperLegSize.X, upperLegSize.Y));

            leftLowerLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator,lowerLegSize.X, lowerLegSize.Y, 1);
            leftLowerLegBody.Position = spiderBody.Position - new Vector2(spiderBodyRadius, 0) - new Vector2(upperLegSize.X, 0) - new Vector2(lowerLegSize.X / 2, 0);
            view.AddRectangleToCanvas(leftLowerLegBody, SM.Colors.Red, new Vector2(lowerLegSize.X, lowerLegSize.Y));

            rightUpperLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator,upperLegSize.X, upperLegSize.Y, 1);
            rightUpperLegBody.Position = spiderBody.Position + new Vector2(spiderBodyRadius, 0) + new Vector2(upperLegSize.X / 2, 0);
            view.AddRectangleToCanvas(rightUpperLegBody, SM.Colors.White, new Vector2(upperLegSize.X, upperLegSize.Y));

            rightLowerLegBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator,lowerLegSize.X, lowerLegSize.Y, 1);
            rightLowerLegBody.Position = spiderBody.Position + new Vector2(spiderBodyRadius, 0) + new Vector2(upperLegSize.X, 0) + new Vector2(lowerLegSize.X / 2, 0);
            view.AddRectangleToCanvas(rightLowerLegBody, SM.Colors.Red, new Vector2(lowerLegSize.X, lowerLegSize.Y));
            
            //load geometries
            spiderGeom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator,spiderBody, spiderBodyRadius, 14);
            leftUpperLegGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator,leftUpperLegBody, upperLegSize.X, upperLegSize.Y);
            leftLowerLegGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator,leftLowerLegBody, lowerLegSize.X, lowerLegSize.Y);
            rightUpperLegGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator,rightUpperLegBody, upperLegSize.X, upperLegSize.Y);
            rightLowerLegGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator,rightLowerLegBody, lowerLegSize.X, lowerLegSize.Y);
            spiderGeom.CollisionGroup = collisionGroup;
            leftUpperLegGeom.CollisionGroup = collisionGroup;
            leftLowerLegGeom.CollisionGroup = collisionGroup;
            rightUpperLegGeom.CollisionGroup = collisionGroup;
            rightLowerLegGeom.CollisionGroup = collisionGroup;

            //load joints
            leftShoulderRevoluteJoint = JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, spiderBody, leftUpperLegBody, spiderBody.Position - new Vector2(spiderBodyRadius, 0));
            leftShoulderAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, spiderBody, leftUpperLegBody);
            leftShoulderAngleJoint.TargetAngle = -.4f;
            leftShoulderAngleJoint.MaxImpulse = 300;

            rightShoulderRevoluteJoint = JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, spiderBody, rightUpperLegBody, spiderBody.Position + new Vector2(spiderBodyRadius, 0));
            rightShoulderAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, spiderBody, rightUpperLegBody);
            rightShoulderAngleJoint.TargetAngle = .4f;
            leftShoulderAngleJoint.MaxImpulse = 300;

            leftKneeRevoluteJoint = JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, leftUpperLegBody, leftLowerLegBody, spiderBody.Position - new Vector2(spiderBodyRadius, 0) - new Vector2(upperLegSize.X, 0));
            leftKneeAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, leftUpperLegBody, leftLowerLegBody);
            leftKneeAngleJoint.TargetAngle = -kneeTargetAngle;
            leftKneeAngleJoint.MaxImpulse = 300;

            rightKneeRevoluteJoint = JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, rightUpperLegBody, rightLowerLegBody, spiderBody.Position + new Vector2(spiderBodyRadius, 0) + new Vector2(upperLegSize.X, 0));
            rightKneeAngleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, rightUpperLegBody, rightLowerLegBody);
            rightKneeAngleJoint.TargetAngle = kneeTargetAngle;
            rightKneeAngleJoint.MaxImpulse = 300;
        }

        bool kneeFlexed = false;
        float kneeTargetAngle = .4f;

        bool shoulderFlexed = false;
        float shoulderTargetAngle = .2f;

        float s = 0;
        public void Update(TimeSpan elapsedTime) {

            s += elapsedTime.Milliseconds;
            if (s > 4000) {
                s = 0;

                kneeFlexed = !kneeFlexed;
                shoulderFlexed = !shoulderFlexed;

                if (kneeFlexed) {
                    kneeTargetAngle = 1.4f;
                }
                else {
                   kneeTargetAngle = .4f;
                }

                if (kneeFlexed) {
                    shoulderTargetAngle = 1.2f;
                }
                else {
                    shoulderTargetAngle = .2f;
                }
            }
            leftKneeAngleJoint.TargetAngle = -kneeTargetAngle;
            rightKneeAngleJoint.TargetAngle = kneeTargetAngle;

            leftShoulderAngleJoint.TargetAngle = -shoulderTargetAngle;
            rightShoulderAngleJoint.TargetAngle = shoulderTargetAngle;

        }

    }
}
