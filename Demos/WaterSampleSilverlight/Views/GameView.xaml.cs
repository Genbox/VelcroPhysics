using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.WaterSampleSilverlight.Models;
using FarseerGames.WaterSampleSilverlight.RenderSystem;

namespace FarseerGames.WaterSampleSilverlight.Views
{
    public partial class GameView
    {
        #region properties

        #endregion

        #region public methods

        public GameView(GameModel gameModel)
        {
            InitializeComponent();
            _gameModel = gameModel;
        }

        public void Initialize(PhysicsSimulator physicsSimulator)
        {
            _gameModel.Initialize(physicsSimulator);
            InitializeBoxViews(physicsSimulator);
            InitializeWater(physicsSimulator);

            CollisionBorder collisionBorder = new CollisionBorder(ConvertUnits.ToSimUnits((float) Width),
                                                                  ConvertUnits.ToSimUnits((float) Height),
                                                                  ConvertUnits.ToSimUnits(100),
                                                                  ConvertUnits.ToSimUnits((float) Width/2f,
                                                                                          (float) Height/2f));
            collisionBorder.Initialize(physicsSimulator);
        }

        public void Draw(TimeSpan elapsedTime)
        {
            _waterView.Draw();
            foreach (var boxView in _boxViews)
            {
                boxView.Draw();
            }
        }

        public void Update(TimeSpan elapsedTime)
        {
            _waterView.WaterModel.Update(elapsedTime);
        }

        #endregion

        #region private methods

        private void InitializeWater(PhysicsSimulator physicsSimulator)
        {
            _waterView = new WaterView(_gameModel.WaterModel, LayoutRoot);
            _waterView.Initialize(physicsSimulator);

            WaveGenerator.Maximum = 2;
            WaveGenerator.Minimum = 0;
            WaveGenerator.Value = 0.20f;

            Density.Maximum = 10;
            Density.Minimum = 0;
            Density.Value = 5;

            LinearDrag.Maximum = 10;
            LinearDrag.Minimum = 0;
            LinearDrag.Value = 4;

            RotationalDrag.Maximum = 3;
            RotationalDrag.Minimum = 0;
            RotationalDrag.Value = 2f;
        }

        private void InitializeBoxViews(PhysicsSimulator physicsSimulator)
        {
            _boxViews = new List<BoxView>();
            foreach (var boxModel in _gameModel.BoxModels)
            {
                _boxViews.Add(InitializeBoxView(boxModel, Colors.White, Colors.Black, 1));
            }

            foreach (var boxModel in _gameModel.PyramidBoxModels)
            {
                _boxViews.Add(InitializeBoxView(boxModel, Colors.White, Colors.Black, 1));
            }

            foreach (var boxView in _boxViews)
            {
                boxView.Initialize(physicsSimulator, LayoutRoot);
            }
        }

        private BoxView InitializeBoxView(BoxModel boxModel, Color fill, Color stroke, float strokeThickness)
        {
            Rectangle rectangle = ShapeFactory.CreateRectangle(ConvertUnits.ToDisplayUnits(boxModel.Width),
                                                               ConvertUnits.ToDisplayUnits(boxModel.Height), fill,
                                                               stroke, strokeThickness);

            Sprite sprite = new Sprite(rectangle);
            sprite.Origin = new Vector2(sprite.Width/2, sprite.Height/2);

            BoxView boxView = new BoxView(boxModel, sprite);
            return boxView;
        }

        private void WaveGenerator_ValueChanged(object sender, RoutedEventArgs e)
        {
            float sliderValue = (float) WaveGenerator.Value;
            _waterView.WaterModel.WaveController.WaveGeneratorMax = sliderValue;
            _waterView.WaterModel.WaveController.WaveGeneratorMin = -sliderValue;
            _waterView.WaterModel.WaveController.WaveGeneratorStep = sliderValue/4f;
            WaveStrengthValue.Text = sliderValue.ToString("0.00");
        }

        private void Density_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _waterView.WaterModel.FluidDragController.Density = (float) Density.Value;
            FluidDensityValue.Text = Density.Value.ToString("0.00");
        }

        private void LinearDrag_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _waterView.WaterModel.FluidDragController.LinearDragCoefficient = (float) LinearDrag.Value;
            FluidLinearDragValue.Text = LinearDrag.Value.ToString("0.00");
        }

        private void RotationalDrag_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _waterView.WaterModel.FluidDragController.RotationalDragCoefficient = (float) RotationalDrag.Value;
            FluidRotationalDragValue.Text = RotationalDrag.Value.ToString("0.00");
        }

        #endregion

        #region events

        #endregion

        #region private variables

        private List<BoxView> _boxViews;
        private GameModel _gameModel;
        private WaterView _waterView;

        #endregion
    }
}