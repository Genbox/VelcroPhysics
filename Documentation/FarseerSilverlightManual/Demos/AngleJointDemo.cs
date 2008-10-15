using System.Windows.Media;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerSilverlightManual.Objects;
using FarseerSilverlightManual.Screens;

namespace FarseerSilverlightManual.Demos
{
    public class AngleJointDemo : SimulatorView
    {
        public AngleJointDemo()
        {
            Initialize();
        }

        public override void Initialize()
        {
            Body rectangleBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody.Position = new Vector2(100, 200);

            Geom rectangleGeom2 = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody, 128, 128);
            rectangleGeom2.CollisionGroup = 3;
            RectangleBrush brush = AddRectangleToCanvas(rectangleBody, new Vector2(128, 128));
            brush.Extender.Color = Color.FromArgb(255, 0, 255, 0);

            FixedAngleJoint joint = JointFactory.Instance.CreateFixedAngleJoint(physicsSimulator, rectangleBody);
            joint.TargetAngle = MathHelper.ToRadians(200);

            Body rectangleBody1 = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody1.Position = new Vector2(ScreenManager.ScreenWidth/2f, 200);

            Geom rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody1, 128, 128);
            rectangleGeom.CollisionGroup = 1;
            AddRectangleToCanvas(rectangleBody1, new Vector2(128, 128));

            Body rectangleBody2 = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody2.Position = new Vector2(ScreenManager.ScreenWidth/2f, 300);

            Geom rectangleGeom1 = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody2, 128, 128);
            rectangleGeom1.CollisionGroup = 2;
            AddRectangleToCanvas(rectangleBody2, new Vector2(128, 128));

            JointFactory.Instance.CreateAngleJoint(physicsSimulator, rectangleBody1, rectangleBody2);

            base.Initialize();
        }
    }
}