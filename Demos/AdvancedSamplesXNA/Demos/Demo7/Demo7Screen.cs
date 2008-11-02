using System.Text;
using FarseerGames.AdvancedSamples.DrawingSystem;
using FarseerGames.AdvancedSamples.ScreenSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerGames.AdvancedSamples.Demos.DemoShare;

namespace FarseerGames.AdvancedSamples.Demos.Demo7
{
    public class Demo7Screen : GameScreen
    {
        private Border _border;
        private Pyramid _pyramid;
        private Texture2D _rectangleTexture;
        private Body _rectangleBody;
        private Geom _rectangleGeom;
        private const int pyramidBaseBodyCount = 8;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            base.Initialize();
        }

        public override void LoadContent()
        {
            _border = new Border(ScreenManager.ScreenWidth, ScreenManager.ScreenHeight, 25, ScreenManager.ScreenCenter);
            _border.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            _rectangleTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 32, 32, 2, 0, 0,
                                                         Color.White, Color.Black);

            _rectangleBody = BodyFactory.Instance.CreateRectangleBody(32, 32, 1f);
            _rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(_rectangleBody, 32, 32);
            _rectangleGeom.FrictionCoefficient = .4f;
            _rectangleGeom.RestitutionCoefficient = 0f;

            _pyramid = new Pyramid(_rectangleBody, _rectangleGeom, 32f / 3f, 32f / 3f, 32, 32, pyramidBaseBodyCount,
                       new Vector2(ScreenManager.ScreenCenter.X - pyramidBaseBodyCount * .5f * (32 + 32 / 3),
                                   ScreenManager.ScreenHeight - 125));

            _pyramid.Load(PhysicsSimulator);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            _pyramid.Draw(ScreenManager.SpriteBatch, _rectangleTexture);
            _border.Draw(ScreenManager.SpriteBatch);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public override void HandleInput(InputState input)
        {
            if (FirstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
                FirstRun = false;
            }
            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
            }
            else
            {
                HandleMouseInput(input);
            }

            base.HandleInput(input);
        }

        private void HandleMouseInput(InputState input)
        {
            if (input.LastMouseState.RightButton == ButtonState.Released && input.CurrentMouseState.RightButton == ButtonState.Pressed)
            {
                Vector2 point = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y);
                Explode(point);
            }
        }

        public void Explode(Vector2 position)
        {
            Vector2 min = Vector2.Subtract(position, new Vector2(100, 100));
            Vector2 max = Vector2.Add(position, new Vector2(100, 100));

            AABB aabb = new AABB(min, max);

            foreach (Body body in PhysicsSimulator.BodyList)
            {
                if (aabb.Contains(body.Position))
                {
                    Vector2 fv = body.Position;
                    fv = Vector2.Subtract(fv, position);
                    fv.Normalize();
                    fv = Vector2.Multiply(fv, 50000);
                    body.ApplyForce(fv);
                }
            }
        }

        public static string GetTitle()
        {
            return "Demo7: Wind and explosions";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Demonstrates forces like");
            sb.AppendLine("explosions and wind.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse:");
            sb.AppendLine("Left click: Apply wind");
            sb.AppendLine("Right click: Apply explosion");
            return sb.ToString();
        }
    }
}