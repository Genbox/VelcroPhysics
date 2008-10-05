using FarseerSilverlightManual.Drawing;

namespace FarseerSilverlightManual.Objects
{
    public partial class CircleBrush : IDrawingBrush
    {
        private float _height;
        private float _width;
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
                _width = value*2;
                ellipse.Width = _width;
                translateTransform.X = -_width/2;
                _height = value*2;
                ellipse.Height = _height;
                translateTransform.Y = -_height/2;
                rotateTransform.CenterX = value;
                rotateTransform.CenterY = value;
            }
            get { return _width/2; }
        }

        #region IDrawingBrush Members

        public void Update()
        {
            Extender.Update();
        }

        #endregion
    }
}