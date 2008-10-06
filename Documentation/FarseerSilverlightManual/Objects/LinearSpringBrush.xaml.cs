using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerSilverlightManual.Drawing;

namespace FarseerSilverlightManual.Objects
{
    public partial class LinearSpringBrush : IDrawingBrush
    {
        private float _x1;
        private float _x2;
        private float _y1;
        private float _y2;
        public BrushExtender Extender = new BrushExtender();
        public LinearSpring LinearSpring;

        public LinearSpringBrush()
        {
            InitializeComponent();
        }

        public Vector2 Endpoint1
        {
            set
            {
                if (_x1 != value.X)
                {
                    _x1 = value.X;
                    line.X1 = _x1;
                }
                if (_y1 != value.Y)
                {
                    _y1 = value.Y;
                    line.Y1 = _y1;
                }
            }
        }

        public Vector2 Endpoint2
        {
            set
            {
                if (_x2 != value.X)
                {
                    _x2 = value.X;
                    line.X2 = _x2;
                }
                if (_y2 != value.Y)
                {
                    _y2 = value.Y;
                    line.Y2 = _y2;
                }
            }
        }

        #region IDrawingBrush Members

        public void Update()
        {
            if (LinearSpring == null) return;
            Endpoint1 = LinearSpring.Body1.GetWorldPosition(LinearSpring.AttachPoint1);
            Endpoint2 = LinearSpring.Body2.GetWorldPosition(LinearSpring.AttachPoint2);
        }

        #endregion
    }
}