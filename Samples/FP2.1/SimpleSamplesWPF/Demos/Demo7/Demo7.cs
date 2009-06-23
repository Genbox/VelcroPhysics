using System;
using System.IO;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using SimpleSamplesWPF.SharedDemoObjects;

namespace SimpleSamplesWPF.Demos.Demo7
{
    public class Demo7 : Demo
    {
        private Agent _agent;
        private Spider[] _spiders;

        public Demo7()
        {
            forceAmount = 5000;
            torqueAmount = 14000;
        }

        public override string Title
        {
            get { return "Dynamic Angle Joints"; }
        }

        public override string Details
        {
            get
            {
                StringWriter sb = new StringWriter();
                sb.WriteLine("This demo demonstrates the use of revolute joints ");
                sb.WriteLine("combined with angle joints that have a dynamic ");
                sb.WriteLine("target angle");
                sb.WriteLine(string.Empty);
                sb.WriteLine("Keyboard:");
                sb.WriteLine("  -Rotate : K and L");
                sb.WriteLine("  -Move : A,S,D,W");
                sb.WriteLine(string.Empty);
                sb.WriteLine("Mouse");
                sb.WriteLine("  -Hold down left button and drag");
                return sb.ToString();
            }
        }

        protected override void Initialize()
        {
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 150));
            PhysicsSimulator.MaxContactsToDetect = 5;
            physicsSimulator.MaxContactsToResolve = 2;
            physicsSimulator.Iterations = 10;
            physicsSimulator.BiasFactor = .4f;

            _agent = new Agent(ScreenCenter - new Vector2(200, 0));
            _agent.CollisionCategory = CollisionCategory.Cat5;
            _agent.CollidesWith = CollisionCategory.All & ~CollisionCategory.Cat4; //collide with all but Cat5(black)
            _agent.Load(this, physicsSimulator);
            _agent.Body.LinearDragCoefficient = .001f;
            controlledBody = _agent.Body;
            AddAgentToCanvas(_agent.Body);
            LoadSpiders();
        }

        private void LoadSpiders()
        {
            _spiders = new Spider[10];
            for (int i = 0; i < _spiders.Length; i++)
            {
                _spiders[i] = new Spider(new Vector2(ScreenCenter.X, (i + 1) * 50 + 100));
                _spiders[i].CollisionGroup = 1001 + (i); //give each spider it's own collision group
                _spiders[i].Load(this, physicsSimulator);
            }
        }

        protected override void Update(TimeSpan elapsedTime)
        {
            for (int i = 0; i < _spiders.Length; i++)
            {
                _spiders[i].Update(elapsedTime);
            }
            base.Update(elapsedTime);
        }
    }
}
