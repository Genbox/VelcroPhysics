using FarseerGames.FarseerPhysics.Collisions;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Interfaces
{
    public interface IFluidContainer
    {
        bool Intersect(ref AABB aabb);
        bool Contains(ref Vector2 vector);
    }
}