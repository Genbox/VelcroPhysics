using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics;

namespace FarseerSilverlightDemos.Drawing
{
    public partial class FixedLinearSpringBrush : UserControl, IDrawingBrush
    {
        public FixedLinearSpring FixedLinearSpring;
        float x1;
        float x2;
        float y1;
        float y2;
        public BrushExtender Extender = new BrushExtender();


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

        public void Update()
        {
            if (FixedLinearSpring == null) return;
            Endpoint1 = FixedLinearSpring.Body.GetWorldPosition(FixedLinearSpring.BodyAttachPoint);
            Endpoint2 = FixedLinearSpring.WorldAttachPoint;
        }

    }
}
