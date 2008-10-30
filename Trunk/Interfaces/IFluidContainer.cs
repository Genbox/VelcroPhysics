using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Mathematics;
#if (XNA)
using Microsoft.Xna.Framework;
#else

#endif

namespace FarseerGames.FarseerPhysics.Interfaces
{
    public interface IFluidContainer
    {
        bool Intersect(AABB aabb);
        bool Contains(ref Vector2 vector);
    }
}