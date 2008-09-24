using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics.Factories;
using Media = System.Windows.Media;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Drawing;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Collections.Generic;
using FarseerSilverlightDemos.Drawing;

namespace FarseerSilverlightDemos.Demos.Demo5
{
    public class Circles
    {
        private Body[] circleBody;
        private Geom[] circleGeom;

        private int radius = 100;
        private Media.Color color = Media.Colors.White;
        private Media.Color borderColor = Media.Colors.Black;

        private int count = 2;
        private Vector2 startPosition;
        private Vector2 endPosition;

        private CollisionCategory collisionCategories = CollisionCategory.All;
        private CollisionCategory collidesWith = CollisionCategory.All;

        public Circles(Vector2 startPosition, Vector2 endPosition, int count, int radius, Media.Color color, Media.Color borderColor)
        {
            if (count < 2) { throw new Exception("count must be 2 or greater"); }
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
                circleBody[i].Position = Vector2.Lerp(startPosition, endPosition, (float)i / (float)(count - 1));
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
