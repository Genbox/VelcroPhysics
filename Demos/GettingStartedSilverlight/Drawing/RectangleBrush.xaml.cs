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
using System.Windows.Markup;

namespace FarseerSilverlightDemos.Drawing
{
    public partial class RectangleBrush : UserControl, IDrawingBrush
    {
        float width;
        float height;
        public BrushExtender Extender = new BrushExtender();
        public RectangleBrush()
        {
            InitializeComponent();
            Extender.rotateTransform = rotateTransform;
            Extender.child = rectangle;
        }


        public Vector2 Size
        {
            set
            {
                width = value.X;
                rectangle.Width = width;
                translateTransform.X = -width / 2;
                height = value.Y;
                rectangle.Height = height;
                translateTransform.Y = -height / 2;
                rotateTransform.CenterX = width/2;
                rotateTransform.CenterY = height/2;
            }
            get
            {
                return new Vector2(width, height);
            }
        }

        public void Update()
        {
            Extender.Update();
        }
    }
}
