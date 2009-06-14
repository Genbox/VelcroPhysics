using System.Windows.Controls;
using FarseerGames.FarseerPhysics.Mathematics;

namespace SimpleSamplesWPF.SharedDemoObjects
{
    /// <summary>
    /// Interaction logic for FixedLinearSpringVisual.xaml
    /// </summary>
    public partial class FixedLinearSpringVisual : UserControl, ILinearSpringVisual
    {
        public FixedLinearSpringVisual()
        {
            InitializeComponent();
        }

        public Vector2 Endpoint1
        {
            get { return new Vector2((float) line.X1, (float) line.Y1); }
            set
            {
                line.X1 = value.X;
                line.Y1 = value.Y;
            }
        }

        public Vector2 Endpoint2
        {
            get { return new Vector2((float) line.X2, (float) line.Y2); }
            set
            {
                line.X2 = value.X;
                line.Y2 = value.Y;
            }
        }
    }
}
