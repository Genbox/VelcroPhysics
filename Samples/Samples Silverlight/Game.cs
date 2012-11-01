using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics
{
    /// <summary>
    /// The main class of the Game
    /// </summary>
    public class Game
    {
        private Storyboard _gameLoop;
        private GameTime _gameTime;
        private TimeSpan _minimumGameLoopDuration = new TimeSpan(0, 0, 0, 0, 1);

        public Game(UserControl userControl, Canvas drawingCanvas, Canvas debugCanvas, TextBlock txtDebug)
        {
            //Initialize
            IsActive = true;
            IsFixedTimeStep = true;
            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 16);
            Components = new List<DrawableGameComponent>();
            World = new World(new Vector2(0, 0));
            _gameTime = new GameTime();
            _gameTime.GameStartTime = DateTime.Now;
            _gameTime.FrameStartTime = DateTime.Now;
            _gameTime.ElapsedGameTime = TimeSpan.Zero;
            _gameTime.TotalGameTime = TimeSpan.Zero;

            //Setup Canvas
            DrawingCanvas = drawingCanvas;
            DebugCanvas = debugCanvas;
            TxtDebug = txtDebug;
            UserControl = userControl;

            //Setup GameLoop
            _gameLoop = new Storyboard();
            _gameLoop.Completed += GameLoop;
            _gameLoop.Duration = TargetElapsedTime;
            DrawingCanvas.Resources.Add("gameloop", _gameLoop);
        }

        public bool IsActive { get; set; }
        public bool IsFixedTimeStep { get; set; }
        public TimeSpan TargetElapsedTime { get; set; }
        public Canvas DrawingCanvas { get; set; }
        public Canvas DebugCanvas { get; set; }
        public TextBlock TxtDebug { get; set; }
        public UserControl UserControl { get; set; }
        public World World { get; set; }
        public List<DrawableGameComponent> Components { get; private set; }

        /// <summary>
        /// This is the method that starts the game engine
        /// </summary>
        public void Run()
        {
            _gameLoop.Begin();
        }
        
        /// <summary>
        /// This method is called each frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop(object sender, EventArgs e)
        {
            //Calculate Frame Time
            if (IsFixedTimeStep)
                _gameTime.ElapsedGameTime = TargetElapsedTime;
            else
                _gameTime.ElapsedGameTime = DateTime.Now - _gameTime.FrameStartTime;

            _gameTime.FrameStartTime = DateTime.Now;
            _gameTime.TotalGameTime = DateTime.Now - _gameTime.GameStartTime;

            //Update physics
            World.Step((float)_gameTime.ElapsedGameTime.TotalSeconds);

            //Update Game Components
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Update(_gameTime);
            }

            //Draw Game Components
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Draw(_gameTime);
            }

            //Calculate the duration of the next sleep
            TimeSpan remainingTime = TargetElapsedTime - (DateTime.Now - _gameTime.FrameStartTime);
            if (remainingTime < _minimumGameLoopDuration)
                remainingTime = _minimumGameLoopDuration;
            _gameLoop.Duration = remainingTime;

            _gameLoop.Begin();
        }
    }
}
