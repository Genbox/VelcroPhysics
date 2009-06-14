using System.Text;
using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace SimpleSamplesWPF.Demos.Demo8
{
    public class Demo8 : Demo
    {
        private Ragdoll _ragdoll;

        private Body[] obstacleBodies;
        private Geom[] obstacleGeoms;


        public override string Title
        {
            get { return "Demo8: Ragdoll"; }
        }

        public override string Details
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("This demo shows how to combine physics objects");
                sb.AppendLine("to create a ragdoll");
                sb.AppendLine(string.Empty);
                sb.AppendLine("Mouse");
                sb.AppendLine("  -Hold down left button and drag");
                return sb.ToString();
            }
        }

        protected override void Initialize()
        {
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 100));

            _ragdoll = new Ragdoll(new Vector2(ScreenCenter.X, 110));
            _ragdoll.Load(this, physicsSimulator);

            LoadObstacles();
        }

        private void LoadObstacles()
        {
            obstacleBodies = new Body[5];
            obstacleGeoms = new Geom[5];
            for (int i = 0; i < obstacleBodies.Length; i++)
            {
                obstacleBodies[i] = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 32, 1);
                AddRectangleToCanvas(obstacleBodies[i], Colors.White, new Vector2(128, 32));
                obstacleBodies[i].IsStatic = true;

                if (i == 0)
                {
                    obstacleGeoms[i] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, obstacleBodies[i], 128,
                                                                                32);
                    obstacleGeoms[i].RestitutionCoefficient = .2f;
                    obstacleGeoms[i].FrictionCoefficient = .2f;
                }
                else
                {
                    obstacleGeoms[i] = GeomFactory.Instance.CreateGeom(physicsSimulator, obstacleBodies[i],
                                                                       obstacleGeoms[0]);
                }
            }

            obstacleBodies[0].Position = ScreenCenter + new Vector2(-80, -180);
            obstacleBodies[1].Position = ScreenCenter + new Vector2(150, -100);
            obstacleBodies[2].Position = ScreenCenter + new Vector2(100, 50);
            obstacleBodies[3].Position = ScreenCenter + new Vector2(-100, 200);
            obstacleBodies[4].Position = ScreenCenter + new Vector2(-170, 0);
        }
    }
}