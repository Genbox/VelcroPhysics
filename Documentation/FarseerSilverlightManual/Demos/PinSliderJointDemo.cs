using System.Windows.Media;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerSilverlightManual.Objects;
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
            //Pin
            Body rectangleBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody.Position = new Vector2(100, 200);

            Geom rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody, 128, 128);
            rectangleGeom.CollisionGroup = 1;
            RectangleBrush brush1 = AddRectangleToCanvas(rectangleBody, new Vector2(128, 128));
            brush1.Extender.Color = Color.FromArgb(255, 255, 0, 0);

            Body rectangleBody1 = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody1.Position = new Vector2(100, 400);

            Geom rectangleGeom1 = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody1, 128, 128);
            rectangleGeom1.CollisionGroup = 2;
            RectangleBrush brush2 = AddRectangleToCanvas(rectangleBody1, new Vector2(128, 128));
            brush2.Extender.Color = Color.FromArgb(255, 255, 0, 0);

            JointFactory.Instance.CreatePinJoint(physicsSimulator, rectangleBody, Vector2.Zero, rectangleBody1,
                                                 Vector2.Zero);

            //Slider
            Body rectangleBody2 = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody2.Position = new Vector2(400, 200);

            Geom rectangleGeom2 = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody2, 128, 128);
            rectangleGeom2.CollisionGroup = 3;
            AddRectangleToCanvas(rectangleBody2, new Vector2(128, 128));

            Body rectangleBody3 = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody3.Position = new Vector2(400, 350);

            Geom rectangleGeom3 = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody3, 128, 128);
            rectangleGeom3.CollisionGroup = 4;
            AddRectangleToCanvas(rectangleBody3, new Vector2(128, 128));

            JointFactory.Instance.CreateSliderJoint(physicsSimulator, rectangleBody2, Vector2.Zero,
                                                    rectangleBody3, Vector2.Zero, 150, 250);

            base.Initialize();
        }
    }
}