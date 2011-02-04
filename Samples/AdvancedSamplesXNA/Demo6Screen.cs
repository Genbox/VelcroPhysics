using System.Collections.Generic;
using System.Text;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.DebugViews;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.AdvancedSamplesXNA
{
    internal class Demo6Screen : PhysicsGameScreen, IDemoScreen
    {
        MSTerrain terrain;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo6: Destructible Terrain";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            World = new World(new Vector2(0, -9.82f));
            base.LoadContent();

            DebugMaterial defaultMaterial = new DebugMaterial(MaterialType.Pavement)
            {
                Color = Color.LightGray,
                Scale = 8f
            };

            terrain = new MSTerrain(World, this.ScreenManager.ContentManager.Load<Texture2D>("Texture"), InsideTerrain, defaultMaterial);
        }

        public override void HandleInput(InputHelper input)
        {
            if (input.CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                Vector2 position = terrain.WorldToMap(Camera2D.ConvertScreenToWorld(new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y)));

                for (int by = 0; by < 25; by++)
                {
                    for (int bx = 0; bx < 25; bx++)
                    {
                        var ax = position.X - 12.5f + bx;
                        var ay = position.Y - 12.5f + by;
                        terrain.ModifyTerrain(new Vector2(ax, ay), -1);
                    }
                }

                terrain.RegenerateTerrain();
            }

            if (input.CurrentMouseState.RightButton == ButtonState.Pressed)
            {
                Vector2 position = terrain.WorldToMap(Camera2D.ConvertScreenToWorld(new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y)));

                for (int by = 0; by < 25; by++)
                {
                    for (int bx = 0; bx < 25; bx++)
                    {
                        var ax = position.X - 12.5f + bx;
                        var ay = position.Y - 12.5f + by;
                        terrain.ModifyTerrain(new Vector2(ax, ay), 1);
                    }
                }

                terrain.RegenerateTerrain();
            }
            
            base.HandleInput(input);
        }

        private bool InsideTerrain(Color color)
        {
            if (color.A > 0)
                return true;
            return false;
        }
    }
}