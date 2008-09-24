using System;
using System.IO;
using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerSilverlightDemos.Demos.DemoShare;

namespace FarseerSilverlightDemos.Demos.Demo6
{
    public class Demo6 : SimulatorView
    {
        private Agent agent;

        private AngularSpringLever angularSpringLever1;
        private AngularSpringLever angularSpringLever2;
        private AngularSpringLever angularSpringLever3;
        private AngularSpringLever angularSpringLever4;
        private Border border;
        private FixedLinearSpring fixedLinearSpring1;
        private RectanglePlatform floor;
        private Body hangingBody;
        private Geom hangingGeom;

        private LinearSpring linearSpring1;

        private LinearSpring linearSpring2;
        private RectanglePlatform platform1;
        private float platform1HeightRatio = .6f;
        private float platform1WidthRatio = .1f;

        private RectanglePlatform platform2;
        private float platform2HeightRatio = .7f;
        private float platform2WidthRatio = .1f;

        private RectanglePlatform platform3;
        private float platform3HeightRatio = .6f;
        private float platform3WidthRatio = .1f;
        private SpringRectangleRope springRectangleRope1;
        private SpringRectangleRope springRectangleRope2;

        public Demo6()
        {
            Initialize();
            forceAmount = 3000;
            torqueAmount = 14000;
        }

        public override string Title
        {
            get { return "Linear and Angular Springs"; }
        }

        public override string Details
        {
            get
            {
                StringWriter sb = new StringWriter();
                sb.Write("This demo shows the use of angular and linear");
                sb.WriteLine(" springs");
                sb.WriteLine("");
                sb.Write("The levers are connected to the walls using");
                sb.Write(" revolute joints and they each have an angular");
                sb.WriteLine(" spring attached.");
                sb.WriteLine();
                sb.Write("The hanging squares are connected by linear");
                sb.WriteLine(" springs.");
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

        public override void Initialize()
        {
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 200));
            physicsSimulator.MaxContactsToDetect = 2;
                //for stacked objects, simultaneous collision are the bottlenecks so limit them to 2 per geometric pair.
            int borderWidth = (int) (ScreenManager.ScreenHeight*.05f);
            border = new Border(ScreenManager.ScreenWidth + borderWidth*2, ScreenManager.ScreenHeight + borderWidth*2,
                                borderWidth, ScreenManager.ScreenCenter);
            border.Load(this, physicsSimulator);

            agent = new Agent(new Vector2(ScreenManager.ScreenCenter.X, 100));
            agent.CollisionCategory = CollisionCategory.Cat5;
            agent.CollidesWith = CollisionCategory.All & ~CollisionCategory.Cat4; //collide with all but Cat5(black)
            agent.Load(this, physicsSimulator);
            AddAgentToCanvas(agent.Body);
            LoadPlatforms();
            controlledBody = agent.Body;
            base.Initialize();
        }

