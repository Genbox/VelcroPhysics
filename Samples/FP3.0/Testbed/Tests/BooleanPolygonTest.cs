using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class BooleanPolygonTest : Test
    {
        private Vertices _left;
        private List<TextMessage> _messages;
        private Vertices _right;
        private Vertices _selectedVertice;

        public override void Initialize()
        {
            _messages = new List<TextMessage>();

            _left = PolygonTools.CreateGear(5, 10, 40, 5);

            /*_left.Add(new Vector2(0.5f, 0.5f));
            _left.Add(new Vector2(2, -2));
            _left.Add(new Vector2(2, 2));
            _left.Add(new Vector2(-2, 2));*/

            base.Initialize();
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            //If the message times out, remove it from the list.
            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                _messages[i].ElapsedTime += settings.Hz;
                if (_messages[i].ElapsedTime > 5)
                {
                    _messages.Remove(_messages[i]);
                }
            }

            if (_left != null)
            {
                Vector2[] array = _left.ToArray();
                DebugView.DrawPolygon(ref array, _left.Count, Color.Red);
            }

            if (_right != null)
            {
                Vector2[] array = _right.ToArray();
                DebugView.DrawPolygon(ref array, _right.Count, Color.Red);
            }

            DebugView.DrawString(50, TextLine, "A,S,D = Create Rectangle");
            TextLine += 15;

            DebugView.DrawString(50, TextLine, "Q,W,E = Create Circle");
            TextLine += 15;

            DebugView.DrawString(50, TextLine, "Click to Drag polygons");
            TextLine += 15;

            DebugView.DrawString(50, TextLine, "Space = Union");
            TextLine += 15;

            DebugView.DrawString(50, TextLine, "Backspace = Subtract");
            TextLine += 15;

            DebugView.DrawString(50, TextLine, "Space = Union");
            TextLine += 15;

            DebugView.DrawString(50, TextLine, "Enter = Add to Simulation");
            TextLine += 15;

            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                DebugView.DrawString(50, TextLine, _messages[i].Text);
                TextLine += 15;
            }

            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (_left == null || _right == null)
            {
                // Add Circles
                if (state.IsKeyDown(Keys.Q) && oldState.IsKeyUp(Keys.Q))
                {
                    AddCircle(3, 8);
                }

                // Add Circles
                if (state.IsKeyDown(Keys.W) && oldState.IsKeyUp(Keys.W))
                {
                    AddCircle(3, 16);
                }

                // Add Circles
                if (state.IsKeyDown(Keys.E) && oldState.IsKeyUp(Keys.E))
                {
                    AddCircle(3, 32);
                }

                // Add Rectangle
                if (state.IsKeyDown(Keys.A) && oldState.IsKeyUp(Keys.A))
                {
                    AddRectangle(5, 5);
                }

                // Add Rectangle
                if (state.IsKeyDown(Keys.S) && oldState.IsKeyUp(Keys.S))
                {
                    AddRectangle(5, 2);
                }

                // Add Rectangle
                if (state.IsKeyDown(Keys.D) && oldState.IsKeyUp(Keys.D))
                {
                    AddRectangle(2, 5);
                }
            }
            else
            {
                WriteMessage("Only 2 polygons allowed at a time.");
            }

            // Perform a Union
            if (state.IsKeyDown(Keys.Space) && oldState.IsKeyUp(Keys.Space))
            {
                if (_left != null && _right != null)
                {
                    DoUnion();
                }
            }

            // Perform a Subtraction
            if (state.IsKeyDown(Keys.Back) && oldState.IsKeyUp(Keys.Back))
            {
                if (_left != null && _right != null)
                {
                    DoSubtract();
                }
            }

            // Add to Simulation
            if (state.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter))
            {
                if (_left != null)
                {
                }
            }

            base.Keyboard(state, oldState);
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {

            Vector2 position = GameInstance.ConvertScreenToWorld(state.X, state.Y);

            if (state.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
            {
                AABB aabbM, aabbL, aabbR;

                Vector2 d = new Vector2(0.001f, 0.001f);

                aabbM.LowerBound = position - d;
                aabbM.UpperBound = position + d;

                if (_left != null)
                {
                    Vector2 centerL = _left.GetCentroid();
                    float leftD = _left.GetRadius();
                    Vector2 lD = new Vector2(leftD, leftD);

                    aabbL.LowerBound = centerL - lD;
                    aabbL.UpperBound = centerL + lD;

                    //View the area that is selectable.
                    DebugView.DrawCircle(centerL, leftD, new Color(0.0f, 1.0f, 0.0f));

                    if (AABB.TestOverlap(ref aabbM, ref aabbL))
                    {
                        _selectedVertice = _left;
                    }
                }

                if (_right != null)
                {
                    Vector2 centerR = _right.GetCentroid();
                    float rightD = _right.GetRadius();
                    Vector2 rD = new Vector2(rightD, rightD);

                    aabbR.LowerBound = centerR - rD;
                    aabbR.UpperBound = centerR + rD;

                    //View the area that is selectable.
                    DebugView.DrawCircle(centerR, rightD, new Color(0.0f, 1.0f, 0.0f));

                    if (AABB.TestOverlap(ref aabbM, ref aabbR))
                    {
                        _selectedVertice = _right;
                    }
                }


            }

            if (state.LeftButton == ButtonState.Released && oldState.LeftButton == ButtonState.Pressed)
            {
                _selectedVertice = null;
            }

            MouseMove(state, oldState, position);
            base.Mouse(state, oldState);
        }

        private void MouseMove(MouseState state, MouseState oldState, Vector2 mousePos)
        {
            if (_selectedVertice != null)
            {
                for (int i = 0; i < _selectedVertice.Count; i++)
                {
                    _selectedVertice[i] = new Vector2(
                        _selectedVertice[i].X + ((float)(state.X - oldState.X) / 12.0f),
                        _selectedVertice[i].Y + ((float)(oldState.Y - state.Y) / 12.0f));
                }
            }
        }

        private void DoUnion()
        {
            // Do the union
            PolyUnionError error;
            Vertices vertices = BooleanTools.Union(_left, _right, out error);

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

            SetResult(vertices);
        }

        private void DoSubtract()
        {
            // Do the subtraction.
            PolyUnionError error;
            Vertices subtract = BooleanTools.Subtract(_left, _right, out error);

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

            SetResult(subtract);
        }

        private void AddCircle(int radius, int numSides)
        {
            Vertices verts = PolygonTools.CreateCircle(radius, numSides);
            SetVertices(verts);
        }

        private void AddRectangle(int width, int height)
        {
            Vertices verts = PolygonTools.CreateRectangle(width, height);
            SetVertices(verts);
        }

        private void SetVertices(Vertices vertices)
        {
            if (_left == null)
            {
                _left = vertices;
            }
            else if (_right == null)
            {
                _right = vertices;
            }
        }

        private void SetResult(Vertices vertices)
        {
            _left = vertices;
            _right = null;
        }

        private void WriteMessage(string message)
        {
            _messages.Add(new TextMessage(message));
        }

        public static Test Create()
        {
            return new BooleanPolygonTest();
        }

        #region Nested type: TextMessage

        private class TextMessage
        {
            public float ElapsedTime;
            public string Text;

            public TextMessage(string text)
            {
                Text = text;
                ElapsedTime = 0;
            }
        }

        #endregion
    }
}