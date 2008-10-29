using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerSilverlightManual.Drawing;

namespace FarseerSilverlightManual.Objects
{
    public partial class FixedLinearSpringBrush : IDrawingBrush
    {
        private float _x1;
        private float _x2;
        private float _y1;
        private float _y2;
        public BrushExtender Extender = new BrushExtender();
        public FixedLinearSpring FixedLinearSpring;

        public FixedLinearSpringBrush()
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
            if (FixedLinearSpring == null) return;
            Endpoint1 = FixedLinearSpring.Body.GetWorldPosition(FixedLinearSpring.BodyAttachPoint);
            Endpoint2 = FixedLinearSpring.WorldAttachPoint;
        }

        #endregion
    }
}