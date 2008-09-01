using System;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.Demos.DemoShare
{
    public class Circles
    {
        private readonly Color borderColor = Color.Black;
        private readonly Color color = Color.White;

        private readonly int count = 2;
        private readonly Vector2 endPosition;
        private readonly int radius = 100;
        private readonly Vector2 startPosition;
        private Body[] circleBody;
        private CircleBrush circleBrush;
        private Geom[] circleGeom;

        private Enums.CollisionCategories collidesWith = Enums.CollisionCategories.All;
        private Enums.CollisionCategories collisionCategories = Enums.CollisionCategories.All;

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

        public Enums.CollisionCategories CollisionCategories
        {
            get { return collisionCategories; }
            set { collisionCategories = value; }
        }

        public Enums.CollisionCategories CollidesWith
        {
            get { return collidesWith; }
            set { collidesWith = value; }
        }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            circleBrush = new CircleBrush(radius, color, borderColor);
            circleBrush.Load(graphicsDevice);

            circleBody = new Body[count];
            circleGeom = new Geom[count];

            circleBody[0] = BodyFactory.Instance.CreateCircleBody(physicsSimulator, radius, .5f);
            circleBody[0].Position = startPosition;
            for (int i = 1; i < count; i++)
            {
                circleBody[i] = BodyFactory.Instance.CreateBody(physicsSimulator, circleBody[0]);
                circleBody[i].Position = Vector2.Lerp(startPosition, endPosition, i/(float) (count - 1));
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

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < count; i++)
            {
                circleBrush.Draw(spriteBatch, circleGeom[i].Position);
            }
        }
    }
}