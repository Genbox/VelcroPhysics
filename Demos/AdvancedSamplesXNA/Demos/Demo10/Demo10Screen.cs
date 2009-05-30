using System;
using System.Text;
using System.Collections.Generic;
using FarseerGames.AdvancedSamplesXNA.DrawingSystem;
using FarseerGames.AdvancedSamplesXNA.ScreenSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace FarseerGames.AdvancedSamplesXNA.Demos.Demo10
{
    public class Demo10Screen : GameScreen
    {
        private List<Splat> splats;
        
        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);
            PhysicsSimulatorView.EnableEdgeView = true;
            PhysicsSimulatorView.EnableAABBView = false;

            Splat.Load(ScreenManager.ContentManager);

            splats = new List<Splat>();

            base.Initialize();
        }

        public override void LoadContent()
        {
            Random rand = new Random();

            for (int i = 0; i < 10; i++)
            {
                splats.Add(new Splat(PhysicsSimulator, new Vector2(rand.Next(150, 950), rand.Next(150, 500))));
            }

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            foreach (Splat s in splats)
            {
                s.Draw(ScreenManager.SpriteBatch);
            }
            
            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }


        public override void HandleInput(InputState input)
        {
            if (firstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
                firstRun = false;
            }
            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
            }
            base.HandleInput(input);
        }

        public static string GetTitle()
        {
            return "Demo10: Auto-divide geometry";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to use the");
            sb.AppendLine("Auto-divide geometry generator.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Move : left and right arrows");
            return sb.ToString();
        }
    }
}
