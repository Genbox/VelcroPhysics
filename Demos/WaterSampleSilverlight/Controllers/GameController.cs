using System;
using FarseerPhysicsWaterDemo.Views;
using FarseerPhysicsWaterDemo.Models;
using FarseerPhysicsWaterDemo.RenderSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerPhysicsWaterDemo.Controllers
{
    public class GameController
    {
        #region properties
        #endregion

        #region public methods
        public GameController(Page pageView)
        {
            _pageView = pageView;
        }

        public void Initialize()
        {
            InitializePhysicsSimulator();
            InitializeRenderLoop();
            ShowSplashScreenView();
        }
        #endregion

        #region private methods
        private void ShowSplashScreenView()
        {
            _splashScreenView = new SplashScreenView();
            _pageView.Navigate(_splashScreenView);
            _splashScreenView.Complete += _splashView_Complete;
            _splashScreenView.Start();
        }

        private void ShowGameView()
        {
            GameModel gameModel = new GameModel();
            _gameView = new GameView(gameModel);
            _pageView.Navigate(_gameView);
            _gameView.Initialize(_physicsSimulator);
            InitializeMousePicker();
            _renderLoop.Update += _gameView.Update;
            _renderLoop.Draw += _gameView.Draw;
            _renderLoop.Draw += _mousePicker.Draw;
        }

        private void InitializeRenderLoop()
        {
            _renderLoop = new RenderLoop();
            _renderLoop.StepSize = 10;
            _renderLoop.Update += _renderLoop_Update;
            _renderLoop.Initialize();
        }

        private void InitializeMousePicker()
        {
            _mousePicker = new MousePicker(_physicsSimulator, _gameView.LayoutRoot);
        }

        void _splashView_Complete(object sender, EventArgs e)
        {
            ShowGameView();
            _splashScreenView = null;            
        }

        void _renderLoop_Update(TimeSpan elapsedTime)
        {
            _physicsSimulator.Update((float)elapsedTime.TotalSeconds);
        }

        private void InitializePhysicsSimulator()
        {
            _physicsSimulator = new PhysicsSimulator(new Vector2(0, 4));
            ConvertUnits.SetDisplayUnitToSimUnitRatio(50); //50 pixels = 1 meter
        }
        #endregion

        #region events
        #endregion

        #region private variables
        private Page _pageView;
        private SplashScreenView _splashScreenView;
        private GameView _gameView;
        private RenderLoop _renderLoop;
        private PhysicsSimulator _physicsSimulator;
        private MousePicker _mousePicker;
        #endregion
    }
}
