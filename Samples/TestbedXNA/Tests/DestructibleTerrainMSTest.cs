using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class DestructibleTerrainMSTest : Test
    {
        private Decomposer _decomposer = Decomposer.Earclip;
        private sbyte[,] _destroyMap;
        private Texture2D _destroyTexture;
        private float _gwid;
        private double _regenerateTime;
        private Vector2 _scale = new Vector2(0.07f, -0.07f);
        private List<Fixture>[,] _terrainFixtures;
        private sbyte[,] _terrainMap;
        private Texture2D _terrainTexture;
        private Vector2 _translate = new Vector2(0, -511);

        private Stopwatch _watch = new Stopwatch();
        private int _xnum;
        private int _ynum;

        private DestructibleTerrainMSTest()
        {
            World = new World(new Vector2(0, -9.82f));
        }

        public override void Initialize()
        {
            GameInstance.IsFixedTimeStep = true;

            // texture used to hold initial terrain shape
            _terrainTexture = GameInstance.Content.Load<Texture2D>("Terrain");

            // all values
            Color[] fFlat = new Color[_terrainTexture.Width * _terrainTexture.Height];

            _terrainTexture.GetData(fFlat);

            _terrainMap = new sbyte[_terrainTexture.Width,_terrainTexture.Height];

            for (int y = 0; y < _terrainTexture.Height; y++)
            {
                for (int x = 0; x < _terrainTexture.Width; x++)
                {
                    sbyte brightness = (sbyte) ((GetBrightness(fFlat[(y * _terrainTexture.Width) + x]) - 0.5f) * 255);

                    if (brightness > 0)
                        _terrainMap[x, y] = 1;
                    else
                        _terrainMap[x, y] = -1;
                }
            }

            // texture used to hold initial terrain shape
            _destroyTexture = GameInstance.Content.Load<Texture2D>("Circle");

            // all values
            fFlat = new Color[_destroyTexture.Width * _destroyTexture.Height];

            _destroyTexture.GetData(fFlat);

            _destroyMap = new sbyte[_destroyTexture.Width,_destroyTexture.Height];

            for (int y = 0; y < _destroyTexture.Height; y++)
            {
                for (int x = 0; x < _destroyTexture.Width; x++)
                {
                    sbyte brightness = (sbyte) ((GetBrightness(fFlat[(y * _destroyTexture.Width) + x]) - 0.5f) * 255);

                    if (brightness > 0)
                        _destroyMap[x, y] = 1;
                    else
                        _destroyMap[x, y] = -1;
                }
            }

            _gwid = 50;
            _xnum = (int) (_terrainTexture.Width / _gwid);
            _ynum = (int) (_terrainTexture.Height / _gwid);

            _terrainFixtures = new List<Fixture>[_xnum,_ynum];

            // generate terrain
            for (int gy = 0; gy < _ynum; gy++)
            {
                for (int gx = 0; gx < _xnum; gx++)
                {
                    GenerateTerrain(gx, gy);
                }
            }

            base.Initialize();
        }

        private float GetBrightness(Color color)
        {
            float red = (color.R) / 255f;
            float green = (color.G) / 255f;
            float blue = (color.B) / 255f;
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
            float ax = gx * _gwid;
            float ay = gy * _gwid;

            List<Vertices> polys =
                MarchingSquares.DetectSquares(new AABB(new Vector2(ax, ay), new Vector2(ax + _gwid, ay + _gwid)), 5, 5,
                                              _terrainMap, 2, true);
            if (polys.Count == 0) return;

            _terrainFixtures[gx, gy] = new List<Fixture>();

            // create physics object for this grid cell
            foreach (Vertices item in polys)
            {
                item.Translate(ref _translate);
                item.Scale(ref _scale);
                item.ForceCounterClockWise();
                Vertices p = SimplifyTools.CollinearSimplify(item);
                List<Vertices> decompPolys = new List<Vertices>();

                switch (_decomposer)
                {
                    case Decomposer.Bayazit:
                        decompPolys = BayazitDecomposer.ConvexPartition(p);
                        break;
                    case Decomposer.CDT:
                        decompPolys = CDTDecomposer.ConvexPartition(p);
                        break;
                    case Decomposer.Earclip:
                        decompPolys = EarclipDecomposer.ConvexPartition(p);
                        break;
                    case Decomposer.Flipcode:
                        decompPolys = FlipcodeDecomposer.ConvexPartition(p);
                        break;
                    case Decomposer.Seidel:
                        decompPolys = SeidelDecomposer.ConvexPartition(p, 0.001f);
                        break;
                    default:
                        break;
                }

                foreach (Vertices poly in decompPolys)
                {
                    if (poly.Count > 1)
                        _terrainFixtures[gx, gy].Add(BodyFactory.CreatePolygon(World, poly, 1).FixtureList[0]);
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

            _watch.Start();

            if (x > 12.5f && x < _terrainTexture.Width - 12.5f && y > 12.5f && y < _terrainTexture.Height - 12.5f)
            {
                for (int by = 0; by < 25; by++)
                {
                    for (int bx = 0; bx < 25; bx++)
                    {
                        float ax = x - 12.5f + bx;
                        float ay = y - 12.5f + by;
                        if (_destroyMap[bx, by] < 0)
                            _terrainMap[(int) ax, (int) ay] = (sbyte) -_destroyMap[bx, by];
                    }
                }

                //iterate effected cells
                int gx0 = (int) ((x - 12.5f) / _gwid);
                int gx1 = (int) ((x + 12.5f) / _gwid) + 1;
                if (gx0 < 0) gx0 = 0;
                if (gx1 > _xnum) gx1 = _xnum;
                int gy0 = (int) ((y - 12.5f) / _gwid);
                int gy1 = (int) ((y + 12.5f) / _gwid) + 1;
                if (gy0 < 0) gy0 = 0;
                if (gy1 > _ynum) gy1 = _ynum;

                for (int gx = gx0; gx < gx1; gx++)
                {
                    for (int gy = gy0; gy < gy1; gy++)
                    {
                        //remove old terrain object at grid cell
                        if (_terrainFixtures[gx, gy] != null)
                        {
                            for (int i = 0; i < _terrainFixtures[gx, gy].Count; i++)
                            {
                                World.RemoveBody(_terrainFixtures[gx, gy][i].Body);
                            }
                        }

                        _terrainFixtures[gx, gy] = null;

                        //generate new one
                        GenerateTerrain(gx, gy);
                    }
                }
            }

            _watch.Stop();

            _regenerateTime = _watch.Elapsed.TotalMilliseconds;

            _watch.Reset();
        }

        private void CreateTerrain(Vector2 location)
        {
            float x = location.X;
            float y = location.Y;

            _watch.Start();

            if (x > 12.5f && x < _terrainTexture.Width - 12.5f && y > 12.5f && y < _terrainTexture.Height - 12.5f)
            {
                for (int by = 0; by < 25; by++)
                {
                    for (int bx = 0; bx < 25; bx++)
                    {
                        float ax = x - 12.5f + bx;
                        float ay = y - 12.5f + by;
                        if (_destroyMap[bx, by] < 0)
                            _terrainMap[(int) ax, (int) ay] = _destroyMap[bx, by];
                    }
                }

                //iterate effected cells
                int gx0 = (int) ((x - 12.5f) / _gwid);
                int gx1 = (int) ((x + 12.5f) / _gwid) + 1;
                if (gx0 < 0) gx0 = 0;
                if (gx1 > _xnum) gx1 = _xnum;
                int gy0 = (int) ((y - 12.5f) / _gwid);
                int gy1 = (int) ((y + 12.5f) / _gwid) + 1;
                if (gy0 < 0) gy0 = 0;
                if (gy1 > _ynum) gy1 = _ynum;

                for (int gx = gx0; gx < gx1; gx++)
                {
                    for (int gy = gy0; gy < gy1; gy++)
                    {
                        //remove old terrain object at grid cell
                        if (_terrainFixtures[gx, gy] != null)
                        {
                            for (int i = 0; i < _terrainFixtures[gx, gy].Count; i++)
                            {
                                World.RemoveBody(_terrainFixtures[gx, gy][i].Body);
                            }
                        }

                        _terrainFixtures[gx, gy] = null;

                        //generate new one
                        GenerateTerrain(gx, gy);
                    }
                }
            }

            _watch.Stop();

            _regenerateTime = _watch.Elapsed.TotalMilliseconds;

            _watch.Reset();
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            Vector2 position = GameInstance.ConvertScreenToWorld(state.X, state.Y);

            Vector2 mapPosition = WorldToMap(position, _translate, 1f / 0.07f);

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);

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
                Body f = BodyFactory.CreateCircle(World, 0.5f, 1, position);

                f.IsStatic = false;
            }

            DebugView.EndCustomDraw();
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.OemMinus))
            {
                _decomposer--;
                if (_decomposer < 0)
                    _decomposer = 0;

                foreach (List<Fixture> fixtureList in _terrainFixtures)
                {
                    if (fixtureList != null)
                    {
                        foreach (Fixture fixture in fixtureList)
                        {
                            fixture.Body.DestroyFixture(fixture);
                        }
                    }
                }

                _terrainFixtures = new List<Fixture>[_xnum,_ynum];

                // generate terrain
                for (int gy = 0; gy < _ynum; gy++)
                {
                    for (int gx = 0; gx < _xnum; gx++)
                    {
                        GenerateTerrain(gx, gy);
                    }
                }
            }

            if (keyboardManager.IsNewKeyPress(Keys.OemPlus))
            {
                _decomposer++;
                if ((int) _decomposer > 4)
                    _decomposer = (Decomposer) 4;

                foreach (List<Fixture> fixtureList in _terrainFixtures)
                {
                    if (fixtureList != null)
                    {
                        foreach (Fixture fixture in fixtureList)
                        {
                            fixture.Body.DestroyFixture(fixture);
                        }
                    }
                }

                _terrainFixtures = new List<Fixture>[_xnum,_ynum];

                // generate terrain
                for (int gy = 0; gy < _ynum; gy++)
                {
                    for (int gx = 0; gx < _xnum; gx++)
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

            foreach (List<Fixture> fixtureList in _terrainFixtures)
            {
                if (fixtureList != null)
                {
                    count += fixtureList.Count;
                }
            }

            DebugView.DrawString(60, TextLine, "Current Decomposer: " + _decomposer);
            TextLine += 15;
            DebugView.DrawString(60, TextLine, "Total Polygons in Terrain: " + count);
            TextLine += 15;
            DebugView.DrawString(60, TextLine, "Regeneration Time in ms: " + _regenerateTime);
            TextLine += 15;

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new DestructibleTerrainMSTest();
        }

        
    }
}