using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using Media = System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Drawing;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Collections.Generic;
using FarseerSilverlightDemos.Drawing;
using System.IO;

namespace FarseerSilverlightDemos.Demos.Demo3
{
    public class Demo3 : SimulatorView
    {
        Body floorBody;
        Geom floorGeom;

        Body[] obstacleBody;
        Geom[] obstacleGeom;

        Body agentBody;
        Geom[] agentGeom;

        public Demo3()
            : base()
        {
            Initialize();
            forceAmount = 800;
            torqueAmount = 8000;
        }

        public override void Initialize()
        {
            ClearCanvas();
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            LoadAgent();
            LoadFloor();
            LoadObstacles();
            base.Initialize();
//            controlledBody = rectangleBody;
        }

        public void LoadAgent()
        {
            agentBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 80, 80, 5);
            agentBody.Position = new Vector2(512, 110);

            agentGeom = new Geom[7];
            agentGeom[0] = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, agentBody, 16, 10, new Vector2(-40, -40), 0);
            agentGeom[0].RestitutionCoefficient = .2f;
            agentGeom[0].FrictionCoefficient = .2f;
            agentGeom[0].CollisionGroup = 1;
            agentGeom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0], new Vector2(-40, 40), 0);
            agentGeom[2] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0], new Vector2(40, -40), 0);
            agentGeom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0], new Vector2(40, 40), 0);
            agentGeom[4] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0], new Vector2(0, 0), 0);

            agentGeom[5] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, agentBody, 16, 130, Vector2.Zero, MathHelper.PiOver4);
            agentGeom[5].CollisionGroup = 1;
            agentGeom[6] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, agentBody, 16, 130, Vector2.Zero, -MathHelper.PiOver4);
            agentGeom[6].CollisionGroup = 1;
            controlledBody = agentBody;
            AddAgentToCanvas(agentBody);
        }

        public void LoadFloor()
        {
            //use the body factory to create the physics body
            floorBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, ScreenManager.ScreenWidth, 100, 1);
            AddRectangleToCanvas(floorBody, Media.Colors.White, new Vector2(ScreenManager.ScreenWidth, 100)); 
            floorBody.IsStatic = true;
            floorGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, floorBody, ScreenManager.ScreenWidth, 100);
            floorGeom.RestitutionCoefficient = .2f;
            floorGeom.FrictionCoefficient = .2f;
            floorBody.Position = new Vector2(ScreenManager.ScreenCenter.X, ScreenManager.ScreenHeight - 50);
        }

        public void LoadObstacles()
        {
            obstacleBody = new Body[5];
            obstacleGeom = new Geom[5];
            for (int i = 0; i < obstacleBody.Length; i++)
            {
                obstacleBody[i] = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 32, 1);
                AddRectangleToCanvas(obstacleBody[i], Media.Colors.White, new Vector2(128, 32));
                obstacleBody[i].IsStatic = true;

                if (i == 0)
                {
                    obstacleGeom[i] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, obstacleBody[i], 128, 32);
                    obstacleGeom[i].RestitutionCoefficient = .2f;
                    obstacleGeom[i].FrictionCoefficient = .2f;
                }
                else
                {
                    obstacleGeom[i] = GeomFactory.Instance.CreateGeom(physicsSimulator, obstacleBody[i], obstacleGeom[0]);
                }
            }

            obstacleBody[0].Position = ScreenManager.ScreenCenter + new Vector2(-50, -200);
            obstacleBody[1].Position = ScreenManager.ScreenCenter + new Vector2(150, -100);
            obstacleBody[2].Position = ScreenManager.ScreenCenter + new Vector2(100, 50);
            obstacleBody[3].Position = ScreenManager.ScreenCenter + new Vector2(-100, 200);
            obstacleBody[4].Position = ScreenManager.ScreenCenter + new Vector2(-170, 0);
        }

        public override string Title
        {
            get
            {
                return "Static Bodies And\nOffset Geometries";
            }
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
                sb.WriteLine("");
                sb.WriteLine("This demo also shows the use of static bodies.");
                sb.WriteLine("");
                sb.WriteLine("Keyboard:");
                sb.WriteLine("  -Rotate : K and L");
                sb.WriteLine("  -Move : A,S,D,W");
                sb.WriteLine("");
                sb.WriteLine("Mouse:");
                sb.WriteLine("  -Hold down left button and drag");
                return sb.ToString();
            }
        }
    }
}
