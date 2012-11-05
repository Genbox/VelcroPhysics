using FarseerPhysics.Physics;
using FarseerPhysics.Physics.Collisions;

namespace FarseerPhysics.Dynamics
{
    public partial class Fixture
    {
        public FluidCollisionProperties FluidProperties { get; set; }
        public RigidBody RigidBody { get; set; }
    }
}