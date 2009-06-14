using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.WaterSampleSilverlight.Models;

namespace FarseerGames.WaterSampleSilverlight.Views
{
    public class WaterView
    {
        #region properties

        public WaterModel WaterModel { get; private set; }

        #endregion

        #region public methods

        public WaterView(WaterModel waterModel, Canvas canvas)
        {
            WaterModel = waterModel;
            _canvas = canvas;
        }

        public void Initialize(PhysicsSimulator physicsSimulator)
        {
            int count = WaterModel.WaveController.NodeCount;
            _points = new PointCollection(); //[count + 2];
            for (int i = 0; i < count; i++)
            {
                _points.Add(new Point(ConvertUnits.ToDisplayUnits(WaterModel.WaveController.XPosition[i]),
                                      ConvertUnits.ToDisplayUnits(WaterModel.WaveController.CurrentWave[i])));
            }

            _bottomRightPoint = new Point(ConvertUnits.ToDisplayUnits(WaterModel.WaveController.XPosition[count - 1]),
                                          ConvertUnits.ToDisplayUnits(WaterModel.WaveController.Position.Y) +
                                          ConvertUnits.ToDisplayUnits(WaterModel.WaveController.Height));
            _bottomLeftPoint = new Point(ConvertUnits.ToDisplayUnits(WaterModel.WaveController.XPosition[0]),
                                         ConvertUnits.ToDisplayUnits(WaterModel.WaveController.Position.Y) +
                                         ConvertUnits.ToDisplayUnits(WaterModel.WaveController.Height));
            _points.Add(_bottomRightPoint);
            _points.Add(_bottomLeftPoint);

            _wavePolygon = ShapeFactory.CreatePolygon(_points, null, null, null);
            _wavePolygon.IsHitTestVisible = false;
            _wavePolygon.Opacity = _opacity;
            _canvas.Children.Add(_wavePolygon);

            _waveBrush = new LinearGradientBrush();
            _waveBrush.EndPoint = new Point(.5f, .5f);
            _waveBrush.StartPoint = new Point(.5f, 0f);

            GradientStop gradientTop = new GradientStop();
            gradientTop.Color = _gradientTopColor;
            gradientTop.Offset = 0;

            GradientStop gradientBottom = new GradientStop();
            gradientBottom.Color = _gradientBottomColor;
            gradientBottom.Offset = 1;

            _waveBrush.GradientStops.Add(gradientTop);
            _waveBrush.GradientStops.Add(gradientBottom);

            _wavePolygon.Fill = _waveBrush;
            _wavePolygon.Stroke = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0));
            _wavePolygon.StrokeThickness = 1f;
        }

        public void Draw()
        {
            _points.Clear();
            for (int i = 0; i < WaterModel.WaveController.NodeCount; i++)
            {
                Point p = new Point(ConvertUnits.ToDisplayUnits(WaterModel.WaveController.XPosition[i]),
                                    ConvertUnits.ToDisplayUnits(WaterModel.WaveController.CurrentWave[i]) +
                                    ConvertUnits.ToDisplayUnits(WaterModel.WaveController.Position.Y));
                _points.Add(p);
            }

            _points.Add(_bottomRightPoint);
            _points.Add(_bottomLeftPoint);
            _wavePolygon.Points = _points;
        }

        #endregion

        #region private methods

        #endregion

        #region events

        #endregion

        #region private variables

        private const float _opacity = .7f;
        private Point _bottomLeftPoint;
        private Point _bottomRightPoint;

        private Canvas _canvas;
        private Color _gradientBottomColor = Color.FromArgb(150, 0, 0, 0);
        private Color _gradientTopColor = Color.FromArgb(127, 7, 52, 96);
        private PointCollection _points;

        private LinearGradientBrush _waveBrush;
        private Polygon _wavePolygon;

        #endregion
    }
}