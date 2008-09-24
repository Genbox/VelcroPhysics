using System;
using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerSilverlightDemos.Demos.Demo5
{
    public class Circles
    {
        private Color borderColor = Colors.Black;
        private Body[] circleBody;
        private Geom[] circleGeom;
        private CollisionCategory collidesWith = CollisionCategory.All;
        private CollisionCategory collisionCategories = CollisionCategory.All;

        private Color color = Colors.White;

        private int count = 2;
        private Vector2 endPosition;
        private int radius = 100;
        private Vector2 startPosition;

        public Circles(Vector2 startPosition, Vector2 endPosition, int count, int radius, Color color, Color borderColor)
        {
            if (count < 2)
            {
                throw new Exception("count must be 2 or greater");
            }
            this.count = count;
            this.radius = radius;
            this.color = color;
            this.borderColor = borderColor;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
        }

        public CollisionCategory CollisionCategories
        {
            get { return collisionCategories; }
            set { collisionCategories = value; }
        }

        public CollisionCategory CollidesWith
        {
            get { return collidesWith; }
            set { collidesWith = value; }
        }

        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator)
        {
            circleBody = new Body[count];
            circleGeom = new Geom[count];

            circleBody[0] = BodyFactory.Instance.CreateCircleBody(physicsSimulator, radius, .1f);
            circleBody[0].Position = startPosition;
            view.AddCircleToCanvas(circleBody[0], color, radius);
            for (int i = 1; i < count; i++)
            {
                circleBody[i] = BodyFactory.Instance.CreateBody(physicsSimulator, circleBody[0]);
                circleBody[i].Position = Vector2.Lerp(startPosition, endPosition, i/(float) (count - 1));
                view.AddCircleToCanvas(circleBody[i], color, radius);
            }

            circleGeom[0] = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, circleBody[0], radius, 10);
            circleGeom[0].RestitutionCoefficient = .7f;
            circleGeom[0].FrictionCoefficient = .2f;
            circleGeom[0].CollisionCategories = collisionCategories;
            circleGeom[0].CollidesWith = collidesWith;
            for (int j = 1; j < count; j++)
            {
                circleGeom[j] = GeomFactory.Instance.CreateGeom(physicsSimulator, circleBody[j], circleGeom[0]);
            }
        }
    }
}