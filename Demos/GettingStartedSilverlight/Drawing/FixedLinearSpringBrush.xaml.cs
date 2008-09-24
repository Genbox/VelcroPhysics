using System.Windows.Controls;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerSilverlightDemos.Drawing
{
    public partial class FixedLinearSpringBrush : UserControl, IDrawingBrush
    {
        public BrushExtender Extender = new BrushExtender();
        public FixedLinearSpring FixedLinearSpring;
        private float x1;
        private float x2;
        private float y1;
        private float y2;


        public FixedLinearSpringBrush()
        {
            InitializeComponent();
        }

        public Vector2 Endpoint1
        {
            set
            {
                if (x1 != value.X)
                {
                    x1 = value.X;
                    line.X1 = x1;
                }
                if (y1 != value.Y)
                {
                    y1 = value.Y;
                    line.Y1 = y1;
                }
            }
        }

        public Vector2 Endpoint2
        {
            set
            {
                if (x2 != value.X)
                {
                    x2 = value.X;
                    line.X2 = x2;
                }
                if (y2 != value.Y)
                {
                    y2 = value.Y;
                    line.Y2 = y2;
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