using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerSilverlightDemos.Demos.DemoShare
{
    public class SpringRectangleRope
    {
        private int collisionGroup;
        private float dampningConstant = 1;
        private LinearSpring[] linearSpring;
        private Vector2 position = Vector2.Zero;
        private Body[] rectangleBody;
        private int rectangleCount = 20;
        private Geom[] rectangleGeom;
        private int rectangleHeight = 20;
        private float rectangleMass = 1;
        private int rectangleWidth = 20;
        private float springConstant = 1;
        private float springLength = 25;


        public Body FirstBody
        {
            get { return rectangleBody[0]; }
            set { rectangleBody[0] = value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public int RectangleCount
        {
            get { return rectangleCount; }
            set { rectangleCount = value; }
        }

        public int RectangleWidth
        {
            get { return rectangleWidth; }
            set { rectangleWidth = value; }
        }

        public int RectangleHeight
        {
            get { return rectangleHeight; }
            set { rectangleHeight = value; }
        }

        public float RectangleMass
        {
            get { return rectangleMass; }
            set { rectangleMass = value; }
        }

        public float SpringLength
        {
            get { return springLength; }
            set { springLength = value; }
        }

        public float SpringConstant
        {
            get { return springConstant; }
            set { springConstant = value; }
        }

        public float DampningConstant
        {
            get { return dampningConstant; }
            set { dampningConstant = value; }
        }

        public int CollisionGroup
        {
            get { return collisionGroup; }
            set { collisionGroup = value; }
        }

        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator)
        {
            linearSpring = new LinearSpring[rectangleCount - 1];
            rectangleBody = new Body[rectangleCount];
            rectangleBody[0] = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, rectangleWidth,
                                                                        rectangleHeight, rectangleMass);
            rectangleBody[0].Position = position;
            view.AddRectangleToCanvas(rectangleBody[0], Colors.White, new Vector2(rectangleWidth, rectangleHeight));
            for (int i = 1; i < rectangleBody.Length; i++)
            {
                rectangleBody[i] = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, rectangleWidth,
                                                                            rectangleHeight, rectangleMass);
                rectangleBody[i].Position = rectangleBody[i - 1].Position + new Vector2(0, springLength);
                view.AddRectangleToCanvas(rectangleBody[i], Colors.White, new Vector2(rectangleWidth, rectangleHeight));
            }

            rectangleGeom = new Geom[rectangleCount];
            rectangleGeom[0] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody[0],
                                                                        rectangleWidth, rectangleHeight);
            rectangleGeom[0].CollisionGroup = collisionGroup;
            for (int j = 1; j < rectangleGeom.Length; j++)
            {
                rectangleGeom[j] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody[j],
                                                                            rectangleWidth, rectangleHeight);
                rectangleGeom[j].CollisionGroup = collisionGroup;
            }

            for (int k = 0; k < linearSpring.Length; k++)
            {
                linearSpring[k] = ControllerFactory.Instance.CreateLinearSpring(physicsSimulator, rectangleBody[k],
                                                                                Vector2.Zero, rectangleBody[k + 1],
                                                                                Vector2.Zero, springConstant,
                                                                                dampningConstant);
            }
        }
    }
}