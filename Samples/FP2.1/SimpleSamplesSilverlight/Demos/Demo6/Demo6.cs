using System;
using System.IO;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.SimpleSamplesSilverlight.Demos.DemoShare;

namespace FarseerGames.SimpleSamplesSilverlight.Demos.Demo6
{
    public class Demo6 : SimulatorView
    {
        private const float _platform1HeightRatio = .6f;
        private const float _platform1WidthRatio = .1f;
        private const float _platform2HeightRatio = .7f;
        private const float _platform2WidthRatio = .1f;
        private const float _platform3HeightRatio = .6f;
        private const float _platform3WidthRatio = .1f;
        private Agent _agent;

        private AngularSpringLever _angularSpringLever1;
        private AngularSpringLever _angularSpringLever2;
        private AngularSpringLever _angularSpringLever3;
        private AngularSpringLever _angularSpringLever4;
        private Border _border;
        private RectanglePlatform _floor;
        private Body _hangingBody;

        private RectanglePlatform _platform1;

        private RectanglePlatform _platform2;

        private RectanglePlatform _platform3;
        private SpringRectangleRope _springRectangleRope1;
        private SpringRectangleRope _springRectangleRope2;

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
                sb.WriteLine(string.Empty);
                sb.Write("The levers are connected to the walls using");
                sb.Write(" revolute joints and they each have an angular");
                sb.WriteLine(" spring attached.");
                sb.WriteLine();
                sb.Write("The hanging squares are connected by linear");
                sb.WriteLine(" springs.");
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
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 200));
            PhysicsSimulator.MaxContactsToDetect = 2;
            //for stacked objects, simultaneous collision are the bottlenecks so limit them to 2 per geometric pair.
            int borderWidth = (int) (ScreenManager.ScreenHeight*.05f);
            _border = new Border(ScreenManager.ScreenWidth + borderWidth*2, ScreenManager.ScreenHeight + borderWidth*2,
                                 borderWidth, ScreenManager.ScreenCenter);
            _border.Load(this, physicsSimulator);

            _agent = new Agent(new Vector2(ScreenManager.ScreenCenter.X, 100));
            _agent.CollisionCategory = CollisionCategory.Cat5;
            _agent.CollidesWith = CollisionCategory.All & ~CollisionCategory.Cat4; //collide with all but Cat5(black)
            _agent.Load(this, physicsSimulator);
            AddAgentToCanvas(_agent.Body);
            LoadPlatforms();
            controlledBody = _agent.Body;
            base.Initialize();
        }

        public void LoadPlatforms()
        {
            //platform1
            int width = Convert.ToInt32(ScreenManager.ScreenWidth*_platform1WidthRatio);
            int height = Convert.ToInt32(ScreenManager.ScreenHeight*_platform1HeightRatio);
            Vector2 position = new Vector2(-5 + width/2f, 5 + ScreenManager.ScreenHeight - height/2f);
            Vector2 springPosition = Vector2.Zero;

            _platform1 = new RectanglePlatform(width, height, position, 100);
            _platform1.Load(this, physicsSimulator);

            _angularSpringLever1 = new AngularSpringLever();
            _angularSpringLever1.AttachPoint = 0;
            _angularSpringLever1.RectangleWidth = 200;
            _angularSpringLever1.RectangleHeight = 20;
            _angularSpringLever1.SpringConstant = 1000000;
            _angularSpringLever1.DampningConstant = 5000;
            _angularSpringLever1.CollisionGroup = 100;
            springPosition = position + new Vector2(width/2f, -height/2f) +
                             new Vector2(-_angularSpringLever1.RectangleHeight - 5, .4f*height);
            _angularSpringLever1.Position = springPosition;
            _angularSpringLever1.Load(this, physicsSimulator);

            //platform 2
            width = Convert.ToInt32(ScreenManager.ScreenWidth*_platform2WidthRatio);
            height = Convert.ToInt32(ScreenManager.ScreenHeight*_platform2HeightRatio);
            position = new Vector2(ScreenManager.ScreenCenter.X, 5 + ScreenManager.ScreenHeight - height/2f);

            _platform2 = new RectanglePlatform(width, height, position, 100);
            _platform2.Load(this, physicsSimulator);

            _angularSpringLever2 = new AngularSpringLever();
            _angularSpringLever2.AttachPoint = 2;
            _angularSpringLever2.RectangleWidth = 200;
            _angularSpringLever2.RectangleHeight = 20;
            _angularSpringLever2.SpringConstant = 1000000;
            _angularSpringLever2.DampningConstant = 5000;
            _angularSpringLever2.CollisionGroup = 100;
            springPosition = position + new Vector2(-width/2f, -height/2f) +
                             new Vector2(_angularSpringLever2.RectangleHeight + 5, .2f*height);
            _angularSpringLever2.Position = springPosition;
            _angularSpringLever2.Load(this, physicsSimulator);

            _angularSpringLever3 = new AngularSpringLever();
            _angularSpringLever3.AttachPoint = 0;
            _angularSpringLever3.RectangleWidth = 150;
            _angularSpringLever3.RectangleHeight = 20;
            _angularSpringLever3.SpringConstant = 1000000;
            _angularSpringLever3.DampningConstant = 1000;
            _angularSpringLever3.CollisionGroup = 100;
            springPosition = position + new Vector2(width/2f, -height/2f) +
                             new Vector2(-_angularSpringLever3.RectangleHeight - 5, .1f*height);
            _angularSpringLever3.Position = springPosition;
            _angularSpringLever3.Load(this, physicsSimulator);

            _springRectangleRope1 = new SpringRectangleRope();
            _springRectangleRope1.Position = springPosition + new Vector2(_angularSpringLever3.RectangleWidth - 5, 25);
            _springRectangleRope1.RectangleCount = 20;
            _springRectangleRope1.RectangleWidth = 15;
            _springRectangleRope1.RectangleHeight = 15;
            _springRectangleRope1.RectangleMass = .5f;
            _springRectangleRope1.SpringLength = 15;
            _springRectangleRope1.SpringConstant = 400;
            _springRectangleRope1.DampningConstant = 3f;
            _springRectangleRope1.Load(this, physicsSimulator);
            SpringFactory.Instance.CreateLinearSpring(physicsSimulator, _angularSpringLever3.Body,
                                                      new Vector2(
                                                          _angularSpringLever3.RectangleWidth/2f, 0),
                                                      _springRectangleRope1.FirstBody, Vector2.Zero,
                                                      400, 3);

            //platform 3
            width = Convert.ToInt32(ScreenManager.ScreenWidth*_platform3WidthRatio);
            height = Convert.ToInt32(ScreenManager.ScreenHeight*_platform3HeightRatio);
            position = new Vector2(ScreenManager.ScreenWidth + 5 - width/2f, 5 + ScreenManager.ScreenHeight - height/2f);
            _platform3 = new RectanglePlatform(width, height, position, 100);
            _platform3.Load(this, physicsSimulator);

            _hangingBody = BodyFactory.Instance.CreateCircleBody(physicsSimulator, 40, 1);
            AddCircleToCanvas(_hangingBody, 40);
            _hangingBody.Position = new Vector2(position.X - 200, 200);
            GeomFactory.Instance.CreateCircleGeom(physicsSimulator, _hangingBody, 40, 20);
            SpringFactory.Instance.CreateFixedLinearSpring(physicsSimulator, _hangingBody,
                                                           new Vector2(0, -35),
                                                           new Vector2(position.X - 200, 100),
                                                           2, .1f);


            _angularSpringLever4 = new AngularSpringLever();
            _angularSpringLever4.AttachPoint = 2;
            _angularSpringLever4.RectangleWidth = 200;
            _angularSpringLever4.RectangleHeight = 20;
            _angularSpringLever4.SpringConstant = 1000000;
            _angularSpringLever4.DampningConstant = 1000;
            _angularSpringLever4.CollisionGroup = 100;
            springPosition = position + new Vector2(-width/2f, -height/2f) +
                             new Vector2(_angularSpringLever4.RectangleHeight + 5, .7f*height);
            _angularSpringLever4.Position = springPosition;
            _angularSpringLever4.Load(this, physicsSimulator);

            height = (int) (ScreenManager.ScreenHeight*.05f);
            _floor = new RectanglePlatform(ScreenManager.ScreenWidth + 10, height,
                                           new Vector2(ScreenManager.ScreenCenter.X,
                                                       ScreenManager.ScreenHeight + 5 - height/2), 0);
            _floor.Load(this, physicsSimulator);

            _springRectangleRope2 = new SpringRectangleRope();
            _springRectangleRope2.Position = new Vector2(ScreenManager.ScreenCenter.X, 100);
            _springRectangleRope2.RectangleCount = 20;
            _springRectangleRope2.RectangleWidth = 10;
            _springRectangleRope2.RectangleHeight = 10;
            _springRectangleRope2.RectangleMass = .2f;
            _springRectangleRope2.SpringLength = 10;
            _springRectangleRope2.SpringConstant = 200;
            _springRectangleRope2.DampningConstant = 4f;
            _springRectangleRope2.CollisionGroup = 1; //same as agent collision group
            _springRectangleRope2.Load(this, physicsSimulator);
            SpringFactory.Instance.CreateLinearSpring(physicsSimulator, _agent.Body, Vector2.Zero,
                                                      _springRectangleRope2.FirstBody, Vector2.Zero,
                                                      200, 2f);
        }
    }
}