        public void LoadPlatforms()
        {
            //platform1
            int width = Convert.ToInt32(ScreenManager.ScreenWidth*platform1WidthRatio);
            int height = Convert.ToInt32(ScreenManager.ScreenHeight*platform1HeightRatio);
            Vector2 position = new Vector2(-5 + width/2f, 5 + ScreenManager.ScreenHeight - height/2f);
            Vector2 springPosition = Vector2.Zero;

            platform1 = new RectanglePlatform(width, height, position, Colors.White, Colors.Black, 100);
            platform1.Load(this, physicsSimulator);

            angularSpringLever1 = new AngularSpringLever();
            angularSpringLever1.AttachPoint = 0;
            angularSpringLever1.RectangleWidth = 200;
            angularSpringLever1.RectangleHeight = 20;
            angularSpringLever1.SpringConstant = 1000000;
            angularSpringLever1.DampningConstant = 5000;
            angularSpringLever1.CollisionGroup = 100;
            springPosition = position + new Vector2(width/2f, -height/2f) +
                             new Vector2(-angularSpringLever1.RectangleHeight - 5, .4f*height);
            angularSpringLever1.Position = springPosition;
            angularSpringLever1.Load(this, physicsSimulator);

            //platform 2
            width = Convert.ToInt32(ScreenManager.ScreenWidth*platform2WidthRatio);
            height = Convert.ToInt32(ScreenManager.ScreenHeight*platform2HeightRatio);
            position = new Vector2(ScreenManager.ScreenCenter.X, 5 + ScreenManager.ScreenHeight - height/2f);

            platform2 = new RectanglePlatform(width, height, position, Colors.White, Colors.Black, 100);
            platform2.Load(this, physicsSimulator);

            angularSpringLever2 = new AngularSpringLever();
            angularSpringLever2.AttachPoint = 2;
            angularSpringLever2.RectangleWidth = 200;
            angularSpringLever2.RectangleHeight = 20;
            angularSpringLever2.SpringConstant = 1000000;
            angularSpringLever2.DampningConstant = 5000;
            angularSpringLever2.CollisionGroup = 100;
            springPosition = position + new Vector2(-width/2f, -height/2f) +
                             new Vector2(angularSpringLever2.RectangleHeight + 5, .2f*height);
            angularSpringLever2.Position = springPosition;
            angularSpringLever2.Load(this, physicsSimulator);

            angularSpringLever3 = new AngularSpringLever();
            angularSpringLever3.AttachPoint = 0;
            angularSpringLever3.RectangleWidth = 150;
            angularSpringLever3.RectangleHeight = 20;
            angularSpringLever3.SpringConstant = 1000000;
            angularSpringLever3.DampningConstant = 1000;
            angularSpringLever3.CollisionGroup = 100;
            springPosition = position + new Vector2(width/2f, -height/2f) +
                             new Vector2(-angularSpringLever3.RectangleHeight - 5, .1f*height);
            angularSpringLever3.Position = springPosition;
            angularSpringLever3.Load(this, physicsSimulator);

            springRectangleRope1 = new SpringRectangleRope();
            springRectangleRope1.Position = springPosition + new Vector2(angularSpringLever3.RectangleWidth - 5, 25);
            springRectangleRope1.RectangleCount = 20;
            springRectangleRope1.RectangleWidth = 15;
            springRectangleRope1.RectangleHeight = 15;
            springRectangleRope1.RectangleMass = .5f;
            springRectangleRope1.SpringLength = 15;
            springRectangleRope1.SpringConstant = 400;
            springRectangleRope1.DampningConstant = 3f;
            springRectangleRope1.Load(this, physicsSimulator);
            linearSpring1 = ControllerFactory.Instance.CreateLinearSpring(physicsSimulator, angularSpringLever3.Body,
                                                                          new Vector2(
                                                                              angularSpringLever3.RectangleWidth/2, 0),
                                                                          springRectangleRope1.FirstBody, Vector2.Zero,
                                                                          400, 3);

            //platform 3
            width = Convert.ToInt32(ScreenManager.ScreenWidth*platform3WidthRatio);
            height = Convert.ToInt32(ScreenManager.ScreenHeight*platform3HeightRatio);
            position = new Vector2(ScreenManager.ScreenWidth + 5 - width/2f, 5 + ScreenManager.ScreenHeight - height/2f);
            platform3 = new RectanglePlatform(width, height, position, Colors.White, Colors.Black, 100);
            platform3.Load(this, physicsSimulator);

            hangingBody = BodyFactory.Instance.CreateCircleBody(physicsSimulator, 40, 1);
            AddCircleToCanvas(hangingBody, 40);
            hangingBody.Position = new Vector2(position.X - 200, 200);
            hangingGeom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, hangingBody, 40, 20);
            fixedLinearSpring1 = ControllerFactory.Instance.CreateFixedLinearSpring(physicsSimulator, hangingBody,
                                                                                    new Vector2(0, -35),
                                                                                    new Vector2(position.X - 200, 100),
                                                                                    2, .1f);


            angularSpringLever4 = new AngularSpringLever();
            angularSpringLever4.AttachPoint = 2;
            angularSpringLever4.RectangleWidth = 200;
            angularSpringLever4.RectangleHeight = 20;
            angularSpringLever4.SpringConstant = 1000000;
            angularSpringLever4.DampningConstant = 1000;
            angularSpringLever4.CollisionGroup = 100;
            springPosition = position + new Vector2(-width/2f, -height/2f) +
                             new Vector2(angularSpringLever4.RectangleHeight + 5, .7f*height);
            angularSpringLever4.Position = springPosition;
            angularSpringLever4.Load(this, physicsSimulator);

            height = (int) (ScreenManager.ScreenHeight*.05f);
            floor = new RectanglePlatform(ScreenManager.ScreenWidth + 10, height,
                                          new Vector2(ScreenManager.ScreenCenter.X,
                                                      ScreenManager.ScreenHeight + 5 - height/2), Colors.Black,
                                          Colors.Black, 0);
            floor.Load(this, physicsSimulator);

            springRectangleRope2 = new SpringRectangleRope();
            springRectangleRope2.Position = new Vector2(ScreenManager.ScreenCenter.X, 100);
            springRectangleRope2.RectangleCount = 20;
            springRectangleRope2.RectangleWidth = 10;
            springRectangleRope2.RectangleHeight = 10;
            springRectangleRope2.RectangleMass = .2f;
            springRectangleRope2.SpringLength = 10;
            springRectangleRope2.SpringConstant = 200;
            springRectangleRope2.DampningConstant = 4f;
            springRectangleRope2.CollisionGroup = 1; //same as agent collision group
            springRectangleRope2.Load(this, physicsSimulator);
            linearSpring2 = ControllerFactory.Instance.CreateLinearSpring(physicsSimulator, agent.Body, Vector2.Zero,
                                                                          springRectangleRope2.FirstBody, Vector2.Zero,
                                                                          200, 2f);
        }
    }
}