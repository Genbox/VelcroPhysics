using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Mathematics;
#if (XNA)
using Microsoft.Xna.Framework;
#endif

namespace FarseerGames.FarseerPhysics.Controllers
{
    public interface IFluidContainer
    {
        bool Intersect(AABB aabb);
        bool Contains(ref Vector2 vector);
    }
}