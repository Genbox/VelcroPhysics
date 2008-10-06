using FarseerSilverlightManual.Screens;

namespace FarseerSilverlightManual.Demos
{
    public class PinSliderJointDemo : SimulatorView
    {
        public PinSliderJointDemo()
        {
            Initialize();
        }

        public override void Initialize()
        {
            ClearCanvas();

            //Pin
            //Body rectangleBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            //rectangleBody.Position = new Vector2(100, 200);

            //Geom rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody, 128, 128);
            //rectangleGeom.CollisionGroup = 1;
            //AddRectangleToCanvas(rectangleBody, new Vector2(128, 128));

            //Body rectangleBody1 = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            //rectangleBody1.Position = new Vector2(100, 300);

            //Geom rectangleGeom1 = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody1, 128, 128);
            //rectangleGeom1.CollisionGroup = 2;
            //AddRectangleToCanvas(rectangleBody1, new Vector2(128, 128));

            //PinJoint pinJoint = JointFactory.Instance.CreatePinJoint(physicsSimulator, rectangleBody, rectangleBody.Position, rectangleBody1,
            //                                     rectangleBody1.Position);
            //pinJoint.TargetDistance = 200;

            //Slider
            //Body rectangleBody2 = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            //rectangleBody2.Position = new Vector2(400, 200);

            //Geom rectangleGeom2 = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody2, 128, 128);
            //rectangleGeom2.CollisionGroup = 3;
            //AddRectangleToCanvas(rectangleBody2, new Vector2(128, 128));

            //Body rectangleBody3 = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            //rectangleBody3.Position = new Vector2(400, 300);

            //Geom rectangleGeom3 = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody3, 128, 128);
            //rectangleGeom3.CollisionGroup = 4;
            //AddRectangleToCanvas(rectangleBody3, new Vector2(128, 128));

            //JointFactory.Instance.CreateSliderJoint(physicsSimulator, rectangleBody2, rectangleBody2.Position,
            //                                        rectangleBody3, rectangleBody3.Position, 40, 100);

            base.Initialize();
        }
    }
}