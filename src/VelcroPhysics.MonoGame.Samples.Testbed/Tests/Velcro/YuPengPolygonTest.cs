/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
*/

using System.Collections.Generic;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Tools.Cutting;
using Genbox.VelcroPhysics.Tools.Cutting.Simple;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests.Velcro
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

            _polygons.Add(PolygonUtils.CreateGear(5f, 10, 0f, 6f));
            _polygons.Add(PolygonUtils.CreateGear(4f, 15, 100f, 3f));

            trans.X = 0f;
            trans.Y = 8f;
            _polygons[0].Translate(ref trans);
            _polygons[1].Translate(ref trans);

            _polygons.Add(PolygonUtils.CreateGear(5f, 10, 50f, 5f));

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

            _polygons.Add(PolygonUtils.CreateRectangle(5f, 5f));
            _polygons.Add(PolygonUtils.CreateRectangle(5f, 5f));
            trans.X = 0f;
            trans.Y = 27f;
            _polygons[5].Translate(ref trans);
            _polygons[6].Translate(ref trans);

            _polygons.Add(PolygonUtils.CreateRectangle(5f, 5f));
            _polygons.Add(PolygonUtils.CreateRectangle(5f, 5f));
            trans.Y = 40f;
            _polygons[7].Translate(ref trans);
            trans.X = 5f;
            _polygons[8].Translate(ref trans);

            _polygons.Add(PolygonUtils.CreateRectangle(5f, 5f));
            _polygons.Add(PolygonUtils.CreateRectangle(5f, 5f));
            trans.Y = 35f;
            trans.X = 20f;
            _polygons[9].Translate(ref trans);
            trans.Y = 45f;
            trans.X = 25f;
            _polygons[10].Translate(ref trans);

            _subject = _polygons[5];
            _clip = _polygons[6];

            base.Initialize();
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            foreach (Vertices vertex in _polygons)
                if (vertex != null)
                {
                    Vector2[] array = vertex.ToArray();
                    Color col = Color.SteelBlue;
                    if (!vertex.IsCounterClockWise())
                        col = Color.Aquamarine;
                    if (vertex == _selected)
                        col = Color.LightBlue;
                    if (vertex == _subject)
                    {
                        col = Color.Green;
                        if (vertex == _selected)
                            col = Color.LightGreen;
                    }

                    if (vertex == _clip)
                    {
                        col = Color.DarkRed;
                        if (vertex == _selected)
                            col = Color.IndianRed;
                    }

                    DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
                    DebugView.DrawPolygon(array, vertex.Count, col);
                    for (int j = 0; j < vertex.Count; ++j)
                        DebugView.DrawPoint(vertex[j], .2f, Color.Red);
                    DebugView.EndCustomDraw();
                }

            DrawString("A,S,D = Create Rectangle");
            DrawString("Q,W,E = Create Circle");
            DrawString("Click to Drag polygons");
            DrawString("1 = Select Subject while dragging [green]");
            DrawString("2 = Select Clip while dragging [red]");
            DrawString("Space = Union");
            DrawString("Backspace = Subtract");
            DrawString("Shift = Intersect");
            DrawString("Holes are colored light blue");
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            // Add Circles
            if (keyboard.IsNewKeyPress(Keys.Q))
                AddCircle(3, 8);

            // Add Circles
            if (keyboard.IsNewKeyPress(Keys.W))
                AddCircle(4, 16);

            // Add Circles
            if (keyboard.IsNewKeyPress(Keys.E))
                AddCircle(5, 32);

            // Add Rectangle
            if (keyboard.IsNewKeyPress(Keys.A))
                AddRectangle(4, 8);

            // Add Rectangle
            if (keyboard.IsNewKeyPress(Keys.S))
                AddRectangle(5, 2);

            // Add Rectangle
            if (keyboard.IsNewKeyPress(Keys.D))
                AddRectangle(2, 5);

            // Perform a Union
            if (keyboard.IsNewKeyPress(Keys.Space))
                if (_subject != null && _clip != null)
                    DoBooleanOperation(YuPengClipper.Union(_subject, _clip, out _err));

            // Perform a Subtraction
            if (keyboard.IsNewKeyPress(Keys.Back))
                if (_subject != null && _clip != null)
                    DoBooleanOperation(YuPengClipper.Difference(_subject, _clip, out _err));

            // Perform a Intersection
            if (keyboard.IsNewKeyPress(Keys.LeftShift))
                if (_subject != null && _clip != null)
                    DoBooleanOperation(YuPengClipper.Intersect(_subject, _clip, out _err));

            // Select Subject
            if (keyboard.IsNewKeyPress(Keys.D1))
                if (_selected != null)
                {
                    if (_clip == _selected)
                        _clip = null;

                    _subject = _selected;
                }

            // Select Clip
            if (keyboard.IsNewKeyPress(Keys.D2))
                if (_selected != null)
                {
                    if (_subject == _selected)
                        _subject = null;

                    _clip = _selected;
                }
        }

        public override void Mouse(MouseManager mouse)
        {
            Vector2 position = GameInstance.ConvertScreenToWorld(mouse.NewPosition);

            if (mouse.IsNewButtonClick(MouseButton.Left))
            {
                foreach (Vertices vertices in _polygons)
                {
                    if (vertices == null)
                        continue;

                    if (vertices.PointInPolygon(ref position) == 1)
                    {
                        _selected = vertices;
                        break;
                    }
                }
            }

            if (mouse.IsNewButtonRelease(MouseButton.Left))
                _selected = null;

            if (_selected != null)
            {
                Vector2 diff = position - GameInstance.ConvertScreenToWorld(mouse.OldPosition);
                _selected.Translate(ref diff);
            }

            base.Mouse(mouse);
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
            Vertices verts = PolygonUtils.CreateCircle(radius, numSides);
            _polygons.Add(verts);
        }

        private void AddRectangle(int width, int height)
        {
            Vertices verts = PolygonUtils.CreateRectangle(width, height);
            _polygons.Add(verts);
        }

        internal static Test Create()
        {
            return new YuPengPolygonTest();
        }
    }
}