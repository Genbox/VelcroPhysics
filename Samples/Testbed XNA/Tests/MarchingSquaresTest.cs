using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class MarchingSquaresTest : Test
    {
        private Vector2 _scale = new Vector2(0.07f, -0.07f);
        private Vector2 _translate = new Vector2(-512, -512);
        private AABB _aabb;
        private float[,] _f;
        private float _gridSize = 5;
        private int _level = 2;
        private List<Vertices> _polys = new List<Vertices>();
        private Stopwatch _sw = new Stopwatch();
        private Texture2D _terrainTex;
        private double _time;
        private bool _combine = true;

        private MarchingSquaresTest()
        {
            World = new World(new Vector2(0, 9.82f));
        }

        public override void Initialize()
        {
            _terrainTex = GameInstance.Content.Load<Texture2D>("Terrain");

            // all values
            Color[] fFlat = new Color[_terrainTex.Width * _terrainTex.Height];

            _terrainTex.GetData(fFlat);

            _f = new float[_terrainTex.Width + 100, _terrainTex.Height + 100];

            for (int y = 0; y < _terrainTex.Height; y++)
            {
                for (int x = 0; x < _terrainTex.Width; x++)
                {
                    Color c = fFlat[(y * _terrainTex.Width) + x];
                    if (c == Color.White)
                        _f[x, y] = 1.0f;
                    else
                        _f[x, y] = -1.0f;
                }
            }

            UpdatePolys();

            base.Initialize();
        }

        private float Eval(float x, float y)
        {
            return _f[(int)x, (int)y];
        }

        private void UpdatePolys()
        {
            World.Clear();
            _sw.Start();

            _aabb = new AABB { LowerBound = new Vector2(0, 0), UpperBound = new Vector2(_terrainTex.Width, _terrainTex.Height), };
            _polys = MarchingSquares.DetectSquares(_aabb, _gridSize, _gridSize, Eval, _level, _combine);

            _sw.Stop();
            _time = _sw.Elapsed.TotalMilliseconds;
            _sw.Reset();

            for (int i = 0; i < _polys.Count; i++)
            {
                Vertices poly = _polys[i];
                poly.Translate(ref _translate);

                poly.Scale(ref _scale);
                poly.ForceCounterClockWise();

                if (!poly.IsConvex())
                {
                    List<Vertices> verts = EarclipDecomposer.ConvexPartition(poly);

                    for (int j = 0; j < verts.Count; j++)
                    {
                        Vertices v = verts[j];
                        FixtureFactory.CreatePolygon(World, v, 1);
                    }
                }
                else
                {
                    FixtureFactory.CreatePolygon(World, poly, 1);
                }
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DebugView.DrawString(50, TextLine, "Recursion Level: " + _level);
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Grid Size: " + _gridSize);
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Time: " + _time + " ms");
            TextLine += 15;

            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            int oldLevel = _level;
            float oldGridSize = _gridSize;
            bool oldCombine = _combine;

            if (keyboardManager.IsNewKeyPress(Keys.W))
                _level++;

            if (keyboardManager.IsNewKeyPress(Keys.S))
                _level--;

            if (_level < 0)
                _level = 0;

            if (keyboardManager.IsNewKeyPress(Keys.A))
                _gridSize -= 0.5f;

            if (keyboardManager.IsNewKeyPress(Keys.D))
                _gridSize += 0.5f;

            if (_gridSize < 4)
                _gridSize = 4;

            if (keyboardManager.IsNewKeyPress(Keys.Tab))
                _combine = !_combine;

            if (oldLevel != _level || oldGridSize != _gridSize || oldCombine != _combine)
                UpdatePolys();

            base.Keyboard(keyboardManager);
        }

        internal static Test Create()
        {
            return new MarchingSquaresTest();
        }
    }
}