using System.Collections.Generic;
using System.Text;
using DemoBaseXNA;
using DemoBaseXNA.DrawingSystem;
using DemoBaseXNA.ScreenSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.AdvancedSamplesXNA.Demo7
{
    public class Demo7Screen : GameScreen
    {
        private List<TextMessage> _messages;
        private Geom _leftGeom;
        private Geom _rightGeom;
        private Geom _selectedGeom;
        private PolygonBrush _leftPolyBrush;
        private PolygonBrush _rightPolyBrush;
        private List<Body> _simulatedPolyBodies;
        private List<PolygonBrush> _simulatedPolyBrushes;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 150));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);
            PhysicsSimulatorView.EnableVerticeView = true;
            PhysicsSimulatorView.EnableEdgeView = false;
            PhysicsSimulatorView.EnableContactView = false;
            PhysicsSimulatorView.EnableAABBView = false;
            PhysicsSimulatorView.EnablePerformancePanelView = false;
            PhysicsSimulatorView.EnableCoordinateAxisView = false;
            PhysicsSimulatorView.EdgeColor = Color.Red;
            PhysicsSimulatorView.EdgeLineThickness = 2;
            PhysicsSimulatorView.VerticeColor = Color.CornflowerBlue;
            PhysicsSimulatorView.VerticeRadius = 5;
            DebugViewEnabled = true;

            _messages = new List<TextMessage>();
            _simulatedPolyBrushes = new List<PolygonBrush>();
            _simulatedPolyBodies = new List<Body>();

            base.Initialize();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            //If the message times out, remove it from the list.
            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                _messages[i].ElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_messages[i].ElapsedTime > 5)
                {
                    _messages.Remove(_messages[i]);
                }
            }

            if (_leftGeom != null)
                _leftGeom.CollisionEnabled = false;

            if (_rightGeom != null)
                _rightGeom.CollisionEnabled = false;

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            if (_leftGeom != null)
                _leftPolyBrush.Draw(_leftGeom.Position, _leftGeom.Rotation);

            if (_rightGeom != null)
                _rightPolyBrush.Draw(_rightGeom.Position, _rightGeom.Rotation);

            for (int i = 0; i < _simulatedPolyBodies.Count; i++)
            {
                _simulatedPolyBrushes[i].Draw(_simulatedPolyBodies[i].Position, _simulatedPolyBodies[i].Rotation);
            }

            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DetailsFont, _messages[i].Text,
                                                     new Vector2(50, 100 + (_messages.Count - 1 - i) * ScreenManager.SpriteFonts.DetailsFont.LineSpacing), Color.White);
            }

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DetailsFont, "A,S,D = Create Rectangle",
                                                 new Vector2(50, ScreenManager.ScreenHeight - 50 - ScreenManager.SpriteFonts.DetailsFont.LineSpacing * 7), Color.White);

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DetailsFont, "Q,W,E = Create Circle",
                                                 new Vector2(50, ScreenManager.ScreenHeight - 50 - ScreenManager.SpriteFonts.DetailsFont.LineSpacing * 6), Color.White);

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DetailsFont, "Click to Drag polygons",
                                                 new Vector2(50, ScreenManager.ScreenHeight - 50 - ScreenManager.SpriteFonts.DetailsFont.LineSpacing * 5), Color.White);

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DetailsFont, "Space = Union",
                                                 new Vector2(50, ScreenManager.ScreenHeight - 50 - ScreenManager.SpriteFonts.DetailsFont.LineSpacing * 4), Color.White);

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DetailsFont, "Backspace = Subtract",
                                                 new Vector2(50, ScreenManager.ScreenHeight - 50 - ScreenManager.SpriteFonts.DetailsFont.LineSpacing * 3), Color.White);

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DetailsFont, "Tab = Simplify",
                                                 new Vector2(50, ScreenManager.ScreenHeight - 50 - ScreenManager.SpriteFonts.DetailsFont.LineSpacing * 2), Color.White);

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DetailsFont, "Enter = Add to Simulation",
                                                 new Vector2(50, ScreenManager.ScreenHeight - 50 - ScreenManager.SpriteFonts.DetailsFont.LineSpacing), Color.White);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public override void HandleInput(InputState input)
        {
            if (firstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
                firstRun = false;
            }
            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
            }
            else
            {
                HandleMouseInput(input);
            }

            HandKeyboardInput(input);
            base.HandleInput(input);
        }

        private void HandKeyboardInput(InputState input)
        {
            if (input.OneOfKeysPressed(Keys.Q, Keys.W, Keys.E, Keys.A, Keys.S, Keys.D))
                if (_leftGeom == null || _rightGeom == null)
                {
                    // Add Circles
                    if (input.IsNewKeyPress(Keys.Q))
                    {
                        AddCircle(50, 8);
                    }

                    // Add Circles
                    if (input.IsNewKeyPress(Keys.W))
                    {
                        AddCircle(50, 16);
                    }

                    // Add Circles
                    if (input.IsNewKeyPress(Keys.E))
                    {
                        AddCircle(50, 32);
                    }

                    // Add Rectangle
                    if (input.IsNewKeyPress(Keys.A))
                    {
                        AddRectangle(100, 100);
                    }

                    // Add Rectangle
                    if (input.IsNewKeyPress(Keys.S))
                    {
                        AddRectangle(100, 50);
                    }

                    // Add Rectangle
                    if (input.IsNewKeyPress(Keys.D))
                    {
                        AddRectangle(50, 100);
                    }
                }
                else
                {
                    WriteMessage("Only 2 polygons allowed at a time.");
                }

            // Perform a Union
            if (input.IsNewKeyPress(Keys.Space))
            {
                if (_leftGeom != null && _rightGeom != null)
                {
                    DoUnion();
                }
            }

            // Perform a Subtraction
            if (input.IsNewKeyPress(Keys.Back))
            {
                if (_leftGeom != null && _rightGeom != null)
                {
                    DoSubtract();
                }
            }

            // Simplify
            if (input.IsNewKeyPress(Keys.Tab))
            {
                if (_leftGeom != null && _rightGeom == null)
                {
                    Vertices simple = new Vertices(_leftGeom.WorldVertices);
                    simple = Vertices.Simplify(simple);

                    SetProduct(simple);
                }
            }

            // Add to Simulation
            if (input.IsNewKeyPress(Keys.Enter))
            {
                if (_leftGeom != null)
                {
                    Body body = BodyFactory.Instance.CreatePolygonBody(_leftGeom.LocalVertices, 1.0f);
                    body.Position = _leftGeom.Position;

                    Geom geom = GeomFactory.Instance.CreatePolygonGeom(body, _leftGeom.LocalVertices, 0);

                    PhysicsSimulator.Add(body);
                    PhysicsSimulator.Add(geom);

                    _simulatedPolyBrushes.Add(new PolygonBrush(_leftGeom.LocalVertices, Color.Red, Color.DarkRed, 1.5f, 0.2f));
                    _simulatedPolyBrushes[_simulatedPolyBrushes.Count - 1].Load(ScreenManager.GraphicsDevice);
                    _simulatedPolyBodies.Add(body);

                }
            }
        }

        private void HandleMouseInput(InputState input)
        {
            if (input.CurrentMouseState.LeftButton == ButtonState.Pressed && input.LastMouseState.LeftButton == ButtonState.Released)
            {
                foreach (Geom g in PhysicsSimulator.GeomList)
                {
                    if (g.Collide(new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                    {
                        _selectedGeom = g;
                        break;
                    }
                }
            }

            if (input.CurrentMouseState.LeftButton == ButtonState.Released && input.LastMouseState.LeftButton == ButtonState.Pressed)
            {
                _selectedGeom = null;
            }

            MouseMove(input.LastMouseState, input.CurrentMouseState);
        }

        private void MouseMove(MouseState oldMouseState, MouseState newMouseState)
        {
            if (_selectedGeom != null)
            {
                _selectedGeom.Body.Position = new Vector2(
                    _selectedGeom.Body.Position.X + (newMouseState.X - oldMouseState.X),
                    _selectedGeom.Body.Position.Y + (newMouseState.Y - oldMouseState.Y));
            }
        }

        private void DoUnion()
        {
            // Get the world coordinates for the left Geometry
            Vertices poly1 = new Vertices(_leftGeom.WorldVertices);

            // Get the world coordinates for the right Geometry
            Vertices poly2 = new Vertices(_rightGeom.WorldVertices);

            // Do the union
            PolyUnionError error;
            Vertices union = Vertices.Union(poly1, poly2, out error);

            // Check for errors.
            switch (error)
            {
                case PolyUnionError.NoIntersections:
                    WriteMessage("ERROR: Polygons do not intersect!");
                    return;
                case PolyUnionError.Poly1InsidePoly2:
                    WriteMessage("Polygon 1 completely inside polygon 2.");
                    return;
                case PolyUnionError.InfiniteLoop:
                    WriteMessage("Infinite Loop detected.");
                    break;
                case PolyUnionError.None:
                    WriteMessage("No errors with union.");
                    break;
            }

            // No errors, set the product of the union.
            SetProduct(union);
        }

        private void DoSubtract()
        {
            // Get the world coordinates for the left Geometry
            Vertices poly1 = new Vertices(_leftGeom.WorldVertices);

            // Get the world coordinates for the right Geometry
            Vertices poly2 = new Vertices(_rightGeom.WorldVertices);

            // Do the subtraction.
            PolyUnionError error;
            Vertices subtract = Vertices.Subtract(poly1, poly2, out error);

            // Check for errors
            switch (error)
            {
                case PolyUnionError.NoIntersections:
                    WriteMessage("ERROR: Polygons do not intersect!");
                    return;

                case PolyUnionError.Poly1InsidePoly2:
                    WriteMessage("Polygon 1 completely inside polygon 2.");
                    return;

                case PolyUnionError.InfiniteLoop:
                    WriteMessage("Infinite Loop detected.");
                    break;

                case PolyUnionError.None:
                    WriteMessage("No errors with subtraction.");
                    break;
            }

            // No errors, set the product of the union.
            SetProduct(subtract);
        }

        private void SetProduct(Vertices product)
        {
            if (product == null)
                return;

            if (_rightGeom != null)
            {
                PhysicsSimulator.Remove(_rightGeom);
                PhysicsSimulator.Remove(_rightGeom.Body);
            }

            if (_leftGeom != null)
            {
                PhysicsSimulator.Remove(_leftGeom);
                PhysicsSimulator.Remove(_leftGeom.Body);
            }

            _rightGeom = null;
            _leftGeom = null;
            _leftPolyBrush = null;
            _rightPolyBrush = null;

            Body body = BodyFactory.Instance.CreatePolygonBody(PhysicsSimulator, product, 1);
            body.IsStatic = true;

            Geom geom = GeomFactory.Instance.CreatePolygonGeom(PhysicsSimulator, body, product, 0);

            _leftGeom = geom;
            _leftPolyBrush = new PolygonBrush(product, Color.White, Color.Black, 1.5f, 1);
            _leftPolyBrush.Load(ScreenManager.GraphicsDevice);
        }

        private void AddCircle(int radius, int numSides)
        {
            Vertices verts = Vertices.CreateCircle(radius, numSides);
            Body body = BodyFactory.Instance.CreateCircleBody(PhysicsSimulator, radius, 1.0f);
            body.Position = ScreenManager.ScreenCenter;
            body.IsStatic = true;
            Geom geom = GeomFactory.Instance.CreatePolygonGeom(PhysicsSimulator, body, verts, 0);

            SetGeom(geom, verts);
        }

        private void AddRectangle(int width, int height)
        {
            Vertices verts = Vertices.CreateRectangle(width, height);
            Body body = BodyFactory.Instance.CreateRectangleBody(PhysicsSimulator, width, height, 1.0f);
            body.Position = ScreenManager.ScreenCenter;
            body.IsStatic = true;
            Geom geom = GeomFactory.Instance.CreatePolygonGeom(PhysicsSimulator, body, verts, 0);

            SetGeom(geom, verts);
        }

        private void SetGeom(Geom geom, Vertices vertices)
        {
            if (_leftGeom == null)
            {
                _leftGeom = geom;
                _leftPolyBrush = new PolygonBrush(vertices, Color.White, Color.Black, 1.5f, 1);
                _leftPolyBrush.Load(ScreenManager.GraphicsDevice);
            }
            else if (_rightGeom == null)
            {
                _rightGeom = geom;
                _rightPolyBrush = new PolygonBrush(vertices, Color.White, Color.Black, 1.5f, 1);
                _rightPolyBrush.Load(ScreenManager.GraphicsDevice);
            }
        }

        private void WriteMessage(string message)
        {
            _messages.Add(new TextMessage(message));
        }

        public static string GetTitle()
        {
            return "Demo7: Polygon subtraction";
        }

        private static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Shows how you can use the");
            sb.AppendLine("powerful vertices modification");
            sb.AppendLine("methods to subtract and add polygons");
            sb.AppendLine("on the fly.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse:");
            sb.AppendLine("Move the polygons by holding down the left");
            sb.AppendLine("mouse button.");
            return sb.ToString();
        }
    }
}