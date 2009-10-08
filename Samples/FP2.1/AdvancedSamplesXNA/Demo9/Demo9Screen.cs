using System;
using System.Text;
using System.Collections.Generic;
using DemoBaseXNA;
using DemoBaseXNA.ScreenSystem;
using FarseerGames.FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.AdvancedSamplesXNA.Demo9
{
    public class Demo9Screen : GameScreen
    {
        private List<Splat> splats;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 100));

            // Use the SAT narrow phase collider
            PhysicsSimulator.NarrowPhaseCollider = NarrowPhaseCollider.SAT;

            // setup our debug view
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);
            PhysicsSimulatorView.EnableEdgeView = true;
            PhysicsSimulatorView.EnableAABBView = false;

            // initalize a list of splats
            splats = new List<Splat>();

            base.Initialize();
        }

        public override void LoadContent()
        {
            Random rand = new Random();

            for (int i = 0; i < 6; i++)
            {
                Splat splat = new Splat(PhysicsSimulator, new Vector2(rand.Next(150, 950), rand.Next(150, 500)));
                splat.Load(ScreenManager.ContentManager);
                splats.Add(splat);
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
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
                firstRun = false;
            }
            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
            }
            base.HandleInput(input);
        }

        public static string GetTitle()
        {
            return "Demo9: Auto-divide geometry with SAT";
        }

        private static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to use the");
            sb.AppendLine("Auto-divide geometry generator.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("It is important to note that");
            sb.AppendLine("decomposing geometry is only");
            sb.AppendLine("nessary with SAT narrow phase.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Move : left and right arrows");
            return sb.ToString();
        }
    }
}