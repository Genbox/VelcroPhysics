using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Interfaces
{
    public interface IDebugView
    {
        void DrawJoint(Joint joint);
        void DrawShape(Shape shape, ref Transform transform, Color color);
    }
}