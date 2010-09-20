using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class YuPengPolygonTest : Test
    {
        private Vertices _clip;
        private PolyClipError _err;
        private List<Vertices> _polygons;
        private Vertices _selected;
        private Vertices _subject;

        public override void Initialize()
        {
            Vector2 trans = new Vector2();
            _polygons = new List<Vertices>();

            _polygons.Add(PolygonTools.CreateGear(5f, 10, 0f, 6f));
            _polygons.Add(PolygonTools.CreateGear(4f, 15, 100f, 3f));

            trans.X = 0f;
            trans.Y = 8f;
            _polygons[0].Translate(ref trans);
            _polygons[1].Translate(ref trans);

            _polygons.Add(PolygonTools.CreateGear(5f, 10, 50f, 5f));

            trans.X = 22f;
            trans.Y = 17f;
            _polygons[2].Translate(ref trans);

            AddRectangle(5, 10);
            AddCircle(5, 32);

            trans.X = -20f;
            trans.Y = 8f;
            _polygons[3].Translate(ref trans);
            trans.Y = 20f;
            _polygons[4].Translate(ref trans);

            _subject = _polygons[0];
            _clip = _polygons[1];

            base.Initialize();

            //Removing debugpanel - this is a tools only simulation
            GameInstance.DebugViewEnabled = false;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            for (int i = 0; i < _polygons.Count; ++i)
            {
                if (_polygons[i] != null)
                {
                    Vector2[] array = _polygons[i].ToArray();
                    Color col = Color.SteelBlue;
                    if (!_polygons[i].IsCounterClockWise())
                    {
                        col = Color.Aquamarine;
                    }
                    if (_polygons[i] == _selected)
                    {
                        col = Color.LightBlue;
                    }
                    if (_polygons[i] == _subject)
                    {
                        col = Color.Green;
                        if (_polygons[i] == _selected)
                        {
                            col = Color.LightGreen;
                        }
                    }
                    if (_polygons[i] == _clip)
                    {
                        col = Color.DarkRed;
                        if (_polygons[i] == _selected)
                        {
                            col = Color.IndianRed;
                        }
                    }
                    DebugView.DrawPolygon(array, _polygons[i].Count, col);
                    for (int j = 0; j < _polygons[i].Count; ++j)
                    {
                        DebugView.DrawPoint(_polygons[i][j], .2f, Color.Red);
                    }
                }
            }

            DebugView.DrawString(500, TextLine, "A,S,D = Create Rectangle");
            TextLine += 15;

            DebugView.DrawString(500, TextLine, "Q,W,E = Create Circle");
            TextLine += 15;

            DebugView.DrawString(500, TextLine, "Click to Drag polygons");
            TextLine += 15;

            DebugView.DrawString(500, TextLine, "1 = Select Subject while dragging [green]");
            TextLine += 15;

            DebugView.DrawString(500, TextLine, "2 = Select Clip while dragging [red]");
            TextLine += 15;

            DebugView.DrawString(500, TextLine, "Space = Union");
            TextLine += 15;

            DebugView.DrawString(500, TextLine, "Backspace = Subtract");
            TextLine += 15;

            DebugView.DrawString(500, TextLine, "Shift = Intersect");
            TextLine += 15;

            DebugView.DrawString(500, TextLine, "Holes are colored light blue");
            TextLine += 15;

            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            // Add Circles
            if (keyboardManager.IsNewKeyPress(Keys.Q))
            {
                AddCircle(3, 8);
            }

            // Add Circles
            if (keyboardManager.IsNewKeyPress(Keys.W))
            {
                AddCircle(4, 16);
            }

            // Add Circles
            if (keyboardManager.IsNewKeyPress(Keys.E))
            {
                AddCircle(5, 32);
            }

            // Add Rectangle
            if (keyboardManager.IsNewKeyPress(Keys.A))
            {
                AddRectangle(4, 8);
            }

            // Add Rectangle
            if (keyboardManager.IsNewKeyPress(Keys.S))
            {
                AddRectangle(5, 2);
            }

            // Add Rectangle
            if (keyboardManager.IsNewKeyPress(Keys.D))
            {
                AddRectangle(2, 5);
            }

            // Perform a Union
            if (keyboardManager.IsNewKeyPress(Keys.Space))
            {

                    if (_subject != null && _clip != null)
                    {
                        DoBooleanOperation(YuPengClipper.Union(_subject, _clip, out _err));
                    }

            }

            // Perform a Subtraction
            if (keyboardManager.IsNewKeyPress(Keys.Back))
            {

                    if (_subject != null && _clip != null)
                    {
                        DoBooleanOperation(YuPengClipper.Difference(_subject, _clip, out _err));
                    }

            }

            // Perform a Intersection
            if (keyboardManager.IsNewKeyPress(Keys.LeftShift))
            {

                    if (_subject != null && _clip != null)
                    {
                        DoBooleanOperation(YuPengClipper.Intersect(_subject, _clip, out _err));
                    }

            }

            // Select Subject
            if (keyboardManager.IsNewKeyPress(Keys.D1))
            {
                if (_selected != null)
                {
                    if (_clip == _selected)
                    {
                        _clip = null;
                    }
                    _subject = _selected;
                }
            }

            // Select Clip
            if (keyboardManager.IsNewKeyPress(Keys.D2))
            {
                if (_selected != null)
                {
                    if (_subject == _selected)
                    {
                        _subject = null;
                    }
                    _clip = _selected;
                }
            }
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            Vector2 position = GameInstance.ConvertScreenToWorld(state.X, state.Y);

            if (state.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < _polygons.Count; ++i)
                {
                    if (_polygons[i] != null)
                    {
                        if (_polygons[i].PointInPolygon(ref position))
                        {
                            _selected = _polygons[i];
                            break;
                        }
                    }
                }
            }

            if (state.LeftButton == ButtonState.Released && oldState.LeftButton == ButtonState.Pressed)
            {
                _selected = null;
            }

            MouseMove(state, oldState);
            base.Mouse(state, oldState);
        }

        private void MouseMove(MouseState state, MouseState oldState)
        {
            if (_selected != null)
            {
                Vector2 trans = new Vector2((state.X - oldState.X) / 12f,
                                            (oldState.Y - state.Y) / 12f);
                _selected.Translate(ref trans);
            }
        }

        private void DoBooleanOperation(List<Vertices> result)
        {
            // Do the union
            _polygons.Remove(_subject);
            _polygons.Remove(_clip);
            _polygons.AddRange(result);
            _subject = null;
            _clip = null;
            _selected = null;
        }

        public Vertices CreateRectangle(float width, float height)
        {
            //Note: The rectangle has vertices along the edges. This is to support the distance grid better.
            Vertices vertices = new Vertices();
            vertices.Add(new Vector2(-width * .5f, -height * .5f));
            vertices.Add(new Vector2(-width * .5f, -height * .25f));
            vertices.Add(new Vector2(-width * .5f, 0));
            vertices.Add(new Vector2(-width * .5f, height * .25f));
            vertices.Add(new Vector2(-width * .5f, height * .5f));
            vertices.Add(new Vector2(-width * .25f, height * .5f));
            vertices.Add(new Vector2(0, height * .5f));
            vertices.Add(new Vector2(width * .25f, height * .5f));
            vertices.Add(new Vector2(width * .5f, height * .5f));
            vertices.Add(new Vector2(width * .5f, height * .25f));
            vertices.Add(new Vector2(width * .5f, 0));
            vertices.Add(new Vector2(width * .5f, -height * .25f));
            vertices.Add(new Vector2(width * .5f, -height * .5f));
            vertices.Add(new Vector2(width * .25f, -height * .5f));
            vertices.Add(new Vector2(0, -height * .5f));
            vertices.Add(new Vector2(-width * .25f, -height * .5f));

            return vertices;
        }

        private void AddCircle(int radius, int numSides)
        {
            Vertices verts = PolygonTools.CreateCircle(radius, numSides);
            _polygons.Add(verts);
        }

        private void AddRectangle(int width, int height)
        {
            Vertices verts = CreateRectangle(width, height);//PolygonTools.CreateRectangle(width, height);
            _polygons.Add(verts);
        }

        public static Test Create()
        {
            return new YuPengPolygonTest();
        }
    }
}