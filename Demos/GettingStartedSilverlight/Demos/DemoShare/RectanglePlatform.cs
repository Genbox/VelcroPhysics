using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerSilverlightDemos.Demos.DemoShare
{
    public class RectanglePlatform
    {
        private Color borderColor;
        private int collisionGroup;
        private Color color;
        private int height;
        private Body platformBody;
        private Geom platformGeom;

        private Vector2 platformOrigin;
        private Vector2 position;
        private int width;

        public RectanglePlatform(int width, int height, Vector2 position, Color color, Color borderColor,
                                 int collisionGroup)
        {
            this.width = width;
            this.height = height;
            this.position = position;
            this.color = color;
            this.borderColor = borderColor;
            this.collisionGroup = collisionGroup;
        }

        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator)
        {
            //use the body factory to create the physics body
            platformBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, width, height, 1);
            view.AddRectangleToCanvas(platformBody, Colors.White, new Vector2(width, height));
            platformBody.IsStatic = true;
            platformBody.Position = position;

            platformGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, platformBody, width, height);
            platformGeom.CollisionGroup = 100;
            platformGeom.CollisionGroup = collisionGroup;
            platformGeom.FrictionCoefficient = 1;
        }
    }
}