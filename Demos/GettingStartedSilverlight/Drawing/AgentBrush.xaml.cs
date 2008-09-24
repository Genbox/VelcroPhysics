using System.Windows.Controls;

namespace FarseerSilverlightDemos.Drawing
{
    public partial class AgentBrush : UserControl, IDrawingBrush
    {
        public BrushExtender Extender = new BrushExtender();

        public AgentBrush()
        {
            InitializeComponent();
            Extender.child = agent;
            Extender.rotateTransform = rotateTransform;
        }

        #region IDrawingBrush Members

        public void Update()
        {
            Extender.Update();
        }

        #endregion
    }
}