using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics.Dynamics;

namespace FarseerSilverlightDemos.Drawing
{
    public partial class CircleBrush : UserControl, IDrawingBrush
    {
        float width;
        float height;
        public BrushExtender Extender = new BrushExtender();


        public CircleBrush()
        {
            InitializeComponent();
            Extender.rotateTransform = rotateTransform;
            Extender.child = ellipse;
        }


        public float Radius
        {
            set
            {
                width = value * 2;
                ellipse.Width = width;
                translateTransform.X = -width / 2;
                height = value * 2;
                ellipse.Height = height;
                translateTransform.Y = -height / 2;
                rotateTransform.CenterX = value;
                rotateTransform.CenterY = value;
            }
            get
            {
                return width / 2;
            }
        }

        public void Update()
        {
            Extender.Update();
        }
    }
}
