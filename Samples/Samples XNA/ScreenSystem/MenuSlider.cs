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
  public sealed class MenuSlider
  {
    private const float MaxTranslation = 15f;
    private const double HighlightTime = 0.3;

    private float _positionX;

    private float _targetY;
    private Vector2 _currentPosition;
    private Color _color;

    private double _hoverFade;


    /// <summary>
    /// Constructs a new menu slider.
    /// </summary>
    public MenuSlider(Vector2 position)
    {
      _currentPosition = position;
      _positionX = _currentPosition.X;
      _targetY = _currentPosition.Y;
    }

    public Vector2 Position
    {
      get { return _currentPosition; }
    }

    public float Target
    {
      set { _targetY = value; }
    }

    public Color TileColor
    {
      get { return _color; }
    }

    /// <summary>
    /// Updates the menu slider.
    /// </summary>
    public void Update(bool isHovered, GameTime gameTime)
    {
      if (isHovered)
      {
        _hoverFade = Math.Min(_hoverFade + (gameTime.ElapsedGameTime.TotalSeconds / HighlightTime), 1.0);
      }
      else
      {
        _hoverFade = Math.Max(_hoverFade - (gameTime.ElapsedGameTime.TotalSeconds / HighlightTime), 0.0);
      }
      _color = Color.Lerp(ContentWrapper.Sky * 0.6f, ContentWrapper.Grey * 0.6f, (float)_hoverFade);

      float deltaY = _targetY - _currentPosition.Y;
      if (Math.Abs(deltaY) > MaxTranslation)
      {
        _currentPosition.Y += MaxTranslation * Math.Sign(deltaY);
      }
      else
      {
        _currentPosition.Y += deltaY;
      }
    }
  }
}