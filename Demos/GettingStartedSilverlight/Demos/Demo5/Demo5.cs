using System.IO;
using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerSilverlightDemos.Demos.DemoShare;

namespace FarseerSilverlightDemos.Demos.Demo5
{
    public class Demo5 : SimulatorView
    {
        private Agent agent;
        private Circles blackCircles1;
        private Circles blackCircles2;
        private Circles blackCircles3;

        private Circles blueCircles1;
        private Circles blueCircles2;
        private Circles blueCircles3;
        private Border border;

        private Circles greenCircles1;
        private Circles greenCircles2;
        private Circles greenCircles3;
        private Circles redCircles1;
        private Circles redCircles2;
        private Circles redCircles3;

        public Demo5()
        {
            Initialize();
            forceAmount = 1000;
            torqueAmount = 14000;
        }

        public override string Title
        {
            get { return "Collision Categories"; }
        }

        public override string Details
        {
            get
            {
                StringWriter sb = new StringWriter();
                sb.WriteLine("This demo shows how to setup complex collision scenarios.");
                sb.WriteLine();
                sb.WriteLine("In this demo:");
                sb.Write("-Red, Green, and Blue are set to only collide with");
                sb.WriteLine(" their own color.");
                sb.Write("-Black is set to collide with itself, Red, Green, ");
                sb.WriteLine(" and Blue.");
                sb.Write("-The 'Agent' (the cross thing) is set to collide");
                sb.WriteLine(" with all but Black");
                sb.WriteLine("");
                sb.Write("NOTE: If two objects define conflicting");
                sb.WriteLine(" collision status, collide wins over not colliding.");
                sb.Write("This is the case with Black vs. the Red, Green, ");
                sb.WriteLine(" and Blue circles");
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
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            int borderWidth = (int) (ScreenManager.ScreenHeight*.05f);
            border = new Border(ScreenManager.ScreenWidth, ScreenManager.ScreenHeight, borderWidth,
                                ScreenManager.ScreenCenter);
            border.Load(this, physicsSimulator);

            agent = new Agent(ScreenManager.ScreenCenter);
            agent.CollisionCategory = CollisionCategory.Cat5;
            agent.CollidesWith = CollisionCategory.All & ~CollisionCategory.Cat4; //collide with all but Cat4(black)
            agent.Load(this, physicsSimulator);

            LoadCircles();
            controlledBody = agent.Body;
            base.Initialize();
        }

        private void LoadCircles()
        {
            Vector2 startPosition;
            Vector2 endPosition;

            //Cat1=Red, Cat2=Green, Cat3=Blue, Cat4=Black, Cat5=Agent
            startPosition = new Vector2(100, 100);
            endPosition = new Vector2(100, ScreenManager.ScreenHeight - 100);
            redCircles1 = new Circles(startPosition, endPosition, 15, 15, Color.FromArgb(175, 200, 0, 0), Colors.Black);
            redCircles1.CollisionCategories = (CollisionCategory.Cat1);
            redCircles1.CollidesWith = (CollisionCategory.Cat1 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            redCircles1.Load(this, physicsSimulator);

            startPosition = new Vector2(200, 200);
            endPosition = new Vector2(200, ScreenManager.ScreenHeight - 200);
            redCircles2 = new Circles(startPosition, endPosition, 15, 12, Color.FromArgb(175, 200, 0, 0), Colors.Black);
            redCircles2.CollisionCategories = (CollisionCategory.Cat1);
            redCircles2.CollidesWith = (CollisionCategory.Cat1 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            redCircles2.Load(this, physicsSimulator);

            startPosition = new Vector2(300, 300);
            endPosition = new Vector2(300, ScreenManager.ScreenHeight - 300);
            redCircles3 = new Circles(startPosition, endPosition, 10, 9, Color.FromArgb(175, 200, 0, 0), Colors.Black);
            redCircles3.CollisionCategories = (CollisionCategory.Cat1);
            redCircles3.CollidesWith = (CollisionCategory.Cat1 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            redCircles3.Load(this, physicsSimulator);

            startPosition = new Vector2(200, 100);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 200, 100);
            greenCircles1 = new Circles(startPosition, endPosition, 15, 15, Color.FromArgb(175, 0, 200, 0), Colors.Black);
            greenCircles1.CollisionCategories = (CollisionCategory.Cat2);
            greenCircles1.CollidesWith = (CollisionCategory.Cat2 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            greenCircles1.Load(this, physicsSimulator);

            startPosition = new Vector2(300, 200);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 300, 200);
            greenCircles2 = new Circles(startPosition, endPosition, 15, 12, Color.FromArgb(175, 0, 200, 0), Colors.Black);
            greenCircles2.CollisionCategories = (CollisionCategory.Cat2);
            greenCircles2.CollidesWith = (CollisionCategory.Cat2 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            greenCircles2.Load(this, physicsSimulator);

            startPosition = new Vector2(400, 300);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 400, 300);
            greenCircles3 = new Circles(startPosition, endPosition, 10, 9, Color.FromArgb(175, 0, 200, 0), Colors.Black);
            greenCircles3.CollisionCategories = (CollisionCategory.Cat2);
            greenCircles3.CollidesWith = (CollisionCategory.Cat2 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            greenCircles3.Load(this, physicsSimulator);

            startPosition = new Vector2(ScreenManager.ScreenWidth - 100, 100);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 100, ScreenManager.ScreenHeight - 100);
            blueCircles1 = new Circles(startPosition, endPosition, 15, 15, Color.FromArgb(175, 0, 0, 200), Colors.Black);
            blueCircles1.CollisionCategories = (CollisionCategory.Cat3);
            blueCircles1.CollidesWith = (CollisionCategory.Cat3 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            blueCircles1.Load(this, physicsSimulator);

            startPosition = new Vector2(ScreenManager.ScreenWidth - 200, 200);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 200, ScreenManager.ScreenHeight - 200);
            blueCircles2 = new Circles(startPosition, endPosition, 15, 12, Color.FromArgb(175, 0, 0, 200), Colors.Black);
            blueCircles2.CollisionCategories = (CollisionCategory.Cat3);
            blueCircles2.CollidesWith = (CollisionCategory.Cat3 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            blueCircles2.Load(this, physicsSimulator);

            startPosition = new Vector2(ScreenManager.ScreenWidth - 300, 300);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 300, ScreenManager.ScreenHeight - 300);
            blueCircles3 = new Circles(startPosition, endPosition, 10, 9, Color.FromArgb(175, 0, 0, 200), Colors.Black);
            blueCircles3.CollisionCategories = (CollisionCategory.Cat3);
            blueCircles3.CollidesWith = (CollisionCategory.Cat3 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            blueCircles3.Load(this, physicsSimulator);

            startPosition = new Vector2(200, ScreenManager.ScreenHeight - 100);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 200, ScreenManager.ScreenHeight - 100);
            blackCircles1 = new Circles(startPosition, endPosition, 15, 15, Color.FromArgb(200, 0, 0, 0), Colors.Black);
            blackCircles1.CollisionCategories = CollisionCategory.Cat4;
            blackCircles1.CollidesWith = CollisionCategory.All & ~CollisionCategory.Cat5; //Collide with all but Cat5
            blackCircles1.Load(this, physicsSimulator);

            startPosition = new Vector2(300, ScreenManager.ScreenHeight - 200);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 300, ScreenManager.ScreenHeight - 200);
            blackCircles2 = new Circles(startPosition, endPosition, 15, 12, Color.FromArgb(200, 0, 0, 0), Colors.Black);
            blackCircles2.CollisionCategories = CollisionCategory.Cat4;
            blackCircles2.CollidesWith = CollisionCategory.All & ~CollisionCategory.Cat5; //Collide with all but Cat5
            blackCircles2.Load(this, physicsSimulator);

            startPosition = new Vector2(400, ScreenManager.ScreenHeight - 300);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 400, ScreenManager.ScreenHeight - 300);
            blackCircles3 = new Circles(startPosition, endPosition, 10, 9, Color.FromArgb(200, 0, 0, 0), Colors.Black);
            blackCircles3.CollisionCategories = CollisionCategory.Cat4;
            blackCircles3.CollidesWith = CollisionCategory.All & ~CollisionCategory.Cat5; //Collide with all but Cat5
            blackCircles3.Load(this, physicsSimulator);
        }
    }
}