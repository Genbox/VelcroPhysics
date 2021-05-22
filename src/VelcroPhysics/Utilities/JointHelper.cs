using Genbox.VelcroPhysics.Dynamics;

namespace Genbox.VelcroPhysics.Utilities
{
    public static class JointHelper
    {
        public static void LinearStiffness(float frequencyHertz, float dampingRatio, Body bodyA, Body bodyB, out float stiffness, out float damping)
        {
            float massA = bodyA.Mass;

            float massB = 0;
            
            if (bodyB != null)
                massB = bodyB.Mass;

            float mass;

            if (massA > 0.0f && massB > 0.0f)
                mass = massA * massB / (massA + massB);
            else if (massA > 0.0f)
                mass = massA;
            else
                mass = massB;

            float omega = MathConstants.TwoPi * frequencyHertz;
            stiffness = mass * omega * omega;
            damping = 2.0f * mass * dampingRatio * omega;
        }

        public static void AngularStiffness(float frequencyHertz, float dampingRatio, Body bodyA, Body bodyB, out float stiffness, out float damping)
        {
            float inertiaA = bodyA.Inertia;
            float inertiaB = bodyB.Inertia;
            float I;

            if (inertiaA > 0.0f && inertiaB > 0.0f)
                I = inertiaA * inertiaB / (inertiaA + inertiaB);
            else if (inertiaA > 0.0f)
                I = inertiaA;
            else
                I = inertiaB;

            float omega = MathConstants.TwoPi * frequencyHertz;
            stiffness = I * omega * omega;
            damping = 2.0f * I * dampingRatio * omega;
        }
    }
}
