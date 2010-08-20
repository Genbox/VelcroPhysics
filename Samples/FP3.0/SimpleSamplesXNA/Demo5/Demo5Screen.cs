using System.Text;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.DemoShare;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace SimpleSamplesXNA.Demo5
{
    internal class Demo5Screen : GameScreen, IDemoScreen
    {
        private Agent _agent;
        private Objects _circles;
        private Objects _gears;
        private Objects _rectangles;
        private Objects _stars;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo5: Collision Categories";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to setup complex collision");
            sb.AppendLine("scenerios.");
            sb.AppendLine("In this demo:");
            sb.AppendLine("-Circles, rectangles, and gears are set to only collide with");
            sb.AppendLine(" their own shape.");
            sb.AppendLine("-Stars is set to collide with itself, circles, rectangles, ");
            sb.AppendLine(" and gears.");
            sb.AppendLine("-The 'Agent' (the cross thing) is set to collide");
            sb.AppendLine(" with all but stars");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }

        #endregion

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

            Vector2 startPosition = new Vector2(-20, 16);
            Vector2 endPosition = new Vector2(20, 16);
            _circles = new Objects(World, startPosition, endPosition, 15, 1, ObjectType.Circle);
            _circles.CollisionCategories = (CollisionCategory.Cat1);

            //Collide with circles (itself only)
            _circles.CollidesWith = (CollisionCategory.Cat1);

            startPosition = new Vector2(-20, -16);
            endPosition = new Vector2(20, -16);
            _rectangles = new Objects(World, startPosition, endPosition, 15, 2, ObjectType.Rectangle);
            _rectangles.CollisionCategories = (CollisionCategory.Cat2);

            //Collides with rectangles (itself only)
            _rectangles.CollidesWith = CollisionCategory.Cat2;

            startPosition = new Vector2(-20, -10);
            endPosition = new Vector2(-20, 10);
            _gears = new Objects(World, startPosition, endPosition, 5, 1, ObjectType.Gear);
            _gears.CollisionCategories = (CollisionCategory.Cat3);

            //Collides with itself (gears) and stars
            _gears.CollidesWith = (CollisionCategory.Cat3);

            startPosition = new Vector2(20, -10);
            endPosition = new Vector2(20, 10);
            _stars = new Objects(World, startPosition, endPosition, 5, 1, ObjectType.Star);
            _stars.CollisionCategories = (CollisionCategory.Cat4);

            //Collides with gears and itself (stars)
            _stars.CollidesWith = (CollisionCategory.All);

            base.LoadContent();
        }

        public override void HandleInput(InputState input)
        {
            if (input.CurrentGamePadState.IsConnected)
            {
                Vector2 force = 1000 * input.CurrentGamePadState.ThumbSticks.Left;
                _agent.Body.ApplyForce(force);

                float rotation = 400 * input.CurrentGamePadState.Triggers.Left;
                _agent.Body.ApplyTorque(rotation);

                rotation = -400 * input.CurrentGamePadState.Triggers.Right;
                _agent.Body.ApplyTorque(rotation);
            }

            base.HandleInput(input);
        }
    }
}