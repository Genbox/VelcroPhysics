using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace FarseerPhysics.TestBed.Tests
{
    public class DestructibleTerrainMSTest : Test
    {
        private sbyte[,] destroyMap;
        private Texture2D destroyTexture;
        private float gwid;
        private List<Fixture>[,] terrainFixtures;
        private sbyte[,] terrainMap;
        private Texture2D terrainTexture;
        private int xnum;
        private int ynum;
        private Vector2 _scale = new Vector2(0.07f, -0.07f);
        private Vector2 _translate = new Vector2(-511, -511);

        private DestructibleTerrainMSTest()
        {
            World = new World(new Vector2(0, -9.82f));
        }

        public override void Initialize()
        {
            GameInstance.IsFixedTimeStep = false;

            // texture used to hold initial terrain shape
            terrainTexture = GameInstance.Content.Load<Texture2D>("Terrain");

            // all values
            Color[] fFlat = new Color[terrainTexture.Width * terrainTexture.Height];

            terrainTexture.GetData(fFlat);

            terrainMap = new sbyte[terrainTexture.Width, terrainTexture.Height];

            for (int y = 0; y < terrainTexture.Height; y++)
            {
                for (int x = 0; x < terrainTexture.Width; x++)
                {
                    Color c = fFlat[(y * terrainTexture.Width) + x];
                    if (c == Color.White)
                        terrainMap[x, y] = 1;
                    else
                        terrainMap[x, y] = -1;
                }
            }

            // texture used to hold initial terrain shape
            destroyTexture = GameInstance.Content.Load<Texture2D>("Circle");

            // all values
            fFlat = new Color[destroyTexture.Width * destroyTexture.Height];

            destroyTexture.GetData(fFlat);

            destroyMap = new sbyte[destroyTexture.Width, destroyTexture.Height];

            for (int y = 0; y < destroyTexture.Height; y++)
            {
                for (int x = 0; x < destroyTexture.Width; x++)
                {
                    Color c = fFlat[(y * destroyTexture.Width) + x];
                    if (c == Color.White)
                        destroyMap[x, y] = 1;
                    else
                        destroyMap[x, y] = -1;
                }
            }

            gwid = 20;
            xnum = (int)(terrainTexture.Width / gwid);
            ynum = (int)(terrainTexture.Height / gwid);

            terrainFixtures = new List<Fixture>[xnum, ynum];

            // generate terrain
            for (int gy = 0; gy < ynum; gy++)
            {
                for (int gx = 0; gx < xnum; gx++)
                {
                    GenerateTerrain(gx, gy);
                }
            }

            base.Initialize();
        }

        private void GenerateTerrain(int gx, int gy)
        {
            float ax = gx * gwid;
            float ay = gy * gwid;

            List<Vertices> polys = MarchingSquares.DetectSquares(new AABB(new Vector2(ax, ay), new Vector2(ax + gwid, ay + gwid)), 6, 6, terrainMap, 3, true);
            if (polys.Count == 0) return;

            terrainFixtures[gx, gy] = new List<Fixture>();

            // create physics object for this grid cell
            foreach (var item in polys)
            {
                item.Translate(ref _translate);
                item.Scale(ref _scale);
                item.ForceCounterClockWise();
                Vertices p = FarseerPhysics.Common.PolygonManipulation.SimplifyTools.CollinearSimplify(item);

                List<Vertices> decompPolys = FarseerPhysics.Common.Decomposition.EarclipDecomposer.ConvexPartition(p);

                foreach (var poly in decompPolys)
                {
                    terrainFixtures[gx, gy].Add(FixtureFactory.CreatePolygon(World, poly, 1));
                }
            }
        }

        private Vector2 WorldToMap(Vector2 point, Vector2 mapOrigin, float metersPerUnit)
        {
            point *= new Vector2(metersPerUnit, -metersPerUnit);
            point -= mapOrigin;
            return point;
        }

        private void DestroyTerrain(Vector2 location)
        {
            float x = location.X;
            float y = location.Y;

            if (x > 12.5f && x < terrainTexture.Width - 12.5f && y > 12.5f && y < terrainTexture.Height - 12.5f)
            {
                for (int by = 0; by < 25; by++)
                {
                    for (int bx = 0; bx < 25; bx++)
                    {
                        var ax = x - 12.5f + bx;
                        var ay = y - 12.5f + by;
                        if (destroyMap[bx, by] < 0)
                            terrainMap[(int)ax, (int)ay] = (sbyte)-destroyMap[bx, by];
                    }
                }

                //iterate effected cells
                var gx0 = (int)((x - 12.5f) / gwid);
                var gx1 = (int)((x + 12.5f) / gwid) + 1;
                if (gx0 < 0) gx0 = 0;
                if (gx1 > xnum) gx1 = xnum;
                var gy0 = (int)((y - 12.5f) / gwid);
                var gy1 = (int)((y + 12.5f) / gwid) + 1;
                if (gy0 < 0) gy0 = 0;
                if (gy1 > ynum) gy1 = ynum;

                for (int gx = gx0; gx < gx1; gx++)
                {
                    for (int gy = gy0; gy < gy1; gy++)
                    {
                        //remove old terrain object at grid cell
                        if (terrainFixtures[gx, gy] != null)
                        {
                            for (int i = 0; i < terrainFixtures[gx, gy].Count; i++)
                            {
                                World.RemoveBody(terrainFixtures[gx, gy][i].Body);
                            }
                        }

                        terrainFixtures[gx, gy] = null;

                        //generate new one
                        GenerateTerrain(gx, gy);
                    }
                }
            }
        }

        private void CreateTerrain(Vector2 location)
        {
            float x = location.X;
            float y = location.Y;

            if (x > 12.5f && x < terrainTexture.Width - 12.5f && y > 12.5f && y < terrainTexture.Height - 12.5f)
            {
                for (int by = 0; by < 25; by++)
                {
                    for (int bx = 0; bx < 25; bx++)
                    {
                        var ax = x - 12.5f + bx;
                        var ay = y - 12.5f + by;
                        if (destroyMap[bx, by] < 0)
                            terrainMap[(int)ax, (int)ay] = (sbyte)destroyMap[bx, by];
                    }
                }

                //iterate effected cells
                var gx0 = (int)((x - 12.5f) / gwid);
                var gx1 = (int)((x + 12.5f) / gwid) + 1;
                if (gx0 < 0) gx0 = 0;
                if (gx1 > xnum) gx1 = xnum;
                var gy0 = (int)((y - 12.5f) / gwid);
                var gy1 = (int)((y + 12.5f) / gwid) + 1;
                if (gy0 < 0) gy0 = 0;
                if (gy1 > ynum) gy1 = ynum;

                for (int gx = gx0; gx < gx1; gx++)
                {
                    for (int gy = gy0; gy < gy1; gy++)
                    {
                        //remove old terrain object at grid cell
                        if (terrainFixtures[gx, gy] != null)
                        {
                            for (int i = 0; i < terrainFixtures[gx, gy].Count; i++)
                            {
                                World.RemoveBody(terrainFixtures[gx, gy][i].Body);
                            }
                        }

                        terrainFixtures[gx, gy] = null;

                        //generate new one
                        GenerateTerrain(gx, gy);
                    }
                }
            }
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            Vector2 position = GameInstance.ConvertScreenToWorld(state.X, state.Y);

            Vector2 mapPosition = WorldToMap(position, _translate, 1f / 0.07f);

            if (state.LeftButton == ButtonState.Pressed)
            {
                DebugView.DrawCircle(position, 0.75f, Color.Red);

                DestroyTerrain(mapPosition);
            }

            if (state.RightButton == ButtonState.Pressed)
            {
                DebugView.DrawCircle(position, 0.75f, Color.Blue);

                CreateTerrain(mapPosition);
            }

            if (state.MiddleButton == ButtonState.Pressed && oldState.MiddleButton == ButtonState.Released)
            {
                Fixture f = FixtureFactory.CreateCircle(World, 0.5f, 1, position);

                f.Body.IsStatic = false;
            }

            base.Mouse(state, oldState);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DebugView.DrawString(50, TextLine, "Left click and drag the mouse to destroy terrain!");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Right click and drag the mouse to create terrain!");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Middle click to create circles!");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Total Polygons: " + World.BodyList.Count);
            TextLine += 15;

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new DestructibleTerrainMSTest();
        }
    }
}