using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerSilverlightDemos.Demos.Demo4
{
    public class Floor
    {
        private Body floorBody;
        private Geom floorGeom;

        private int height;
        private Vector2 position;
        private int width;

        public Floor(int width, int height, Vector2 position)
        {
            this.width = width;
            this.height = height;
            this.position = position;
        }

        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator)
        {
            //use the body factory to create the physics body
            floorBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, width, height, 1);
            view.AddRectangleToCanvas(floorBody, Colors.White, new Vector2(width, height));
            floorBody.IsStatic = true;
            floorGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, floorBody, width, height);
            floorGeom.RestitutionCoefficient = .4f;
            floorGeom.FrictionCoefficient = .4f;
            floorBody.Position = position;
        }
    }
}