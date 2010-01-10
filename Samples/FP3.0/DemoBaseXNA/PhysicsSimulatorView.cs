using System;
using System.Collections.Generic;
using DemoBaseXNA.ScreenSystem;
using DemoBaseXNA.DrawingSystem;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DemoBaseXNA
{
    /// <summary>
    /// Draws the elements inside a <see cref="PhysicsSimulator"/>. Great for debugging physics related problems.
    /// </summary>
    public sealed class PhysicsSimulatorView
    {
        private World _physicsSimulator;
        private GraphicsDevice _graphics;
        public bool EnableDiagnostics;

        //Performance panel
        private bool _enablePerformancePanelView = true;
        private const string _stepTime = "Time: {0}ms";
        private const string _bodyCount = "Bodies: {0}";
        private const string _jointCount = "Joints: {0}";
        private Color _performancePanelColor = new Color(0, 0, 0, 150);
        private Vector2 _performancePanelPosition = new Vector2(50, 50);
        private Color _performancePanelTextColor = new Color(0, 0, 0, 255);
        private Texture2D _performancePanelTexture;
        private int _performancePanelWidth = 300;
        private const int _performancePanelHeight = 130;
        private bool _movingPanel;
        private Vector2 _panelMouseOffset;
        private SpriteFont _diagnosticSpriteFont;
        private List<Rectangle> _buttonRects;
        private List<bool> _buttonValues;
        private List<string> _buttonTexts;
        private List<float> _graphValues;
        private LineRenderHelper _line;

        public PhysicsSimulatorView(World physicsSimulator)
        {
            _physicsSimulator = physicsSimulator;

            _graphValues = new List<float>();

            _buttonRects = new List<Rectangle>();
            _buttonTexts = new List<string>();
            _buttonValues = new List<bool>();

            _buttonRects.Add(new Rectangle(135, 11, 12, 12));
            _buttonTexts.Add("Warm Starting");
            _buttonValues.Add(true);

            _buttonRects.Add(new Rectangle(135, 26, 12, 12));
            _buttonTexts.Add("Continuous Physics");
            _buttonValues.Add(true);

            _buttonRects.Add(new Rectangle(135, 41, 12, 12));
            _buttonTexts.Add("Allow Sleep");
            _buttonValues.Add(true);
        }

        public void LoadContent(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            _graphics = GraphicsDevice;
            
            _performancePanelTexture = DrawingHelper.CreateRectangleTexture(GraphicsDevice, _performancePanelWidth, _performancePanelHeight, 3, Color.Gray, Color.Black);

            _diagnosticSpriteFont = Content.Load<SpriteFont>("Content/Fonts/diagnosticFont");

            _line = new LineRenderHelper(10000, GraphicsDevice);
        }

        public void HandleInput(InputState input)
        {
            //Windows
            if (!input.LastKeyboardState.IsKeyDown(Keys.F1) && input.CurrentKeyboardState.IsKeyDown(Keys.F1))
            {
                EnableDiagnostics = !EnableDiagnostics;
            }
            
            Mouse(input.CurrentMouseState, input.LastMouseState);

            _physicsSimulator.WarmStarting = _buttonValues[0];
            _physicsSimulator.ContinuousPhysics = _buttonValues[1];
            _physicsSimulator.AllowSleep = _buttonValues[2];

            UpdateGraph();
        }

        private void Mouse(MouseState state, MouseState oldState)
        {
            Vector2 position = new Vector2(state.X, state.Y);
            
            if (state.LeftButton == ButtonState.Released && oldState.LeftButton == ButtonState.Pressed)
            {
                MouseUp(position);
            }
            else if (state.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
            {
                MouseDown(position);
            }

            MouseMove(position);
        }

        private void MouseDown(Vector2 position)
        {
            int selectedButton = -1;
            Rectangle transformedButton;
            
            // create a rectangle containing the panel
            Rectangle panelRect = new Rectangle((int)_performancePanelPosition.X, (int)_performancePanelPosition.Y, (int)_performancePanelWidth, (int)_performancePanelHeight);

            Rectangle mouseRect = new Rectangle((int)position.X, (int)position.Y, 1, 1);

            // for every button we have
            for (int i = 0; i < _buttonRects.Count; i++)
            {
                transformedButton = new Rectangle((int)_performancePanelPosition.X + _buttonRects[i].Location.X,
                    (int)_performancePanelPosition.Y + _buttonRects[i].Location.Y, _buttonRects[i].Width, _buttonRects[i].Height);

                if (transformedButton.Intersects(mouseRect))
                {
                    selectedButton = i;
                }
            }

            if (selectedButton >= 0)
            {
                _buttonValues[selectedButton] = !_buttonValues[selectedButton];
            }
            else if (panelRect.Intersects(mouseRect))
            {
                _movingPanel = true;
                _panelMouseOffset = _performancePanelPosition - position;
            }
        }

        private void MouseUp(Vector2 position)
        {
            _movingPanel = false;
        }

        private void MouseMove(Vector2 position)
        {
            if (_movingPanel)
            {
                _performancePanelPosition = position + _panelMouseOffset;
            }
        }

        private void UpdateGraph()
        {
            _graphValues.Add((float)_physicsSimulator.UpdateTime.TotalMilliseconds);

            if (_graphValues.Count > 100)
                _graphValues.RemoveAt(0);
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            if (EnableDiagnostics)
            {
                if (_enablePerformancePanelView)
                {
                    DrawPerformancePanel(spriteBatch);
                }
            }
        }
        
        private void DrawPerformancePanel(SpriteBatch spriteBatch)
        {
            Rectangle transformedButton;
            float x = _performancePanelPosition.X + 295;
            
            spriteBatch.Draw(_performancePanelTexture, _performancePanelPosition, _performancePanelColor);

            for (int i = 0; i < _buttonRects.Count; i++)
            {
                transformedButton = new Rectangle((int)_performancePanelPosition.X + _buttonRects[i].Location.X,
                    (int)_performancePanelPosition.Y + _buttonRects[i].Location.Y + 3, _buttonRects[i].Width, _buttonRects[i].Height);

                spriteBatch.Draw(_performancePanelTexture, transformedButton, Color.White);

                spriteBatch.DrawString(_diagnosticSpriteFont, _buttonTexts[i],
                                   new Vector2(transformedButton.X + 20, _performancePanelPosition.Y + (i * 15) + 12), Color.White);
                
                if (_buttonValues[i])
                    spriteBatch.Draw(_performancePanelTexture, transformedButton, Color.Green);
            }


            spriteBatch.DrawString(_diagnosticSpriteFont,
                                   String.Format(_stepTime, (float)_physicsSimulator.UpdateTime.TotalMilliseconds),
                                   _performancePanelPosition + new Vector2(12, 8), Color.White);

            spriteBatch.DrawString(_diagnosticSpriteFont,
                                   String.Format(_bodyCount, _physicsSimulator.BodyCount),
                                   _performancePanelPosition + new Vector2(12, 23), Color.White);

            spriteBatch.DrawString(_diagnosticSpriteFont,
                                   String.Format(_jointCount, _physicsSimulator.JointCount),
                                   _performancePanelPosition + new Vector2(12, 38), Color.White);


            if (_graphValues.Count > 2)
            {
                for (int i = 1; i < _graphValues.Count; i++)
                {
                    _line.Submit(new Vector3(x, _performancePanelPosition.Y + 125 - (_graphValues[i - 1] * 2.5f), 0),
                        new Vector3(x - 3, _performancePanelPosition.Y + 125 - (_graphValues[i] * 2.5f), 0), Color.Red);
                    x -= 2.95f;
                }
            }
            _line.Render(_graphics, Matrix.CreateOrthographicOffCenter(0, _graphics.Viewport.Width, _graphics.Viewport.Height, 0, 0, 1), Matrix.Identity);
            _line.Clear();
        }
    }
}