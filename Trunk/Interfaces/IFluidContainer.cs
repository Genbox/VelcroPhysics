using FarseerGames.FarseerPhysics.Collisions;
#if (XNA)
using Microsoft.Xna.Framework; 
#endif

namespace FarseerGames.FarseerPhysics.Interfaces
{
    public interface IFluidContainer
    {
        bool Intersect(AABB aabb);
        bool Contains(ref Vector2 vector);
    }
}