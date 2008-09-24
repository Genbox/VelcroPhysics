using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics;

namespace FarseerSilverlightDemos.Drawing
{
    public class BrushExtender
    {
        public Body Body;
        public RotateTransform rotateTransform;
        public FrameworkElement child;
        float X;
        float Y;
        float rotation;

        public Vector2 Position
        {
            set
            {
                child.SetValue(Canvas.LeftProperty, Convert.ToDouble(value.X));
                child.SetValue(Canvas.TopProperty, Convert.ToDouble(value.Y));
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
                rotateTransform.Angle = (rotation * 360) / (2 * Math.PI);
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

    }
}
