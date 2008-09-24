using System.Windows.Controls;

namespace FarseerSilverlightDemos.Drawing
{
    public partial class CircleBrush : UserControl, IDrawingBrush
    {
        public BrushExtender Extender = new BrushExtender();
        private float height;
        private float width;


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
                width = value*2;
                ellipse.Width = width;
                translateTransform.X = -width/2;
                height = value*2;
                ellipse.Height = height;
                translateTransform.Y = -height/2;
                rotateTransform.CenterX = value;
                rotateTransform.CenterY = value;
            }
            get { return width/2; }
        }

        #region IDrawingBrush Members

        public void Update()
        {
            Extender.Update();
        }

        #endregion
    }
}