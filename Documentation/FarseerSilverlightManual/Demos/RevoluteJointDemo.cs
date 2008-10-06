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

            Geom rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody, 128, 128);
            rectangleGeom.CollisionGroup = 15;
            rectangleGeom.Body.RotationalDragCoefficient = 100f;
            AddRectangleToCanvas(rectangleBody, new Vector2(128, 128));

            JointFactory.Instance.CreateFixedRevoluteJoint(physicsSimulator, rectangleBody, rectangleBody.Position);

            CircleBrush center = AddCircleToCanvas(Color.FromArgb(255, 0, 255, 0), 50);
            center.translateTransform.X = rectangleBody.Position.X;
            center.translateTransform.Y = rectangleBody.Position.Y;

            base.Initialize();
        }
    }
}