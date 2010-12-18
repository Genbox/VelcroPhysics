using System;
using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.DebugViews;
using FarseerPhysics.DemoBaseXNA.DemoShare;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Collision.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.DemoBaseXNA.ScreenSystem
{
    public class PhysicsGameScreen : GameScreen
    {
        public const float LineWidth = .4f;

        public World World;
        public DebugViewXNA DebugView;

        private FixedMouseJoint _fixedMouseJoint;
        private Border _border;

        private static VertexPositionColorTexture[] _vertsLines;
        private static VertexPositionColorTexture[] _vertsFill;
        private static int _fillCount;
        private static int _lineCount;
        private Dictionary<MaterialType, List<Fixture>> _shapeFixtures;
        private List<Fixture> _lineFixtures;

        public MaterialManager MaterialManager;

        private Texture2D _lineTexture;

        protected PhysicsGameScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.75);
            TransitionOffTime = TimeSpan.FromSeconds(0.75);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            MaterialManager = new MaterialManager();
            MaterialManager.LoadContent(ScreenManager.ContentManager);

            _shapeFixtures = new Dictionary<MaterialType, List<Fixture>>();

            int maxPrimitiveCount = ScreenManager.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach ? 65535 : 1048575;
            _vertsLines = new VertexPositionColorTexture[maxPrimitiveCount];
            _vertsFill = new VertexPositionColorTexture[maxPrimitiveCount];

            _lineTexture = ScreenManager.ContentManager.Load<Texture2D>("Common/line");
            _lineFixtures = new List<Fixture>();

            if (World == null)
                return;

            World.FixtureAdded += AddFixture;
            World.FixtureRemoved += RemoveFixture;

            //We enable diagnostics to show get values for our performance counters.
            Settings.EnableDiagnostics = true;

            DebugView = new DebugViewXNA(World);
            DebugView.RemoveFlags(DebugViewFlags.Shape);
            DebugView.RemoveFlags(DebugViewFlags.Joint);

            DebugView.DefaultShapeColor = Color.White;
            DebugView.SleepingShapeColor = Color.LightGray;

            DebugView.LoadContent(ScreenManager.GraphicsDevice, ScreenManager.ContentManager);
            Vector2 gameWorld = Camera2D.ConvertScreenToWorld(new Vector2(ScreenManager.Camera.ScreenWidth, ScreenManager.Camera.ScreenHeight));
            _border = new Border(World, gameWorld.X, gameWorld.Y, 1);

            ScreenManager.Camera.ProjectionUpdated += UpdateScreen;
            Camera2D.Effect.TextureEnabled = true;

            // Loading may take a while... so prevent the game from "catching up" once we finished loading
            ScreenManager.Game.ResetElapsedTime();
        }

        private void UpdateScreen()
        {
            if (World != null)
            {
                Vector2 gameWorld = Camera2D.ConvertScreenToWorld(new Vector2(ScreenManager.Camera.ScreenWidth, ScreenManager.Camera.ScreenHeight));
                _border.ResetBorder(gameWorld.X, gameWorld.Y);
            }
        }

        private void AddFixture(Fixture fixture)
        {
            if (fixture.ShapeType == ShapeType.Edge || fixture.ShapeType == ShapeType.Loop)
            {
                _lineFixtures.Add(fixture);
            }
            else if (fixture.ShapeType == ShapeType.Circle || fixture.ShapeType == ShapeType.Polygon)
            {
                if (fixture.UserData as DemoMaterial == null)
                {
                    fixture.UserData = new DemoMaterial(MaterialType.Blank);
                }
                DemoMaterial tempMat = (DemoMaterial)fixture.UserData;
                if (!_shapeFixtures.ContainsKey(tempMat.Type))
                {
                    _shapeFixtures[tempMat.Type] = new List<Fixture>();
                }
                _shapeFixtures[tempMat.Type].Add(fixture);
            }
        }

        private void RemoveFixture(Fixture fixture)
        {
            _lineFixtures.Remove(fixture);
            foreach (KeyValuePair<MaterialType, List<Fixture>> p in _shapeFixtures)
            {
                p.Value.Remove(fixture);
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!coveredByOtherScreen && !otherScreenHasFocus)
            {
                if (World != null)
                {
                    // variable time step but never less then 30 Hz
                    World.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f,
                                        (1f / 30f)));
                }
                MaterialManager.Update(gameTime);
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(InputHelper input)
        {
            //Xbox
            if (input.IsNewButtonPress(Buttons.X))
            {
                EnableOrDisableFlag(DebugViewFlags.Shape);
            }

            if (input.IsNewButtonPress(Buttons.Y))
            {
                EnableOrDisableFlag(DebugViewFlags.DebugPanel);
            }

            if (input.IsNewButtonPress(Buttons.B))
            {
                ExitScreen();
            }

            //Windows
            if (input.IsNewKeyPress(Keys.F1))
            {
                EnableOrDisableFlag(DebugViewFlags.Shape);
            }
            else if (input.IsNewKeyPress(Keys.F2))
            {
                EnableOrDisableFlag(DebugViewFlags.DebugPanel);
            }
            else if (input.IsNewKeyPress(Keys.F3))
            {
                EnableOrDisableFlag(DebugViewFlags.PerformanceGraph);
            }
            else if (input.IsNewKeyPress(Keys.F4))
            {
                EnableOrDisableFlag(DebugViewFlags.AABB);
            }
            else if (input.IsNewKeyPress(Keys.F5))
            {
                EnableOrDisableFlag(DebugViewFlags.CenterOfMass);
            }
            else if (input.IsNewKeyPress(Keys.F6))
            {
                EnableOrDisableFlag(DebugViewFlags.Joint);
            }
            else if (input.IsNewKeyPress(Keys.F7))
            {
                EnableOrDisableFlag(DebugViewFlags.ContactPoints);
                EnableOrDisableFlag(DebugViewFlags.ContactNormals);
            }
            else if (input.IsNewKeyPress(Keys.F8))
            {
                EnableOrDisableFlag(DebugViewFlags.PolygonPoints);
            }
        
            if (input.IsNewKeyPress(Keys.Escape))
            {
                ExitScreen();
            }

            if (World != null)
            {
#if !XBOX
                Mouse(input);
#else
                GamePad(input.CurrentGamePadState, input.LastGamePadState);
#endif
            }

            base.HandleInput(input);
        }

        private void EnableOrDisableFlag(DebugViewFlags flag)
        {
            if ((DebugView.Flags & flag) == flag)
                DebugView.RemoveFlags(flag);
            else
                DebugView.AppendFlags(flag);
        }

