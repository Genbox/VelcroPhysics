using System.Text;
using DemoBaseXNA;
using DemoBaseXNA.DrawingSystem;
using DemoBaseXNA.ScreenSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.AdvancedSamplesXNA.Demo3
{
    public class Demo3Screen : GameScreen
    {
        private Texture2D _polygonTexture;
        private Vector2 _polygonOrigin;
        private Body _polygonBody;

        private Texture2D _circleTexture;
        private Vector2 _circleOrigin;
        private Body _circleBody;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            base.Initialize();
        }

        public override void LoadContent()
        {
            //load texture that will represent the physics body
            _polygonTexture = ScreenManager.ContentManager.Load<Texture2D>("Content/Texture");

            //Create an array to hold the data from the texture
            uint[] data = new uint[_polygonTexture.Width * _polygonTexture.Height];

            //Transfer the texture data to the array
            _polygonTexture.GetData(data);

            //Calculate the vertices from the array
            Vertices verts = Vertices.CreatePolygon(data, _polygonTexture.Width, _polygonTexture.Height);

            //Make sure that the origin of the texture is the centroid (real center of geometry)
            _polygonOrigin = verts.GetCentroid();

            //Use the body factory to create the physics body
            _polygonBody = BodyFactory.Instance.CreatePolygonBody(PhysicsSimulator, verts, 5);
            _polygonBody.Position = new Vector2(500, 400);

            GeomFactory.Instance.CreatePolygonGeom(PhysicsSimulator, _polygonBody, verts, 0);

            _circleTexture = DrawingHelper.CreateCircleTexture(ScreenManager.GraphicsDevice, 35, Color.Gold, Color.Black);
            _circleOrigin = new Vector2(_circleTexture.Width / 2f, _circleTexture.Height / 2f);
            _circleBody = BodyFactory.Instance.CreateCircleBody(PhysicsSimulator, 35, 1);
            _circleBody.Position = new Vector2(300, 400);

            GeomFactory.Instance.CreateCircleGeom(PhysicsSimulator, _circleBody, 35, 20);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            ScreenManager.SpriteBatch.Draw(_polygonTexture, _polygonBody.Position, null, Color.White,
                                           _polygonBody.Rotation, _polygonOrigin, 1, SpriteEffects.None, 0);
            ScreenManager.SpriteBatch.Draw(_circleTexture, _circleBody.Position, null, Color.White,
                                           _circleBody.Rotation, _circleOrigin, 1, SpriteEffects.None, 0);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public override void HandleInput(InputState input)
        {
            if (firstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
                firstRun = false;
            }
            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
            }
            else
            {
                HandleKeyboardInput(input);
            }
            base.HandleInput(input);
        }

        private void HandleKeyboardInput(InputState input)
        {
            const float forceAmount = 50;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.A))
            {
                force += new Vector2(-forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S))
            {
                force += new Vector2(0, forceAmount);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D))
            {
                force += new Vector2(forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W))
            {
                force += new Vector2(0, -forceAmount);
            }

            _polygonBody.ApplyForce(force);

            const float torqueAmount = 1000;
            float torque = 0;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                torque -= torqueAmount;
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right))
            {
                torque += torqueAmount;
            }
            _polygonBody.ApplyTorque(torque);
        }

        public static string GetTitle()
        {
            return "Demo3: Map vertices from textures";
        }

        private static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to map vertices from a texture");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate : left and right arrows");
            sb.AppendLine("  -Move : A,S,D,W");
            return sb.ToString();
        }
    }
}