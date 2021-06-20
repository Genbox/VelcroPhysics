#if WINDOWS
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Tools.TextureTools;
using Genbox.VelcroPhysics.Tools.Triangulation.TriangulationBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests.Velcro
{
    public class DestructibleTerrainTest : Test
    {
        private readonly Terrain _terrain;
        private float _circleRadius = 2.5f;
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
                Iterations = 2
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
            for (int x = 0; x < texture.Width; x++)
            {
                //If the color on the coordinate is black, we include it in the terrain.
                bool inside = colorData[y * texture.Width + x] == Color.Black;

                if (!inside)
                    data[x, y] = 1;
                else
                    data[x, y] = -1;
            }

            _terrain.ApplyData(data, new Vector2(250, 250));

            base.Initialize();
        }

        public override void Mouse(MouseManager mouse)
        {
            Vector2 position = GameInstance.ConvertScreenToWorld(mouse.NewPosition);

            if (mouse.IsButtonDown(MouseButton.Right))
            {
                DrawCircleOnMap(position, -1);
                _terrain.RegenerateTerrain();

                DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
                DebugView.DrawSolidCircle(position, _circleRadius, Vector2.UnitY, Color.Blue * 0.5f);
                DebugView.EndCustomDraw();
            }
            else if (mouse.IsButtonDown(MouseButton.Left))
            {
                DrawCircleOnMap(position, 1);
                _terrain.RegenerateTerrain();

                DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
                DebugView.DrawSolidCircle(position, _circleRadius, Vector2.UnitY, Color.Red * 0.5f);
                DebugView.EndCustomDraw();
            }
            else if (mouse.IsButtonDown(MouseButton.Middle))
            {
                Body circle = BodyFactory.CreateCircle(World, 1, 1);
                circle.BodyType = BodyType.Dynamic;
                circle.Position = position;
            }
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsKeyDown(Keys.G))
                _circleRadius += 0.05f;
            else if (keyboard.IsKeyDown(Keys.H))
                _circleRadius -= 0.05f;

            if (keyboard.IsNewKeyPress(Keys.T))
            {
                _terrain.Decomposer++;

                if (_terrain.Decomposer > TriangulationAlgorithm.Seidel)
                    _terrain.Decomposer--;
            }
            else if (keyboard.IsNewKeyPress(Keys.Y))
            {
                _terrain.Decomposer--;

                if (_terrain.Decomposer < TriangulationAlgorithm.Bayazit)
                    _terrain.Decomposer++;
            }

            base.Keyboard(keyboard);
        }

        private void DrawCircleOnMap(Vector2 center, sbyte value)
        {
            for (float by = -_circleRadius; by < _circleRadius; by += 0.1f)
            for (float bx = -_circleRadius; bx < _circleRadius; bx += 0.1f)
                if (bx * bx + by * by < _circleRadius * _circleRadius)
                {
                    float ax = bx + center.X;
                    float ay = by + center.Y;
                    _terrain.ModifyTerrain(new Vector2(ax, ay), value);
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