#if !XBOX
        private void Mouse(InputHelper state)
        {
            Vector2 position = Camera2D.ConvertScreenToWorld(state.MousePosition);

            if (state.IsOldButtonPress(MouseButtons.LeftButton))
            {
                MouseUp();
            }
            else if (state.IsNewButtonPress(MouseButtons.LeftButton))
            {
                MouseDown(position);
            }

            if (_fixedMouseJoint != null)
            {
                _fixedMouseJoint.WorldAnchorB = position;
            }
        }
#else
        private void GamePad(GamePadState state, GamePadState oldState)
        {
            Vector3 worldPosition = ScreenManager.GraphicsDevice.Viewport.Unproject(new Vector3(state.ThumbSticks.Left.X, state.ThumbSticks.Left.Y, 0),
                                                                                    Projection, View, Matrix.Identity);
            Vector2 position = new Vector2(worldPosition.X, worldPosition.Y);

            if (state.Buttons.A == ButtonState.Released && oldState.Buttons.A == ButtonState.Pressed)
            {
                MouseUp();
            }
            else if (state.Buttons.A == ButtonState.Pressed && oldState.Buttons.A == ButtonState.Released)
            {
                MouseDown(position);
            }

            GamePadMove(position);
        }

        private void GamePadMove(Vector2 p)
        {
            if (_fixedMouseJoint != null)
            {
                _fixedMouseJoint.Target = p;
            }
        }
