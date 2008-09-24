using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FarseerSilverlightDemos.Drawing
{
    public partial class AgentBrush : System.Windows.Controls.UserControl, IDrawingBrush
    {
        float width = 120;
        float height = 120;
        public BrushExtender Extender = new BrushExtender();
        public AgentBrush()
        {
            InitializeComponent();
            Extender.child = agent;
            Extender.rotateTransform = rotateTransform;
        }

        public void Update()
        {
            Extender.Update();
        }
    }
}
