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

namespace FarseerGames.AdvancedSamplesXNA.Demos.Demo10
{
    public class Demo10Screen : GameScreen
    {
        private Geom _floor;        // high detail floor geom with 500+ vertices
        private List<Geom> _splitGeoms;  // floor geom split up
        
        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            base.Initialize();
        }

        public override void LoadContent()
        {
            

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            
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
