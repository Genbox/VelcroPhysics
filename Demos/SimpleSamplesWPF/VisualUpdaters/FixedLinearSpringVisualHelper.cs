using FarseerGames.FarseerPhysics.Dynamics.Springs;

namespace SimpleSamplesWPF.VisualUpdaters
{
    public class FixedLinearSpringVisualHelper
    {
        private readonly ILinearSpringVisual visual;
        private readonly FixedLinearSpring spring;

        public FixedLinearSpringVisualHelper(ILinearSpringVisual visual,FixedLinearSpring spring)
        {
            this.visual = visual;
            this.spring = spring;

            spring.SpringUpdated += OnSpringUpdated;
            Update();
        }

        bool OnSpringUpdated(Spring sender, FarseerGames.FarseerPhysics.Dynamics.Body body)
        {
            Update();

            return true;
        }

        private void Update()
        {
            if (spring == null) return;
            visual.Endpoint1 = spring.Body.GetWorldPosition(spring.BodyAttachPoint);
            visual.Endpoint2 = spring.WorldAttachPoint;
        }
    }
}