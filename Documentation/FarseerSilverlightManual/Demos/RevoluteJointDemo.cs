using System.Windows.Media;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerSilverlightManual.Objects;
using FarseerSilverlightManual.Screens;

namespace FarseerSilverlightManual.Demos
{
    public class RevoluteJointDemo : SimulatorView
    {
        public RevoluteJointDemo()
        {
            Initialize();
        }

        public override void Initialize()
        {
            Body rectangleBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody.Position = ScreenManager.ScreenCenter;
            rectangleBody.RotationalDragCoefficient = 100f;

            Geom rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody, 128, 128);
            rectangleGeom.CollisionGroup = 15;
            AddRectangleToCanvas(rectangleBody, new Vector2(128, 128));

            JointFactory.Instance.CreateFixedRevoluteJoint(physicsSimulator, rectangleBody, rectangleBody.Position);

            Body rectangleBody1 = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 200, 200, 1);
            rectangleBody1.Position = ScreenManager.ScreenCenter;
            rectangleBody1.RotationalDragCoefficient = 100f;

            Geom rectangleGeom1 = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody1, 200, 200);
            rectangleGeom1.CollisionGroup = 15;
            RectangleBrush brush1 = AddRectangleToCanvas(rectangleBody1, new Vector2(200, 200));
            brush1.Extender.Color = Color.FromArgb(255, 255, 0, 0);

            Body rectangleBody2 = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody2.Position = ScreenManager.ScreenCenter;
            rectangleBody2.RotationalDragCoefficient = 100f;

            Geom rectangleGeom2 = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody2, 128, 128);
            rectangleGeom2.CollisionGroup = 15;
            rectangleGeom2.Body.RotationalDragCoefficient = 100f;
            RectangleBrush brush2 = AddRectangleToCanvas(rectangleBody2, new Vector2(128, 128));
            brush2.Extender.Color = Color.FromArgb(255, 0, 255, 0);

            JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, rectangleBody1, rectangleBody2,
                                                      rectangleGeom1.Position);

            base.Initialize();
        }
    }
}