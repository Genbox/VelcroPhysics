using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerPhysicsWaterDemo.RenderSystem
{
    public class Sprite : UserControl
    {
        #region properties
        TransformGroup transformGroup;
        public TranslateTransform TranslateTransform { get; set; }
        public RotateTransform RotateTransform { get; set; }
        public ScaleTransform ScaleTransform { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public double Rotation
        {
            get { return this.RotateTransform.Angle; }
            set { this.RotateTransform.Angle = value; }
        }

        public double Scale
        {
            get { return this.ScaleTransform.ScaleX; }//proportional scales (x=y)
            set
            {
                this.ScaleTransform.ScaleX = value;
                this.ScaleTransform.ScaleY = value;
            }
        }

        public Vector2 Origin
        {
            get
            {
                return _origin;
            }
            set
            {
                _origin = value;
                this.RotateTransform.CenterX = Origin.X;
                this.RotateTransform.CenterY = Origin.Y;
                this.TranslateTransform.X = _position.X - Origin.X;
                this.TranslateTransform.Y = _position.Y - Origin.Y;
            }
        }

        public float PositionX
        {
            get
            {
                return _position.X;
            }
            set
            {
                _position.X = value;
                this.TranslateTransform.X = value - Origin.X;
                //this.SetValue(Canvas.LeftProperty, (double)value);
            }
        }

        public float PositionY
        {
            get
            {
                return _position.Y;
            }
            set
            {
                _position.Y = value - Origin.Y;
                this.TranslateTransform.Y = value - Origin.Y;
                //this.SetValue(Canvas.TopProperty, (double)value);
            }
        }

        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                PositionX = value.X;
                PositionY = value.Y;
            }
        }
        #endregion

        #region public methods
        public Sprite(FrameworkElement content)
        {
            this.Content = content;
            this.Width = (float)content.Width;
            this.Height = (float)content.Height;

            this.TranslateTransform = new TranslateTransform();
            this.TranslateTransform.X = 0;
            this.TranslateTransform.Y = 0;

            this.RotateTransform = new RotateTransform();
            this.RotateTransform.CenterX = Origin.X;
            this.RotateTransform.CenterY = Origin.Y;
            this.RotateTransform.Angle = 0;

            this.ScaleTransform = new ScaleTransform();
            this.ScaleTransform.ScaleX = 1;
            this.ScaleTransform.ScaleY = 1;

            transformGroup = new TransformGroup();
            transformGroup.Children.Add(RotateTransform);
            transformGroup.Children.Add(TranslateTransform);
            transformGroup.Children.Add(ScaleTransform);
            this.RenderTransform = transformGroup;
        }

        //public Sprite(FrameworkElement content, bool centerOrigin)
        //{
        //    if (centerOrigin)
        //    {
        //        Origin = new Vector2((float)content.Width / 2, (float)content.Height / 2);
        //    }
        //} 

        public
        #endregion

        #region private methods
        #endregion

        #region events
        #endregion

        #region private variables
        Vector2 _position;
        Vector2 _origin;
        #endregion
    }
}

