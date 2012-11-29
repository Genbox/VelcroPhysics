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
  /// Helper class represents a single entry in a MenuScreen. By default this
  /// just draws the entry text string, but it can be customized to display menu
  /// entries in different ways. This also provides an event that will be raised
  /// when the menu entry is selected.
  /// </summary>
  public sealed class MenuEntry : IComparable
  {
    private const double TranslationTime = 0.5;
    private const double FadeTime = 0.3;

    /// <summary>
    /// The target position at which the entry is drawn. This is set by the MenuScreen.
    /// Each frame in Update the current position is interpolated.
    /// </summary>
    private Vector2 _targetPosition;
    private Vector2 _currentPosition;
    private Vector2 _startPosition;
    private double _translationPosition;
    private Vector2 _size;
    private Color _color;
    private Color _textColor;

    private PhysicsGameScreen _screen;
    private Texture2D _preview;

    /// <summary>
    /// Tracks a fading selection effect on the entry.
    /// </summary>
    /// <remarks>
    /// The entries transition out of the selection effect when they are deselected.
    /// </remarks>
    private double _hoverFade;
    private double _selectionFade;
    private float _alpha;

    /// <summary>
    /// The text rendered for this entry.
    /// </summary>
    private string _text;

    /// <summary>
    /// Constructs a new menu entry with the specified text.
    /// </summary>
    public MenuEntry(string text, PhysicsGameScreen screen, Texture2D preview)
    {
      _text = text;
      _screen = screen;
      _preview = preview;

      _alpha = 1f;
      _hoverFade = 0.0;
      _selectionFade = 0.0;
      _translationPosition = 0.0;

      SpriteFont font = MediaManager.GetFont("menuFont");
      _size = font.MeasureString(text);
    }

    public void InitializePosition(Vector2 position)
    {
      _startPosition = _targetPosition = _currentPosition = position;
    }

    public string Text
    {
      get { return _text; }
    }

    public Vector2 Position
    {
      get { return _currentPosition; }
      set
      {
        _targetPosition = value;
        _startPosition = _currentPosition;
        _translationPosition = 0.0;
      }
    }

    public Vector2 Origin
    {
      get { return _size / 2f; }
    }

    public Vector2 Size
    {
      get { return _size; }
    }

    public float Alpha
    {
      get { return _alpha; }
    }

    public GameScreen Screen
    {
      get { return _screen; }
    }

    public Texture2D Preview
    {
      get { return _preview; }
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
        _hoverFade = Math.Min(_hoverFade + (gameTime.ElapsedGameTime.TotalSeconds / FadeTime), 1.0);
      }
      else
      {
        _hoverFade = Math.Max(_hoverFade - (gameTime.ElapsedGameTime.TotalSeconds / FadeTime), 0.0);
      }
      if (isSelected)
      {
        _selectionFade = Math.Min(_selectionFade + (gameTime.ElapsedGameTime.TotalSeconds / FadeTime), 1.0);
      }
      else
      {
        _selectionFade = Math.Max(_selectionFade - (gameTime.ElapsedGameTime.TotalSeconds / FadeTime), 0.0);
      }

      _textColor = Color.Lerp(AssetCreator.Beige, AssetCreator.Gold, (float)_selectionFade);
      _color = Color.Lerp(AssetCreator.Sky * 0.6f, AssetCreator.Grey * 0.6f, (float)Math.Max(_selectionFade, _hoverFade));

      if (_translationPosition < 1.0)
      {
        _translationPosition = Math.Min(_translationPosition + (gameTime.ElapsedGameTime.TotalSeconds / TranslationTime), 1.0);
        _currentPosition = Vector2.Lerp(_startPosition, _targetPosition, (float)_translationPosition);
      }
    }

    #region IComparable Members

    public int CompareTo(object obj)
    {
      MenuEntry entry = obj as MenuEntry;
      if (entry == null)
      {
        return 0;
      }
      else
      {
        return _screen.GetIndex().CompareTo(entry._screen.GetIndex());
      }
    }

    #endregion
  }
}