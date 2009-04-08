using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.SimpleSamplesSilverlight.Drawing
{
    public class BrushExtender
    {
        private float _rotation;
        private float _x;
        private float _y;
        public Body Body;
        public FrameworkElement child;
        public RotateTransform rotateTransform;

        public Vector2 Position
        {
            set
            {
                child.SetValue(Canvas.LeftProperty, Convert.ToDouble(value.X));
                child.SetValue(Canvas.TopProperty, Convert.ToDouble(value.Y));
            }
        }

        public Color Color
        {
            set
            {
                if (child is Shape)
                {
                    (child as Shape).Fill = new SolidColorBrush(value);
                }
            }
        }

        public virtual void Update()
        {
            if (Body == null) return;
            if (_x != Body.Position.X)
            {
                _x = Body.Position.X;
                child.SetValue(Canvas.LeftProperty, Convert.ToDouble(_x));
            }
            if (_y != Body.Position.Y)
            {
                _y = Body.Position.Y;
                child.SetValue(Canvas.TopProperty, Convert.ToDouble(_y));
            }
            if (Body.Rotation != _rotation)
            {
                _rotation = Body.Rotation;
                rotateTransform.Angle = (_rotation*360)/(2*Math.PI);
            }
        }
    }
}