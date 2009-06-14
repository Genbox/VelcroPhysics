using System.IO;
using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace SimpleSamplesWPF.Demos.Demo3
{
    public class Demo3 : Demo
    {
        private Body agentBody;
        private Geom[] agentGeom;

        private Body[] obstacleBody;
        private Geom[] obstacleGeom;

        public Demo3()
        {
            forceAmount = 800;
            torqueAmount = 8000;
        }

        public override string Title
        {
            get { return "Static Bodies And\nOffset Geometries"; }
        }

        public override string Details
        {
            get
            {
                StringWriter sb = new StringWriter();
                sb.Write("This demo shows a single body with multiple geometry");
                sb.Write(" objects attached.  The yellow circles are offset");
                sb.Write(" from the bodies center. The body itself is created");
                sb.Write(" using 'CreateRectangleBody' so that its moment of");
                sb.WriteLine(" inertia is that of a rectangle.");
                sb.WriteLine(string.Empty);
                sb.WriteLine("This demo also shows the use of static bodies.");
                sb.WriteLine(string.Empty);
                sb.WriteLine("Keyboard:");
                sb.WriteLine("  -Rotate : K and L");
                sb.WriteLine("  -Move : A,S,D,W");
                sb.WriteLine(string.Empty);
                sb.WriteLine("Mouse:");
                sb.WriteLine("  -Hold down left button and drag");
                return sb.ToString();
            }
        }

        protected override void Initialize()
        {
            ClearCanvas();
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            LoadAgent();
            LoadObstacles();
        }

        private void LoadAgent()
        {
            agentBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 80, 80, 5);
            agentBody.Position = new Vector2(512, 110);

            agentGeom = new Geom[7];
            agentGeom[0] = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, agentBody, 16, 10,
                                                                  new Vector2(-40, -40), 0);
            agentGeom[0].RestitutionCoefficient = .2f;
            agentGeom[0].FrictionCoefficient = .2f;
            agentGeom[0].CollisionGroup = 1;
            agentGeom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0],
                                                            new Vector2(-40, 40), 0);
            agentGeom[2] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0],
                                                            new Vector2(40, -40), 0);
            agentGeom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0],
                                                            new Vector2(40, 40), 0);
            agentGeom[4] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0],
                                                            new Vector2(0, 0),
                                                            0);

            agentGeom[5] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, agentBody, 16, 130, Vector2.Zero,
                                                                     MathHelper.PiOver4);
            agentGeom[5].CollisionGroup = 1;
            agentGeom[6] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, agentBody, 16, 130, Vector2.Zero,
                                                                     -MathHelper.PiOver4);
            agentGeom[6].CollisionGroup = 1;
            controlledBody = agentBody;
            AddAgentToCanvas(agentBody);
        }

        private void LoadObstacles()
        {
            obstacleBody = new Body[5];
            obstacleGeom = new Geom[5];
            for (int i = 0; i < obstacleBody.Length; i++)
            {
                obstacleBody[i] = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 32, 1);
                AddRectangleToCanvas(obstacleBody[i], Colors.White, new Vector2(128, 32));
                obstacleBody[i].IsStatic = true;

                if (i == 0)
                {
                    obstacleGeom[i] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, obstacleBody[i], 128,
                                                                                32);
                    obstacleGeom[i].RestitutionCoefficient = .2f;
                    obstacleGeom[i].FrictionCoefficient = .2f;
                }
                else
                {
                    obstacleGeom[i] = GeomFactory.Instance.CreateGeom(physicsSimulator, obstacleBody[i],
                                                                       obstacleGeom[0]);
                }
            }

            obstacleBody[0].Position = ScreenCenter + new Vector2(-50, -200);
            obstacleBody[1].Position = ScreenCenter + new Vector2(150, -100);
            obstacleBody[2].Position = ScreenCenter + new Vector2(100, 50);
            obstacleBody[3].Position = ScreenCenter + new Vector2(-100, 200);
            obstacleBody[4].Position = ScreenCenter + new Vector2(-170, 0);
        }
    }
}
