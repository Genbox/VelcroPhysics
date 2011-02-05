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
using FarseerPhysics.Collision;

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
            this.ScreenManager.Game.IsFixedTimeStep = false;

            World = new World(new Vector2(0, -9.82f));
            base.LoadContent();

            DebugMaterial defaultMaterial = new DebugMaterial(MaterialType.Stars)
            {
                Color = Color.YellowGreen,
                Scale = 8f
            };

            terrain = new MSTerrain(World, this.ScreenManager.ContentManager.Load<Texture2D>("Texture"), InsideTerrain, defaultMaterial);
        }

        public override void HandleInput(InputHelper input)
        {
            if (input.CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                Vector2 position = terrain.WorldToMap(Camera2D.ConvertScreenToWorld(new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y)));

                for (int by = -25; by < 25; by++)
                {
                    for (int bx = -25; bx < 25; bx++)
                    {
                        if ((bx * bx) + (by * by) < 625)
                        {
                            var ax = position.X - bx;
                            var ay = position.Y - by;
                            terrain.ModifyTerrain(new Vector2(ax, ay), 1);
                        }
                    }
                }

                terrain.RegenerateTerrain();
            }

            if (input.CurrentMouseState.RightButton == ButtonState.Pressed)
            {
                Vector2 position = terrain.WorldToMap(Camera2D.ConvertScreenToWorld(new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y)));

                for (int by = -25; by < 25; by++)
                {
                    for (int bx = -25; bx < 25; bx++)
                    {
                        if ((bx * bx) + (by * by) < 625f)
                        {
                            var ax = position.X - bx;
                            var ay = position.Y - by;
                            terrain.ModifyTerrain(new Vector2(ax, ay), -1);
                        }
                    }
                }

                terrain.RegenerateTerrain();
            }

            if (input.CurrentMouseState.MiddleButton == ButtonState.Pressed)
            {
                Vector2 position = Camera2D.ConvertScreenToWorld(new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y));

                DebugMaterial circleMaterial = new DebugMaterial(MaterialType.Face)
                {
                    Color = Color.LightGray,
                    Scale = 1.25f
                };

                Fixture circle = FixtureFactory.CreateCircle(World, 0.5f, 1, position, circleMaterial);

                circle.Body.BodyType = BodyType.Dynamic;
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