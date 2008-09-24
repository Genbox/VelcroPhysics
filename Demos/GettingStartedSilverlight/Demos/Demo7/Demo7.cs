using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Drawing;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Collections.Generic;
using FarseerSilverlightDemos.Drawing;
using FarseerSilverlightDemos.Demos.DemoShare;
using FarseerGames.FarseerPhysics.Collisions;
using System.IO;

namespace FarseerSilverlightDemos.Demos.Demo7
{
    public class Demo7 : SimulatorView
    {
        FarseerSilverlightDemos.Demos.DemoShare.Border border;
        Agent agent;

        Body body1;
        Geom geometry1;

        Body body2;
        Geom geometry2;

        Spider[] spiders;

        public Demo7() 
        {
            Initialize();
            forceAmount = 5000;
            torqueAmount = 14000;
        }
         
        public override void Initialize()
        {
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 150));
            physicsSimulator.MaxContactsToDetect = 5;
            physicsSimulator.MaxContactsToResolve = 2;
            physicsSimulator.Iterations = 10;
            physicsSimulator.BiasFactor = .4f;


            int borderWidth = (int)(ScreenManager.ScreenHeight * .05f);
            border = new FarseerSilverlightDemos.Demos.DemoShare.Border(ScreenManager.ScreenWidth, ScreenManager.ScreenHeight, borderWidth, ScreenManager.ScreenCenter);
            border.Load(this, physicsSimulator);

            agent = new Agent(ScreenManager.ScreenCenter - new Vector2(200, 0));
            agent.CollisionCategory = CollisionCategory.Cat5;
            agent.CollidesWith = CollisionCategory.All & ~CollisionCategory.Cat4; //collide with all but Cat5(black)
            agent.Load(this, physicsSimulator);
            agent.Body.LinearDragCoefficient = .001f;
            controlledBody = agent.Body;
            AddAgentToCanvas(agent.Body);
            LoadSpiders();
            base.Initialize();
        }

        private void LoadSpiders()
        {
            spiders = new Spider[10];
            for (int i = 0; i < spiders.Length; i++)
            {
                spiders[i] = new Spider(new Vector2(ScreenManager.ScreenCenter.X, (i + 1) * 50 + 100));
                spiders[i].CollisionGroup = 1001 + (i); //give each spider it's own collision group
                spiders[i].Load(this, physicsSimulator);
            }
        }

        private bool HandleCollision(Geom g1, Geom g2, ContactList contactList)
        {
            if (g2.Tag != null)
            {
                if (g2.Tag.ToString() == "Test")
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            else
            {
                return true;
            }
        }

        public override void Update(TimeSpan ElapsedTime)
        {
            if (MenuActive == false)
            {
                for (int i = 0; i < spiders.Length; i++)
                {
                    spiders[i].Update(ElapsedTime);
                }
            }
            base.Update(ElapsedTime);
        }

        public override string Details
        {
            get
            {
                StringWriter sb = new StringWriter();
                sb.WriteLine("This demo demonstrates the use of revolute joints ");
                sb.WriteLine("combined with angle joints that have a dynamic ");
                sb.WriteLine("target angle");
                sb.WriteLine("");
                sb.WriteLine("Keyboard:");
                sb.WriteLine("  -Rotate : K and L");
                sb.WriteLine("  -Move : A,S,D,W");
                sb.WriteLine("");
                sb.WriteLine("Mouse");
                sb.WriteLine("  -Hold down left button and drag");
                return sb.ToString();
            }
        }


    }
}
