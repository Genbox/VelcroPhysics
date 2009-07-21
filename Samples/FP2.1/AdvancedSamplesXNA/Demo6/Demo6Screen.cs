using System.Collections.Generic;
using System.Text;
using DemoBaseXNA;
using DemoBaseXNA.DemoShare;
using DemoBaseXNA.DrawingSystem;
using DemoBaseXNA.ScreenSystem;
using FarseerGames.AdvancedSamplesXNA.Demos.Demo7;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.AdvancedSamplesXNA.Demo6
{
    public class Demo6Screen : GameScreen
    {
        private Pyramid _pyramid;
        private Texture2D _rectangleTexture;
        private Body _rectangleBody;
        private Geom _rectangleGeom;
        private const int pyramidBaseBodyCount = 14;
        private Texture2D _grenadeTexture;
        private HairDryer _hairDryer;

        private List<Grenade> _grenades = new List<Grenade>();

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            PhysicsSimulator.BiasFactor = 0.3f;
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            base.Initialize();
        }

        public override void LoadContent()
        {
            //We load the grenade texture here instead of inside Grenade class. Load once, use multiple times.
            _grenadeTexture = ScreenManager.ContentManager.Load<Texture2D>("Content/Grenade");

            _rectangleTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 32, 32, 2, 0, 0,
                                                                     Color.White, Color.Black);

            _rectangleBody = BodyFactory.Instance.CreateRectangleBody(32, 32, 1f);
            _rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(_rectangleBody, 32, 32);
            _rectangleGeom.FrictionCoefficient = .4f;
            _rectangleGeom.RestitutionCoefficient = 0.0f;

            _pyramid = new Pyramid(_rectangleBody, _rectangleGeom, 32f / 5f, 32f / 5f, 32, 32, pyramidBaseBodyCount,
                                   new Vector2(ScreenManager.ScreenCenter.X - pyramidBaseBodyCount * .5f * (32 + 32 / 3),
                                               ScreenManager.ScreenHeight - 125));

            _pyramid.Load(PhysicsSimulator);

            _hairDryer = new HairDryer(new Vector2(100, 100), PhysicsSimulator);
            _hairDryer.Load(ScreenManager.ContentManager.Load<Texture2D>("Content/HairDryer"));

            base.LoadContent();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            for (int i = _grenades.Count - 1; i >= 0; i--)
            {
                _grenades[i].Update(gameTime);
            }

            _hairDryer.Update();

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            _pyramid.Draw(ScreenManager.SpriteBatch, _rectangleTexture);
            _hairDryer.Draw(ScreenManager.SpriteBatch);

            for (int i = _grenades.Count - 1; i >= 0; i--)
            {
                _grenades[i].Draw(ScreenManager.SpriteBatch, ScreenManager.SpriteFonts.DetailsFont);
            }

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
                HandleMouseInput(input);
            }

            base.HandleInput(input);
        }

        private void HandleMouseInput(InputState input)
        {
            if (input.LastMouseState.RightButton == ButtonState.Released && input.CurrentMouseState.RightButton == ButtonState.Pressed)
            {
                Vector2 point = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y);
                Grenade grenade = new Grenade(point, PhysicsSimulator);
                grenade.Load(_grenadeTexture);
                grenade.OnTimeout += Grenade_OnTimeout;

                _grenades.Add(grenade);
            }

            if (input.CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                Vector2 point = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y);
                _hairDryer.Position = point;
            }
        }

        private void Grenade_OnTimeout(Grenade sender, Vector2 position)
        {
            //Remember to remove event registations or the GC will not collect it.
            sender.OnTimeout -= Grenade_OnTimeout;

            //Remove from update/drawing list
            _grenades.Remove(sender);
        }

        public static string GetTitle()
        {
            return "Demo6: Wind and explosions";
        }

        private static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Demonstrates forces like");
            sb.AppendLine("explosions and wind.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse:");
            sb.AppendLine("Left click to move hair dryer.");
            sb.AppendLine("Right click to place grenade.");
            return sb.ToString();
        }
    }
}