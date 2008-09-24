using System.Windows.Controls;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerSilverlightDemos.Drawing
{
    public partial class RectangleBrush : UserControl, IDrawingBrush
    {
        public BrushExtender Extender = new BrushExtender();
        private float height;
        private float width;

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
                translateTransform.X = -width/2;
                height = value.Y;
                rectangle.Height = height;
                translateTransform.Y = -height/2;
                rotateTransform.CenterX = width/2;
                rotateTransform.CenterY = height/2;
            }
            get { return new Vector2(width, height); }
        }

        #region IDrawingBrush Members

        public void Update()
        {
            Extender.Update();
        }

        #endregion
    }
}