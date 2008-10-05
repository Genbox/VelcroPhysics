using System.Windows.Media;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerSilverlightManual.Screens;

namespace FarseerSilverlightManual.Demos.Demo3
{
    public class Demo3 : SimulatorView
    {
        private Body _agentBody;
        private Geom[] _agentGeom;
        private Body[] _obstacleBody;
        private Geom[] _obstacleGeom;

        public Demo3()
        {
            Initialize();
        }

        public override void Initialize()
        {
            ClearCanvas();
            LoadAgent();
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
            AddAgentToCanvas(_agentBody);
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