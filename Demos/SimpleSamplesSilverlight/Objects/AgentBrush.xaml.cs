using FarseerGames.SimpleSamplesSilverlight.Drawing;

namespace FarseerGames.SimpleSamplesSilverlight.Objects
{
    public partial class AgentBrush : IDrawingBrush
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