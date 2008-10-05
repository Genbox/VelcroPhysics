using System;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerSilverlightManual.Demos.DemoShare;
using FarseerSilverlightManual.Screens;

namespace FarseerSilverlightManual.Demos.Demo7
{
    public class Demo7 : SimulatorView
    {
        private Agent _agent;
        private Spider[] _spiders;

        public Demo7()
        {
            Initialize();
        }

        public override void Initialize()
        {
            //TODO: reset when disposing
            physicsSimulator.MaxContactsToDetect = 5;
            physicsSimulator.MaxContactsToResolve = 2;
            physicsSimulator.Iterations = 10;
            physicsSimulator.BiasFactor = .4f;

            _agent = new Agent(ScreenManager.ScreenCenter - new Vector2(200, 0));
            _agent.CollisionCategory = CollisionCategory.Cat5;
            _agent.CollidesWith = CollisionCategory.All & ~CollisionCategory.Cat4; //collide with all but Cat5(black)
            _agent.Load(this, physicsSimulator);
            _agent.Body.LinearDragCoefficient = .001f;
            AddAgentToCanvas(_agent.Body);
            LoadSpiders();
            base.Initialize();
        }

        private void LoadSpiders()
        {
            _spiders = new Spider[10];
            for (int i = 0; i < _spiders.Length; i++)
            {
                _spiders[i] = new Spider(new Vector2(ScreenManager.ScreenCenter.X, (i + 1)*50 + 100));
                _spiders[i].CollisionGroup = 1001 + (i); //give each spider it's own collision group
                _spiders[i].Load(this, physicsSimulator);
            }
        }

        public override void Update(TimeSpan elapsedTime)
        {
            for (int i = 0; i < _spiders.Length; i++)
            {
                _spiders[i].Update(elapsedTime);
            }

            base.Update(elapsedTime);
        }
    }
}