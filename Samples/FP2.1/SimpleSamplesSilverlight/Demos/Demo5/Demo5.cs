using System.IO;
using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.SimpleSamplesSilverlight.Demos.DemoShare;

namespace FarseerGames.SimpleSamplesSilverlight.Demos.Demo5
{
    public class Demo5 : SimulatorView
    {
        private Agent _agent;
        private Circles _blackCircles1;
        private Circles _blackCircles2;
        private Circles _blackCircles3;

        private Circles _blueCircles1;
        private Circles _blueCircles2;
        private Circles _blueCircles3;
        private Border _border;

        private Circles _greenCircles1;
        private Circles _greenCircles2;
        private Circles _greenCircles3;
        private Circles _redCircles1;
        private Circles _redCircles2;
        private Circles _redCircles3;

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
                sb.WriteLine(string.Empty);
                sb.Write("NOTE: If two objects define conflicting");
                sb.WriteLine(" collision status, collide wins over not colliding.");
                sb.Write("This is the case with Black vs. the Red, Green, ");
                sb.WriteLine(" and Blue circles");
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
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            int borderWidth = (int) (ScreenManager.ScreenHeight*.05f);
            _border = new Border(ScreenManager.ScreenWidth, ScreenManager.ScreenHeight, borderWidth,
                                 ScreenManager.ScreenCenter);
            _border.Load(this, physicsSimulator);

            _agent = new Agent(ScreenManager.ScreenCenter);
            _agent.CollisionCategory = CollisionCategory.Cat5;
            _agent.CollidesWith = CollisionCategory.All & ~CollisionCategory.Cat4; //collide with all but Cat4(black)
            _agent.Load(this, physicsSimulator);

            LoadCircles();
            controlledBody = _agent.Body;
            base.Initialize();
        }

