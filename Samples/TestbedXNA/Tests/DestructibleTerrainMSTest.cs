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
using System.Diagnostics;
using System.Linq;

namespace FarseerPhysics.TestBed.Tests
{
    public class DestructibleTerrainMSTest : Test
    {
        internal enum Decomposer
        {
            Bayazit,
            CDT,
            Earclip,
            Flipcode,
            Seidel,
        }

        private sbyte[,] destroyMap;
        private Texture2D destroyTexture;
        private float gwid;
        private List<Fixture>[,] terrainFixtures;
        private sbyte[,] terrainMap;
        private Texture2D terrainTexture;
        private int xnum;
        private int ynum;
        private Vector2 _scale = new Vector2(0.07f, -0.07f);
        private Vector2 _translate = new Vector2(0, -511);

        private Stopwatch watch = new Stopwatch();
        private double regenerateTime = 0;

        //Performance graph
        public bool AdaptiveLimits = true;
        public int ValuesToGraph = 500;
        public int MinimumValue;
        public int MaximumValue = 1000;
        private List<float> _graphValues = new List<float>();
        private Vector2[] _background = new Vector2[4];

        private float _max;
        private float _avg;
        private float _min;

        private Decomposer decomposer = Decomposer.Earclip;


        private DestructibleTerrainMSTest()
        {
            World = new World(new Vector2(0, -9.82f));
        }

        public override void Initialize()
        {
            GameInstance.IsFixedTimeStep = true;

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
                    sbyte brightness = (sbyte)((GetBrightness(fFlat[(y * terrainTexture.Width) + x]) - 0.5f) * 255);

                    if (brightness > 0)
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
                    sbyte brightness = (sbyte)((GetBrightness(fFlat[(y * destroyTexture.Width) + x]) - 0.5f) * 255);

                    if (brightness > 0)
                        destroyMap[x, y] = 1;
                    else
                        destroyMap[x, y] = -1;
                }
            }

            gwid = 50;
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

        public float GetBrightness(Color color)
        {
            float red = ((float)color.R) / 255f;
            float green = ((float)color.G) / 255f;
            float blue = ((float)color.B) / 255f;
            float num4 = red;
            float num5 = red;
            if (green > num4)
            {
                num4 = green;
            }
            if (blue > num4)
            {
                num4 = blue;
            }
            if (green < num5)
            {
                num5 = green;
            }
            if (blue < num5)
            {
                num5 = blue;
            }
            return ((num4 + num5) / 2f);
        }



        private void GenerateTerrain(int gx, int gy)
        {
            float ax = gx * gwid;
            float ay = gy * gwid;

            List<Vertices> polys = MarchingSquares.DetectSquares(new AABB(new Vector2(ax, ay), new Vector2(ax + gwid, ay + gwid)), 5, 5, terrainMap, 2, true);
            if (polys.Count == 0) return;

            terrainFixtures[gx, gy] = new List<Fixture>();

            // create physics object for this grid cell
            foreach (var item in polys)
            {
                item.Translate(ref _translate);
                item.Scale(ref _scale);
                item.ForceCounterClockWise();
                Vertices p = FarseerPhysics.Common.PolygonManipulation.SimplifyTools.CollinearSimplify(item);
                List<Vertices> decompPolys = new List<Vertices>();

                switch (decomposer)
                {
                    case Decomposer.Bayazit:
                        decompPolys = FarseerPhysics.Common.Decomposition.BayazitDecomposer.ConvexPartition(p);
                        break;
                    case Decomposer.CDT:
                        decompPolys = FarseerPhysics.Common.Decomposition.CDTDecomposer.ConvexPartition(p);
                        break;
                    case Decomposer.Earclip:
                        decompPolys = FarseerPhysics.Common.Decomposition.EarclipDecomposer.ConvexPartition(p);
                        break;
                    case Decomposer.Flipcode:
                        decompPolys = FarseerPhysics.Common.Decomposition.FlipcodeDecomposer.ConvexPartition(p);
                        break;
                    case Decomposer.Seidel:
                        decompPolys = FarseerPhysics.Common.Decomposition.SeidelDecomposer.ConvexPartition(p, 0.001f);
                        break;
                    default:
                        break;
                }

                foreach (var poly in decompPolys)
                {
                    if (poly.Count > 1)
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

            watch.Start();

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

            watch.Stop();

            regenerateTime = watch.Elapsed.TotalMilliseconds;

            _graphValues.Add((float)regenerateTime);

            watch.Reset();
        }

        private void CreateTerrain(Vector2 location)
        {
            float x = location.X;
            float y = location.Y;

            watch.Start();

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

            watch.Stop();

            regenerateTime = watch.Elapsed.TotalMilliseconds;

            _graphValues.Add((float)regenerateTime);

            watch.Reset();
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

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.OemMinus))
            {
                decomposer--;
                if (decomposer < 0)
                    decomposer = 0;

                foreach (var fixtureList in terrainFixtures)
                {
                    if (fixtureList != null)
                    {
                        foreach (var fixture in fixtureList)
                        {
                            fixture.Body.DestroyFixture(fixture);
                        }
                    }
                }

                terrainFixtures = new List<Fixture>[xnum, ynum];

                // generate terrain
                for (int gy = 0; gy < ynum; gy++)
                {
                    for (int gx = 0; gx < xnum; gx++)
                    {
                        GenerateTerrain(gx, gy);
                    }
                }
            }

            if (keyboardManager.IsNewKeyPress(Keys.OemPlus))
            {
                decomposer++;
                if ((int)decomposer > 4)
                    decomposer = (Decomposer)4;

                foreach (var fixtureList in terrainFixtures)
                {
                    if (fixtureList != null)
                    {
                        foreach (var fixture in fixtureList)
                        {
                            fixture.Body.DestroyFixture(fixture);
                        }
                    }
                }

                terrainFixtures = new List<Fixture>[xnum, ynum];

                // generate terrain
                for (int gy = 0; gy < ynum; gy++)
                {
                    for (int gx = 0; gx < xnum; gx++)
                    {
                        GenerateTerrain(gx, gy);
                    }
                }
            }
            
            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DebugView.DrawString(50, TextLine, "Left click and drag the mouse to destroy terrain!");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Right click and drag the mouse to create terrain!");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Middle click to create circles!");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Press + or - to cycle between decomposers.");
            TextLine += 25;


