using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.SimpleSamplesSilverlight.Drawing;

namespace FarseerGames.SimpleSamplesSilverlight.Objects
{
    public partial class RectangleBrush : IDrawingBrush
    {
        private float _height;
        private float _width;
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
                _width = value.X;
                rectangle.Width = _width;
                translateTransform.X = -_width/2;
                _height = value.Y;
                rectangle.Height = _height;
                translateTransform.Y = -_height/2;
                rotateTransform.CenterX = _width/2;
                rotateTransform.CenterY = _height/2;
            }
            get { return new Vector2(_width, _height); }
        }

        #region IDrawingBrush Members

        public void Update()
        {
            Extender.Update();
        }

        #endregion
    }
}