        private void LoadCircles()
        {
            //Cat1=Red, Cat2=Green, Cat3=Blue, Cat4=Black, Cat5=Agent
            Vector2 startPosition = new Vector2(100, 100);
            Vector2 endPosition = new Vector2(100, ScreenManager.ScreenHeight - 100);
            _redCircles1 = new Circles(startPosition, endPosition, 15, 15, Color.FromArgb(175, 200, 0, 0));
            _redCircles1.CollisionCategories = (CollisionCategory.Cat1);
            _redCircles1.CollidesWith = (CollisionCategory.Cat1 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            _redCircles1.Load(this, physicsSimulator);

            startPosition = new Vector2(200, 200);
            endPosition = new Vector2(200, ScreenManager.ScreenHeight - 200);
            _redCircles2 = new Circles(startPosition, endPosition, 15, 12, Color.FromArgb(175, 200, 0, 0));
            _redCircles2.CollisionCategories = (CollisionCategory.Cat1);
            _redCircles2.CollidesWith = (CollisionCategory.Cat1 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            _redCircles2.Load(this, physicsSimulator);

            startPosition = new Vector2(300, 300);
            endPosition = new Vector2(300, ScreenManager.ScreenHeight - 300);
            _redCircles3 = new Circles(startPosition, endPosition, 10, 9, Color.FromArgb(175, 200, 0, 0));
            _redCircles3.CollisionCategories = (CollisionCategory.Cat1);
            _redCircles3.CollidesWith = (CollisionCategory.Cat1 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            _redCircles3.Load(this, physicsSimulator);

            startPosition = new Vector2(200, 100);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 200, 100);
            _greenCircles1 = new Circles(startPosition, endPosition, 15, 15, Color.FromArgb(175, 0, 200, 0));
            _greenCircles1.CollisionCategories = (CollisionCategory.Cat2);
            _greenCircles1.CollidesWith = (CollisionCategory.Cat2 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            _greenCircles1.Load(this, physicsSimulator);

            startPosition = new Vector2(300, 200);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 300, 200);
            _greenCircles2 = new Circles(startPosition, endPosition, 15, 12, Color.FromArgb(175, 0, 200, 0));
            _greenCircles2.CollisionCategories = (CollisionCategory.Cat2);
            _greenCircles2.CollidesWith = (CollisionCategory.Cat2 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            _greenCircles2.Load(this, physicsSimulator);

            startPosition = new Vector2(400, 300);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 400, 300);
            _greenCircles3 = new Circles(startPosition, endPosition, 10, 9, Color.FromArgb(175, 0, 200, 0));
            _greenCircles3.CollisionCategories = (CollisionCategory.Cat2);
            _greenCircles3.CollidesWith = (CollisionCategory.Cat2 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            _greenCircles3.Load(this, physicsSimulator);

            startPosition = new Vector2(ScreenManager.ScreenWidth - 100, 100);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 100, ScreenManager.ScreenHeight - 100);
            _blueCircles1 = new Circles(startPosition, endPosition, 15, 15, Color.FromArgb(175, 0, 0, 200));
            _blueCircles1.CollisionCategories = (CollisionCategory.Cat3);
            _blueCircles1.CollidesWith = (CollisionCategory.Cat3 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            _blueCircles1.Load(this, physicsSimulator);

            startPosition = new Vector2(ScreenManager.ScreenWidth - 200, 200);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 200, ScreenManager.ScreenHeight - 200);
            _blueCircles2 = new Circles(startPosition, endPosition, 15, 12, Color.FromArgb(175, 0, 0, 200));
            _blueCircles2.CollisionCategories = (CollisionCategory.Cat3);
            _blueCircles2.CollidesWith = (CollisionCategory.Cat3 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            _blueCircles2.Load(this, physicsSimulator);

            startPosition = new Vector2(ScreenManager.ScreenWidth - 300, 300);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 300, ScreenManager.ScreenHeight - 300);
            _blueCircles3 = new Circles(startPosition, endPosition, 10, 9, Color.FromArgb(175, 0, 0, 200));
            _blueCircles3.CollisionCategories = (CollisionCategory.Cat3);
            _blueCircles3.CollidesWith = (CollisionCategory.Cat3 | CollisionCategory.Cat4 | CollisionCategory.Cat5);
            _blueCircles3.Load(this, physicsSimulator);

            startPosition = new Vector2(200, ScreenManager.ScreenHeight - 100);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 200, ScreenManager.ScreenHeight - 100);
            _blackCircles1 = new Circles(startPosition, endPosition, 15, 15, Color.FromArgb(200, 0, 0, 0));
            _blackCircles1.CollisionCategories = CollisionCategory.Cat4;
            _blackCircles1.CollidesWith = CollisionCategory.All & ~CollisionCategory.Cat5; //Collide with all but Cat5
            _blackCircles1.Load(this, physicsSimulator);

            startPosition = new Vector2(300, ScreenManager.ScreenHeight - 200);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 300, ScreenManager.ScreenHeight - 200);
            _blackCircles2 = new Circles(startPosition, endPosition, 15, 12, Color.FromArgb(200, 0, 0, 0));
            _blackCircles2.CollisionCategories = CollisionCategory.Cat4;
            _blackCircles2.CollidesWith = CollisionCategory.All & ~CollisionCategory.Cat5; //Collide with all but Cat5
            _blackCircles2.Load(this, physicsSimulator);

            startPosition = new Vector2(400, ScreenManager.ScreenHeight - 300);
            endPosition = new Vector2(ScreenManager.ScreenWidth - 400, ScreenManager.ScreenHeight - 300);
            _blackCircles3 = new Circles(startPosition, endPosition, 10, 9, Color.FromArgb(200, 0, 0, 0));
            _blackCircles3.CollisionCategories = CollisionCategory.Cat4;
            _blackCircles3.CollidesWith = CollisionCategory.All & ~CollisionCategory.Cat5; //Collide with all but Cat5
            _blackCircles3.Load(this, physicsSimulator);
        }
    }
}