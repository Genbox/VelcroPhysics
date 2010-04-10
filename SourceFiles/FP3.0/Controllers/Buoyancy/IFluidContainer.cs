using FarseerPhysics.Collision;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Controllers.Buoyancy
{
    public interface IFluidContainer
    {
        bool Intersect(ref AABB aabb);
        bool Contains(ref Vector2 point);
        void Update(float dt);
    }
}