            // count fixtures in terrain only
            int count = 0;

            foreach (var fixtureList in terrainFixtures)
            {
                if (fixtureList != null)
                {
                    foreach (var fixture in fixtureList)
                    {
                        count++;
                    }
                }
            }

            DebugView.DrawString(60, TextLine, "Current Decomposer: " + decomposer.ToString());
            TextLine += 15;
            DebugView.DrawString(60, TextLine, "Total Polygons in Terrain: " + count);
            TextLine += 15;
            DebugView.DrawString(60, TextLine, "Regeneration Time in ms: " + regenerateTime);
            TextLine += 15;

            DrawPerformanceGraph();

            base.Update(settings, gameTime);
        }

        private void DrawPerformanceGraph()
        {
            Rectangle PerformancePanelBounds = new Rectangle(50, 200, 250, 200);

            if (_graphValues.Count > ValuesToGraph + 1)
                _graphValues.RemoveAt(0);

            float x = PerformancePanelBounds.X;
            float deltaX = PerformancePanelBounds.Width / (float)ValuesToGraph;
            float yScale = PerformancePanelBounds.Bottom - (float)PerformancePanelBounds.Top;

            // we must have at least 2 values to start rendering
            if (_graphValues.Count > 2)
            {
                _max = (float)_graphValues.Max();
                _avg = (float)_graphValues.Average();
                _min = (float)_graphValues.Min();

                if (AdaptiveLimits)
                {
                    MaximumValue = (int)_max;
                    MinimumValue = 0;
                }

                // start at last value (newest value added)
                // continue until no values are left
                for (int i = _graphValues.Count - 1; i > 0; i--)
                {
                    float y1 = PerformancePanelBounds.Bottom - ((_graphValues[i] / (MaximumValue - MinimumValue)) * yScale);
                    float y2 = PerformancePanelBounds.Bottom - ((_graphValues[i - 1] / (MaximumValue - MinimumValue)) * yScale);

                    Vector2 x1 = new Vector2(MathHelper.Clamp(x, PerformancePanelBounds.Left, PerformancePanelBounds.Right),
                                             MathHelper.Clamp(y1, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));

                    Vector2 x2 = new Vector2(MathHelper.Clamp(x + deltaX, PerformancePanelBounds.Left, PerformancePanelBounds.Right),
                                             MathHelper.Clamp(y2, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));

                    DebugView.DrawLocalSegment(x1, x2, Color.LightGreen);

                    x += deltaX;
                }
            }

            DebugView.DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Top, "Max: " + _max);
            DebugView.DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Center.Y - 7, "Avg: " + _avg);
            DebugView.DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Bottom - 15, "Min: " + _min);

            //Draw background.
            _background[0] = new Vector2(PerformancePanelBounds.X, PerformancePanelBounds.Y);
            _background[1] = new Vector2(PerformancePanelBounds.X, PerformancePanelBounds.Y + PerformancePanelBounds.Height);
            _background[2] = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width, PerformancePanelBounds.Y + PerformancePanelBounds.Height);
            _background[3] = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width, PerformancePanelBounds.Y);

            DebugView.DrawLocalSolidPolygon(_background, 4, Color.DarkGray, true);
        }

        internal static Test Create()
        {
            return new DestructibleTerrainMSTest();
        }
    }
}