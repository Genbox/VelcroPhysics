/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
*/

using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
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

      _polygons.Add(PolygonTools.CreateRectangle(5f, 5f));
      _polygons.Add(PolygonTools.CreateRectangle(5f, 5f));
      trans.X = 0f;
      trans.Y = 27f;
      _polygons[5].Translate(ref trans);
      _polygons[6].Translate(ref trans);

      _polygons.Add(PolygonTools.CreateRectangle(5f, 5f));
      _polygons.Add(PolygonTools.CreateRectangle(5f, 5f));
      trans.Y = 40f;
      _polygons[7].Translate(ref trans);
      trans.X = 5f;
      _polygons[8].Translate(ref trans);

      _polygons.Add(PolygonTools.CreateRectangle(5f, 5f));
      _polygons.Add(PolygonTools.CreateRectangle(5f, 5f));
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
      {
        if (vertex != null)
        {
          Vector2[] array = vertex.ToArray();
          Color col = Color.SteelBlue;
          if (!vertex.IsCounterClockWise())
          {
            col = Color.Aquamarine;
          }
          if (vertex == _selected)
          {
            col = Color.LightBlue;
          }
          if (vertex == _subject)
          {
            col = Color.Green;
            if (vertex == _selected)
            {
              col = Color.LightGreen;
            }
          }
          if (vertex == _clip)
          {
            col = Color.DarkRed;
            if (vertex == _selected)
            {
              col = Color.IndianRed;
            }
          }
          DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
          DebugView.DrawPolygon(array, vertex.Count, col);
          for (int j = 0; j < vertex.Count; ++j)
          {
            DebugView.DrawPoint(vertex[j], .2f, Color.Red);
          }
          DebugView.EndCustomDraw();
        }
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

    public override void Keyboard(KeyboardManager keyboardManager)
    {
      // Add Circles
      if (keyboardManager.IsNewKeyPress(Keys.Q))
        AddCircle(3, 8);

      // Add Circles
      if (keyboardManager.IsNewKeyPress(Keys.W))
        AddCircle(4, 16);

      // Add Circles
      if (keyboardManager.IsNewKeyPress(Keys.E))
        AddCircle(5, 32);

      // Add Rectangle
      if (keyboardManager.IsNewKeyPress(Keys.A))
        AddRectangle(4, 8);

      // Add Rectangle
      if (keyboardManager.IsNewKeyPress(Keys.S))
        AddRectangle(5, 2);

      // Add Rectangle
      if (keyboardManager.IsNewKeyPress(Keys.D))
        AddRectangle(2, 5);

      // Perform a Union
      if (keyboardManager.IsNewKeyPress(Keys.Space))
      {
        if (_subject != null && _clip != null)
          DoBooleanOperation(YuPengClipper.Union(_subject, _clip, out _err));
      }

      // Perform a Subtraction
      if (keyboardManager.IsNewKeyPress(Keys.Back))
      {
        if (_subject != null && _clip != null)
          DoBooleanOperation(YuPengClipper.Difference(_subject, _clip, out _err));
      }

      // Perform a Intersection
      if (keyboardManager.IsNewKeyPress(Keys.LeftShift))
      {
        if (_subject != null && _clip != null)
          DoBooleanOperation(YuPengClipper.Intersect(_subject, _clip, out _err));
      }

      // Select Subject
      if (keyboardManager.IsNewKeyPress(Keys.D1))
      {
        if (_selected != null)
        {
          if (_clip == _selected)
            _clip = null;

          _subject = _selected;
        }
      }

      // Select Clip
      if (keyboardManager.IsNewKeyPress(Keys.D2))
      {
        if (_selected != null)
        {
          if (_subject == _selected)
            _subject = null;

          _clip = _selected;
        }
      }
    }

    public override void Mouse(MouseState state, MouseState oldState)
    {
      Vector2 position = GameInstance.ConvertScreenToWorld(state.X, state.Y);

      if (state.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
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
        Vector2 trans = new Vector2((state.X - oldState.X) / 12f, (oldState.Y - state.Y) / 12f);
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