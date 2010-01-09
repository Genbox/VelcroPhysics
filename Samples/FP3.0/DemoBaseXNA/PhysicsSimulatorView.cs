using System;
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
        public bool EnableDiagnostics;

        //Performance panel
        private bool _enablePerformancePanelView = true;
        private const string _stepTime = "Update Time: {0}ms";
        private const string _bodyCount = "Bodies: {0}";
        private const string _jointCount = "Joints: {0}";
        private Color _performancePanelColor = new Color(0, 0, 0, 150);
        private Vector2 _performancePanelPosition = new Vector2(50, 50);
        private Color _performancePanelTextColor = new Color(0, 0, 0, 255);
        private Texture2D _performancePanelTexture;
        private int _performancePanelWidth = 220;
        private const int _performancePanelHeight = 130;
        private bool _movingPanel;
        private Vector2 _panelMouseOffset;
        private SpriteFont _diagnosticSpriteFont;

        public PhysicsSimulatorView(World physicsSimulator)
        {
            _physicsSimulator = physicsSimulator;
        }

        public void LoadContent(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            _performancePanelTexture = DrawingHelper.CreateRectangleTexture(GraphicsDevice, _performancePanelWidth, _performancePanelHeight, 3, Color.Gray, Color.Black);

            _diagnosticSpriteFont = Content.Load<SpriteFont>("Content/Fonts/diagnosticFont");

        }

        public void HandleInput(InputState input)
        {
            //Windows
            if (!input.LastKeyboardState.IsKeyDown(Keys.F1) && input.CurrentKeyboardState.IsKeyDown(Keys.F1))
            {
                EnableDiagnostics = !EnableDiagnostics;
            }
            
            Mouse(input.CurrentMouseState, input.LastMouseState);
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
            // create a rectangle containing the panel
            Rectangle panelRect = new Rectangle((int)_performancePanelPosition.X, (int)_performancePanelPosition.Y, (int)_performancePanelWidth, (int)_performancePanelHeight);

            Rectangle mouseRect = new Rectangle((int)position.X, (int)position.Y, 1, 1);

            if (panelRect.Intersects(mouseRect))
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
            spriteBatch.Draw(_performancePanelTexture, _performancePanelPosition, _performancePanelColor);

            spriteBatch.DrawString(_diagnosticSpriteFont,
                                   String.Format(_stepTime, (float)_physicsSimulator.UpdateTime.TotalMilliseconds),
                                   _performancePanelPosition + new Vector2(12, 8), Color.White);

            spriteBatch.DrawString(_diagnosticSpriteFont,
                                   String.Format(_bodyCount, _physicsSimulator.BodyCount),
                                   _performancePanelPosition + new Vector2(12, 23), Color.White);

            spriteBatch.DrawString(_diagnosticSpriteFont,
                                   String.Format(_jointCount, _physicsSimulator.JointCount),
                                   _performancePanelPosition + new Vector2(12, 38), Color.White);


            //spriteBatch.DrawString(_spriteFont, String.Format("Broadphase Pairs: {0}",this._physicsSimulator.sweepAndPrune.collisionPairs.Keys.Count), new Vector2(120, 215), Color.White);
        }
    }
}