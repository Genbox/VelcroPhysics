using FarseerPhysics.Testbed.Framework;
#if WINDOWS
using FarseerPhysics.Collision;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.TextureTools;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
{
    public class DestructibleTerrainTest : Test
    {
        private float _circleRadius = 2.5f;
        private Terrain _terrain;
        private AABB _terrainArea;

        private DestructibleTerrainTest()
        {
            World = new World(new Vector2(0, -10));

            _terrainArea = new AABB(new Vector2(0, 0), 100, 100);
            _terrain = new Terrain(World, _terrainArea)
                           {
                               PointsPerUnit = 10,
                               CellSize = 50,
                               SubCellSize = 5,
                               Decomposer = TriangulationAlgorithm.Earclip,
                               Iterations = 2,
                           };

            _terrain.Initialize();
        }

        public override void Initialize()
        {
            Texture2D texture = GameInstance.Content.Load<Texture2D>("Terrain");
            Color[] colorData = new Color[texture.Width * texture.Height];

            texture.GetData(colorData);

            sbyte[,] data = new sbyte[texture.Width, texture.Height];

            for (int y = 0; y < texture.Height; y++)
            {
                for (int x = 0; x < texture.Width; x++)
                {
                    //If the color on the coordinate is black, we include it in the terrain.
                    bool inside = colorData[(y * texture.Width) + x] == Color.Black;

                    if (!inside)
                        data[x, y] = 1;
                    else
                        data[x, y] = -1;
                }
            }

            _terrain.ApplyData(data, new Vector2(250, 250));

            base.Initialize();
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            Vector2 position = GameInstance.ConvertScreenToWorld(state.X, state.Y);

            if (state.RightButton == ButtonState.Pressed)
            {
                DrawCircleOnMap(position, -1);
                _terrain.RegenerateTerrain();

                DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
                DebugView.DrawSolidCircle(position, _circleRadius, Vector2.UnitY, Color.Blue * 0.5f);
                DebugView.EndCustomDraw();
            }
            else if (state.LeftButton == ButtonState.Pressed)
            {
                DrawCircleOnMap(position, 1);
                _terrain.RegenerateTerrain();

                DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
                DebugView.DrawSolidCircle(position, _circleRadius, Vector2.UnitY, Color.Red * 0.5f);
                DebugView.EndCustomDraw();
            }
            else if (state.MiddleButton == ButtonState.Pressed)
            {
                Body circle = BodyFactory.CreateCircle(World, 1, 1);
                circle.BodyType = BodyType.Dynamic;
                circle.Position = position;
            }
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsKeyDown(Keys.G))
                _circleRadius += 0.05f;
            else if (keyboardManager.IsKeyDown(Keys.H))
                _circleRadius -= 0.05f;

            if (keyboardManager.IsNewKeyPress(Keys.T))
            {
                _terrain.Decomposer++;

                if (_terrain.Decomposer > TriangulationAlgorithm.Seidel)
                    _terrain.Decomposer--;
            }
            else if (keyboardManager.IsNewKeyPress(Keys.Y))
            {
                _terrain.Decomposer--;

                if (_terrain.Decomposer < TriangulationAlgorithm.Bayazit)
                    _terrain.Decomposer++;
            }

            base.Keyboard(keyboardManager);
        }

        private void DrawCircleOnMap(Vector2 center, sbyte value)
        {
            for (float by = -_circleRadius; by < _circleRadius; by += 0.1f)
            {
                for (float bx = -_circleRadius; bx < _circleRadius; bx += 0.1f)
                {
                    if ((bx * bx) + (by * by) < _circleRadius * _circleRadius)
                    {
                        float ax = bx + center.X;
                        float ay = by + center.Y;
                        _terrain.ModifyTerrain(new Vector2(ax, ay), value);
                    }
                }
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            DebugView.DrawAABB(ref _terrainArea, Color.Red * 0.5f);
            DebugView.EndCustomDraw();

            DrawString("Left click and drag the mouse to destroy terrain!");
            DrawString("Right click and drag the mouse to create terrain!");
            DrawString("Middle click to create circles!");
            DrawString("Press t or y to cycle between decomposers: " + _terrain.Decomposer);
            TextLine += 25;
            DrawString("Press g or h to decrease/increase circle radius: " + _circleRadius);

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new DestructibleTerrainTest();
        }
    }
}
#endif