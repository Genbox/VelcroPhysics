using System.Text;
using FarseerPhysics.DemoBaseXNA.DemoShare;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace SimpleSamplesXNA.Demo5
{
    internal class Demo5Screen : GameScreen
    {
        private Agent _agent;
        private Objects _circles;
        private Objects _rectangles;
        private Objects _gears;
        private Objects _stars;

        public override void Initialize()
        {
            World = new World(Vector2.Zero);

            base.Initialize();
        }

        public override void LoadContent()
        {
            //Cat1=Circles, Cat2=Rectangles, Cat3=Gears, Cat4=Stars, Cat5=Agent
            _agent = new Agent(World, Vector2.Zero);
            _agent.CollisionCategories = CollisionCategory.Cat5;

            //Collide with all but cat4
            _agent.CollidesWith = CollisionCategory.All & ~CollisionCategory.Cat4;

            Vector2 startPosition = new Vector2(-20, 17);
            Vector2 endPosition = new Vector2(20, 17);
            _circles = new Objects(World, startPosition, endPosition, 15, 1, ObjectType.Circle);
            _circles.CollisionCategories = (CollisionCategory.Cat1);

            //Collide with stars and gears
            _circles.CollidesWith = (CollisionCategory.Cat3 | CollisionCategory.Cat4);

            startPosition = new Vector2(-20, -17);
            endPosition = new Vector2(20, -17);
            _rectangles = new Objects(World, startPosition, endPosition, 15, 2, ObjectType.Rectangle);
            _rectangles.CollisionCategories = (CollisionCategory.Cat2);

            //Collides with rectangles (itself only)
            _rectangles.CollidesWith = CollisionCategory.Cat2;

            startPosition = new Vector2(-20, -10);
            endPosition = new Vector2(-20, 10);
            _gears = new Objects(World, startPosition, endPosition, 5, 1, ObjectType.Gear);
            _gears.CollisionCategories = (CollisionCategory.Cat3);

            //Collides with itself (gears) and stars
            _gears.CollidesWith = (CollisionCategory.Cat3 | CollisionCategory.Cat4);

            startPosition = new Vector2(20, -10);
            endPosition = new Vector2(20, 10);
            _stars = new Objects(World, startPosition, endPosition, 5, 1, ObjectType.Star);
            _stars.CollisionCategories = (CollisionCategory.Cat4);

            //Collides with gears and itself (stars)
            _stars.CollidesWith = (CollisionCategory.Cat3 | CollisionCategory.Cat4);

            base.LoadContent();
        }

        public string GetTitle()
        {
            return "Demo5: Collision Categories";
        }

        private string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to setup complex collision");
            sb.AppendLine("scenerios.");
            sb.AppendLine("In this demo:");
            sb.AppendLine("-Red, Green, and Blue are set to only collide with");
            sb.AppendLine(" their own color.");
            sb.AppendLine("-Black is set to collide with itself, Red, Green, ");
            sb.AppendLine(" and Blue.");
            sb.AppendLine("-The 'Agent' (the cross thing) is set to collide");
            sb.AppendLine(" with all but Black");
            sb.AppendLine(string.Empty);
            sb.AppendLine("NOTE: If two objects define conflicting");
            sb.AppendLine("collision status, collide wins over not colliding.");
            sb.AppendLine("This is the case with Black vs. the Red, Green, ");
            sb.AppendLine("and Blue circles");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate: left and right triggers");
            sb.AppendLine("  -Move: left thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate: left and right arrows");
            sb.AppendLine("  -Move: A,S,D,W");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }

    }
}