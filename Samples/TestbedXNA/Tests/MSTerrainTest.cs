using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.TestBed.Framework;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.TestBed.Tests
{
    public class MSTerrainTest : Test
    {
        private MSTerrain terrain;
        private float circleRadius = 2.5f;

        public MSTerrainTest()
        {
            World = new World(new Vector2(0, -10));

            terrain = new MSTerrain(World, new AABB(new Vector2(0, 0), 80, 80))
            {
                PointsPerUnit = 10,
                CellSize = 50,
                SubCellSize = 5,
                Decomposer = Decomposer.Bayazit,
                Iterations = 2,
            };

            terrain.Initialize();
        }

        public override void Initialize()
        {
            Texture2D texture = GameInstance.Content.Load<Texture2D>("Terrain");

            terrain.ApplyTexture(texture, new Vector2(400,0), InsideTerrainTest);
            
            base.Initialize();
        }

        private bool InsideTerrainTest(Color color)
        {
            return (color == Color.Black);
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            Vector2 position = GameInstance.ConvertScreenToWorld(state.X, state.Y);
            
            if (state.RightButton == ButtonState.Pressed)
            {
                DrawCircleOnMap(position, -1);
                terrain.RegenerateTerrain();

                DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
                DebugView.DrawSolidCircle(position, circleRadius, Vector2.UnitY, Color.Blue * 0.5f);
                DebugView.EndCustomDraw();
            }

            if (state.LeftButton == ButtonState.Pressed)
            {
                DrawCircleOnMap(position, 1);
                terrain.RegenerateTerrain();

                DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
                DebugView.DrawSolidCircle(position, circleRadius, Vector2.UnitY, Color.Red * 0.5f);
                DebugView.EndCustomDraw();
            }

            if (state.MiddleButton == ButtonState.Pressed)
            {
                Body circle = BodyFactory.CreateCircle(World, 1, 1);
                circle.BodyType = BodyType.Dynamic;
                circle.Position = position;
            }

            base.Mouse(state, oldState);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsKeyDown(Keys.OemComma))
            {
                circleRadius += 0.05f;
            }
            else if (keyboardManager.IsKeyDown(Keys.OemPeriod))
            {
                circleRadius -= 0.05f;
            }

            if (keyboardManager.IsNewKeyPress(Keys.OemPlus))
            {
                terrain.Decomposer++;

                if (terrain.Decomposer > Decomposer.Seidel)
                    terrain.Decomposer--;
            }
            else if (keyboardManager.IsNewKeyPress(Keys.OemMinus))
            {
                terrain.Decomposer--;

                if (terrain.Decomposer < Decomposer.Bayazit)
                    terrain.Decomposer++;
            }
            
            base.Keyboard(keyboardManager);
        }

        private void DrawCircleOnMap(Vector2 center, sbyte value)
        {
            for (float by = -circleRadius; by < circleRadius; by += 0.1f)
            {
                for (float bx = -circleRadius; bx < circleRadius; bx += 0.1f)
                {
                    if ((bx * bx) + (by * by) < circleRadius * circleRadius)
                    {
                        var ax = bx + center.X;
                        var ay = by + center.Y;
                        terrain.ModifyTerrain(new Vector2(ax, ay), value);
                    }
                }
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DebugView.DrawString(50, TextLine, "Left click and drag the mouse to destroy terrain!");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Right click and drag the mouse to create terrain!");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Middle click to create circles!");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Press + or - to cycle between decomposers: " + terrain.Decomposer);
            TextLine += 25;
            DebugView.DrawString(50, TextLine, "Press < or > to decrease/increase circle radius: " + circleRadius);
            TextLine += 15;
            
            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new MSTerrainTest();
        }
    }
}
