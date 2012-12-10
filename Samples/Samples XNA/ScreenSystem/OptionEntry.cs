#region Using System
using System;
#endregion
#region Using XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion
#region Using Farseer
using FarseerPhysics.Samples.MediaSystem;
#endregion

namespace FarseerPhysics.Samples.ScreenSystem
{
  /// <summary>
  /// Helper class represents a single entry in a OptionScreen.
  /// </summary>
  public sealed class OptionEntry
  {
    private const double HighlightTime = 0.3;
    private const double FadeTime = 0.4;

    private Vector2 _position;
    private Vector2 _size;
    private Color _color;
    private Color _textColor;

    private bool _isChecked;

    private double _hoverFade;
    private double _selectionFade;

    private string _text;

    /// <summary>
    /// Constructs a new option entry with the specified text.
    /// </summary>
    public OptionEntry(string text, bool isChecked)
    {
      _text = text;
      _isChecked = isChecked;

      _hoverFade = 0.0;
      _selectionFade = 0.0;

      SpriteFont font = ContentWrapper.GetFont("menuFont");
      _size = font.MeasureString(text);
    }

    public void Switch()
    {
      _isChecked = !_isChecked;
    }

    public void InitializePosition(Vector2 target)
    {
      _position = target;
    }

    public string Text
    {
      get { return _text; }
    }

    public Vector2 Position
    {
      get { return _position; }
    }

    public bool IsChecked
    {
      get { return _isChecked; }
    }

    public Vector2 Origin
    {
      get { return _size / 2f; }
    }

    public Vector2 Size
    {
      get { return _size; }
    }

    public Color TextColor
    {
      get { return _textColor; }
    }

    public Color TileColor
    {
      get { return _color; }
    }

    public float Scale
    {
      get { return 0.9f + 0.1f * (float)_hoverFade; }
    }

    /// <summary>
    /// Updates the menu entry.
    /// </summary>
    public void Update(bool isSelected, bool isHovered, GameTime gameTime)
    {
      if (isHovered)
      {
        _hoverFade = Math.Min(_hoverFade + (gameTime.ElapsedGameTime.TotalSeconds / HighlightTime), 1.0);
      }
      else
      {
        _hoverFade = Math.Max(_hoverFade - (gameTime.ElapsedGameTime.TotalSeconds / HighlightTime), 0.0);
      }
      if (isSelected)
      {
        _selectionFade = Math.Min(_selectionFade + (gameTime.ElapsedGameTime.TotalSeconds / HighlightTime), 1.0);
      }
      else
      {
        _selectionFade = Math.Max(_selectionFade - (gameTime.ElapsedGameTime.TotalSeconds / HighlightTime), 0.0);
      }

      _textColor = Color.Lerp(ContentWrapper.Beige, ContentWrapper.Gold, (float)_selectionFade);
      _color = Color.Lerp(ContentWrapper.Sky * 0.6f, ContentWrapper.Grey * 0.6f, (float)Math.Max(_selectionFade, _hoverFade));
    }
  }
}