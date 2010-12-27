using System.Text;
using FarseerPhysics.DebugViews;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.DemoShare;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.SimpleSamplesXNA
{
    internal class Demo5Screen : PhysicsGameScreen, IDemoScreen
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
            sb.AppendLine("-Circles and rectangles are set to only collide with");
            sb.AppendLine(" their own shape.");
            sb.AppendLine("-Stars are set to collide with gears.");
            sb.AppendLine("-Gears are set to collide with stars.");
            sb.AppendLine("-The 'Agent' (the cross thing) is set to collide");
            sb.AppendLine(" with all but stars");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            World = new World(Vector2.Zero);
            base.LoadContent();

            //Cat1=Circles, Cat2=Rectangles, Cat3=Gears, Cat4=Stars
            _agent = new Agent(World, Vector2.Zero);

            //Collide with all but stars
            _agent.CollisionCategories = Category.All & ~Category.Cat4;
            _agent.CollidesWith = Category.All & ~Category.Cat4;

            DebugMaterial matCircle = new DebugMaterial(MaterialType.Dots)
                                          {
                                              Color = Color.DarkRed,
                                              Scale = 8f
                                          };
            DebugMaterial matSquare = new DebugMaterial(MaterialType.Squares)
                                          {
                                              Color = Color.SeaGreen,
                                              Scale = 9f
                                          };
            DebugMaterial matGear = new DebugMaterial(MaterialType.Dots)
                                        {
                                            Color = Color.SkyBlue,
                                            Scale = 8f
                                        };
            DebugMaterial matStar = new DebugMaterial(MaterialType.Dots)
                                        {
                                            Color = Color.Gold,
                                            Scale = 8f
                                        };

            Vector2 startPosition = new Vector2(-20, 16);
            Vector2 endPosition = new Vector2(20, 16);
            _circles = new Objects(World, startPosition, endPosition, 15, 1, ObjectType.Circle, matCircle);

            //Collide with itself only
            _circles.CollisionCategories = Category.Cat1;
            _circles.CollidesWith = Category.Cat1;

            startPosition = new Vector2(-20, -16);
            endPosition = new Vector2(20, -16);
            _rectangles = new Objects(World, startPosition, endPosition, 15, 2, ObjectType.Rectangle, matSquare);

            //Collides with itself only
            _rectangles.CollisionCategories = Category.Cat2;
            _rectangles.CollidesWith = Category.Cat2;

            startPosition = new Vector2(-20, -10);
            endPosition = new Vector2(-20, 10);
            _gears = new Objects(World, startPosition, endPosition, 5, 1, ObjectType.Gear, matGear);

            //Collides with stars
            _gears.CollisionCategories = Category.Cat3;
            _gears.CollidesWith = Category.Cat3 | Category.Cat4;

            startPosition = new Vector2(20, -10);
            endPosition = new Vector2(20, 10);
            _stars = new Objects(World, startPosition, endPosition, 5, 1, ObjectType.Star, matStar);

            //Collides with gears
            _stars.CollisionCategories = Category.Cat4;
            _stars.CollidesWith = Category.Cat3 | Category.Cat4;
        }

        public override void HandleGamePadInput(InputHelper input)
        {
            Vector2 force = 1000*input.CurrentGamePadState.ThumbSticks.Left;
            _agent.Body.ApplyForce(force);

            float rotation = 400*input.CurrentGamePadState.Triggers.Left;
            _agent.Body.ApplyTorque(rotation);

            rotation = -400*input.CurrentGamePadState.Triggers.Right;
            _agent.Body.ApplyTorque(rotation);

            base.HandleGamePadInput(input);
        }

        public override void HandleKeyboardInput(InputHelper input)
        {
            const float forceAmount = 1000;
            Vector2 force = Vector2.Zero;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.A))
            {
                force += new Vector2(-forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S))
            {
                force += new Vector2(0, -forceAmount);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D))
            {
                force += new Vector2(forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W))
            {
                force += new Vector2(0, forceAmount);
            }

            _agent.Body.ApplyForce(force);

            const float torqueAmount = 400;
            float torque = 0;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.Q))
            {
                torque += torqueAmount;
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.E))
            {
                torque -= torqueAmount;
            }

            _agent.Body.ApplyTorque(torque);

            base.HandleKeyboardInput(input);
        }
    }
}