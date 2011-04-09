/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
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
                    DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
                    DebugView.DrawPolygon(array, _polygons[i].Count, col);
                    for (int j = 0; j < _polygons[i].Count; ++j)
                    {
                        DebugView.DrawPoint(_polygons[i][j], .2f, Color.Red);
                    }
                    DebugView.EndCustomDraw();
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
                    if (_polygons[i] == null)
                        continue;

                    if (_polygons[i].PointInPolygon(ref position) == 1)
                    {
                        _selected = _polygons[i];
                        break;
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

        private void DoBooleanOperation(IEnumerable<Vertices> result)
        {
            // Do the union
            _polygons.Remove(_subject);
            _polygons.Remove(_clip);
            _polygons.AddRange(result);
            _subject = null;
            _clip = null;
            _selected = null;
        }

        private void AddCircle(int radius, int numSides)
        {
            Vertices verts = PolygonTools.CreateCircle(radius, numSides);
            _polygons.Add(verts);
        }

        private void AddRectangle(int width, int height)
        {
            Vertices verts = PolygonTools.CreateRectangle(width, height);
            _polygons.Add(verts);
        }

        public static Test Create()
        {
            return new YuPengPolygonTest();
        }
    }
}