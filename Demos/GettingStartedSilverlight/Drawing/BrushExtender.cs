using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerSilverlightDemos.Drawing
{
    public class BrushExtender
    {
        public Body Body;
        public FrameworkElement child;
        public RotateTransform rotateTransform;
        private float rotation;
        private float X;
        private float Y;

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
            if (X != Body.Position.X)
            {
                X = Body.Position.X;
                child.SetValue(Canvas.LeftProperty, Convert.ToDouble(X));
            }
            if (Y != Body.Position.Y)
            {
                Y = Body.Position.Y;
                child.SetValue(Canvas.TopProperty, Convert.ToDouble(Y));
            }
            if (Body.Rotation != rotation)
            {
                rotation = Body.Rotation;
                rotateTransform.Angle = (rotation*360)/(2*Math.PI);
            }
        }
    }
}