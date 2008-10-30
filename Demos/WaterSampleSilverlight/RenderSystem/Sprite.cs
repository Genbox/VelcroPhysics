using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            get { return RotateTransform.Angle; }
            set { RotateTransform.Angle = value; }
        }

        public double Scale
        {
            get { return ScaleTransform.ScaleX; }//proportional scales (x=y)
            set
            {
                ScaleTransform.ScaleX = value;
                ScaleTransform.ScaleY = value;
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
                RotateTransform.CenterX = Origin.X;
                RotateTransform.CenterY = Origin.Y;
                TranslateTransform.X = _position.X - Origin.X;
                TranslateTransform.Y = _position.Y - Origin.Y;
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
                TranslateTransform.X = value - Origin.X;
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
                TranslateTransform.Y = value - Origin.Y;
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
            Content = content;
            Width = (float)content.Width;
            Height = (float)content.Height;

            TranslateTransform = new TranslateTransform();
            TranslateTransform.X = 0;
            TranslateTransform.Y = 0;

            RotateTransform = new RotateTransform();
            RotateTransform.CenterX = Origin.X;
            RotateTransform.CenterY = Origin.Y;
            RotateTransform.Angle = 0;

            ScaleTransform = new ScaleTransform();
            ScaleTransform.ScaleX = 1;
            ScaleTransform.ScaleY = 1;

            transformGroup = new TransformGroup();
            transformGroup.Children.Add(RotateTransform);
            transformGroup.Children.Add(TranslateTransform);
            transformGroup.Children.Add(ScaleTransform);
            RenderTransform = transformGroup;
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