#endif

        private void MouseDown(Vector2 p)
        {
            if (_fixedMouseJoint != null)
            {
                return;
            }

            Fixture savedFixture = World.TestPoint(p);

            if (savedFixture != null)
            {
                Body body = savedFixture.Body;
                _fixedMouseJoint = new FixedMouseJoint(body, p);
                _fixedMouseJoint.MaxForce = 1000.0f * body.Mass;
                World.AddJoint(_fixedMouseJoint);
                body.Awake = true;
            }
        }

        private void MouseUp()
        {
            if (_fixedMouseJoint != null)
            {
                World.RemoveJoint(_fixedMouseJoint);
                _fixedMouseJoint = null;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            ScreenManager.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            _lineCount = 0;
            foreach (KeyValuePair<MaterialType, List<Fixture>> p in _shapeFixtures)
            {
                _fillCount = 0;
                foreach (Fixture f in p.Value)
                {
                    if (f.Shape.ShapeType == ShapeType.Circle)
                    {
                        DrawSolidCircle(f);
                    }
                    if (f.Shape.ShapeType == ShapeType.Polygon)
                    {
                        DrawSolidPolygon(f);
                    }
                }

                if (MaterialManager.GetMaterialWrap(p.Key))
                {
                    ScreenManager.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                }
                else
                {
                    ScreenManager.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                }
                Camera2D.Effect.Texture = MaterialManager.GetMaterialTexture(p.Key);
                Camera2D.Effect.Techniques[0].Passes[0].Apply();
                if (_fillCount > 0)
                {
                    ScreenManager.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _vertsFill, 0, _fillCount);
                }
            }
            foreach (Fixture f in _lineFixtures)
            {
                DrawLineShape(f);
            }
            Camera2D.Effect.Texture = _lineTexture;
            Camera2D.Effect.Techniques[0].Passes[0].Apply();
            if (_lineCount > 0)
            {
                ScreenManager.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _vertsLines, 0, _lineCount);
            }

            if (World != null)
            {
                Matrix projection = Camera2D.Effect.Projection;
                Matrix view = Camera2D.Effect.View;
                DebugView.RenderDebugData(ref projection, ref view);
            }

            base.Draw(gameTime);
        }

        private void DrawLine(Vector2 a, Vector2 b)
        {
            Vector2 tang = b - a;
            tang.Normalize();
            tang *= LineWidth / 2f;
            Vector2 norm = new Vector2(-tang.Y, tang.X);

            // define vertices
            VertexPositionColorTexture[] vertsLine = new VertexPositionColorTexture[8];
            vertsLine[0].Position = new Vector3(a - tang + norm, -1f);
            vertsLine[0].Color = Color.Black;
            vertsLine[0].TextureCoordinate = new Vector2(0f, .25f);
            vertsLine[1].Position = new Vector3(a - tang - norm, -1f);
            vertsLine[1].Color = Color.Black;
            vertsLine[1].TextureCoordinate = new Vector2(0f, .75f);
            vertsLine[2].Position = new Vector3(a - norm, -1f);
            vertsLine[2].Color = Color.Black;
            vertsLine[2].TextureCoordinate = new Vector2(.25f, .75f);
            vertsLine[3].Position = new Vector3(b - norm, -1f);
            vertsLine[3].Color = Color.Black;
            vertsLine[3].TextureCoordinate = new Vector2(.75f, .75f);
            vertsLine[4].Position = new Vector3(b + tang - norm, -1f);
            vertsLine[4].Color = Color.Black;
            vertsLine[4].TextureCoordinate = new Vector2(1f, .75f);
            vertsLine[5].Position = new Vector3(b + tang + norm, -1f);
            vertsLine[5].Color = Color.Black;
            vertsLine[5].TextureCoordinate = new Vector2(1f, .25f);
            vertsLine[6].Position = new Vector3(b + norm, -1f);
            vertsLine[6].Color = Color.Black;
            vertsLine[6].TextureCoordinate = new Vector2(.75f, .25f);
            vertsLine[7].Position = new Vector3(a + norm, -1f);
            vertsLine[7].Color = Color.Black;
            vertsLine[7].TextureCoordinate = new Vector2(.25f, .25f);

            // add triangles
            _vertsLines[_lineCount * 3] = vertsLine[0];
            _vertsLines[_lineCount * 3 + 1] = vertsLine[1];
            _vertsLines[_lineCount * 3 + 2] = vertsLine[7];
            _lineCount++;
            _vertsLines[_lineCount * 3] = vertsLine[1];
            _vertsLines[_lineCount * 3 + 1] = vertsLine[2];
            _vertsLines[_lineCount * 3 + 2] = vertsLine[7];
            _lineCount++;
            _vertsLines[_lineCount * 3] = vertsLine[7];
            _vertsLines[_lineCount * 3 + 1] = vertsLine[2];
            _vertsLines[_lineCount * 3 + 2] = vertsLine[6];
            _lineCount++;
            _vertsLines[_lineCount * 3] = vertsLine[2];
            _vertsLines[_lineCount * 3 + 1] = vertsLine[3];
            _vertsLines[_lineCount * 3 + 2] = vertsLine[6];
            _lineCount++;
            _vertsLines[_lineCount * 3] = vertsLine[6];
            _vertsLines[_lineCount * 3 + 1] = vertsLine[3];
            _vertsLines[_lineCount * 3 + 2] = vertsLine[5];
            _lineCount++;
            _vertsLines[_lineCount * 3] = vertsLine[3];
            _vertsLines[_lineCount * 3 + 1] = vertsLine[4];
            _vertsLines[_lineCount * 3 + 2] = vertsLine[5];
            _lineCount++;
        }

        private void DrawLineShape(Fixture fixture)
        {
            Transform xf;
            fixture.Body.GetTransform(out xf);
            if (fixture.ShapeType == ShapeType.Edge)
            {
                EdgeShape edge = (EdgeShape)fixture.Shape;
                DrawLine(MathUtils.Multiply(ref xf, edge.Vertex1), MathUtils.Multiply(ref xf, edge.Vertex2));
            }
            else if (fixture.ShapeType == ShapeType.Loop)
            {
                Vertices loopVerts = ((LoopShape)fixture.Shape).Vertices;
                for (int i = 0; i < loopVerts.Count - 1; ++i)
                {
                    DrawLine(MathUtils.Multiply(ref xf, loopVerts[i]), MathUtils.Multiply(ref xf, loopVerts[i + 1]));
                }
                DrawLine(MathUtils.Multiply(ref xf, loopVerts[loopVerts.Count - 1]), MathUtils.Multiply(ref xf, loopVerts[0]));
            }
        }

        private void DrawSolidPolygon(Fixture fixture)
        {
            Transform xf;
            fixture.Body.GetTransform(out xf);
            PolygonShape poly = (PolygonShape)fixture.Shape;
            DemoMaterial material = (DemoMaterial)fixture.UserData;
            int count = poly.Vertices.Count;

            if (count == 2)
            {
                return;
            }

            Color colorFill = material.Color1;
            Color colorHighlight = material.Color2;
            float depth = material.Depth;
            Vector2 texCoordCenter = new Vector2(.5f, .5f);
            if (material.CenterOnBody)
            {
                texCoordCenter += poly.Vertices[0] / material.Scale;
            }

            Vector2 v0 = MathUtils.Multiply(ref xf, poly.Vertices[0]);
            for (int i = 1; i < count - 1; i++)
            {
                Vector2 v1 = MathUtils.Multiply(ref xf, poly.Vertices[i]);
                Vector2 v2 = MathUtils.Multiply(ref xf, poly.Vertices[i + 1]);

                _vertsFill[_fillCount * 3].Position = new Vector3(v0, depth);
                _vertsFill[_fillCount * 3].Color = colorHighlight;
                _vertsFill[_fillCount * 3].TextureCoordinate = texCoordCenter;
                _vertsFill[_fillCount * 3].TextureCoordinate.Y = 1 - _vertsFill[_fillCount * 3].TextureCoordinate.Y;

                _vertsFill[_fillCount * 3 + 1].Position = new Vector3(v1, depth);
                _vertsFill[_fillCount * 3 + 1].Color = colorFill;
                _vertsFill[_fillCount * 3 + 1].TextureCoordinate = texCoordCenter + (poly.Vertices[i] - poly.Vertices[0]) / material.Scale;
                _vertsFill[_fillCount * 3 + 1].TextureCoordinate.Y = 1 - _vertsFill[_fillCount * 3 + 1].TextureCoordinate.Y;

                _vertsFill[_fillCount * 3 + 2].Position = new Vector3(v2, depth);
                _vertsFill[_fillCount * 3 + 2].Color = colorFill;
                _vertsFill[_fillCount * 3 + 2].TextureCoordinate = texCoordCenter + (poly.Vertices[i + 1] - poly.Vertices[0]) / material.Scale;
                _vertsFill[_fillCount * 3 + 2].TextureCoordinate.Y = 1 - _vertsFill[_fillCount * 3 + 2].TextureCoordinate.Y;

                _fillCount++;

                // outline
                DrawLine(v1, v2);
            }
            DrawLine(v0, MathUtils.Multiply(ref xf, poly.Vertices[1]));
            DrawLine(MathUtils.Multiply(ref xf, poly.Vertices[count - 1]), v0);
        }

        private void DrawSolidCircle(Fixture fixture)
        {
            Transform xf;
            fixture.Body.GetTransform(out xf);
            CircleShape circle = (CircleShape)fixture.Shape;
            DemoMaterial material = (DemoMaterial)fixture.UserData;
            Vector2 center = circle.Position;
            float radius = circle.Radius;

            const int segments = 32;
            const double increment = Math.PI * 2.0 / segments;
            double theta = increment;

            Color colorFill = material.Color1;
            Color colorHighlight = material.Color2;
            float depth = material.Depth;
            Vector2 v0 = center + radius * Vector2.UnitX;
            Vector2 texCoordCenter = new Vector2(.5f, .5f) + Vector2.UnitX * radius / material.Scale;
            if (material.CenterOnBody)
            {
                texCoordCenter += center / material.Scale;
            }

            for (int i = 1; i < segments - 1; i++)
            {
                Vector2 v1 = center + radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                Vector2 v2 = center + radius * new Vector2((float)Math.Cos(theta + increment), (float)Math.Sin(theta + increment));

                _vertsFill[_fillCount * 3].Position = new Vector3(MathUtils.Multiply(ref xf, v0), depth);
                _vertsFill[_fillCount * 3].Color = colorHighlight;
                _vertsFill[_fillCount * 3].TextureCoordinate = texCoordCenter;
                _vertsFill[_fillCount * 3].TextureCoordinate.Y = 1 - _vertsFill[_fillCount * 3].TextureCoordinate.Y;

                _vertsFill[_fillCount * 3 + 1].Position = new Vector3(MathUtils.Multiply(ref xf, v1), depth);
                _vertsFill[_fillCount * 3 + 1].Color = colorFill;
                _vertsFill[_fillCount * 3 + 1].TextureCoordinate = texCoordCenter + (v1 - v0) / material.Scale;
                _vertsFill[_fillCount * 3 + 1].TextureCoordinate.Y = 1 - _vertsFill[_fillCount * 3 + 1].TextureCoordinate.Y;

                _vertsFill[_fillCount * 3 + 2].Position = new Vector3(MathUtils.Multiply(ref xf, v2), depth);
                _vertsFill[_fillCount * 3 + 2].Color = colorFill;
                _vertsFill[_fillCount * 3 + 2].TextureCoordinate = texCoordCenter + (v2 - v0) / material.Scale;
                _vertsFill[_fillCount * 3 + 2].TextureCoordinate.Y = 1 - _vertsFill[_fillCount * 3 + 2].TextureCoordinate.Y;

                _fillCount++;
                theta += increment;

                // outline
                DrawLine(MathUtils.Multiply(ref xf, v1), MathUtils.Multiply(ref xf, v2));
                if (i == 1)
                {
                    DrawLine(MathUtils.Multiply(ref xf, v0), MathUtils.Multiply(ref xf, v1));
                }
                if (i == segments - 2)
                {
                    DrawLine(MathUtils.Multiply(ref xf, v2), MathUtils.Multiply(ref xf, v0));
                }
            }
        }
    }
}
