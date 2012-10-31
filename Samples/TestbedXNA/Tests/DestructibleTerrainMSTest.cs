#if XNA

using FarseerPhysics.Collision;
using FarseerPhysics.Common;
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
        private float _circleRadius = 2.5f;
        private MSTerrain _terrain;

        private DestructibleTerrainMSTest()
        {
            World = new World(new Vector2(0, -10));

            _terrain = new MSTerrain(World, new AABB(new Vector2(0, 0), 80, 80))
                           {
                               PointsPerUnit = 10,
                               CellSize = 50,
                               SubCellSize = 5,
                               Decomposer = Decomposer.Earclip,
                               Iterations = 2,
                           };

            _terrain.Initialize();
        }

        public override void Initialize()
        {
            Texture2D texture = GameInstance.Content.Load<Texture2D>("Terrain");

            _terrain.ApplyTexture(texture, new Vector2(400, 0), InsideTerrainTest);

            base.Initialize();
        }

        private bool InsideTerrainTest(Color color)
        {
            return color == Color.Black;
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

            if (state.LeftButton == ButtonState.Pressed)
            {
                DrawCircleOnMap(position, 1);
                _terrain.RegenerateTerrain();

                DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
                DebugView.DrawSolidCircle(position, _circleRadius, Vector2.UnitY, Color.Red * 0.5f);
                DebugView.EndCustomDraw();
            }

            if (state.MiddleButton == ButtonState.Pressed)
            {
                Body circle = BodyFactory.CreateCircle(World, 1, 1);
                circle.BodyType = BodyType.Dynamic;
                circle.Position = position;
            }
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsKeyDown(Keys.G))
            {
                _circleRadius += 0.05f;
            }
            else if (keyboardManager.IsKeyDown(Keys.H))
            {
                _circleRadius -= 0.05f;
            }

            if (keyboardManager.IsNewKeyPress(Keys.T))
            {
                _terrain.Decomposer++;

                if (_terrain.Decomposer > Decomposer.Seidel)
                    _terrain.Decomposer--;
            }
            else if (keyboardManager.IsNewKeyPress(Keys.Y))
            {
                _terrain.Decomposer--;

                if (_terrain.Decomposer < Decomposer.Bayazit)
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
            DebugView.DrawString(50, TextLine, "Left click and drag the mouse to destroy terrain!");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Right click and drag the mouse to create terrain!");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Middle click to create circles!");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Press t or y to cycle between decomposers: " + _terrain.Decomposer);
            TextLine += 25;
            DebugView.DrawString(50, TextLine, "Press g or h to decrease/increase circle radius: " + _circleRadius);
            TextLine += 15;

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new DestructibleTerrainMSTest();
        }
    }
}
#endif