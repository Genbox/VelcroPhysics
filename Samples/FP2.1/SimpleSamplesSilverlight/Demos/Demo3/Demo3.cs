using System.IO;
using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.SimpleSamplesSilverlight.Demos.Demo3
{
    public class Demo3 : SimulatorView
    {
        private Body _agentBody;
        private Geom[] _agentGeom;
        private Body _floorBody;
        private Geom _floorGeom;

        private Body[] _obstacleBody;
        private Geom[] _obstacleGeom;

        public Demo3()
        {
            Initialize();
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

        public override void Initialize()
        {
            ClearCanvas();
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            LoadAgent();
            LoadFloor();
            LoadObstacles();
            base.Initialize();
        }

        public void LoadAgent()
        {
            _agentBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 80, 80, 5);
            _agentBody.Position = new Vector2(512, 110);

            _agentGeom = new Geom[7];
            _agentGeom[0] = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, _agentBody, 16, 10,
                                                                  new Vector2(-40, -40), 0);
            _agentGeom[0].RestitutionCoefficient = .2f;
            _agentGeom[0].FrictionCoefficient = .2f;
            _agentGeom[0].CollisionGroup = 1;
            _agentGeom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, _agentBody, _agentGeom[0],
                                                            new Vector2(-40, 40), 0);
            _agentGeom[2] = GeomFactory.Instance.CreateGeom(physicsSimulator, _agentBody, _agentGeom[0],
                                                            new Vector2(40, -40), 0);
            _agentGeom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, _agentBody, _agentGeom[0],
                                                            new Vector2(40, 40), 0);
            _agentGeom[4] = GeomFactory.Instance.CreateGeom(physicsSimulator, _agentBody, _agentGeom[0],
                                                            new Vector2(0, 0),
                                                            0);

            _agentGeom[5] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _agentBody, 16, 130, Vector2.Zero,
                                                                     MathHelper.PiOver4);
            _agentGeom[5].CollisionGroup = 1;
            _agentGeom[6] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _agentBody, 16, 130, Vector2.Zero,
                                                                     -MathHelper.PiOver4);
            _agentGeom[6].CollisionGroup = 1;
            controlledBody = _agentBody;
            AddAgentToCanvas(_agentBody);
        }

        public void LoadFloor()
        {
            //use the body factory to create the physics body
            _floorBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, ScreenManager.ScreenWidth, 100, 1);
            AddRectangleToCanvas(_floorBody, Colors.White, new Vector2(ScreenManager.ScreenWidth, 100));
            _floorBody.IsStatic = true;
            _floorGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _floorBody,
                                                                  ScreenManager.ScreenWidth,
                                                                  100);
            _floorGeom.RestitutionCoefficient = .2f;
            _floorGeom.FrictionCoefficient = .2f;
            _floorBody.Position = new Vector2(ScreenManager.ScreenCenter.X, ScreenManager.ScreenHeight - 50);
        }

        public void LoadObstacles()
        {
            _obstacleBody = new Body[5];
            _obstacleGeom = new Geom[5];
            for (int i = 0; i < _obstacleBody.Length; i++)
            {
                _obstacleBody[i] = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 32, 1);
                AddRectangleToCanvas(_obstacleBody[i], Colors.White, new Vector2(128, 32));
                _obstacleBody[i].IsStatic = true;

                if (i == 0)
                {
                    _obstacleGeom[i] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _obstacleBody[i], 128,
                                                                                32);
                    _obstacleGeom[i].RestitutionCoefficient = .2f;
                    _obstacleGeom[i].FrictionCoefficient = .2f;
                }
                else
                {
                    _obstacleGeom[i] = GeomFactory.Instance.CreateGeom(physicsSimulator, _obstacleBody[i],
                                                                       _obstacleGeom[0]);
                }
            }

            _obstacleBody[0].Position = ScreenManager.ScreenCenter + new Vector2(-50, -200);
            _obstacleBody[1].Position = ScreenManager.ScreenCenter + new Vector2(150, -100);
            _obstacleBody[2].Position = ScreenManager.ScreenCenter + new Vector2(100, 50);
            _obstacleBody[3].Position = ScreenManager.ScreenCenter + new Vector2(-100, 200);
            _obstacleBody[4].Position = ScreenManager.ScreenCenter + new Vector2(-170, 0);
        }
    }
}