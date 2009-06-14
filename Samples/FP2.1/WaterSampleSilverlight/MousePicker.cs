using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.WaterSampleSilverlight
{
    public class MousePicker
    {
        #region properties

        #endregion

        #region public methods

        public MousePicker(PhysicsSimulator physicsSimulator, Canvas canvas)
        {
            _canvas = canvas;
            _physicsSimulator = physicsSimulator;

            _canvas.MouseLeftButtonDown += MouseLeftButtonDown;
            _canvas.MouseLeftButtonUp += MouseLeftButtonUp;
            _canvas.MouseMove += MouseMove;

            _mousePickLine = new Line();
            _mousePickLine.Stroke = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0));
            _mousePickLine.Visibility = Visibility.Collapsed;
            _mousePickLine.SetValue(Canvas.ZIndexProperty, 1000);
            _canvas.Children.Add(_mousePickLine);
        }

        public void Draw(TimeSpan elapsedTime)
        {
            if (_mousePickSpring != null)
            {
                _mousePickLine.X1 = _mousePosition.X;
                _mousePickLine.Y1 = _mousePosition.Y;
                Vector2 anchor = _pickedGeom.Body.GetWorldPosition(_mousePickSpring.BodyAttachPoint);
                anchor = ConvertUnits.ToDisplayUnits(anchor);
                _mousePickLine.X2 = anchor.X;
                _mousePickLine.Y2 = anchor.Y;
            }
        }

        #endregion

        #region private methods

        private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_mousePickSpring != null && _mousePickSpring.IsDisposed == false)
            {
                _mousePickSpring.Dispose();
                _mousePickSpring = null;
                _mousePickLine.Visibility = Visibility.Collapsed;
            }
        }

        private void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_mousePickSpring != null) return;
            Vector2 point = new Vector2((float) (e.GetPosition(_canvas).X), (float) (e.GetPosition(_canvas).Y));
            point = ConvertUnits.ToSimUnits(point);
            _pickedGeom = _physicsSimulator.Collide(point);
            if (_pickedGeom != null)
            {
                _mousePickSpring = SpringFactory.Instance.CreateFixedLinearSpring(_physicsSimulator, _pickedGeom.Body,
                                                                                  _pickedGeom.Body.GetLocalPosition(
                                                                                      point), point, 20, 10);
                _mousePickLine.Visibility = Visibility.Visible;
            }
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            _mousePosition = new Vector2((float) (e.GetPosition(_canvas).X), (float) (e.GetPosition(_canvas).Y));
            if (_mousePickSpring != null)
            {
                Vector2 point = ConvertUnits.ToSimUnits(_mousePosition);
                _mousePickSpring.WorldAttachPoint = point;
            }
        }

        #endregion

        #region events

        #endregion

        #region private variables

        private Canvas _canvas;
        private Line _mousePickLine;
        private FixedLinearSpring _mousePickSpring;
        private Vector2 _mousePosition;
        private PhysicsSimulator _physicsSimulator;
        private Geom _pickedGeom;

        #endregion
    }
}