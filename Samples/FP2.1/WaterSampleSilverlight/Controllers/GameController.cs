using System;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.WaterSampleSilverlight.Models;
using FarseerGames.WaterSampleSilverlight.RenderSystem;
using FarseerGames.WaterSampleSilverlight.Views;

namespace FarseerGames.WaterSampleSilverlight.Controllers
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

        private void _splashView_Complete(object sender, EventArgs e)
        {
            ShowGameView();
            _splashScreenView = null;
        }

        private void _renderLoop_Update(TimeSpan elapsedTime)
        {
            _physicsSimulator.Update((float) elapsedTime.TotalSeconds);
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

        private GameView _gameView;
        private MousePicker _mousePicker;
        private Page _pageView;
        private PhysicsSimulator _physicsSimulator;
        private RenderLoop _renderLoop;
        private SplashScreenView _splashScreenView;

        #endregion
    }
}