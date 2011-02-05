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

namespace FarseerPhysics.TestBed.Tests
{
    public class MSTerrainTest : Test
    {
        private MSTerrain terrain;

        public MSTerrainTest()
        {
            World = new World(new Vector2(0, -9.82f));

            terrain = new MSTerrain(World, new AABB(80, 80, new Vector2(-40, 40)), null)
            {
                CellSize = 50,
                SubCellSize = 10,
                Decomposer = Decomposer.Earclip,
                MetersPerUnit = new Vector2(0.1f, 0.1f),
                Iterations = 2,
            };

            terrain.Initialize();
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            Vector2 position = GameInstance.ConvertScreenToWorld(state.X, state.Y);
            Vector2 mapPosition = terrain.WorldToMap(position);

            if (state.RightButton == ButtonState.Pressed)
            {
                DrawCircleOnMap(mapPosition, -1);
                terrain.RegenerateTerrain();
            }
            if (state.LeftButton == ButtonState.Pressed)
            {
                DrawCircleOnMap(mapPosition, 1);
                terrain.RegenerateTerrain();
            }

            base.Mouse(state, oldState);
        }

        private void DrawCircleOnMap(Vector2 center, sbyte value)
        {
            for (int by = -25; by < 25; by++)
            {
                for (int bx = -25; bx < 25; bx++)
                {
                    if ((bx * bx) + (by * by) < 625f)
                    {
                        var ax = center.X - bx;
                        var ay = center.Y - by;
                        terrain.ModifyTerrain(new Vector2(ax, ay), value);
                    }
                }
            }
        }

        internal static Test Create()
        {
            return new MSTerrainTest();
        }
    }
}
